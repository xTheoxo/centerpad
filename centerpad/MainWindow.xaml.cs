using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace centerpad
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        string Path = "Extensions";
        string version = "0.1";

        
        public MainWindow()
        {
            InitializeComponent();

            label.Content = version;

            if (!Directory.Exists(Path)) 
                Directory.CreateDirectory(Path);
        }
    }
}