using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Controls;
using to_do_list;

namespace WPF_Projekt
{
    public class SubTask : INotifyPropertyChanged
    {
        private bool _completed;

        public string Title { get; set; }

        public bool Completed
        {
            get => _completed;
            set
            {
                if (_completed != value)
                {
                    _completed = value;
                    OnPropertyChanged(nameof(Completed));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class TaskItem
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public Category Category { get; set; } 
        public string Priority { get; set; } // "Niski", "Średni", "Wysoki"
        public DateTime? Deadline { get; set; }
        public bool Completed { get; set; }
        public List<SubTask> Subtasks { get; set; } = new();

        public string StatusSymbol => Completed ? "✔" : "✖";
    }

}
