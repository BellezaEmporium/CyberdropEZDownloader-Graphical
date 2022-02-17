using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace CyberdropEZDownloaderGraphical.ListBase
{
    public class BaseCyberdrop
    {
        private string nom_album;

        private int nb_fichiers;

        private List<string> liens;

        public string Nom_album
        {
            get => nom_album;
            set
            {
                nom_album = value;
            }
        }

        public int Nb_fichiers
        {
            get => nb_fichiers;
            set
            {
                nb_fichiers = value;
            }
        }

        public List<string> Liens
        {
            get => liens;
            set
            {
                liens = value;
            }
        }


        public BaseCyberdrop(string nom, int fichiers, List<string> urls)
        {
            Nom_album = nom;
            Nb_fichiers = fichiers;
            Liens = urls;
        }
    }
}
