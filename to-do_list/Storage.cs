using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json;
using to_do_list;  // tu masz Category i AppData
using WPF_Projekt; // tu masz TaskItem i SubTask (dostosuj wg własnego projektu)

namespace WPF_Projekt
{
    public static class Storage
    {
        private static readonly string TasksFilePath = "tasks.json";
        private static readonly string CategoriesFilePath = "categories.json";

        public static ObservableCollection<TaskItem> LoadTasks()
        {
            if (!File.Exists(TasksFilePath))
                return new ObservableCollection<TaskItem>();

            try
            {
                var json = File.ReadAllText(TasksFilePath);
                return JsonSerializer.Deserialize<ObservableCollection<TaskItem>>(json) ?? new ObservableCollection<TaskItem>();
            }
            catch
            {
                return new ObservableCollection<TaskItem>();
            }
        }

        public static void SaveTasks(ObservableCollection<TaskItem> tasks)
        {
            try
            {
                var json = JsonSerializer.Serialize(tasks, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(TasksFilePath, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Błąd zapisu zadań: " + ex.Message);
            }
        }

        public static ObservableCollection<Category> LoadCategories()
        {
            if (!File.Exists(CategoriesFilePath))
                return new ObservableCollection<Category>();

            try
            {
                var json = File.ReadAllText(CategoriesFilePath);
                return JsonSerializer.Deserialize<ObservableCollection<Category>>(json) ?? new ObservableCollection<Category>();
            }
            catch
            {
                return new ObservableCollection<Category>();
            }
        }

        public static void SaveCategories(ObservableCollection<Category> categories)
        {
            try
            {
                var json = JsonSerializer.Serialize(categories, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(CategoriesFilePath, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Błąd zapisu kategorii: " + ex.Message);
            }
        }
    }
}
