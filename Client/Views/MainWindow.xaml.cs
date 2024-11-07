﻿using System.Windows;
using System.Windows.Input;
using Client.ViewModels;

namespace Client.Views;
public partial class MainWindow : Window
{
    private readonly MainViewModel _viewModel;

    public MainWindow(MainViewModel viewModel)
    {
        InitializeComponent();

        _viewModel = viewModel;
        DataContext = _viewModel;

        this.Closing += MainWindow_Closing;
        this.Loaded += MainWindow_Loaded;
    }

    private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        //await _viewModel.ConnectAsync();
    }

    private async void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
    {
        if (_viewModel.IsConnected)
        {
            await _viewModel.DisconnectAsync();
        }
    }

    private void MessageTextBox_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            if (_viewModel.SendMessageCommand.CanExecute(null))
            {
                _viewModel.SendMessageCommand.Execute(null);
                e.Handled = true;
            }
        }
    }
}