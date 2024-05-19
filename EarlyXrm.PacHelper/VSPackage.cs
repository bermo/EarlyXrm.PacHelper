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
    [ProvideAutoLoad(VSConstants.UICONTEXT.SolutionOpening_string, PackageAutoLoadFlags.BackgroundLoad)]
    public sealed class PacHelperPackage : AsyncPackage
    {
        protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            await this.JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);

            var scripts = new Dictionary<string, string>
            {
                { nameof(PackageIds.PacOrgSelect), "" },

                { nameof(PackageIds.PacSolutionSync), ".cdsproj" },
                { nameof(PackageIds.PacSolutionImport), ".cdsproj" },

                { nameof(PackageIds.PacDataSync), "schema.xml" },
                { nameof(PackageIds.PacDataImport), "schema.xml" },

                { nameof(PackageIds.PacPagesSync), "website.yml" },
                { nameof(PackageIds.PacPagesImport), "website.yml" }
            };

            foreach (var script in scripts)
            {
                var id = (int)typeof(PackageIds).GetField(script.Key, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static).GetRawConstantValue();

                await PowershellBase.InitializeAsync(this, id, script.Key, script.Value);
            }
        }
    }
}