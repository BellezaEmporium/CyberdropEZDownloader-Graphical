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
        IProgression prog = new Progression();
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
                try
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
                catch (HttpRequestException ex)
                {
                    MessageBox.Show("Une erreur est survenue lors de la récupération de votre fichier : " + ex.Message);
                }
            }
        }

        private void RecupDoc_Click(object sender, RoutedEventArgs e)
        {
            _ = LoadUpSomeShit(CDLink.Text);
        }

        private void TelechargerContenu_Click(object sender, RoutedEventArgs e)
        {
            _ = prog.TelechargerContenu(liste_docs);
        }
    }
}
