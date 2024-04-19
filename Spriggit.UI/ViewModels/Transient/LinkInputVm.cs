using System.Collections.ObjectModel;
using System.Reactive.Linq;
using System.Windows.Input;
using DynamicData;
using Microsoft.WindowsAPICodePack.Dialogs;
using Mutagen.Bethesda;
using Noggog;
using Noggog.WPF;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Spriggit.Engine;
using Spriggit.UI.Services;
using Spriggit.UI.Settings;

namespace Spriggit.UI.ViewModels.Transient;

public class LinkInputVm : ViewModel
{
    public PathPickerVM ModPathPicker { get; } = new()
    {
        ExistCheckOption = PathPickerVM.CheckOptions.On,
        PathType = PathPickerVM.PathTypeOptions.File
    };
    
    public PathPickerVM GitFolderPicker { get; } = new()
    {
        PathType = PathPickerVM.PathTypeOptions.Folder,
        ExistCheckOption = PathPickerVM.CheckOptions.On
    };

    [Reactive] public GameRelease Release { get; set; } = GameRelease.SkyrimSE;

    private static ObservableCollection<GameRelease> ReleasesStatic { get; } = new(Enums<GameRelease>.Values.OrderBy(x => x.ToString()));
    public ObservableCollection<GameRelease> Releases { get; } = ReleasesStatic;

    [Reactive] public string PackageName { get; set; } = string.Empty;

    [Reactive] public string Version { get; set; } = string.Empty;

    [Reactive] public LinkSourceCategory SourceCategory { get; set; } = LinkSourceCategory.Yaml;

    private readonly ObservableAsPropertyHelper<bool> _inError;
    public bool InError => _inError.Value;

    public delegate LinkInputVm Factory();

    private readonly ObservableAsPropertyHelper<FilePath?> _spriggitConfigPath;
    public FilePath? SpriggitConfigPath => _spriggitConfigPath.Value;
    
    public ICommand OpenSpriggitConfigFolderCommand { get; }
    public ICommand OpenSpriggitConfigCommand { get; }

    public LinkInputVm(
        INavigateTo navigateTo,
        SpriggitMetaLocator locator)
    {
        ModPathPicker.Filters.Add(new CommonFileDialogFilter("Plugin", ".esp,.esl,.esm"));

        _inError = Observable.CombineLatest(
                this.WhenAnyValue(x => x.ModPathPicker.InError),
                this.WhenAnyValue(x => x.GitFolderPicker.InError),
                (m, g) => m || g)
            .ToProperty(this, nameof(InError));

        _spriggitConfigPath = this.WhenAnyValue(x => x.GitFolderPicker.TargetPath)
            .Select(path =>
            {
                return locator.LocateSpriggitConfigFile(path);
            })
            .ToProperty(this, nameof(SpriggitConfigPath));

        var spriggitConfig = this.WhenAnyValue(x => x.SpriggitConfigPath)
            .ObserveOn(RxApp.TaskpoolScheduler)
            .Select(path =>
            {
                return locator.Parse(path);
            })
            .ObserveOn(RxApp.MainThreadScheduler)
            .Replay(1)
            .RefCount();

        spriggitConfig
            .NotNull()
            .Subscribe(x =>
            {
                PackageName = x.Source.PackageName;
                Version = x.Source.Version;
                Release = x.Release;
            });

        OpenSpriggitConfigCommand = ReactiveCommand.Create(
            canExecute: this.WhenAnyValue(x => x.SpriggitConfigPath)
                .Select(x => x != null),
            execute: () =>
            {
                if (!SpriggitConfigPath.HasValue) return;
                navigateTo.Navigate(SpriggitConfigPath.Value);
            });
        OpenSpriggitConfigFolderCommand = ReactiveCommand.Create(
            canExecute: this.WhenAnyValue(x => x.SpriggitConfigPath)
                .Select(x => x != null),
            execute: () =>
            {
                if (SpriggitConfigPath?.Directory == null) return;
                navigateTo.Navigate(SpriggitConfigPath.Value.Directory);
            });
    }

    public void Absorb(LinkSettings settings)
    {
        ModPathPicker.TargetPath = settings.ModPath;
        PackageName = settings.SpriggitPackageName;
        Version = settings.SpriggitPackageVersion;
        SourceCategory = settings.SourceCategory;
        Release = settings.GameRelease;
        
        // Needs to go last to drive config reading
        GitFolderPicker.TargetPath = settings.GitPath;
    }

    public LinkSettings ToSettings()
    {
        return new LinkSettings
        {
            GitPath = GitFolderPicker.TargetPath,
            ModPath = ModPathPicker.TargetPath,
            SpriggitPackageName = PackageName,
            SpriggitPackageVersion = Version,
            SourceCategory = SourceCategory,
            GameRelease = Release,
        };
    }

    public void CopyFrom(LinkInputVm rhs)
    {
        Absorb(rhs.ToSettings());
    }
}