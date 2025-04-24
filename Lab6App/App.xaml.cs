using System.Configuration;
using System.Data;
using System.Reflection;
using System.Windows;
using System.Windows.Media;

namespace Lab6App
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        static string path = "C:\\";
        public static void InitPath()
        {
            string[] p = Assembly.GetExecutingAssembly().Location.Split("\\");
            string s = "";
            for (int i = 0; i < p.Length - 1; i++)
                s += p[i] + "\\";
            path = s;
        }


        public static string Path { get { return path; } }
        public static SolidColorBrush brushBlack = new SolidColorBrush(Color.FromArgb(0xFF, 0, 0, 0));
        public static SolidColorBrush brushGray = new SolidColorBrush(Color.FromRgb(0x80, 0x80, 0x80));
        public static SolidColorBrush brushRedOrange = new SolidColorBrush(Color.FromRgb(0xFF, 0x20, 0));
        public static SolidColorBrush brushLight = new SolidColorBrush(Color.FromRgb(0xEE, 0xEE, 0xEE));
        public static SolidColorBrush brushIndigo = new SolidColorBrush(Color.FromRgb(0x3F, 0x51, 0xB5));
    }

}
