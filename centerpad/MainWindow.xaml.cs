using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;   // <-- ajoute celle-ci
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using System.CodeDom.Compiler;
using System.Reflection;
using Microsoft.CSharp;
using System.Diagnostics;

namespace centerpad
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        string? appchoisi;

        string Path_extension = "Extensions";
        string version = "0.1.2.1";


        public MainWindow()
        {
            InitializeComponent();

            label.Content = version;

            if (!Directory.Exists(Path_extension))
                Directory.CreateDirectory(Path_extension);


            string[] files = Directory.GetFiles(Path_extension, "*.exe");
            select_app.ItemsSource = files;
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            appchoisi = select_app.SelectedItem.ToString();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(appchoisi);
        }
    }
}