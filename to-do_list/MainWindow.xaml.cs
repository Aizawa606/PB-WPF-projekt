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
using to_do_list;
using System.Diagnostics;

namespace WPF_Projekt
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        public ObservableCollection<TaskItem> Tasks { get; set; }

        private bool IsSortDescending = false;

        private List<TaskItem> AllTasks = new();

        public MainWindow()
        {
            InitializeComponent();

            // Wczytaj dane z pliku
            AppData.Categories.Clear();
            foreach (var cat in Storage.LoadCategories())
            {
                AppData.Categories.Add(cat);
            }

            Tasks = Storage.LoadTasks() ?? new ObservableCollection<TaskItem>();
            TasksList.ItemsSource = Tasks;

            AllTasks = Tasks.ToList();

            AddTaskBtn.Click += AddTaskBtn_Click;
            EditTaskBtn.Click += EditTaskBtn_Click;
            DeleteTaskBtn.Click += DeleteTaskBtn_Click;
            TasksList.SelectionChanged += TasksList_SelectionChanged;
            MarkAsDoneBtn.Click += MarkAsDoneBtn_Click;
            AddSubtaskBtn.Click += AddSubtaskBtn_Click;

            CategoriesList.ItemsSource = AppData.Categories;
            AddCategoryBtn.Click += AddCategoryBtn_Click;

            SearchTextBox.TextChanged += SearchTextBox_TextChanged;

            SortComboBox.SelectionChanged += SortComboBox_SelectionChanged;
            SortDirectionBtn.Click += SortDirectionBtn_Click;

            EditCategoryBtn.Click += EditCategoryBtn_Click;
            DeleteCategoryBtn.Click += DeleteCategoryBtn_Click;

            FilterAndSortTasks();
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

                // Odswiezenie do filtrowania
                FilterAndSortTasks();
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
                TaskCategory.Text = $"Kategoria: {selectedTask.Category?.Name}";
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
                AllTasks.Add(window.CreatedTask);
                Tasks.Add(window.CreatedTask);
                FilterAndSortTasks(); // sortowanie po dodaniu
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

                window.CategoryComboBox.SelectedItem = AppData.Categories.FirstOrDefault(c => c.Name == selectedTask.Category.Name);

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
                    // Zapis do pliku
                    Storage.SaveTasks(Tasks);

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
                    // zapis do pliku
                    Storage.SaveTasks(Tasks);

                }
            }
            else
            {
                MessageBox.Show("Wybierz zadanie do usunięcia.", "Informacja", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void AddCategoryBtn_Click(object sender, RoutedEventArgs e)
        {
            var window = new AddCategoryWindow()
            {
                Owner = this, 
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };

            if (window.ShowDialog() == true)
            {
                string newCategory = window.CategoryName;

                if (!AppData.Categories.Any(c => c.Name == newCategory))
                {
                    AppData.Categories.Add(new Category { Name = newCategory });
                    // zapis do pliku
                    Storage.SaveCategories(AppData.Categories);
                }
                else
                {
                    MessageBox.Show("Kategoria o takiej nazwie już istnieje.", "Informacja", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }

        private void BtnExportPdf_Click(object sender, RoutedEventArgs e)
        {
            // Załóżmy, że lista zadań to `List<TaskItem> taskList`
            PDFExportService.ExportTasksToPdf(Tasks.ToList());
        }


        private void SortComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            FilterAndSortTasks();
        }
        private int GetPriorityValue(string priority)
        {
            return priority switch
            {
                "Wysoki" => 3,
                "Średni" => 2,
                "Niski" => 1,
                _ => 0
            };
        }

        private void SortDirectionBtn_Click(object sender, RoutedEventArgs e)
        {
            IsSortDescending = !IsSortDescending;

            SortDirectionBtn.Content = IsSortDescending ? "▼" : "▲";

            //Debug.WriteLine("Sort direction changed: " + (IsSortDescending ? "DESC" : "ASC"));
            Debug.WriteLine("Clicked");

            FilterAndSortTasks();
        }

        private void FilterAndSortTasks()
        {
            if (Tasks == null || AllTasks == null) return;

            var selectedItem = (SortComboBox.SelectedItem as ComboBoxItem)?.Content.ToString();
            if (string.IsNullOrEmpty(selectedItem)) return;

            string searchText = SearchTextBox.Text?.Trim().ToLower() ?? "";
            string selectedStatus = (StatusFilterComboBox.SelectedItem as ComboBoxItem)?.Content.ToString();

            IEnumerable<TaskItem> filtered = AllTasks;

            // Filtrowanie po statusie
            switch (selectedStatus)
            {
                case "Zakończone":
                    filtered = filtered.Where(t => t.Completed);
                    break;
                case "Niezakończone":
                    filtered = filtered.Where(t => !t.Completed);
                    break;
                    // "Wszystkie" — bez filtrowania
            }

            // Wyszukiwanie
            if (!string.IsNullOrWhiteSpace(searchText))
            {
                filtered = filtered.Where(t => t.Title.ToLower().Contains(searchText));
            }

            // Sortowanie
            switch (selectedItem)
            {
                case "Sortuj po dacie":
                    filtered = IsSortDescending
                        ? filtered.OrderByDescending(t => t.Deadline ?? DateTime.MinValue)
                        : filtered.OrderBy(t => t.Deadline ?? DateTime.MaxValue);
                    break;

                case "Sortuj po priorytecie":
                    filtered = IsSortDescending
                        ? filtered.OrderByDescending(t => GetPriorityValue(t.Priority))
                        : filtered.OrderBy(t => GetPriorityValue(t.Priority));
                    break;

                case "Sortuj po kategorii":
                    filtered = IsSortDescending
                        ? filtered.OrderByDescending(t => t.Category?.Name ?? "")
                        : filtered.OrderBy(t => t.Category?.Name ?? "");
                    break;


                case "Sortuj po nazwie":
                    filtered = IsSortDescending
                        ? filtered.OrderByDescending(t => t.Title)
                        : filtered.OrderBy(t => t.Title);
                    break;
            }

            Tasks.Clear();
            foreach (var task in filtered)
            {
                Tasks.Add(task);
            }
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            base.OnClosing(e);
            Storage.SaveTasks(Tasks);
            Storage.SaveCategories(AppData.Categories);
        }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            FilterAndSortTasks();
        }

        private void StatusFilterComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            FilterAndSortTasks();
        }

        private void EditCategoryBtn_Click(object sender, RoutedEventArgs e)
        {
            if (CategoriesList.SelectedItem is not Category selectedCategory)
            {
                MessageBox.Show("Wybierz kategorię do edycji.", "Informacja", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var editWindow = new EditCategoryWindow(selectedCategory.Name)
            {
                Owner = this,
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };

            if (editWindow.ShowDialog() == true)
            {
                string newName = editWindow.UpdatedCategoryName.Trim();

                if (string.IsNullOrWhiteSpace(newName))
                {
                    MessageBox.Show("Nazwa kategorii nie może być pusta.");
                    return;
                }

                if (AppData.Categories.Any(c => c.Name == newName && c != selectedCategory))
                {
                    MessageBox.Show("Kategoria o takiej nazwie już istnieje.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Aktualizuj nazwę kategorii
                string oldName = selectedCategory.Name;
                selectedCategory.Name = newName;

                // Zaktualizuj wszystkie przypisane zadania
                foreach (var task in Tasks)
                {
                    if (task.Category?.Name == oldName)
                    {
                        task.Category.Name = newName;
                    }
                }

                CategoriesList.Items.Refresh();
                TasksList.Items.Refresh();

                // Zapisz zmiany
                Storage.SaveCategories(AppData.Categories);
                Storage.SaveTasks(Tasks);
            }

        }

        private void DeleteCategoryBtn_Click(object sender, RoutedEventArgs e)
        {
            if (CategoriesList.SelectedItem is not Category selectedCategory)
            {
                MessageBox.Show("Wybierz kategorię do usunięcia.", "Uwaga", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Sprawdź, czy kategoria jest przypisana do jakiegoś zadania
            bool isUsed = AllTasks.Any(t => t.Category.Name == selectedCategory.Name);
            if (isUsed)
            {
                MessageBox.Show("Nie można usunąć kategorii, która jest przypisana do istniejących zadań.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var result = MessageBox.Show($"Czy na pewno chcesz usunąć kategorię \"{selectedCategory.Name}\"?", "Potwierdzenie", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                AppData.Categories.Remove(selectedCategory);
                Storage.SaveCategories(AppData.Categories);
            }
        }


    }

}
