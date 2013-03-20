using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CoApp.Powershell.Commands {
    using System.Management.Automation;
    using ClrPlus.Core.Exceptions;
    using ClrPlus.Platform;
    using ClrPlus.Powershell.Core;
    using ClrPlus.Powershell.Rest.Commands;

    [Cmdlet(AllVerbs.Invoke, "Build"),Alias("ptk")]
    public class InvokeBuildCmdlet : RestableCmdlet<InvokeBuildCmdlet> {

        [Parameter()]
        public string ScriptFile{ get; set; }

        [Parameter()]
        public SwitchParameter RescanTools { get; set; }

        [Parameter(Position = 0)]
        public string Command { get; set; }

        [Parameter(ValueFromRemainingArguments = true)]
        public string[] Targets { get; set; }

        protected override void BeginProcessing() {
            if(Remote) {
                ProcessRecordViaRest();
                return;
            }

            // Invoking a ptk script.
            if (string.IsNullOrWhiteSpace(ScriptFile)) {
                // search for it.
                ScriptFile = @"..\copkg\.buildinfo".WalkUpPath();
                if (string.IsNullOrEmpty(ScriptFile)) {
                    throw new ClrPlusException(@"Unable to find copkg\.buildinfo file anywhere in the current directory structure.");
                }
                
            }

        }
    }
}
