using DynamicData;
using Microsoft.WindowsAPICodePack.Dialogs;
using Noggog.WPF;

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
}