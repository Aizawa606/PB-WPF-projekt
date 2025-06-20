﻿using System;
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
            UpdateCompletionStatus();
        }

        private void AddSubtaskBtn_Click(object sender, RoutedEventArgs e)
        {
            if (TasksList.SelectedItem is TaskItem selectedTask)
            {
                // Zbierz już istniejące tytuły podzadań
                var existingNames = selectedTask.Subtasks
                    .Select(st => st.Title)
                    .ToList();

                // Użyj tego samego okna co w TaskEditWindow
                var inputWindow = new SubtaskInputWindow
                {
                    Owner = this,
                    ExistingSubtaskNames = existingNames
                };

                if (inputWindow.ShowDialog() == true)
                {
                    string newTitle = inputWindow.SubtaskName;

                    if (!string.IsNullOrWhiteSpace(newTitle))
                    {
                        // Dodaj podzadanie do modelu
                        selectedTask.Subtasks.Add(new SubTask { Title = newTitle, Completed = false });

                        // Odśwież widoki
                        SubtasksList.Items.Refresh();
                        TasksList.Items.Refresh();
                    }
                }
            }
            else
            {
                MessageBox.Show(Lang.L("msg_wybierz_zadanie_dla_podzadania"), Lang.L("msg_information"), MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }




        private void MarkAsDoneBtn_Click(object sender, RoutedEventArgs e)
        {
            if (TasksList.SelectedItem is TaskItem selectedTask)
            {
                // Odwracamy status "Completed"
                selectedTask.Completed = !selectedTask.Completed;

                // Odświeżamy statystyki
                UpdateCompletionStatus();

                // Odśwież listę i szczegóły
                TasksList.Items.Refresh();

                // Aktualizuj panel szczegółów
                TasksList_SelectionChanged(null, null);

                // Odswiezenie do filtrowania
                FilterAndSortTasks();
            }
            else
            {
                MessageBox.Show(Lang.L("msg_wybierz_zadanie_dla_statusu"), Lang.L("msg_information"), MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void TasksList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (TasksList.SelectedItem is TaskItem selectedTask)
            {
                TaskCategory.Text = $"{Lang.L("lbl_category")}: {selectedTask.Category?.Name}";
                TaskPriority.Text = $"{Lang.L("lbl_priority")}: {selectedTask.Priority}";

                TaskDeadline.Text = selectedTask.Deadline.HasValue
                    ? $"{Lang.L("lbl_deadline")}: {selectedTask.Deadline.Value.ToShortDateString()}"
                    : Lang.L("lbl_deadline_none");

                TaskDescription.Text = string.IsNullOrWhiteSpace(selectedTask.Description)
                    ? Lang.L("lbl_no_description")
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
                UpdateCompletionStatus();
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
                window.SubtasksListBox.Items.Clear();
                foreach (var sub in selectedTask.Subtasks)
                {
                    window.SubtasksListBox.Items.Add(sub.Title);
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

                    TasksList_SelectionChanged(null, null);
                    UpdateCompletionStatus();

                }
            }
            else
            {
                MessageBox.Show(Lang.L("msg_wybierz_zadanie_do_edycji"), Lang.L("msg_information"), MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void DeleteTaskBtn_Click(object sender, RoutedEventArgs e)
        {
            if (TasksList.SelectedItem is TaskItem selectedTask)
            {
                var result = MessageBox.Show(string.Format(Lang.L("msg_confirm_delete_task"), selectedTask.Title),
                    Lang.L("msg_deletion_confirmation"), MessageBoxButton.YesNo, MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    Tasks.Remove(selectedTask);
                    AllTasks.Remove(selectedTask);
                    // zapis do pliku
                    Storage.SaveTasks(Tasks);
                    UpdateCompletionStatus();

                }
            }
            else
            {
                MessageBox.Show(Lang.L("msg_wybierz_zadanie_do_usuniecia"), Lang.L("msg_information"), MessageBoxButton.OK, MessageBoxImage.Information);
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
                    MessageBox.Show(Lang.L("msg_kategoria_istnieje"), Lang.L("msg_information"), MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }

        private void DeleteSubtaskBtn_Click(object sender, RoutedEventArgs e)
        {
            var selectedSubtask = SubtasksList.SelectedItem as SubTask;

            if (selectedSubtask == null)
            {
                MessageBox.Show(Lang.L("msg_select_subtask_to_delete"), Lang.L("msg_nothing_selected"), MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

          
            if (TasksList.SelectedItem is TaskItem selectedTask)
            {
                var result = MessageBox.Show(string.Format(Lang.L("msg_confirm_delete_subtask"), selectedSubtask.Title),
                                             Lang.L("msg_deletion_confirmation"), MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    selectedTask.Subtasks.Remove(selectedSubtask);
                    SubtasksList.Items.Refresh();
                    TasksList.Items.Refresh();
                    Storage.SaveTasks(Tasks);
                }
            }
            else
            {
                MessageBox.Show(Lang.L("msg_proszę_wybrać_zadanie"), Lang.L("msg_no_task_selected"), MessageBoxButton.OK, MessageBoxImage.Information);
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

            var selectedItem = (SortComboBox.SelectedItem as ComboBoxItem)?.Tag?.ToString();
            if (string.IsNullOrEmpty(selectedItem)) return;

            string searchText = SearchTextBox.Text?.Trim().ToLower() ?? "";
            string selectedStatus = (StatusFilterComboBox.SelectedItem as ComboBoxItem)?.Tag?.ToString();

            IEnumerable<TaskItem> filtered = AllTasks;

            // Filtrowanie po statusie
            switch (selectedStatus)
            {
                case "done":
                    filtered = filtered.Where(t => t.Completed);
                    break;
                case "notdone":
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
                case "data":
                    filtered = IsSortDescending
                        ? filtered.OrderByDescending(t => t.Deadline ?? DateTime.MinValue)
                        : filtered.OrderBy(t => t.Deadline ?? DateTime.MaxValue);
                    break;

                case "priorytet":
                    filtered = IsSortDescending
                        ? filtered.OrderByDescending(t => GetPriorityValue(t.Priority))
                        : filtered.OrderBy(t => GetPriorityValue(t.Priority));
                    break;

                case "kategoria":
                    filtered = IsSortDescending
                        ? filtered.OrderByDescending(t => t.Category?.Name ?? "")
                        : filtered.OrderBy(t => t.Category?.Name ?? "");
                    break;


                case "nazwa":
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

        private void UpdateCompletionStatus()
        {
            if (TasksList.Items.Count == 0)
            {
                CompletionStatusText.Text = Lang.L("lbl_no_tasks");
                return;
            }

            int totalTasks = TasksList.Items.Count;
            int doneTasks = 0;

            foreach (TaskItem task in TasksList.Items)
            {
                if (task.Completed) doneTasks++;
            }

            double percentDone = (double)doneTasks / totalTasks * 100;
            CompletionStatusText.Text = string.Format(Lang.L("lbl_completed_status"),doneTasks, totalTasks, percentDone);
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
                MessageBox.Show(Lang.L("msg_wybierz_kategorie_do_edycji"), Lang.L("msg_information"), MessageBoxButton.OK, MessageBoxImage.Information);
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
                    MessageBox.Show(Lang.L("msg_nazwa_kategorii_pusta"));
                    return;
                }

                if (AppData.Categories.Any(c => c.Name == newName && c != selectedCategory))
                {
                    MessageBox.Show(Lang.L("msg_kategoria_istnieje"), Lang.L("msg_error"), MessageBoxButton.OK, MessageBoxImage.Warning);
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
                MessageBox.Show(Lang.L("msg_wybierz_kategorie_do_usuniecia"), Lang.L("msg_warning"), MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Sprawdź, czy kategoria jest przypisana do jakiegoś zadania
            bool isUsed = AllTasks.Any(t => t.Category.Name == selectedCategory.Name);
            if (isUsed)
            {
                MessageBox.Show(Lang.L("msg_nie_mozna_usunac_kategorii"), Lang.L("msg_error"), MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var result = MessageBox.Show(string.Format(Lang.L("msg_confirm_delete_category"), selectedCategory.Name), Lang.L("msg_confirmation"), MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                AppData.Categories.Remove(selectedCategory);
                Storage.SaveCategories(AppData.Categories);
            }
        }

        private void LanguageComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var combo = sender as ComboBox;
            var selectedItem = combo.SelectedItem as ComboBoxItem;
            var lang = selectedItem.Tag.ToString();

            App.SetLanguage(lang);
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            base.OnClosing(e);
            Storage.SaveTasks(Tasks);
            Storage.SaveCategories(AppData.Categories);
        }
    }

}
