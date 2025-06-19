using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;


namespace ChatClient
{
    public partial class InputDialog : Window
    {
        public string InputText { get; private set; }

        public InputDialog(string title = "입력", string defaultText = "")
        {
            InitializeComponent();
            this.Title = title;
            InputTextBox.Text = defaultText;
            InputTextBox.Focus();
            InputTextBox.SelectAll();
        }

        private void Confirm_Click(object sender, RoutedEventArgs e)
        {
            InputText = InputTextBox.Text;
            this.DialogResult = true;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }
    }
}
