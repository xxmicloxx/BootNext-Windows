using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Management;
using System.Runtime.Remoting.Channels;
using System.Windows;
using System.Windows.Input;

namespace BootNext
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public Config Config { get; }
        public string BootNextPath { get; }
        
        public MainWindow(string bootNextPath)
        {
            BootNextPath = bootNextPath;
            
            var configPath = Path.Combine(bootNextPath, "config.conf");
            Config = ConfigParser.Parse(configPath);

            InitializeComponent();
            
            var iconConverter = (IconConverter) FindResource("IconConverter");
            iconConverter.BootNextRoot = bootNextPath;

            ListBox.ItemsSource = Config.BootEntries.Values;
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            var settings = new SettingsWindow();
            settings.Show();
            Close();
        }

        private void RebootButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (!(ListBox.SelectedItem is BootSection selected)) return;
            
            BootNextInterface.BootTo(BootNextPath, selected.Key);
            
            // Ugly reboot hack, thanks MS
            var w32 = new ManagementClass("Win32_OperatingSystem");
            w32.Scope.Options.EnablePrivileges = true;

            foreach (var o in w32.GetInstances())
            {
                var obj = (ManagementObject) o;
                var inParams = obj.GetMethodParameters("Win32Shutdown");
                inParams["Flags"] = 2;
                inParams["Reserved"] = 0;

                var outParams = obj.InvokeMethod("Win32Shutdown", inParams, null)!;
                var result = Convert.ToInt32(outParams["returnValue"]);
                if (result != 0) throw new Win32Exception(result);
            }
        }

        private void ListBox_OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            RebootButton_OnClick(this, new RoutedEventArgs());
        }
    }
}