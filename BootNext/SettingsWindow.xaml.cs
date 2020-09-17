using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using BootNext.Properties;

namespace BootNext
{
    public partial class SettingsWindow : Window
    {
        [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
        public class InstalledPartition
        {
            public Partition Partition { get; }

            public bool IsSelected => Partition.Guid == Settings.Default.PreferredEFI;
            
            internal InstalledPartition(Partition part)
            {
                Partition = part;
            }
        }
        
        public SettingsWindow()
        {
            InitializeComponent();

            var efis = DiskScanner.FindEfis().ToList();
            AllEfisList.ItemsSource = efis;
            
            ReloadInstalled();
        }

        private void ReloadInstalled()
        {
            var installedEfis = AllEfisList.Items
                .OfType<Partition>()
                .Where(DiskScanner.HasBootNextInstalled)
                .Select(partition =>
                {
                    var part = new InstalledPartition(partition);
                    return part;
                });
            
            FoundInstallationsList.ItemsSource = installedEfis;
        }

        private void SelectButton_OnClick(object sender, RoutedEventArgs e)
        {
            var selected = (InstalledPartition?) FoundInstallationsList.SelectedItem;
            if (selected == null) return;

            Settings.Default.PreferredEFI = selected.Partition.Guid;
            Settings.Default.Save();
            
            FoundInstallationsList.Items.Refresh();
        }

        private void SettingsWindow_OnClosing(object sender, CancelEventArgs e)
        {
            var target = FoundInstallationsList.Items
                .OfType<InstalledPartition>()
                .Where(x => x.IsSelected)
                .Select(x => x.Partition)
                .FirstOrDefault();

            Debug.WriteLine($"Exiting {target}");
            
            if (target == null) return;
            
            // build config path
            var bootDir = DiskScanner.GetBootNextDir(target)!;
            // show main window
            var window = new MainWindow(bootDir);
            window.Show();
        }

        private void InstallButton_OnClick(object sender, RoutedEventArgs e)
        {
            var selected = (Partition?) AllEfisList.SelectedItem;
            if (selected == null) return;
            
            var result = MessageBox.Show("Installing BootNext will reset any BootNext configuration on " +
                                         "that device to default. Are you sure you want to continue?",
                "BootNext", MessageBoxButton.YesNo);

            if (result == MessageBoxResult.No) return;

            try {
                BootNextInterface.InstallTo(selected);
            }
            catch (Exception ex)
            {
                BootMessageBox.Show("Installation failed", $"Could not install BootNext: {ex}");
                return;
            }
            
            // set target
            Settings.Default.PreferredEFI = selected.Guid;
            Settings.Default.Save();
            ReloadInstalled();

            var bootNextDir = DiskScanner.GetBootNextDir(selected.GetDriveLetter()!);
            BootMessageBox.Show("Installation successful", 
                "Installation of BootNext is now finished. In order to use BootNext, " +
                "you need to add it to your boot order manually.\n\n" +
                "In order to configure BootNext, the installation folder will now be opened in an Explorer++ window.");
            
            var dir = Path.GetDirectoryName(Assembly.GetEntryAssembly()!.Location)!;
            const string explorerPath = @"Resources\Explorer++.exe";
            Process.Start(Path.Combine(dir, explorerPath), bootNextDir);
        }

        private void FoundInstallationsList_OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            SelectButton_OnClick(this, new RoutedEventArgs());
        }

        private void AllEfisList_OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            InstallButton_OnClick(this, new RoutedEventArgs());
        }
    }
}