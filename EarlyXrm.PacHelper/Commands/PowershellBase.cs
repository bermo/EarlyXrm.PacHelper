namespace EarlyXrm.PacHelper.Commands
{
    using EnvDTE;
    using EnvDTE80;
    using Microsoft.VisualStudio.Shell;
    using Microsoft;
    using System;
    using System.ComponentModel.Design;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Reflection;
    using System.IO;

    internal abstract class PowershellBase
    {
        private static DTE2 _dte;

        public static async Task InitializeAsync(AsyncPackage package, int cmdId, string resource, string suffix)
        {
            var commandService = await package.GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;
            Assumes.Present(commandService);

            _dte = await package.GetServiceAsync(typeof(DTE)) as DTE2;
            Assumes.Present(_dte);

            var commandId = new CommandID(PackageGuids.guidPacHelperPackageCmdSet, cmdId);

            EventHandler ExOle = (object sender, EventArgs e) => {

                ThreadHelper.ThrowIfNotOnUIThread();

                var resourceStream = Assembly.GetExecutingAssembly()
                        .GetManifestResourceStream($"EarlyXrm.PacHelper.Commands.{resource}.ps1");

                var tempFile = Path.GetTempFileName() + ".ps1";
                //try
                //{
                    using (var fileStream = File.OpenWrite(tempFile))
                        resourceStream.CopyTo(fileStream);

                    var process = new System.Diagnostics.Process();
                    process.StartInfo.FileName = "powershell.exe";
                    process.StartInfo.Arguments = $@"-NoExit -File ""{tempFile}""";
                    process.StartInfo.WorkingDirectory = AppContext.BaseDirectory;
                    process.Start();
                //}
                //finally
                //{
                //    File.Delete(tempFile);
                //}
            };

            var command = new OleMenuCommand(ExOle, commandId);
            command.BeforeQueryStatus += (object sender, EventArgs e) =>
            {
                ThreadHelper.ThrowIfNotOnUIThread();

                var cmd = (OleMenuCommand)sender;

                if (string.IsNullOrEmpty(suffix))
                {
                    cmd.Visible = cmd.Enabled = true;
                    return;
                }

                var activeDocument = ((Array)_dte.ToolWindows.SolutionExplorer.SelectedItems).Cast<UIHierarchyItem>().FirstOrDefault();
                var projectItem = activeDocument?.Object as ProjectItem;
                if (projectItem == null)
                {
                    return;
                }
                var selectedPath = projectItem.FileNames[1];

                cmd.Visible = cmd.Enabled = selectedPath?.EndsWith(suffix) ?? false;
            };
            commandService.AddCommand(command);
        }
    }
}