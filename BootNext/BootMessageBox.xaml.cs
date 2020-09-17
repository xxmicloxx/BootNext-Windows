using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using BootNext.Annotations;

namespace BootNext
{
    public sealed partial class BootMessageBox : Window, INotifyPropertyChanged
    {
        public static void Show(string messageText, string informationalText = "")
        {
            var box = new BootMessageBox
            {
                MessageText = messageText,
                InformationalText = informationalText
            };
            box.ShowDialog();
        }
        
        private string _messageText;
        private string _informationalText;

        public string MessageText
        {
            get => _messageText;
            set
            {
                _messageText = value;
                OnPropertyChanged(nameof(MessageText));
            }
        }

        public string InformationalText
        {
            get => _informationalText;
            set
            {
                _informationalText = value;
                OnPropertyChanged(nameof(InformationalText));
            }
        }
        
        public BootMessageBox()
        {
            _messageText = "Message text";
            _informationalText = "Informational text.";
            
            InitializeComponent();
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        [NotifyPropertyChangedInvocator]
        private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}