using System.Collections.ObjectModel;
using System.Reactive.Linq;
using DynamicData;
using Microsoft.WindowsAPICodePack.Dialogs;
using Mutagen.Bethesda;
using Noggog;
using Noggog.WPF;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
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

    public LinkInputVm()
    {
        ModPathPicker.Filters.Add(new CommonFileDialogFilter("Plugin", ".esp,.esl,.esm"));

        _inError = Observable.CombineLatest(
                this.WhenAnyValue(x => x.ModPathPicker.InError),
                this.WhenAnyValue(x => x.GitFolderPicker.InError),
                (m, g) => m || g)
            .ToProperty(this, nameof(InError));
    }

    public LinkInputVm(LinkSettings settings)
        : this()
    {
        Absorb(settings);
    }

    public void Absorb(LinkSettings settings)
    {
        GitFolderPicker.TargetPath = settings.GitPath;
        ModPathPicker.TargetPath = settings.ModPath;
        PackageName = settings.SpriggitPackageName;
        Version = settings.SpriggitPackageVersion;
        SourceCategory = settings.SourceCategory;
        Release = settings.GameRelease;
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