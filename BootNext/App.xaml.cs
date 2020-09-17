using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using BootNext.Properties;

namespace BootNext
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        private Partition? FindTarget()
        {
            return FindTarget(DiskScanner.FindInstalledEfis().ToList());
        }
        
        private Partition? FindTarget(IList<Partition> efis)
        {
            if (efis.Count == 1)
            {
                return efis[0];
            } else if (efis.Count > 1)
            {
                var preferred = Settings.Default.PreferredEFI;
                return efis.FirstOrDefault(t => t.Guid == preferred);
            }

            return null;
        }
        
        private void App_OnStartup(object sender, StartupEventArgs e)
        {
            var efis = DiskScanner.FindInstalledEfis().ToList();
            var target = FindTarget(efis);
            
            if (efis.Count == 0)
            {
                BootMessageBox.Show("BootNext not installed",
                    "BootNext is not installed yet. You will be taken to the settings dialog.");
            } else if (target == null)
            {
                BootMessageBox.Show("Multiple instances found",
                    "Multiple BootNext instances found. Please select the one you want to use.");                
            }

            if (target == null)
            {
                var settings = new SettingsWindow();
                settings.Show();
            }
            else
            {
                Settings.Default.PreferredEFI = target.Guid;
                Settings.Default.Save();
                
                var bootDir = DiskScanner.GetBootNextDir(target)!;
                var window = new MainWindow(bootDir);
                window.Show();
            }
        }
    }
}