namespace EarlyXrm.PacHelper
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

            void ExOle(object sender, EventArgs e)
            {
                ThreadHelper.ThrowIfNotOnUIThread();

                var name = resource + ".ps1";

                var solutionDir = Path.GetDirectoryName(_dte.Solution.FullName);
                var solutionArg = $@" -SolutionDir ""{solutionDir}""";

                var targetPath = "";
                var targetDirArg = "";
                var projDir = "";
                var projectArg = "";
                var itemArg = "";

                var activeProjects = ((Array)_dte.ActiveSolutionProjects).Cast<Project>().ToArray();

                if (activeProjects.Length == 1)
                {
                    projDir = Path.GetDirectoryName(activeProjects[0].FullName);
                    projectArg = $@" -ProjDir ""{projDir}""";

                    var selectedItem = SelectedItem(_dte);
                    itemArg = selectedItem == null ? "" :  $@" -ItemPath ""{selectedItem}""";

                    var activeConfig = activeProjects[0].ConfigurationManager.ActiveConfiguration;
                    var targetPathProperty = activeConfig.Properties.Item("OutputPath");
                    targetPath = targetPathProperty.Value.ToString();
                    targetDirArg = $@" -TargetDir {projDir}\{targetPath}";
                }
                
                var process = new System.Diagnostics.Process();
                process.StartInfo.FileName = "powershell.exe";
                process.StartInfo.Arguments = $@"-NoExit -ExecutionPolicy Unrestricted ";

                if (targetPath == "")
                {
                    var resourceStream = Assembly.GetExecutingAssembly().GetManifestResourceStream($"EarlyXrm.PacHelper.Commands.{name}");

                    string cmd;
                    using (var fileStream = new StreamReader(resourceStream))
                        cmd = fileStream.ReadToEnd();

                    process.StartInfo.Arguments += $@"-Command ""{cmd}""";
                }
                else
                {
                    var projFile = $"{projDir}\\{name}";
                    var solutionFile = $"{solutionDir}\\{name}";

                    if (projDir != "" && File.Exists(projFile))
                    {
                        process.StartInfo.Arguments += $@"-File ""{projFile}""";
                    }
                    else if (solutionFile != "" && File.Exists(solutionFile))
                    {
                        process.StartInfo.Arguments += $@"-File ""{solutionFile}""";
                    }
                    else
                    {
                        var targetFile = $"{projDir}\\{targetPath}{name}";

                        var resourceStream = Assembly.GetExecutingAssembly().GetManifestResourceStream($"EarlyXrm.PacHelper.Commands.{name}");

                        using (var fileStream = File.Open(targetFile, FileMode.Create))
                                resourceStream.CopyTo(fileStream);

                        process.StartInfo.Arguments += $@"-File ""{targetFile}""";
                    }

                    process.StartInfo.WorkingDirectory = projDir;
                }

                process.StartInfo.Arguments += $@"{solutionArg}{projectArg}{itemArg}{targetDirArg}";
                process.Start();
            }

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

                cmd.Visible = cmd.Enabled = SelectedItem(_dte)?.EndsWith(suffix) ?? false;
            };
            commandService.AddCommand(command);
        }

        private static string SelectedItem(DTE2 dte)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            var activeDocument = ((Array)dte.ToolWindows.SolutionExplorer.SelectedItems).Cast<UIHierarchyItem>().FirstOrDefault();
            var projectItem = activeDocument?.Object as ProjectItem;
            
            if (projectItem?.FileCount != 1) return null;

            var selectedFilename = projectItem.FileNames[0];

            return selectedFilename;
        }
    }
}