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

namespace to_do_list
{
    /// <summary>
    /// Logika interakcji dla klasy AddCategoryWindow.xaml
    /// </summary>
    public partial class AddCategoryWindow : Window
    {
        public string CategoryName { get; private set; }
        public AddCategoryWindow()
        {
            InitializeComponent();
        }

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(CategoryTextBox.Text))
            {
                CategoryName = CategoryTextBox.Text.Trim();
                DialogResult = true;
            }
            else
            {
                MessageBox.Show(Lang.L("msg_enter_category"), Lang.L("msg_error"), MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
