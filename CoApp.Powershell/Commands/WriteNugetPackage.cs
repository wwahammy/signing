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
    using System.Threading.Tasks;
    using ClrPlus.Powershell.Core;
    using ClrPlus.Powershell.Rest.Commands;
    using ClrPlus.Scripting.MsBuild;

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

        [Parameter(HelpMessage = "Just generate the intermediate files")]
        public SwitchParameter NoNupkg { get; set; }

        protected override void ProcessRecord() {
            if(Remote) {
                ProcessRecordViaRest();
                return;
            }

            ProviderInfo packagePathProviderInfo;
            var pkgPath = SessionState.Path.GetResolvedProviderPathFromPSPath(Package, out packagePathProviderInfo);

            using (var script = new PackageScript(pkgPath.FirstOrDefault())) {
                if (script.Validate()) {
                    script.SaveProps();
                    script.SaveTargets();
                    script.SaveNuspec();

                    script.NuPack();
                }
            }
        }
    }
}
