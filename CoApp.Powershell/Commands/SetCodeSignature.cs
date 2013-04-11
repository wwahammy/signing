//-----------------------------------------------------------------------
// <copyright company="CoApp Project">
//     Copyright (c) 2010-2012 Garrett Serack and CoApp Contributors. 
//     Contributors can be discovered using the 'git log' command.
//     All rights reserved.
// </copyright>
// <license>
//     The software is licensed under the Apache 2.0 License (the "License")
//     You may not use the software except in compliance with the License. 
// </license>
//-----------------------------------------------------------------------

namespace CoApp.Powershell.Commands {
    using System;
    using System.IO;
    using System.Linq;
    using System.Management.Automation;
    using System.Management.Automation.Runspaces;
    using System.Security.Cryptography.X509Certificates;
    using ClrPlus.Core.Exceptions;
    using ClrPlus.Core.Extensions;
    using ClrPlus.Powershell.Core;
    using ClrPlus.Powershell.Provider.Base;
    using ClrPlus.Powershell.Provider.Filesystem;
    using ClrPlus.Powershell.Rest.Commands;
    using ClrPlus.Signing.Signers;

    [Cmdlet(VerbsCommon.Set, "CodeSignature")]
    public class SetCodeSignature : RestableCmdlet<SetCodeSignature> {
        [Parameter(ValueFromPipeline = true, Mandatory = true, Position = 0)]
        [ValidateNotNullOrEmpty]
        public string Path {get; set;}

        [Parameter(Position = 1)]
        public string Destination {get; set;}

        [Parameter]
        public SwitchParameter StrongName {get; set;}

        private X509Certificate2 _certificate {get; set;}

        [Parameter]
        public string Certificate {get; set;}

        protected override void ProcessRecord() {
            //we need to absolutize(?) the path so when it gets to the server, it's not wildly confused about what to do with it
            var inputPath = ResolveSource();
            Path = inputPath.AbsolutePath;
            ILocation outputPath = String.IsNullOrWhiteSpace(Destination) ? inputPath : ResolveDestinationLocation();
            Destination = outputPath.AbsolutePath;

            if (Remote) {
                ProcessRecordViaRest();
                return;
            }

            try {
                using (var ps = Runspace.DefaultRunspace.Dynamic()) {
                    if (!String.IsNullOrWhiteSpace(Certificate)) {
                        _certificate = Enumerable.First(ps.GetItem(Path: Certificate));
                    }

                    var tempPath = CreateTempPath(inputPath);
                    Console.WriteLine(tempPath);
                    ps.CopyItemEx(Path: inputPath.AbsolutePath, Destination: tempPath);
                    if (AttemptToSignAuthenticode(tempPath) || AttemptToSignOPC(tempPath)) {
                        if (Destination == null) {
                            ps.CopyItemEx(Path: tempPath, Destination: Path, Force: true);
                            WriteObject(inputPath);
                        } else {
                            ps.CopyItemEx(Path: tempPath, Destination: Destination,  Force:true);
                            WriteObject(outputPath);
                        }
                    }
                }
            } catch (Exception e) {
                ThrowTerminatingError(new ErrorRecord(e, "0", ErrorCategory.PermissionDenied, null));
            }
        }

        private string CreateTempPath(ILocation file) {
            return System.IO.Path.Combine(System.IO.Path.GetTempPath(), Guid.NewGuid().ToString() + file.Name);
        }

        private bool AttemptToSignAuthenticode(string path) {
            try {
                var authenticode = new AuthenticodeSigner(_certificate);

                AttemptToSign(() => authenticode.Sign(path, StrongName));

                return true;
            } catch (Exception) {
                return false;
            }
        }

        private bool AttemptToSignOPC(string path) {
            try {
                var opc = new OPCSigner(_certificate);

                return AttemptToSign(() => opc.Sign(path));
            } catch (Exception) {
                return false;
            }
        }

        private bool AttemptToSign(Action signAction) {
            try {
                signAction();
                return true;
            } catch (FileNotFoundException fnfe) {
                ThrowTerminatingError(new ErrorRecord(fnfe, "none", ErrorCategory.OpenError, null));
            } catch (PathTooLongException ptle) {
                ThrowTerminatingError(new ErrorRecord(ptle, "none", ErrorCategory.InvalidData, null));
            } catch (UnauthorizedAccessException uae) {
                ThrowTerminatingError(new ErrorRecord(uae, "none", ErrorCategory.PermissionDenied, null));
            } catch (Exception e) {
                ThrowTerminatingError(new ErrorRecord(e, "none", ErrorCategory.NotSpecified, null));
            }
            return false;
        }

        private ILocation ResolveSource() {
            ProviderInfo sourceProviderInfo;
            var source = SessionState.Path.GetResolvedProviderPathFromPSPath(Path, out sourceProviderInfo);
            var path = source[0];
            return GetLocationResolver(sourceProviderInfo).GetLocation(path);
        }

        private ILocation ResolveDestinationLocation() {
            ProviderInfo destinationProviderInfo;
            try {
                //if Destination doesn't exist, this will throw
                var destination = SessionState.Path.GetResolvedProviderPathFromPSPath(Destination, out destinationProviderInfo);
                var path = destination[0];

                return GetLocationResolver(destinationProviderInfo).GetLocation(path);
            } catch (Exception) {
                //the destination didn't exist, probably a file
                var lastSlash = Destination.LastIndexOf('\\');
                var probablyDirectoryDestination = Destination.Substring(0, lastSlash);
                //if this throws not even the directory exists
                var destination = SessionState.Path.GetResolvedProviderPathFromPSPath(probablyDirectoryDestination, out destinationProviderInfo);

                var path = destination[0];

                return GetLocationResolver(destinationProviderInfo).GetLocation(path);
            }
        }

        public static ILocationResolver GetLocationResolver(ProviderInfo providerInfo) {
            var result = providerInfo as ILocationResolver;
            if (result == null) {
                if (providerInfo.Name == "FileSystem") {
                    return new FilesystemLocationProvider(providerInfo);
                }
            }
            if (result == null) {
                throw new ClrPlusException("Unable to create location resolver for {0}".format(providerInfo.Name));
            }
            return result;
        }
    }
}