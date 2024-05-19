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
    using System.Collections.Generic;

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

                var process = new System.Diagnostics.Process();
                process.StartInfo.FileName = "powershell.exe";
                process.StartInfo.Arguments = $@"-NoExit -ExecutionPolicy Unrestricted";

                var scriptName = resource + ".ps1";

                if (_dte.Solution.Count == 0)
                {
                    var resourceStream = Assembly.GetExecutingAssembly().GetManifestResourceStream($"EarlyXrm.PacHelper.Commands.{scriptName}");

                    string cmd;
                    using (var fileStream = new StreamReader(resourceStream))
                        cmd = fileStream.ReadToEnd();

                    process.StartInfo.Arguments += $" -Command \"{cmd}\"";
                    process.Start();

                    return;
                }

                var solutionDir = Path.GetDirectoryName(_dte.Solution.FullName);

                var scriptArgs = new Dictionary<string, string>
                {
                    { "SolutionDir", solutionDir }
                };

                var projDir = ""; 
                var targetPath = "";

                var activeProjects = ((Array)_dte.ActiveSolutionProjects).Cast<Project>().ToArray();

                if (activeProjects.Length == 1)
                {
                    projDir = Path.GetDirectoryName(activeProjects[0].FullName);
                    scriptArgs.Add("ProjDir", projDir);

                    var selectedItem = SelectedItem(_dte);
                    if (selectedItem != null)
                        scriptArgs.Add("ItemPath", selectedItem);

                    var activeConfig = activeProjects[0].ConfigurationManager.ActiveConfiguration;
                    var targetPathProperty = activeConfig.Properties.Item("OutputPath");
                    targetPath = targetPathProperty.Value.ToString();
                    scriptArgs.Add("TargetDir", $"{projDir}\\{targetPath}");
                }

                if (targetPath == "")
                {
                    var resourceStream = Assembly.GetExecutingAssembly().GetManifestResourceStream($"EarlyXrm.PacHelper.Commands.{scriptName}");

                    string cmd;
                    using (var fileStream = new StreamReader(resourceStream))
                        cmd = fileStream.ReadToEnd();

                    process.StartInfo.Arguments += $" -Command \"{cmd}\"";
                }
                else
                {
                    var projFile = $"{projDir}\\{scriptName}";
                    var solutionFile = $"{solutionDir}\\{scriptName}";

                    if (projDir != "" && File.Exists(projFile))
                    {
                        process.StartInfo.Arguments += $" -File \"{projFile}\"";
                    }
                    else if (solutionFile != "" && File.Exists(solutionFile))
                    {
                        process.StartInfo.Arguments += $" -File \"{solutionFile}\"";
                    }
                    else
                    {
                        var targetFile = $"{projDir}\\{targetPath}{scriptName}";

                        var resourceStream = Assembly.GetExecutingAssembly().GetManifestResourceStream($"EarlyXrm.PacHelper.Commands.{scriptName}");

                        using (var fileStream = File.Open(targetFile, FileMode.Create))
                                resourceStream.CopyTo(fileStream);

                        process.StartInfo.Arguments += $" -File \"{targetFile}\"";
                    }

                    process.StartInfo.WorkingDirectory = projDir;
                }

                foreach(var scriptArg in scriptArgs)
                    process.StartInfo.Arguments += $@" -{scriptArg.Key} ""{scriptArg.Value.TrimEnd('\\')}""";

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

            var selectedItems = ((Array)dte.ToolWindows.SolutionExplorer.SelectedItems).Cast<UIHierarchyItem>().ToArray();
            
            if (selectedItems.Length != 1)
            {
                return null;
            }
                
            var selectedItem = selectedItems[0];

            var projectItem = selectedItem?.Object as ProjectItem;

            if (projectItem == null)
            {
                var project = selectedItem?.Object as Project;
                return project.FullName;
            }

            if (projectItem?.FileCount != 1)
            {
                return null;
            }

            var selectedFilename = projectItem.FileNames[0];

            return selectedFilename;
        }
    }
}