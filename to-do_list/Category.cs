using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WPF_Projekt;

namespace to_do_list
{
    public class Category
    {
        public string Name { get; set; }

        public List<TaskItem> Tasks { get; set; } = new();
    }

}
