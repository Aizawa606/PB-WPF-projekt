using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace WPF_Projekt
{
    /// <summary>
    /// Logika interakcji dla klasy AddSubtaskWindow.xaml
    /// </summary>
    public partial class AddSubtaskWindow : Window
    {
        public string ResponseText { get; private set; }

        public AddSubtaskWindow()
        {
            InitializeComponent();
            InputTextBox.Focus();
        }

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            ResponseText = InputTextBox.Text.Trim();
            if (string.IsNullOrEmpty(ResponseText))
            {
                MessageBox.Show("Tytuł podzadania nie może być pusty.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            DialogResult = true;
        }
    }
}
