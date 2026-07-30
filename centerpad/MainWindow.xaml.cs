using Microsoft.CSharp;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Text.Json;
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
using System.Net.Http;
using System.Text.Json;

namespace centerpad
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        string? appchoisi;

        string Path_extension = "Extensions";
        string version = "0.1.2.4";


        public MainWindow()
        {
            InitializeComponent();

            label.Content = version;

            if (!Directory.Exists(Path_extension))
                Directory.CreateDirectory(Path_extension);

            VerifierMiseAJour();

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
            if (appchoisi == null)
            {
                MessageBox.Show("Veuillez sélectionner une application à lancer.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            else
                Process.Start(Path_extension + "\\" + appchoisi + ".exe");
        }

        

        async void VerifierMiseAJour()
        {
            try
            {
                using var client = new HttpClient();
                client.DefaultRequestHeaders.Add("User-Agent", "centerpad-app"); // obligatoire, GitHub refuse sans ça

                string json = await client.GetStringAsync("https://api.github.com/repos/xTheoxo/centerpad/releases/latest");

                using var doc = JsonDocument.Parse(json);
                string versionDistante = doc.RootElement.GetProperty("tag_name").GetString(); // ex: "v0.1.3"
                string urlTelechargement = doc.RootElement
                    .GetProperty("assets")[0]
                    .GetProperty("browser_download_url")
                    .GetString();

                string versionDistanteNettoyee = versionDistante.TrimStart('v'); // enlève le "v" devant si présent

                if (versionDistanteNettoyee != version)
                {
                    var resultat = MessageBox.Show(
                        $"Une nouvelle version est disponible : {versionDistanteNettoyee} (actuelle : {version}).\nTélécharger maintenant ?",
                        "Mise à jour disponible",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Information);

                    if (resultat == MessageBoxResult.Yes)
                    {
                        Process.Start(new ProcessStartInfo(urlTelechargement) { UseShellExecute = true });
                    }
                }
            }
            catch (Exception ex)
            {
                // Pas grave si ça échoue (pas d'internet, repo sans release, etc.) — on ignore silencieusement ou on log
                Console.WriteLine("Erreur vérification MAJ : " + ex.Message);
            }
        }
    }
}