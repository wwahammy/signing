using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CoApp.Powershell.Commands {
    using System.IO;
    using System.Management.Automation;
    using ClrPlus.Powershell.Core;
    using ClrPlus.Powershell.Rest.Commands;
    using ClrPlus.Scripting.MsBuild;

    [Cmdlet(AllVerbs.Write, "NugetPackage")]
    public class WriteNugetPackage : RestableCmdlet<WriteNugetPackage> {
        static WriteNugetPackage() {
            var asmDir = Path.GetDirectoryName(typeof (WriteNugetPackage).Assembly.Location);
            if (!string.IsNullOrEmpty(asmDir)) {
                var path = Environment.GetEnvironmentVariable("path");

                if (string.IsNullOrEmpty(path) || !path.Contains(asmDir)) {
                    Environment.SetEnvironmentVariable("path", path + ";" + asmDir);
                }
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
