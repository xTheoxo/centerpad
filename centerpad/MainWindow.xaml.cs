using Microsoft.CSharp;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Net.Http;
using System.Net.Http;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;   // <-- ajoute celle-ci
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

        string? appchoisi;
        string? chemin_extension;
        string url;

        string Path_extension = "Extensions";
        string version = "0.1.3.4";

        // background -> #FF444242
        public MainWindow()
        {
            InitializeComponent();

            label_maj.Visibility = Visibility.Hidden;
            button_maj.Visibility = Visibility.Hidden;

            label.Content = version;

            if (!Directory.Exists(Path_extension))
                Directory.CreateDirectory(Path_extension);

            VerifierMiseAJour();

            ChargerExtensionsDisponibles();

            ChargeExtensionInstalle();           
        }
       

    private void ChargeExtensionInstalle()
    {
        var apps = new List<AppInstallee>();

        // Cas A : les .exe directement dans Extensions\
        string[] exeDirects = Directory.GetFiles(Path_extension, "*.exe");
        foreach (var chemin in exeDirects)
        {
            string nom = System.IO.Path.GetFileNameWithoutExtension(chemin);
            apps.Add(new AppInstallee
            {
                Nom = nom,
                Version = null,          // pas de version connue pour ce cas
                CheminExe = chemin
            });
        }

        // Cas B : les dossiers contenant un .exe, avec version dans le nom du dossier
        string[] dossiers = Directory.GetDirectories(Path_extension);
        foreach (var dossier in dossiers)
        {
            string[] exeDansDossier = Directory.GetFiles(dossier, "*.exe");
            if (exeDansDossier.Length == 0)
                continue; // pas de .exe dedans, on ignore

            string nomDossier = System.IO.Path.GetFileName(dossier); // ex: "Dow_gui-1.4.1"
            var (nom, version) = ExtraireNomEtVersion(nomDossier);

            apps.Add(new AppInstallee
            {
                Nom = nom,
                Version = version,
                CheminExe = exeDansDossier[0] // le premier .exe trouvé dans le dossier
            });
        }

        select_app.ItemsSource = apps;
        select_app.DisplayMemberPath = "Affichage";
    }

    private (string nom, string version) ExtraireNomEtVersion(string nomDossier)
    {
        // Cherche un motif du style "NomApp-1.4.1" à la fin du nom
        var match = Regex.Match(nomDossier, @"^(.*)-(\d+(\.\d+)*)$");

        if (match.Success)
        {
            string nom = match.Groups[1].Value;
            string version = match.Groups[2].Value;
            return (nom, version);
        }

        // Si ça ne correspond pas au motif attendu, on garde le nom du dossier tel quel
        return (nomDossier, null);
    }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (select_app.SelectedItem != null)
                button_select_app.IsEnabled = true;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (select_app.SelectedItem is not AppInstallee appChoisie)
                return;

            Process.Start(appChoisie.CheminExe);
        }
        /*
            MessageBox.Show("Veuillez sélectionner une application à lancer.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        */


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
                    else if (resultat == MessageBoxResult.No)
                    {
                        label_maj.Visibility = Visibility.Visible;
                        button_maj.Visibility = Visibility.Visible;
                    }
                }
                else
                {
                    label_maj.Visibility = Visibility.Hidden;
                }
            }
            catch (Exception ex)
            {
                // Pas grave si ça échoue (pas d'internet, repo sans release, etc.) — on ignore silencieusement ou on log
                Console.WriteLine("Erreur vérification MAJ : " + ex.Message);
            }
        }
        public class ExtensionDisponible
        {
            public string Nom { get; set; }
            public string Description { get; set; }
            public string Version { get; set; }
            public string UrlTelechargement { get; set; }

            public string Affichage => Version != null ? $"{Nom} v{Version}" : Nom;
        }

        async Task<List<ExtensionDisponible>> ChercherExtensionsGitHub()
        {
            var extensions = new List<ExtensionDisponible>();
            string[] comptes = { "xTheoxo", "Valentin30-wq" };

            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("User-Agent", "centerpad-app");

            foreach (var compte in comptes)
            {
                try
                {
                    string urlRecherche = $"https://api.github.com/search/repositories?q=user:{compte}+topic:centerpad-extension";
                    string json = await client.GetStringAsync(urlRecherche);

                    using var doc = JsonDocument.Parse(json);
                    var items = doc.RootElement.GetProperty("items");

                    foreach (var repo in items.EnumerateArray())
                    {
                        string nom = repo.GetProperty("name").GetString();
                        string description = repo.TryGetProperty("description", out var d) ? d.GetString() ?? "" : "";
                        string owner = repo.GetProperty("owner").GetProperty("login").GetString();

                        // Aller chercher la dernière release de ce repo précis
                        try
                        {
                            string jsonRelease = await client.GetStringAsync($"https://api.github.com/repos/{owner}/{nom}/releases/latest");
                            using var docRelease = JsonDocument.Parse(jsonRelease);
                            var assets = docRelease.RootElement.GetProperty("assets");

                            if (assets.GetArrayLength() > 0)
                            {
                                string urlTelechargement = assets[0].GetProperty("browser_download_url").GetString();
                                string versionTag = docRelease.RootElement.GetProperty("tag_name").GetString();
                                string versionNettoyee = versionTag?.TrimStart('v');

                                extensions.Add(new ExtensionDisponible
                                {
                                    Nom = nom,
                                    Description = description,
                                    Version = versionNettoyee,
                                    UrlTelechargement = urlTelechargement
                                });
                            }
                        }
                        catch
                        {
                            // Ce repo n'a pas de release publiée, on l'ignore
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Erreur recherche extensions pour {compte} : {ex.Message}");
                }
            }
            return extensions;
        }
        async void ChargerExtensionsDisponibles()
        {
            var extensions = await ChercherExtensionsGitHub();

            jsp.ItemsSource = extensions;
            jsp.DisplayMemberPath = "Affichage"; // au lieu de "Nom"
        }

        private void jsp_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (jsp.SelectedItem != null)
                button_extension_dl.IsEnabled = true;
        }

        private async void button_extension_dl_Click(object sender, RoutedEventArgs e)
        {
            button_extension_dl.IsEnabled = false;

            if (jsp.SelectedItem is not ExtensionDisponible extensionChoisie)
                return;

            // Récupère le nom du fichier tel quel depuis l'URL GitHub
            string nomFichierOrigine = System.IO.Path.GetFileName(new Uri(extensionChoisie.UrlTelechargement).LocalPath);
            string extension = System.IO.Path.GetExtension(nomFichierOrigine).ToLower();

            // On garde le nom d'origine, pas besoin de le reconstruire
            string cheminTelecharge = System.IO.Path.Combine(Path_extension, nomFichierOrigine);

            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("User-Agent", "centerpad-app");

                using (HttpResponseMessage response = await client.GetAsync(extensionChoisie.UrlTelechargement))
                {
                    if (!response.IsSuccessStatusCode)
                    {
                        MessageBox.Show($"Échec du téléchargement : {response.StatusCode}", "Erreur");
                        button_extension_dl.IsEnabled = true;
                        return;
                    }

                    using (FileStream fs = new FileStream(cheminTelecharge, FileMode.Create))
                    {
                        await response.Content.CopyToAsync(fs);
                    }
                }
            }

            if (extension == ".zip")
            {
                var dossiersAvant = Directory.GetDirectories(Path_extension);

                ZipFile.ExtractToDirectory(cheminTelecharge, Path_extension, overwriteFiles: true);

                var dossiersApres = Directory.GetDirectories(Path_extension);
                string nouveauDossier = dossiersApres.Except(dossiersAvant).FirstOrDefault()
                    ?? dossiersApres.FirstOrDefault(d =>
                        System.IO.Path.GetFileName(d).StartsWith(extensionChoisie.Nom, StringComparison.OrdinalIgnoreCase));

                File.Delete(cheminTelecharge);
            }
            else if (extension == ".exe")
            {
                // Rien à faire de plus, le .exe est déjà nommé correctement et à la bonne place
            }
            else
            {
                MessageBox.Show($"Type de fichier non géré : {extension}", "Erreur");
            }

            ChargeExtensionInstalle();
        }

        private void button_maj_Click(object sender, RoutedEventArgs e)
        {

        }
        public class AppInstallee
        {
            public string Nom { get; set; }
            public string Version { get; set; }
            public string CheminExe { get; set; }   // chemin complet vers le .exe à lancer

            public string Affichage => Version != null ? $"{Nom} (v{Version})" : Nom;
        }
    }
}