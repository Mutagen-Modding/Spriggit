using System.IO;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Windows;
using Noggog;
using Noggog.WPF;
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

            this.OneWayBind(ViewModel, x => x.EditSettingsCommand, x => x.SettingsButton.Command)
                .DisposeWith(disp);
            
            this.OneWayBind(ViewModel, x => x.SyncToModCommand, x => x.SyncToModButton.Command)
                .DisposeWith(disp);
            this.WhenAnyValue(x => x.ViewModel!.SyncToModCommand.CanExecute)
                .Switch()
                .CombineLatest(this.WhenAnyValue(x => x.IsMouseOver),
                    (canExecute, mouseOver) => canExecute && mouseOver)
                .Select(x => x ? Visibility.Visible : Visibility.Hidden)
                .BindTo(this, x => x.SyncToModButton.Visibility);
            this.OneWayBind(ViewModel, x => x.SyncToGitCommand, x => x.SyncToGitButton.Command)
                .DisposeWith(disp);
            this.WhenAnyValue(x => x.ViewModel!.SyncToGitCommand.CanExecute)
                .Switch()
                .CombineLatest(this.WhenAnyValue(x => x.IsMouseOver),
                    (canExecute, mouseOver) => canExecute && mouseOver)
                .Select(x => x ? Visibility.Visible : Visibility.Hidden)
                .BindTo(this, x => x.SyncToGitButton.Visibility);
            this.OneWayBind(ViewModel, x => x.CancelSyncToGitCommand, x => x.CancelSyncToGitButton.Command)
                .DisposeWith(disp);
            this.WhenAnyValue(x => x.ViewModel!.CancelSyncToGitCommand.CanExecute)
                .Switch()
                .CombineLatest(this.WhenAnyValue(x => x.IsMouseOver),
                    (canExecute, mouseOver) => canExecute && mouseOver)
                .Select(x => x ? Visibility.Visible : Visibility.Hidden)
                .BindTo(this, x => x.CancelSyncToGitButton.Visibility);
            this.OneWayBind(ViewModel, x => x.CancelSyncToModCommand, x => x.CancelSyncToModButton.Command)
                .DisposeWith(disp);
            this.WhenAnyValue(x => x.ViewModel!.CancelSyncToModCommand.CanExecute)
                .Switch()
                .CombineLatest(this.WhenAnyValue(x => x.IsMouseOver),
                    (canExecute, mouseOver) => canExecute && mouseOver)
                .Select(x => x ? Visibility.Visible : Visibility.Hidden)
                .BindTo(this, x => x.CancelSyncToModButton.Visibility);

            this.WhenAnyValue(x => x.IsMouseOver)
                .Select(x => x ? Visibility.Visible : Visibility.Hidden)
                .BindTo(this, x => x.SettingsButton.Visibility)
                .DisposeWith(disp);

            // ContextMenu
            this.WhenAnyValue(x => x.ViewModel!.DeleteCommand)
                .BindTo(this, x => x.DeleteContextMenuButton.Command)
                .DisposeWith(disp);
        });
    }
}