using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Permissions;
using System.Windows;
using System.Windows.Navigation;
using MozaikaApp.Properties;

namespace MozaikaApp
{
    /// <summary>
    /// Логика взаимодействия для App.xaml
    /// </summary>
    public partial class App
    {
        public static event EventHandler LanguageChanged;

        public static CultureInfo Language
        {
            get => System.Threading.Thread.CurrentThread.CurrentUICulture;
            set
            {
                //if (Equals(value, System.Threading.Thread.CurrentThread.CurrentUICulture)) return;
                //1. Меняем язык приложения:
                System.Threading.Thread.CurrentThread.CurrentUICulture = value;
                //2. Создаём ResourceDictionary для новой культуры
                ResourceDictionary dict = new ResourceDictionary();
                switch (value.Name)
                {
                    case "ru-RU":
                        dict.Source = new Uri($"Resources\\Language.{value.Name}.xaml",
                            UriKind.Relative);
                        break;
                    default:
                        dict.Source = new Uri("Resources\\Language.xaml", UriKind.Relative);
                        break;
                }

                //3. Находим старую ResourceDictionary и удаляем его и добавляем новую ResourceDictionary
                ResourceDictionary oldDict = null;
                try
                {
                    oldDict = (from d in Current.Resources.MergedDictionaries
                        where d.Source != null && d.Source.OriginalString.StartsWith("Resources/Language.")
                        select d).First();
                }
                catch(Exception)
                {
                    //ignored
                }
                if (oldDict != null)
                {
                    int ind = Current.Resources.MergedDictionaries.IndexOf(oldDict);
                    Current.Resources.MergedDictionaries.Remove(oldDict);
                    Current.Resources.MergedDictionaries.Insert(ind, dict);
                }
                else
                {
                    Current.Resources.MergedDictionaries.Add(dict);
                }

                //4. Вызываем евент для оповещения всех окон.
                LanguageChanged?.Invoke(Current, new EventArgs());
            }
        }

        public static List<CultureInfo> Languages { get; } = new List<CultureInfo>();

        private void App_LanguageChanged(object sender, EventArgs e)
        {
            Settings.Default.DefaultLanguage = Language;
            Settings.Default.Save();
        }
        [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
        public App()
        {
            //Languages.Clear();
            //Languages.Add(new CultureInfo("en-US")); //Default Language
            if (!File.Exists(Environment.CurrentDirectory+"\\facebase.xml"))
                File.AppendAllText(Environment.CurrentDirectory + "\\facebase.xml", MozaikaApp.Properties.Resources.haarcascade_frontalface_alt2);
            LanguageChanged += App_LanguageChanged;
            Settings.Default.ViewDpi =(int) Graphics.FromHwnd(IntPtr.Zero).DpiX;
            Settings.Default.Save();
        }

        private void App_OnLoadCompleted(object sender, NavigationEventArgs e)
        {
        }

        private void App_OnStartup(object sender, StartupEventArgs e)
        {
            Language = Settings.Default.DefaultLanguage;
        }
    }
}

