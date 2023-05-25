using System.ComponentModel;
using System.Windows;

namespace Spriggit.UI;

public interface IMainWindow
{
    public Visibility Visibility { get; set; }
    public object DataContext { get; set; }
    event CancelEventHandler Closing;
}
    
/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : IMainWindow
{
    public MainWindow()
    {
        InitializeComponent();
    }
}