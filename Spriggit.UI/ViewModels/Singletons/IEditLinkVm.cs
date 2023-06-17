using System.Windows.Input;
using Spriggit.UI.ViewModels.Transient;

namespace Spriggit.UI.ViewModels.Singletons;

public interface IEditLinkVm
{
    LinkInputVm LinkInput { get; }
    ICommand FinishCommand { get; }
    ICommand DiscardCommand { get; }
}