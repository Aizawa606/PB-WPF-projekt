using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace WPF_Projekt
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public ObservableCollection<TaskItem> Tasks { get; set; } = new();

        public MainWindow()
        {
            InitializeComponent();

            TasksList.ItemsSource = Tasks;

            AddTaskBtn.Click += AddTaskBtn_Click;
            EditTaskBtn.Click += EditTaskBtn_Click;
            DeleteTaskBtn.Click += DeleteTaskBtn_Click;
            TasksList.SelectionChanged += TasksList_SelectionChanged;
            MarkAsDoneBtn.Click += MarkAsDoneBtn_Click;
            AddSubtaskBtn.Click += AddSubtaskBtn_Click;

        }

        private void AddSubtaskBtn_Click(object sender, RoutedEventArgs e)
        {
            // Sprawdź, czy zostało wybrane jakieś zadanie z listy
            if (TasksList.SelectedItem is TaskItem selectedTask)
            {
                // Utwórz i ustaw właściciela nowego okna dodawania podzadania
                var addSubtaskWindow = new AddSubtaskWindow();
                addSubtaskWindow.Owner = this;

                // Pokaż okno dialogowe i poczekaj na wynik (OK lub Anuluj)
                if (addSubtaskWindow.ShowDialog() == true)
                {
                    // Pobierz tekst podzadania wpisany przez użytkownika
                    string newTitle = addSubtaskWindow.ResponseText;

                    // Jeśli tekst nie jest pusty lub samymi spacjami
                    if (!string.IsNullOrWhiteSpace(newTitle))
                    {
                        // Dodaj nowe podzadanie do listy podzadań wybranego zadania
                        selectedTask.Subtasks.Add(new SubTask { Title = newTitle, Completed = false });

                        // Odśwież listę podzadań w UI, aby nowy element się pojawił
                        SubtasksList.Items.Refresh();

                        // Odśwież listę zadań (jeśli w UI jest zależność od podzadań)
                        TasksList.Items.Refresh();
                    }
                }
            }
            else
            {
                // Jeśli nie wybrano zadania, pokaż komunikat informujący o tym
                MessageBox.Show("Wybierz zadanie, do którego chcesz dodać podzadanie.", "Informacja", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }



        private void MarkAsDoneBtn_Click(object sender, RoutedEventArgs e)
        {
            if (TasksList.SelectedItem is TaskItem selectedTask)
            {
                // Odwracamy status "Completed"
                selectedTask.Completed = !selectedTask.Completed;

                // Odśwież listę i szczegóły
                TasksList.Items.Refresh();

                // Aktualizuj panel szczegółów
                TasksList_SelectionChanged(null, null);
            }
            else
            {
                MessageBox.Show("Wybierz zadanie, aby zmienić jego status.", "Informacja", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void TasksList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (TasksList.SelectedItem is TaskItem selectedTask)
            {
                TaskTitle.Text = selectedTask.Title;
                TaskCategory.Text = $"Kategoria: {selectedTask.Category}";
                TaskPriority.Text = $"Priorytet: {selectedTask.Priority}";

                TaskDeadline.Text = selectedTask.Deadline.HasValue
                    ? $"Termin: {selectedTask.Deadline.Value.ToShortDateString()}"
                    : "Termin: brak";

                TaskDescription.Text = string.IsNullOrWhiteSpace(selectedTask.Description)
                    ? "Brak opisu"
                    : selectedTask.Description;

                SubtasksList.ItemsSource = selectedTask.Subtasks;
            }
            else
            {
                TaskTitle.Text = "";
                TaskCategory.Text = "";
                TaskPriority.Text = "";
                TaskDeadline.Text = "";
                TaskDescription.Text = "";
                SubtasksList.ItemsSource = null;
            }
        }

        private void AddTaskBtn_Click(object sender, RoutedEventArgs e)
        {
            var window = new TaskEditWindow();
            if (window.ShowDialog() == true && window.CreatedTask != null)
            {
                Tasks.Add(window.CreatedTask);
            }
        }

        private void EditTaskBtn_Click(object sender, RoutedEventArgs e)
        {
            if (TasksList.SelectedItem is TaskItem selectedTask)
            {
                var window = new TaskEditWindow();

                // Wczytaj dane wybranego zadania do okna edycji
                window.TaskNameTextBox.Text = selectedTask.Title;
                window.DescriptionTextBox.Text = selectedTask.Description;

                // Ustaw kategorię w ComboBox (zakładam, że masz metody na dodanie kategorii, tutaj prosty przykład)
                foreach (ComboBoxItem item in window.CategoryComboBox.Items)
                {
                    if (item.Content.ToString() == selectedTask.Category)
                    {
                        window.CategoryComboBox.SelectedItem = item;
                        break;
                    }
                }

                foreach (ComboBoxItem item in window.PriorityComboBox.Items)
                {
                    if (item.Content.ToString() == selectedTask.Priority)
                    {
                        window.PriorityComboBox.SelectedItem = item;
                        break;
                    }
                }

                window.DeadlineDatePicker.SelectedDate = selectedTask.Deadline;

                // Dodaj podzadania
                window.SubtasksPanel.Children.Clear();
                foreach (var sub in selectedTask.Subtasks)
                {
                    var tb = new TextBox
                    {
                        Text = sub.Title,
                        Margin = new Thickness(0, 5, 0, 0),
                        MinWidth = 200
                    };
                    window.SubtasksPanel.Children.Add(tb);
                }

                if (window.ShowDialog() == true && window.CreatedTask != null)
                {
                    // Aktualizuj dane zadania
                    selectedTask.Title = window.CreatedTask.Title;
                    selectedTask.Description = window.CreatedTask.Description;
                    selectedTask.Category = window.CreatedTask.Category;
                    selectedTask.Priority = window.CreatedTask.Priority;
                    selectedTask.Deadline = window.CreatedTask.Deadline;
                    selectedTask.Subtasks = window.CreatedTask.Subtasks;

                    // Odśwież listę (ObservableCollection powinno odświeżać UI, ale jeśli nie, możesz użyć ToList + Clear + Add)
                    TasksList.Items.Refresh();
                }
            }
            else
            {
                MessageBox.Show("Wybierz zadanie do edycji.", "Informacja", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void DeleteTaskBtn_Click(object sender, RoutedEventArgs e)
        {
            if (TasksList.SelectedItem is TaskItem selectedTask)
            {
                var result = MessageBox.Show($"Czy na pewno chcesz usunąć zadanie \"{selectedTask.Title}\"?",
                    "Potwierdzenie usunięcia", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    Tasks.Remove(selectedTask);
                }
            }
            else
            {
                MessageBox.Show("Wybierz zadanie do usunięcia.", "Informacja", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
    }

}
