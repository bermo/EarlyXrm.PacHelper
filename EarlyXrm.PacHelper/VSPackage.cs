namespace EarlyXrm.PacHelper
{
    using Microsoft.VisualStudio;
    using Microsoft.VisualStudio.Shell;
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using System.Threading;
    using Task = System.Threading.Tasks.Task;

    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [Guid(PackageGuids.guidPacHelperPackageString)]
    [InstalledProductRegistration(Vsix.Name, Vsix.Description, Vsix.Version)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [ProvideAutoLoad(VSConstants.UICONTEXT.SolutionHasSingleProject_string, PackageAutoLoadFlags.BackgroundLoad)]
    public sealed class PacHelperPackage : AsyncPackage
    {
        protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            await this.JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);

            var scripts = new Dictionary<int, (string, string)>
            {
                { PackageIds.cmdidorg_select, ("PacOrgSelect", "") },
                { PackageIds.cmdidPacSolutionSync, ("PacSolutionSync", ".cdsproj") },
                { PackageIds.cmdidPacSolutionImport, ("PacSolutionImport", ".cdsproj") },
                { PackageIds.PacDataSync, ("PacDataSync", "schema.xml") }
            };

            foreach (var script in scripts)
            {
                await PowershellBase.InitializeAsync(this, script.Key, script.Value.Item1, script.Value.Item2);
            }
        }
    }
}