using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace to_do_list
{
    public static class Lang
    {
        public static string L(string key)
        {
            return Application.Current.Resources[key] as string ?? $"[{key}]";
        }
    }
}
