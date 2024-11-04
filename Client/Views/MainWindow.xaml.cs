using Client.ViewModels;
using System.Collections.Specialized;
using System.Windows;

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
    }

    private void Messages_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.Action == NotifyCollectionChangedAction.Add)
        {
            MessagesListBox.ScrollIntoView(MessagesListBox.Items[MessagesListBox.Items.Count - 1]);
        }
    }
}
