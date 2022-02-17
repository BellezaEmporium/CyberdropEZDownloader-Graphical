using CyberdropEZDownloaderGraphical.ListBase;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace CyberdropEZDownloaderGraphical
{
    /// <summary>
    /// Logique d'interaction pour Progression.xaml
    /// </summary>
    public partial class Progression : Window, IProgression, INotifyPropertyChanged
    {

        public Progression()
        {
            InitializeComponent();
        }

        // Initialize getters and setters for the progress percentage.
        private double _checkedPercentage;

        public double CheckedPercentage
        {
            get => _checkedPercentage;
            set
            {
                _checkedPercentage = value;
                // Event to trigger when the progress has changed.
                OnPropertyChanged(nameof(CheckedPercentage));
            }
        }

        // Event triggered when the progress has changed.
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string PropertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(PropertyName));
        }

        public async Task<(List<BaseCyberdrop>, bool)> TelechargerContenu(List<BaseCyberdrop> baseCyberdrop)
        {
            await Task.Run(async () =>
            {
                foreach (var nom_dossier in baseCyberdrop)
                {
                    string lienDossier = Environment.CurrentDirectory + '\\' + nom_dossier.Nom_album;
                    if (!Directory.Exists(lienDossier))
                    {
                        Directory.CreateDirectory(lienDossier);
                    }
                    int nombre_fichiers = nom_dossier.Nb_fichiers;
                    double compteur = 0;
                    foreach (var donnee in nom_dossier.Liens)
                    {
                        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                        // retrouver le nom du fichier sur le lien de "téléchargement"
                        Uri url = new Uri(donnee);
                        string filename = System.IO.Path.GetFileName(url.LocalPath);

                        // logique téléchargement avec try/catch pour vérif.
                        using var downloader = new HttpClient();
                        try
                        {
                            var request = new HttpRequestMessage(HttpMethod.Get, donnee);
                            var sendTask = downloader.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
                            var document = sendTask.Result.EnsureSuccessStatusCode();
                            var httpStream = await document.Content.ReadAsStreamAsync();
                            var lien_fichier = System.IO.Path.Combine(lienDossier, filename);
                            using var fileStream = File.Create(lien_fichier);
                            using var reader = new StreamReader(httpStream);
                            httpStream.CopyTo(fileStream);
                            fileStream.Flush();
                            compteur++;
                            CheckedPercentage = compteur * 100.0 / nombre_fichiers;
                        }
                        catch (HttpRequestException ex)
                        {
                            MessageBox.Show("Un ou plusieurs fichiers n'ont pas pu être téléchargés : " + ex.Message, "Whoopsie !", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                    }
                }
                MessageBox.Show("Tout les documents de la liste ont été téléchargés. Profitez !", "Finito amigo", MessageBoxButton.OK, MessageBoxImage.Asterisk);
            });
            return (baseCyberdrop, true);
        }
    }
}
