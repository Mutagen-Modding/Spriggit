using System.Reactive.Linq;
using System.Windows.Input;
using Noggog.WPF;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Serilog;
using Spriggit.Core;
using Spriggit.Engine;
using Spriggit.UI.Services;

namespace Spriggit.UI.ViewModels;

public class LinkVm : ViewModel
{
    private readonly ILogger _logger;
    private readonly SpriggitEngine _engine;
    public LinkInputVm Input { get; }
    
    public ICommand SyncToGitCommand { get; }
    public ICommand SyncToModCommand { get; }
    
    [Reactive] public bool Syncing { get; private set; }

    private readonly ObservableAsPropertyHelper<bool> _inError;
    public bool InError => _inError.Value;

    public delegate LinkVm InputFactory(LinkInputVm input);

    public LinkVm(
        ILogger logger,
        SpriggitEngine engine,
        LinkInputVm input)
    {
        _logger = logger;
        _engine = engine;
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

        SyncToModCommand = ReactiveCommand.Create(
            SyncToMod,
            canRun);

        SyncToGitCommand = ReactiveCommand.Create(
            SyncToGit,
            canRun);
    }

    private async Task SyncToGit()
    {
        try
        {
            Syncing = true;
            await _engine.Serialize(
                bethesdaPluginPath: Input.ModPathPicker.TargetPath,
                outputFolder: Input.GitFolderPicker.TargetPath,
                meta: new SpriggitMeta());
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
                spriggitPluginPath: Input.ModPathPicker.TargetPath,
                outputFile: Input.GitFolderPicker.TargetPath);
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
}