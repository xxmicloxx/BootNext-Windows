using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows.Shapes;
using BootNext.Annotations;

namespace BootNext
{
    public struct BootSection : INotifyPropertyChanged
    {
        private string? _icon;
        private string? _title;
        
        public BootSection(string key)
        {
            Key = key;
            _icon = null;
            _title = null;

            PropertyChanged = null;
        }

        public string Key { get; }

        public string? Icon
        {
            get => _icon;
            set
            {
                _icon = value;
                OnPropertyChanged(nameof(Icon));
            }
        }

        public string? Title
        {
            get => _title;
            set
            {
                _title = value;
                OnPropertyChanged(nameof(Title));
                OnPropertyChanged(nameof(DisplayTitle));
            }
        }

        public string DisplayTitle => Title ?? $"<{Key}>";
        
        
        public event PropertyChangedEventHandler? PropertyChanged;

        [NotifyPropertyChangedInvocator]
        private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
    
    public class Config
    {
        public readonly Dictionary<string, BootSection> BootEntries;
        
        public Config()
        {
            BootEntries = new Dictionary<string, BootSection>();
        }
    }
}