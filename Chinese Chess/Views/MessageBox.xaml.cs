using System.Windows;

namespace Chinese_Chess.Views
{
    public partial class MessageBox : Window
    {
        public string ConfirmText { get; set; } = "Yes";
        public string CancelText { get; set; } = "Cancel";

        // Constructor (éo biết bị gì những cần đưa vào title và message ngược nhau mới đúng)
        public MessageBox(string title, string message, string confirmText, string cancelText)
        {
            InitializeComponent();

            this.DataContext = this;

            if (TxtTitle != null) TxtTitle.Text = title;
            if (TxtMessage != null) TxtMessage.Text = message;

            ConfirmText = confirmText;
            CancelText = cancelText;
        }

        private void Confirm_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            this.Close();
        }


        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false; 
            this.Close();
        }

        public static bool Show(string message, string title = "THÔNG BÁO", string confirm = "Đồng ý", string cancel = "Hủy")
        {
            var msgBox = new MessageBox(title, message, confirm, cancel);
            return msgBox.ShowDialog() == true;
        }
    }
}