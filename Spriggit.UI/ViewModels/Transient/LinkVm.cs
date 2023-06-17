using System.Reactive.Linq;
using System.Windows.Input;
using Noggog;
using Noggog.WPF;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Serilog;
using Spriggit.Core;
using Spriggit.Engine;
using Spriggit.UI.Services;
using Spriggit.UI.ViewModels.Singletons;

namespace Spriggit.UI.ViewModels.Transient;

public class LinkVm : ViewModel
{
    private readonly ILogger _logger;
    private readonly SpriggitEngine _engine;
    private readonly ActivePanelVm _activePanelVm;
    private readonly EditLinkVm _editLinkVm;
    private readonly PackageInputQuery _packageInputQuery;
    public LinkInputVm Input { get; }
    
    public ICommand SyncToGitCommand { get; }
    public ICommand SyncToModCommand { get; }
    public ICommand EditSettingsCommand { get; }
    
    [Reactive] public bool Syncing { get; private set; }

    private readonly ObservableAsPropertyHelper<bool> _inError;
    public bool InError => _inError.Value;

    public delegate LinkVm InputFactory(LinkInputVm input);

    private readonly ObservableAsPropertyHelper<GetResponse<SpriggitMeta>> _metaToUse;
    public GetResponse<SpriggitMeta> MetaToUse => _metaToUse.Value;

    public LinkVm(
        ILogger logger,
        SpriggitEngine engine,
        ActivePanelVm activePanelVm,
        EditLinkVm editLinkVm,
        LinkInputVm input,
        PackageInputQuery packageInputQuery,
        LinkSourceCategoryToPackageName linkSourceCategoryToPackageName)
    {
        _logger = logger;
        _engine = engine;
        _activePanelVm = activePanelVm;
        _editLinkVm = editLinkVm;
        _packageInputQuery = packageInputQuery;
        Input = input;
        _inError = Observable.CombineLatest(
                Input.ModPathPicker.WhenAnyValue(x => x.InError),
                Input.GitFolderPicker.WhenAnyValue(x => x.InError),
                (m, g) => m && g)
            .ToProperty(this, nameof(InError));

        var canRun = Observable.CombineLatest(
            this.WhenAnyValue(x => x.InError),
            this.WhenAnyValue(x => x.Syncing),
            (err, syncing) => !err && !syncing);

        _metaToUse = this.WhenAnyValue(
                x => x.Input.SourceCategory,
                x => x.Input.Release,
                x => x.Input.Version,
                x => x.Input.PackageName)
            .Select(async x =>
            {
                var package = linkSourceCategoryToPackageName.GetPackageName(x.Item1, x.Item2, x.Item4);
                if (package.Failed) return package.BubbleFailure<SpriggitMeta>();
                    
                var vers = await packageInputQuery.GetVersionToUse(x.Item1, x.Item2, x.Item3, x.Item4,
                    CancellationToken.None);
                if (vers.Failed) return vers.BubbleFailure<SpriggitMeta>();

                return GetResponse<SpriggitMeta>.Succeed(new SpriggitMeta(
                    new SpriggitSource()
                    {
                        Version = vers.Value,
                        PackageName = package.Value,
                    },
                    x.Item2));
            })
            .Switch()
            .ToProperty(this, nameof(MetaToUse));

        SyncToModCommand = ReactiveCommand.Create(
            SyncToMod,
            canRun);

        SyncToGitCommand = ReactiveCommand.Create(
            SyncToGit,
            canRun);

        EditSettingsCommand = ReactiveCommand.Create(OpenSettings);
    }

    private async Task SyncToGit()
    {
        try
        {
            var meta = MetaToUse;
            if (meta.Failed)
            {
                _logger.Error("Could not sync to git {Reason}", meta.Reason);
                return;
            }

            Syncing = true;
            await _engine.Serialize(
                bethesdaPluginPath: Input.ModPathPicker.TargetPath,
                outputFolder: Input.GitFolderPicker.TargetPath,
                meta: meta.Value);
        }
        catch (Exception e)
        {
            _logger.Error(e, "Error syncing to Git");
        }
        finally
        {
            Syncing = false;
        }
    }
    
    private async Task SyncToMod()
    {
        try
        {
            Syncing = true;
            await _engine.Deserialize(
                spriggitPluginPath: Input.GitFolderPicker.TargetPath,
                outputFile: Input.ModPathPicker.TargetPath,
                source: null);
        }
        catch (Exception e)
        {
            _logger.Error(e, "Error syncing to Mod");
        }
        finally
        {
            Syncing = false;
        }
    }

    public void OpenSettings()
    {
        _editLinkVm.Target = Input;
        _activePanelVm.Focus(_editLinkVm);
    }
}