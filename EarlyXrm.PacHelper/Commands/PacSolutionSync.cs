namespace EarlyXrm.PacHelper.Commands
{
    using EnvDTE;
    using EnvDTE80;
    using Microsoft;
    using Microsoft.VisualStudio.Shell;
    using System;
    using System.ComponentModel.Design;
    using System.Linq;
    using System.Windows.Forms;
    using Task = System.Threading.Tasks.Task;

    internal sealed class PacSolutionSync
    {
        public static PacSolutionSync Instance
        {
            get;
            private set;
        }

        private static DTE2 _dte;

        public static async Task InitializeAsync(AsyncPackage package)
        {
            var commandService = await package.GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;
            Assumes.Present(commandService);

            _dte = await package.GetServiceAsync(typeof(DTE)) as DTE2;
            Assumes.Present(_dte);

            var commandId = new CommandID(PackageGuids.guidPacHelperPackageCmdSet, PackageIds.cmdidPacSolutionSync);
            var command = new OleMenuCommand(Execute, commandId);
            command.BeforeQueryStatus += Command_BeforeQueryStatus;
            commandService.AddCommand(command);
        }

        private static void Command_BeforeQueryStatus(object sender, EventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            var command = (OleMenuCommand)sender;
            var activeDocument = ((Array)_dte.ToolWindows.SolutionExplorer.SelectedItems).Cast<UIHierarchyItem>().FirstOrDefault();
            var selectedPath = ((ProjectItem)activeDocument.Object).FileNames[1];

            command.Visible = command.Enabled = selectedPath?.EndsWith(".cs") ?? false;
        }

        private static void Execute(object sender, EventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            MessageBox.Show("sdfdsf");
        }
    }
}
