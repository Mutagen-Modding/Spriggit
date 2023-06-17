using System.IO;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Windows;
using Noggog;
using ReactiveUI;

namespace Spriggit.UI;

public partial class LinkListingView
{
    public LinkListingView()
    {
        InitializeComponent();
        this.WhenActivated(disp =>
        {
            this.WhenAnyValue(x => x.ViewModel!.Input.ModPathPicker.TargetPath)
                .Select(x => x.IsNullOrWhitespace() ? "[No Name]" : Path.GetFileNameWithoutExtension(x))
                .BindTo(this, x => x.LinkModNameBox.Text)
                .DisposeWith(disp);
            this.WhenAnyValue(x => x.ViewModel!.Input.ModPathPicker.TargetPath)
                .Select(x => x.IsNullOrWhitespace() ? string.Empty : Path.GetExtension(x))
                .BindTo(this, x => x.LinkModExtensionBox.Text)
                .DisposeWith(disp);
            this.WhenAnyValue(x => x.ViewModel!.Input.ModPathPicker.TargetPath)
                .BindTo(this, x => x.LinkModNameBox.ToolTip)
                .DisposeWith(disp);
            this.WhenAnyValue(x => x.ViewModel!.Input.GitFolderPicker.TargetPath)
                .Select(x =>
                {
                    var dirName = Path.GetDirectoryName(x);
                    if (dirName == null) return string.Empty;
                    return $"{dirName}{Path.DirectorySeparatorChar}";
                })
                .BindTo(this, x => x.LinkGitLeftPathBox.Text)
                .DisposeWith(disp);
            this.WhenAnyValue(x => x.ViewModel!.Input.GitFolderPicker.TargetPath)
                .Select(x => x.IsNullOrWhitespace() ? "[No Path]" : Path.GetFileName(x))
                .BindTo(this, x => x.LinkGitPathFolderNameBox.Text)
                .DisposeWith(disp);
            this.WhenAnyValue(x => x.ViewModel!.Input.GitFolderPicker.TargetPath)
                .BindTo(this, x => x.LinkGitPathFolderNameBox.ToolTip)
                .DisposeWith(disp);

            this.OneWayBind(ViewModel, x => x.SyncToGitCommand, x => x.SyncToGitButton.Command)
                .DisposeWith(disp);
            this.OneWayBind(ViewModel, x => x.SyncToModCommand, x => x.SyncToModButton.Command)
                .DisposeWith(disp);
            this.OneWayBind(ViewModel, x => x.EditSettingsCommand, x => x.SettingsButton.Command)
                .DisposeWith(disp);

            this.WhenAnyValue(x => x.IsMouseOver)
                .Select(x => x ? Visibility.Visible : Visibility.Hidden)
                .BindTo(this, x => x.SettingsButton.Visibility)
                .DisposeWith(disp);
            this.WhenAnyValue(x => x.IsMouseOver)
                .Select(x => x ? Visibility.Visible : Visibility.Hidden)
                .BindTo(this, x => x.SyncToGitButton.Visibility)
                .DisposeWith(disp);
            this.WhenAnyValue(x => x.IsMouseOver)
                .Select(x => x ? Visibility.Visible : Visibility.Hidden)
                .BindTo(this, x => x.SyncToModButton.Visibility)
                .DisposeWith(disp);
            this.WhenAnyValue(x => x.IsMouseOver)
                .Select(x => x ? Visibility.Hidden : Visibility.Visible)
                .BindTo(this, x => x.SeparationLine.Visibility)
                .DisposeWith(disp);
            this.WhenAnyValue(x => x.GitPathGrid.IsMouseOver)
                .Select(x => x ? 1d : 0.3d)
                .BindTo(this, x => x.LinkGitLeftPathBox.Opacity)
                .DisposeWith(disp);
        });
    }
}