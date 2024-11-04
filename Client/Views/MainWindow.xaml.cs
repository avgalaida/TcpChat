using Client.ViewModels;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Input;

namespace Client.Views;
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        var viewModel = DataContext as MainViewModel;
        if (viewModel != null)
        {
            var messages = viewModel.Messages;
            messages.CollectionChanged += Messages_CollectionChanged;
        }

        MessageTextBox.KeyDown += MessageTextBox_KeyDown;
    }

    private void Messages_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.Action == NotifyCollectionChangedAction.Add)
        {
            MessagesListBox.ScrollIntoView(MessagesListBox.Items[MessagesListBox.Items.Count - 1]);
        }
    }

    private void MessageTextBox_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            var viewModel = DataContext as MainViewModel;
            if (viewModel?.SendMessageCommand?.CanExecute(null) == true)
            {
                viewModel.SendMessageCommand.Execute(null);
                e.Handled = true; 
            }
        }
    }
}