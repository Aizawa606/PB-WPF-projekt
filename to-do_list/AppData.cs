using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace to_do_list
{
    public static class AppData
    {
        public static ObservableCollection<Category> Categories { get; } = new();
    }
}
