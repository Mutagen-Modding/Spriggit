using System.Collections.ObjectModel;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Windows.Input;
using System.Windows.Shapes;
using DynamicData;
using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Dialogs;
using Mutagen.Bethesda;
using Mutagen.Bethesda.Starfield;
using Noggog;
using Noggog.WPF;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Serilog;
using Spriggit.Core;
using Spriggit.Core.Services.Singletons;
using Spriggit.Engine;
using Spriggit.Engine.Services.Singletons;
using Spriggit.UI.Services;
using Spriggit.UI.Settings;

namespace Spriggit.UI.ViewModels.Transient;

public class LinkInputVm : ViewModel
{
    private readonly WriteSpriggitConfig _writeSpriggitConfig;

    public PathPickerVM ModPathPicker { get; } = new()
    {
        ExistCheckOption = PathPickerVM.CheckOptions.On,
        PathType = PathPickerVM.PathTypeOptions.File
    };
    
    public PathPickerVM GitFolderPicker { get; } = new()
    {
        PathType = PathPickerVM.PathTypeOptions.Folder,
        ExistCheckOption = PathPickerVM.CheckOptions.Off,
    };
    
    public PathPickerVM DataFolderPicker { get; } = new()
    {
        PathType = PathPickerVM.PathTypeOptions.Folder,
        ExistCheckOption = PathPickerVM.CheckOptions.IfPathNotEmpty
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
    public ICommand CreateSpriggitConfigCommand { get; }

    private readonly Subject<Unit> _refreshSpriggitConfig = new();
    
    private readonly ObservableAsPropertyHelper<bool> _needsDataFolder;
    public bool NeedsDataFolder => _needsDataFolder.Value;

    public LinkInputVm(
        ILogger logger,
        INavigateTo navigateTo,
        WriteSpriggitConfig writeSpriggitConfig,
        SpriggitFileLocator locator,
        LinkSourceCategoryToPackageName linkSourceCategoryToPackageName)
    {
        _writeSpriggitConfig = writeSpriggitConfig;
        ModPathPicker.Filters.Add(new CommonFileDialogFilter("Plugin", ".esp,.esl,.esm"));

        _inError = Observable.CombineLatest(
                this.WhenAnyValue(x => x.ModPathPicker.InError),
                this.WhenAnyValue(x => x.GitFolderPicker.InError),
                (m, g) => m || g)
            .ToProperty(this, nameof(InError));

        _spriggitConfigPath = this.WhenAnyValue(x => x.GitFolderPicker.TargetPath)
            .ReplayMostRecent(_refreshSpriggitConfig)
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
            .Select(x => x?.Meta)
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
        CreateSpriggitConfigCommand = ReactiveCommand.Create(
            canExecute: this.WhenAnyValue(x => x.SpriggitConfigPath)
                .Select(x => x == null),
            execute: () =>
            {
                var package = linkSourceCategoryToPackageName.GetPackageName(SourceCategory, Release, PackageName);
                if (package.Failed)
                {
                    logger.Warning("Could not get package name {SourceCategory}, {Release}, {PackageName}",
                        SourceCategory, Release, PackageName);
                    return;
                }
                
                var dialog = new SaveFileDialog();
                dialog.Filter = $"Spriggit Config file|*{SpriggitFileLocator.ConfigFileName}";
                dialog.Title = $"Save a {SpriggitFileLocator.ConfigFileName} file";
                dialog.FileName = SpriggitFileLocator.ConfigFileName;
                dialog.InitialDirectory = System.IO.Path.GetDirectoryName(GitFolderPicker.TargetPath) ?? "";
                dialog.ShowDialog();

                if (dialog.FileName == "") return;
                
                _writeSpriggitConfig.Write(
                    dialog.FileName,
                    new SpriggitMeta(
                        new SpriggitSource()
                        {
                            PackageName = PackageName,
                            Version = Version
                        },
                        Release),
                    IFileSystemExt.DefaultFilesystem);
                
                _refreshSpriggitConfig.OnNext(Unit.Default);
            });

        _needsDataFolder = this.WhenAnyValue(x => x.Release)
            .Select(x => x == GameRelease.Starfield)
            .ToGuiProperty(this, nameof(NeedsDataFolder));
    }

    public void Absorb(LinkSettings settings)
    {
        ModPathPicker.TargetPath = settings.ModPath;
        PackageName = settings.SpriggitPackageName;
        Version = settings.SpriggitPackageVersion;
        SourceCategory = settings.SourceCategory;
        Release = settings.GameRelease;
        DataFolderPicker.TargetPath = settings.DataFolderPath;
        
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
            DataFolderPath = DataFolderPicker.TargetPath
        };
    }

    public void CopyFrom(LinkInputVm rhs)
    {
        Absorb(rhs.ToSettings());
    }
}