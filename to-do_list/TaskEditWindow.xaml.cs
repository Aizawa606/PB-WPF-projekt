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
            DeleteSubtaskBtn.Click += DeleteSubtaskBtn_Click;

            // Załaduj kategorie do ComboBox
            foreach (var category in AppData.Categories)
            {
                CategoryComboBox.Items.Add(category); 
            }
            CategoryComboBox.DisplayMemberPath = "Name"; 


            // Domyślny wybór
            if (CategoryComboBox.Items.Count > 0)
                CategoryComboBox.SelectedIndex = 0;
        }

        public void SetSubtasks(List<SubTask> subtasks)
        {
            // Zamiana listy obiektów SubTask na listę stringów z tytułami
            SubtasksListBox.ItemsSource = subtasks.Select(s => s.Title).ToList();
        }


        private void DeleteSubtaskBtn_Click(object sender, RoutedEventArgs e)
        {
            if (SubtasksListBox.SelectedItem == null)
            {
                MessageBox.Show("Proszę zaznaczyć podzadanie do usunięcia.", "Brak zaznaczenia", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            SubtasksListBox.Items.Remove(SubtasksListBox.SelectedItem);
        }
        private void AddSubtaskBtn_Click(object sender, RoutedEventArgs e)
        {
            var existingNames = SubtasksListBox.Items
                .OfType<string>() 
                .Select(name => name.ToString())
                .ToList();

            var inputWindow = new SubtaskInputWindow
            {
                Owner = this,
                ExistingSubtaskNames = existingNames
            };

            if (inputWindow.ShowDialog() == true)
            {
                SubtasksListBox.Items.Add(inputWindow.SubtaskName);
            }
        }



        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            var subtasks = new List<SubTask>();
            foreach (var item in SubtasksListBox.Items)
            {
                if (item is string text && !string.IsNullOrWhiteSpace(text))
                {
                    subtasks.Add(new SubTask { Title = text, Completed = false });
                }
            }


            // Priorytet
            var selectedPriorityItem = PriorityComboBox.SelectedItem as ComboBoxItem;
            string priority = selectedPriorityItem?.Content.ToString() ?? "Niski";

            // Kategoria
            Category category;
            var selectedCategory = CategoryComboBox.SelectedItem as Category;
            if (selectedCategory != null)
            {
                category = selectedCategory;
            }
            else
            {
                category = new Category { Name = "Ogólna" };
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
