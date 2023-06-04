using System.IO;
using System.Reactive.Linq;
using DynamicData;
using Microsoft.WindowsAPICodePack.Dialogs;
using Noggog;
using Noggog.WPF;
using ReactiveUI;

namespace Spriggit.UI.ViewModels;

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

    public LinkInputVm()
    {
        ModPathPicker.Filters.Add(new CommonFileDialogFilter("Master", ".esm"));
        ModPathPicker.Filters.Add(new CommonFileDialogFilter("Light Master", ".esl"));
        ModPathPicker.Filters.Add(new CommonFileDialogFilter("Plugin", ".esp"));
    }

    public LinkInputVm(string git, string mod)
    {
        GitFolderPicker.TargetPath = git;
        ModPathPicker.TargetPath = mod;   
    }
}