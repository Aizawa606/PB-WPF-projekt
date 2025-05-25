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
    /// Logika interakcji dla klasy TaskEditWindow.xaml
    /// </summary>
    public partial class TaskEditWindow : Window
    {
        public TaskItem CreatedTask { get; private set; }

        public TaskEditWindow()
        {
            InitializeComponent();
            SaveButton.Click += SaveButton_Click;
            AddSubtaskBtn.Click += AddSubtaskBtn_Click;
        }

        private void AddSubtaskBtn_Click(object sender, RoutedEventArgs e)
        {
            var tb = new TextBox
            {
                Margin = new Thickness(0, 5, 0, 0),
                MinWidth = 200
            };
            SubtasksPanel.Children.Add(tb);
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            var subtasks = new List<SubTask>();
            foreach (var child in SubtasksPanel.Children)
            {
                if (child is TextBox tb && !string.IsNullOrWhiteSpace(tb.Text))
                {
                    subtasks.Add(new SubTask { Title = tb.Text, Completed = false });
                }
            }

            // Priorytet
            var selectedPriorityItem = PriorityComboBox.SelectedItem as ComboBoxItem;
            string priority = selectedPriorityItem?.Content.ToString() ?? "Niski";

            // Kategoria
            string category;
            var selectedCategory = CategoryComboBox.SelectedItem as ComboBoxItem;
            if (selectedCategory != null && selectedCategory.Content.ToString() == "Nowa kategoria...")
            {
                category = "Nowa"; // Możesz rozszerzyć o pobieranie z innego pola
            }
            else
            {
                category = selectedCategory?.Content.ToString() ?? "Ogólna";
            }

            CreatedTask = new TaskItem
            {
                Title = TaskNameTextBox.Text,
                Description = DescriptionTextBox.Text,
                Category = category,
                Priority = priority,
                Deadline = DeadlineDatePicker.SelectedDate,
                Completed = false,
                Subtasks = subtasks
            };

            DialogResult = true;
            Close();
        }
    }
}
