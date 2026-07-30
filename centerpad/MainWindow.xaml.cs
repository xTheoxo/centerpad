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
        string version = "0.1.2.3";


        public MainWindow()
        {
            InitializeComponent();

            label.Content = version;

            if (!Directory.Exists(Path_extension))
                Directory.CreateDirectory(Path_extension);

            
            string[] cheminsComplets = Directory.GetFiles(Path_extension, "*.exe");
            string[] nomsFichiers = new string[cheminsComplets.Length];
            int nomsFichiersSansExtension;

            for (int i = 0; i < cheminsComplets.Length; i++)
            {
                nomsFichiersSansExtension = Convert.ToInt32(cheminsComplets[i].Length - 11 - 4);
                nomsFichiers[i] = cheminsComplets[i].Substring(11, nomsFichiersSansExtension);
                
            }

            select_app.ItemsSource = nomsFichiers;
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            appchoisi = select_app.SelectedItem.ToString();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(Path_extension + "\\" + appchoisi + ".exe");
        }
    }
}