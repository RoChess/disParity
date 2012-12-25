﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms; // for FolderBrowserDialog
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using disParity;
using System.Reflection;

namespace disParityUI
{

  public partial class MainWindow : Window
  {

    private MainWindowViewModel viewModel;
    private DispatcherTimer timer;

    public MainWindow()
    {
      viewModel = new MainWindowViewModel(this);
      DataContext = viewModel;
      Initialized += HandleInitialized;
      Loaded += HandleLoaded;
      Closed += HandleClosed;
      Closing += HandleClosing;

      InitializeComponent();
      timer = new DispatcherTimer();
      timer.Tick += HandleTimer;
      timer.Interval = new TimeSpan(0, 0, 1);
      timer.Start();
    }

    private void HandleInitialized(object sender, EventArgs args)
    {
      if (SingleInstance.AlreadyRunning()) {
        MessageWindow.ShowError(null, "Already running", "Another instance of disParity is already running");
        Close();
      }
    }

    private void HandleTimer(object sender, EventArgs args)
    {
      CommandManager.InvalidateRequerySuggested();
    }

    private void HandleLoaded(object sender, EventArgs args)
    {
      viewModel.Loaded();
    }

    private void HandleClosing(object sender, CancelEventArgs args)
    {
      if (MainWindowViewModel.Busy) {
        MessageWindow.Show(this, "Operation in progress", "Please cancel the current operation before closing disParity", MessageWindowIcon.Caution, MessageWindowButton.OK);
        args.Cancel = true;
      }
    }

    private void HandleClosed(object sender, EventArgs args)
    {
      viewModel.Shutdown();
    }

    #region Command CanExecute/Executed methods

    void AddDriveCanExecute(object sender, CanExecuteRoutedEventArgs e)
    {
      e.CanExecute = !String.IsNullOrEmpty(viewModel.Config.ParityDir) && !MainWindowViewModel.Busy;
    }

    void AddDriveExecuted(object sender, ExecutedRoutedEventArgs e)
    {
      FolderBrowserDialog d = new FolderBrowserDialog();
      d.Description = "Choose a drive or path to add:";
      DialogResult r = d.ShowDialog();
      if (r == System.Windows.Forms.DialogResult.OK)
        viewModel.AddDrive(d.SelectedPath);
      e.Handled = true;
    }

    void RemoveDriveCanExecute(object sender, CanExecuteRoutedEventArgs e)
    {
      e.CanExecute = !MainWindowViewModel.Busy && (DriveList.SelectedItems.Count == 1);
    }

    void RemoveDriveExecuted(object sender, ExecutedRoutedEventArgs e)
    {
      if (DriveList.SelectedItems.Count == 1)  // sanity check
        viewModel.RemoveDrive((DataDriveViewModel)DriveList.SelectedItem);
      e.Handled = true;
    }

    void ScanDriveCanExecute(object sender, CanExecuteRoutedEventArgs e)
    {
      e.CanExecute = !MainWindowViewModel.Busy && (DriveList.SelectedItems.Count == 1);
    }

    void ScanDriveExecuted(object sender, ExecutedRoutedEventArgs e)
    {
      if (DriveList.SelectedItems.Count == 1)  // sanity check
        viewModel.ScanDrive((DataDriveViewModel)DriveList.SelectedItem);
      e.Handled = true;
    }

    void ScanAllCanExecute(object sender, CanExecuteRoutedEventArgs e)
    {
      e.CanExecute = !MainWindowViewModel.Busy && DriveList.HasItems;
    }

    void ScanAllExecuted(object sender, ExecutedRoutedEventArgs e)
    {
      viewModel.ScanAll();
      e.Handled = true;
    }

    void UpdateAllCanExecute(object sender, CanExecuteRoutedEventArgs e)
    {
      if (MainWindowViewModel.Busy || viewModel.Drives.Count == 0)
        e.CanExecute = false;
      else
        e.CanExecute = true;
    }

    void UpdateAllExecuted(object sender, ExecutedRoutedEventArgs e)
    {
      viewModel.Update();
      e.Handled = true;
    }

    void RecoverDriveCanExecute(object sender, CanExecuteRoutedEventArgs e)
    {
      e.CanExecute = !MainWindowViewModel.Busy && (DriveList.SelectedItems.Count == 1) &&
        ((DataDriveViewModel)DriveList.SelectedItem).DataDrive.FileCount > 0;
    }

    void RecoverDriveExecuted(object sender, ExecutedRoutedEventArgs e)
    {
      viewModel.Recover((DataDriveViewModel)DriveList.SelectedItem);
      e.Handled = true;
    }

    void VerifyCanExecute(object sender, CanExecuteRoutedEventArgs e)
    {
      if (MainWindowViewModel.Busy || viewModel.Drives.Count == 0)
        e.CanExecute = false;
      else
        e.CanExecute = true;
    }

    void VerifyExecuted(object sender, ExecutedRoutedEventArgs e)
    {
      viewModel.Verify();
      e.Handled = true;
    }

    void OptionsCanExecute(object sender, CanExecuteRoutedEventArgs e)
    {
      e.CanExecute = !MainWindowViewModel.Busy;
    }

    void OptionsExecuted(object sender, ExecutedRoutedEventArgs e)
    {
      OptionsDialog dialog = new OptionsDialog(viewModel.GetOptionsDialogViewModel());
      dialog.Owner = this;
      if (dialog.ShowDialog() == true)
        viewModel.OptionsChanged();
      e.Handled = true;
    }

    void AboutExecuted(object sender, ExecutedRoutedEventArgs e)
    {
      new AboutWindow(this, new AboutWindowViewModel()).ShowDialog();
    }

    void CancelCanExecute(object sender, CanExecuteRoutedEventArgs e)
    {
      e.CanExecute = MainWindowViewModel.Busy;
    }

    void CancelExecuted(object sender, ExecutedRoutedEventArgs e)
    {
      viewModel.Cancel();
      e.Handled = true;
    }

    void HashcheckCanExecute(object sender, CanExecuteRoutedEventArgs e)
    {
      e.CanExecute = !MainWindowViewModel.Busy && (DriveList.SelectedItems.Count == 1) &&
        ((DataDriveViewModel)DriveList.SelectedItem).DataDrive.FileCount > 0;
    }

    void HashcheckExecuted(object sender, ExecutedRoutedEventArgs e)
    {
      viewModel.Hashcheck((DataDriveViewModel)DriveList.SelectedItem);
      e.Handled = true;
    }

    void HashcheckAllCanExecute(object sender, CanExecuteRoutedEventArgs e)
    {
      e.CanExecute = !MainWindowViewModel.Busy && DriveList.HasItems;
    }

    void HashcheckAllExecuted(object sender, ExecutedRoutedEventArgs e)
    {
      viewModel.Hashcheck();
      e.Handled = true;
    }

    void UndeleteCanExecute(object sender, CanExecuteRoutedEventArgs e)
    {
      e.CanExecute = !MainWindowViewModel.Busy && (DriveList.SelectedItems.Count == 1) &&
        ((((DataDriveViewModel)DriveList.SelectedItem).DataDrive.Deletes.Count > 0) || 
         (((DataDriveViewModel)DriveList.SelectedItem).DataDrive.Edits.Count > 0));
    }

    void UndeleteExecuted(object sender, ExecutedRoutedEventArgs e)
    {
      viewModel.Undelete((DataDriveViewModel)DriveList.SelectedItem);
      e.Handled = true;
    }



    #endregion

  }


}
