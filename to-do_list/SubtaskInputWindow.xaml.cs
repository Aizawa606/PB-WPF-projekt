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
using to_do_list;

namespace WPF_Projekt
{
    public partial class SubtaskInputWindow : Window
    {
        public string SubtaskName { get; private set; }

        public List<string> ExistingSubtaskNames { get; set; } = new List<string>();

        public SubtaskInputWindow()
        {
            InitializeComponent();
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            var input = SubtaskNameTextBox.Text?.Trim();

            if (string.IsNullOrWhiteSpace(input))
            {
                MessageBox.Show("Nazwa podzadania nie może być pusta.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (ExistingSubtaskNames.Any(name => string.Equals(name, input, StringComparison.OrdinalIgnoreCase)))
            {
                MessageBox.Show("Podzadanie o takiej nazwie już istnieje.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            SubtaskName = input;
            DialogResult = true;
            Close();
        }
    }
}
