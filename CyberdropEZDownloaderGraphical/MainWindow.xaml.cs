using CyberdropEZDownloaderGraphical.ListBase;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;

namespace CyberdropEZDownloaderGraphical
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        List<BaseCyberdrop> liste_docs = new List<BaseCyberdrop>();
        List<string> liens_fichiers = new List<string>();
        HttpClient client = new HttpClient();
        public MainWindow()
        {
            InitializeComponent();
        }

        public async Task LoadUpSomeShit(string link)
        {
            // vérification que le lien donné est bien une URL valide, sinon rejet
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            bool result = Uri.TryCreate(link, UriKind.Absolute, out Uri uriResult) && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
            Uri treatedLink = new Uri(link);

            // vérification supplémentaire avec le lien, pour éviter qu'un malin joue avec d'autres sites.
            if (result && treatedLink.Host == "cyberdrop.me")
            {
                // récupération de l'HTML de la page pour analyse
                var response = await client.GetStringAsync(link);

                // regex 1 : détecter le nom donné au dossier Cyberdrop
                var nom_album = Regex.Match(response, "<h1 id=\"title\" class=\"title has-text-centered\" title=\"(.+?)\">", RegexOptions.Compiled).Groups[1].Value;

                // regex 2 : trouver le nombre de fichiers inclus dans le dossier
                MatchCollection matches = Regex.Matches(response, "class=\"image\" href=\"(https://fs-0([0-9]).cyberdrop.(cc|to)/(.+?))\"", RegexOptions.Compiled);

                if (matches.Count >= 0)
                {
                    MessageBox.Show("J'ai trouvé un dossier nommé " + nom_album + " contenant " + matches.Count + " fichiers", "Bien vu chacal", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    foreach (Match match in matches)
                    {
                        liens_fichiers.Add(match.Groups[1].Value);
                    }
                    liste_docs.Add(new BaseCyberdrop(nom_album, matches.Count, liens_fichiers));
                    DocList.DataContext = null;
                    DocList.Items.Add(nom_album + " - " + matches.Count + " fichiers");
                }
            }
        }

        private void RecupDoc_Click(object sender, RoutedEventArgs e)
        {
            _ = LoadUpSomeShit(CDLink.Text);
        }

        private void TelechargerContenu_Click(object sender, RoutedEventArgs e)
        {
            _ = TelechargerContenu();
        }

        private async Task TelechargerContenu()
        {
            foreach (var donnee in liste_docs)
            {
                string lienDossier = Environment.CurrentDirectory + '\\' + donnee.Nom_album;
                if (!Directory.Exists(lienDossier))
                {
                    Directory.CreateDirectory(lienDossier);
                }

                foreach (string lien in donnee.Liens)
                {
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                    // retrouver le nom du fichier sur le lien de "téléchargement"
                    Uri url = new Uri(lien);
                    string filename = Path.GetFileName(url.LocalPath);

                    // logique téléchargement avec try/catch pour vérif.
                    using var downloader = new HttpClient();
                    try
                    {
                        var request = new HttpRequestMessage(HttpMethod.Get, lien);
                        var sendTask = client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
                        var document = sendTask.Result.EnsureSuccessStatusCode();
                        var httpStream = await document.Content.ReadAsStreamAsync();
                        var lien_fichier = Path.Combine(lienDossier, filename);
                        using var fileStream = File.Create(lien_fichier);
                        using var reader = new StreamReader(httpStream);
                        httpStream.CopyTo(fileStream);
                        fileStream.Flush();
                    }
                    catch (HttpRequestException ex)
                    {
                        MessageBox.Show("Un ou plusieurs fichiers n'ont pas pu être téléchargés : " + ex.Message, "Whoopsie !", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
            MessageBox.Show("Tout les documents de la liste ont été téléchargés. Profitez !", "Finito amigo", MessageBoxButton.OK, MessageBoxImage.Asterisk);
        }
    }
}
