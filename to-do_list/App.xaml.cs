using System.Configuration;
using System.Data;
using System.Windows;

namespace to_do_list
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static void SetLanguage(string lang)
        {
            var dict = new ResourceDictionary();
            switch (lang)
            {
                case "pl":
                    dict.Source = new Uri("Resources/Strings.pl.xaml", UriKind.Relative);
                    break;
                case "en":
                    dict.Source = new Uri("Resources/Strings.en.xaml", UriKind.Relative);
                    break;
                default:
                    dict.Source = new Uri("Resources/Strings.en.xaml", UriKind.Relative);
                    break;
            }

            Application.Current.Resources.MergedDictionaries.Clear();
            Application.Current.Resources.MergedDictionaries.Add(dict);
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
        }
    }

}
