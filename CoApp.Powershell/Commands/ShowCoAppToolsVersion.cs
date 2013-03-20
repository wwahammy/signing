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
    using System.Diagnostics;
    using System.IO;
    using System.Management.Automation;
    using System.Management.Automation.Runspaces;
    using System.Net;
    using ClrPlus.Core.Extensions;
    using ClrPlus.Platform;
    using ClrPlus.Powershell.Core;
    using ClrPlus.Powershell.Rest.Commands;
    using Microsoft.Deployment.WindowsInstaller;

    [Cmdlet(AllVerbs.Show, "CoAppToolsVersion")]
    public class ShowCoAppToolsVersion : RestableCmdlet<ShowCoAppToolsVersion> {
        protected override void ProcessRecord() {
            if (Remote) {
                ProcessRecordViaRest();
                return;
            }
            WriteObject("CoApp Powershell Developer Tools Version: {0}".format(this.Assembly().Version()));
        }
    }

    [Cmdlet(AllVerbs.Update, "CoAppTools")]
    public class UpdateCoAppTools : RestableCmdlet<UpdateCoAppTools> {

        [Parameter]
        public SwitchParameter KillPowershells;

        protected override void ProcessRecord() {
            if(Remote) {
                ProcessRecordViaRest();
                return;
            }

            var wc = new WebClient();
            var tmp = "coapp.tools.powershell.msi".GenerateTemporaryFilename();
            wc.DownloadFile(@"http://downloads.coapp.org/files/CoApp.Tools.Powershell.msi", tmp);
            FourPartVersion ver = FileVersionInfo.GetVersionInfo(tmp);

            if (ver == 0) {
                using (Database db = new Database(tmp)) {
                    ver = db.ExecuteScalar("SELECT `Value` FROM `Property` WHERE `Property` = 'ProductVersion'") as string;
                }
            }

            FourPartVersion thisVer = this.Assembly().Version();
            if (ver < thisVer) {
                WriteObject("The current version {0} is newer than the version on the web {1}.".format( thisVer, ver));
                return;
            }

            if(ver == thisVer) {
                WriteObject("The current version {0} is the current version.".format(thisVer, ver));
                return;
            }

            WriteObject("The current version {0} will be replaced with the newer than the version from the web {1}.".format(thisVer, ver));

            using (dynamic ps = Runspace.DefaultRunspace.Dynamic()) {
                ps.InvokeExpression("msiexec.exe /i {0}".format(tmp));
            }

            if (!KillPowershells) {
                WriteObject("FYI, the installer can't actually update without killing all the powershell tasks.");
                WriteObject("If you are running as admin, you can do this automatically with the -KillPowershells switch on this command.");
            } else {
                using (var ps = Runspace.DefaultRunspace.Dynamic()) {
                    ps.StopProcess(name: "powershell");
                }
            }

        }
    }
}