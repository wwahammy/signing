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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CoApp.Powershell.Commands {
    using System.IO;
    using System.Management.Automation;
    using System.Management.Automation.Runspaces;
    using System.Threading.Tasks;
    using ClrPlus.Core.Extensions;
    using ClrPlus.Core.Tasks;
    using ClrPlus.Powershell.Core;
    using ClrPlus.Powershell.Rest.Commands;
    using ClrPlus.Scripting.Languages.PropertySheet;
    using ClrPlus.Scripting.Languages.PropertySheetV3;
    using ClrPlus.Scripting.MsBuild;
    using ClrPlus.Scripting.MsBuild.Packaging;

    [Cmdlet(AllVerbs.Write, "NuGetPackage")]
    public class WriteNuGetPackage : RestableCmdlet<WriteNuGetPackage> {
        static WriteNuGetPackage() {
            try {
                var asmDir = Path.GetDirectoryName(typeof (WriteNuGetPackage).Assembly.Location);
                if (!string.IsNullOrEmpty(asmDir)) {
                    var path = Environment.GetEnvironmentVariable("path");

                    if (string.IsNullOrEmpty(path) || !path.Contains(asmDir)) {
                        Environment.SetEnvironmentVariable("path", path + ";" + asmDir + ";" + asmDir + "\\etc");
                    }
                }
            } catch (Exception e) {
                Console.WriteLine("EXC: {0}/{1}",e.Message, e.StackTrace);
            }
        }

        [Parameter(HelpMessage = "Autopackage script file (.autopkg)", Mandatory = true, Position = 0)]
        public string Package { get; set; }

        [Parameter(HelpMessage = "Don't clean up intermediate  files")]
        public SwitchParameter NoClean { get; set; }


        protected override void ProcessRecord() {
            if(Remote) {
                ProcessRecordViaRest();
                return;
            }

            ProviderInfo packagePathProviderInfo;
            var pkgPath = SessionState.Path.GetResolvedProviderPathFromPSPath(Package, out packagePathProviderInfo);

            using(var local = LocalEventSource) {
                
                local.Events += new SourceError((code, location, message, objects) => {
                    location = location ?? SourceLocation.Unknowns;
                    Host.UI.WriteErrorLine("{0}:Error {1}:{2}".format(location.FirstOrDefault(), code, message.format(objects)));
                    return true;
                });

                if (!NoWarnings) {
                    local.Events += new SourceWarning((code, location, message, objects) => {
                        WriteWarning("{0}:Warning {1}:{2}".format( (location ?? SourceLocation.Unknowns).FirstOrDefault(), message.format(objects)));
                        return false;
                    });
                }
                
                local.Events += new SourceDebug((code, location, message, objects) => {
                    WriteVerbose("{0}:DebugMessage {1}:{2}".format((location ?? SourceLocation.Unknowns).FirstOrDefault(), code, message.format(objects)));
                    return false;
                });
             
/*
                Event<Warning>.Raise("msg123", "warning message");
                Event<Debug>.Raise("dbg123", "debug message");
                Event<Trace>.Raise("trace123", "trace message");
                Event<Error>.Raise("err123", "error message");
                Event<Progress>.Raise("activitycode", -1, "something", "working along");
*/

                using(var script = new PackageScript(pkgPath.FirstOrDefault())) {
                    script.Save(PackageTypes.NuGet, !NoClean);
                }
             
            }

         
        }
    }
}
