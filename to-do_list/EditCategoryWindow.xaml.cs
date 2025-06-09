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
    /// Logika interakcji dla klasy EditCategoryWindow.xaml
    /// </summary>
    public partial class EditCategoryWindow : Window
    {
        public string UpdatedCategoryName { get; private set; }

        public EditCategoryWindow(string currentName)
        {
            InitializeComponent();
            CategoryNameTextBox.Text = currentName;
            CategoryNameTextBox.SelectAll();
            CategoryNameTextBox.Focus();
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            UpdatedCategoryName = CategoryNameTextBox.Text.Trim();
            if (!string.IsNullOrWhiteSpace(UpdatedCategoryName))
                DialogResult = true;
        }
    }
}
