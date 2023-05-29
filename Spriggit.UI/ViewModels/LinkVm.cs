using Noggog.WPF;
using Spriggit.UI.Settings;

namespace Spriggit.UI.ViewModels;

public class LinkVm : ViewModel
{
    public LinkInputVm Input { get; }

    public LinkVm(LinkInputVm input)
    {
        Input = input;
    }

    public LinkVm(LinkSettings settings)
    {
        Input = new LinkInputVm();
        Input.GitFolderPicker.TargetPath = settings.GitPath;
        Input.ModPathPicker.TargetPath = settings.ModPath;
    }
}