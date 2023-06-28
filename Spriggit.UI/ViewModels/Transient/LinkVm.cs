using System.Reactive;
using System.Reactive.Linq;
using System.Windows.Input;
using Mutagen.Bethesda.Serialization.Streams;
using Noggog;
using Noggog.WPF;
using NSubstitute;
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
    
    public ReactiveCommand<Unit, Unit> SyncToGitCommand { get; }
    public ReactiveCommand<Unit, Unit> SyncToModCommand { get; }
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

        SyncToModCommand = ReactiveCommand.Create<Unit>(
            execute: _ => { },
            Observable.CombineLatest(
                canRun,
                Input.GitFolderPicker.WhenAnyValue(x => x.Exists),
                (r, e) => r && e));
        WrapTranslation(
            SyncToModCommand.EndingExecution(),
            SyncToMod);

        SyncToGitCommand = ReactiveCommand.Create<Unit>(
            execute: _ => { },
            Observable.CombineLatest(
                canRun,
                Input.ModPathPicker.WhenAnyValue(x => x.Exists),
                (r, e) => r && e));
        WrapTranslation(
            SyncToGitCommand.EndingExecution(),
            SyncToGit);

        EditSettingsCommand = ReactiveCommand.Create(OpenSettings);
    }

    private void WrapTranslation(IObservable<Unit> signal, Func<Task> toDo)
    {
        signal
            .WithLatestFrom(this.WhenAnyValue(x => x.Syncing), (_, x) => x)
            .Where(x => !x)
            .Do(_ => Syncing = true)
            .ObserveOn(RxApp.TaskpoolScheduler)
            .SelectTask(_ => toDo())
            .ObserveOn(RxApp.MainThreadScheduler)
            .Do(_ => Syncing = false)
            .Subscribe()
            .DisposeWith(this);
    }

    private async Task SyncToGit()
    {
        try
        {
            _logger.Information("Syncing from Mod to Git. {ModPath} -> {GitPath}", Input.ModPathPicker.TargetPath, Input.GitFolderPicker.TargetPath);
            var meta = MetaToUse;
            if (meta.Failed)
            {
                _logger.Error("Could not sync to git {Reason}", meta.Reason);
                return;
            }

            await _engine.Serialize(
                bethesdaPluginPath: Input.ModPathPicker.TargetPath,
                outputFolder: Input.GitFolderPicker.TargetPath,
                meta: meta.Value);
        }
        catch (Exception e)
        {
            _logger.Error(e, "Error syncing to Git");
        }
    }
    
    private async Task SyncToMod()
    {
        try
        {
            _logger.Information("Syncing from Git to Mod. {GitPath} -> {ModPath}", Input.GitFolderPicker.TargetPath, Input.ModPathPicker.TargetPath);
            await _engine.Deserialize(
                spriggitPluginPath: Input.GitFolderPicker.TargetPath,
                outputFile: Input.ModPathPicker.TargetPath,
                source: null);
        }
        catch (Exception e)
        {
            _logger.Error(e, "Error syncing to Mod");
        }
    }

    public void OpenSettings()
    {
        _editLinkVm.Target = Input;
        _activePanelVm.Focus(_editLinkVm);
    }
}