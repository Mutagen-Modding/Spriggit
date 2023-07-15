using System.Reactive;
using System.Reactive.Disposables;
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
    public LinkInputVm Input { get; }
    
    public ReactiveCommand<Unit, Unit> SyncToGitCommand { get; }
    public ReactiveCommand<Unit, Unit> SyncToModCommand { get; }
    public ReactiveCommand<Unit, Unit> CancelSyncToGitCommand { get; }
    public ReactiveCommand<Unit, Unit> CancelSyncToModCommand { get; }
    public ICommand EditSettingsCommand { get; }

    private readonly ObservableAsPropertyHelper<bool> _syncing;
    public bool Syncing => _syncing.Value;

    public enum SyncState
    {
        None,
        Mod,
        Git,
        Cancelling
    }
    
    [Reactive] public SyncState State { get; private set; }

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
        EntryPointCache entryPointCache,
        LinkSourceCategoryToPackageName linkSourceCategoryToPackageName)
    {
        _logger = logger;
        _engine = engine;
        _activePanelVm = activePanelVm;
        _editLinkVm = editLinkVm;
        Input = input;
        _inError = Observable.CombineLatest(
                Input.ModPathPicker.WhenAnyValue(x => x.InError),
                Input.GitFolderPicker.WhenAnyValue(x => x.InError),
                (m, g) => m && g)
            .ToProperty(this, nameof(InError));
        _syncing = this.WhenAnyValue(x => x.State)
            .Select(x => x != SyncState.None)
            .ToProperty(this, nameof(Syncing));

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
                (r, e) => r && e)
                .ObserveOnGui());
        CancelSyncToModCommand = ReactiveCommand.Create(
            () => { },
            canExecute: this.WhenAnyValue(x => x.State).Select(x => x == SyncState.Mod)
                .ObserveOnGui());
        WrapTranslation(
            SyncToModCommand.EndingExecution(),
            CancelSyncToModCommand.EndingExecution(),
            SyncState.Mod,
            SyncToMod);

        SyncToGitCommand = ReactiveCommand.Create<Unit>(
            execute: _ => { },
            Observable.CombineLatest(
                canRun,
                Input.ModPathPicker.WhenAnyValue(x => x.Exists),
                (r, e) => r && e)
                .ObserveOnGui());
        CancelSyncToGitCommand = ReactiveCommand.Create(
            () => { },
            canExecute: this.WhenAnyValue(x => x.State).Select(x => x == SyncState.Git)
                .ObserveOnGui());
        WrapTranslation(
            SyncToGitCommand.EndingExecution(),
            CancelSyncToGitCommand.EndingExecution(),
            SyncState.Git,
            SyncToGit);

        EditSettingsCommand = ReactiveCommand.Create(OpenSettings);
        
        // Pre-cache entry points
        this.WhenAnyValue(x => x.MetaToUse)
            .Subscribe(async m =>
            {
                if (m.Succeeded)
                {
                    await entryPointCache.GetFor(m.Value, CancellationToken.None);
                }
            })
            .DisposeWith(this);
    }

    private void WrapTranslation(
        IObservable<Unit> signal,
        IObservable<Unit> cancel,
        SyncState state,
        Func<CancellationToken, Task> toDo)
    {
        signal
            .WithLatestFrom(this.WhenAnyValue(x => x.Syncing), (_, x) => x)
            .Where(x => !x)
            .Do(_ => State = state)
            .ObserveOn(RxApp.TaskpoolScheduler)
            .Select(_ =>
            {
                return Observable.Create<Unit>(async (o) =>
                {
                    try
                    {
                        CancellationTokenSource cancelSource = new();
                        using var cancelDisp = cancel.Subscribe(x =>
                        {
                            State = SyncState.Cancelling;
                            cancelSource.Cancel();
                        });
                        await toDo(cancelSource.Token);
                    }
                    finally
                    {
                        State = SyncState.None;
                        o.OnCompleted();
                    }
                    
                    return Disposable.Empty;
                });
            })
            .Switch()
            .ObserveOn(RxApp.MainThreadScheduler)
            .Do(_ => State = SyncState.None)
            .Subscribe()
            .DisposeWith(this);
    }

    private async Task SyncToGit(CancellationToken cancel)
    {
        try
        {
            _logger.Information("Syncing from Mod to Git. {ModPath} -> {GitPath}", Input.ModPathPicker.TargetPath,
                Input.GitFolderPicker.TargetPath);
            var meta = MetaToUse;
            if (meta.Failed)
            {
                _logger.Error("Could not sync to git {Reason}", meta.Reason);
                return;
            }

            await _engine.Serialize(
                bethesdaPluginPath: Input.ModPathPicker.TargetPath,
                outputFolder: Input.GitFolderPicker.TargetPath,
                meta: meta.Value,
                cancel);
            _logger.Information("Finished syncing from Mod to Git. {ModPath} -> {GitPath}", Input.ModPathPicker.TargetPath,
                Input.GitFolderPicker.TargetPath);
        }
        catch (OperationCanceledException)
        {
            _logger.Information("Cancelled syncing from Mod to Git. {ModPath} -> {GitPath}", Input.ModPathPicker.TargetPath,
                Input.GitFolderPicker.TargetPath);
        }
        catch (Exception e)
        {
            _logger.Error(e, "Error syncing to Git");
        }
    }
    
    private async Task SyncToMod(CancellationToken cancel)
    {
        try
        {
            _logger.Information("Syncing from Git to Mod. {GitPath} -> {ModPath}", Input.GitFolderPicker.TargetPath, Input.ModPathPicker.TargetPath);
            await _engine.Deserialize(
                spriggitPluginPath: Input.GitFolderPicker.TargetPath,
                outputFile: Input.ModPathPicker.TargetPath,
                source: null,
                cancel: cancel);
            _logger.Information("Finished syncing from Git to Mod. {GitPath} -> {ModPath}", Input.GitFolderPicker.TargetPath, Input.ModPathPicker.TargetPath);
        }
        catch (OperationCanceledException)
        {
            _logger.Information("Cancelled syncing from Git to Mod. {GitPath} -> {ModPath}", Input.GitFolderPicker.TargetPath, Input.ModPathPicker.TargetPath);
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