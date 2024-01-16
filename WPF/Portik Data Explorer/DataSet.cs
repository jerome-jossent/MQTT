using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Portik_Data_Explorer
{
    public class DataSet : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public bool Vide { get; set; }
        public int GeoImages_nbr { get; set; }
        public int GeoInferences_nbr { get; set; }
        public int GeoObjets_nbr { get; set; }
        public int GeoFlips_nbr { get; set; }
        public string Nom { get; set; }
        public float FolderSize_Mo { get; set; }

        string folder;
        DirectoryInfo directoryInfo;

        public ObservableCollection<Fichier> fichiers
        {
            get => _fichiers; set
            {
                if (value == null || value == _fichiers) return;
                _fichiers = value;
                OnPropertyChanged();
            }
        }
        ObservableCollection<Fichier> _fichiers;

        public Fichier activeFichier
        {
            get => _activeFichier; set
            {
                if (value == null || value == _activeFichier) return;
                _activeFichier = value;
                OnPropertyChanged();
            }
        }
        Fichier _activeFichier;

        long duration_raw;

        List<long> times = new List<long>();
        Dictionary<double, Fichier> timesfiles;
        List<double> times_f = new List<double>();

        public float duration
        {
            get => _duration; set
            {
                if (value == null || value == _duration) return;
                _duration = value;
                OnPropertyChanged();
            }
        }
        float _duration;
        public float currenttime
        {
            get => _currenttime; set
            {
                if (value == null || value == _currenttime) return;
                _currenttime = value;
                OnPropertyChanged();
            }
        }
        float _currenttime;

        public DataSet(string path, string folder)
        {
            this.folder = folder;
            Nom = folder.Replace(path, "");
            directoryInfo = new DirectoryInfo(folder);

            times = new List<long>();
            fichiers = new ObservableCollection<Fichier>();
            int index = 0;
            foreach (FileInfo fichier in directoryInfo.GetFiles())
            {
                FolderSize_Mo += (float)fichier.Length / (1024 * 1024);
                Fichier f;
                switch (".portik" + fichier.Extension)
                {
                    case FichierGeoImage.extension:
                        f = new FichierGeoImage(fichier);
                        GeoImages_nbr++;
                        break;
                    case FichierInferences.extension:
                        f = new FichierInferences(fichier);
                        GeoInferences_nbr++;
                        break;
                    case FichierGeoFlips.extension:
                        f = new FichierGeoFlips(fichier);
                        GeoFlips_nbr++;
                        break;
                    case FichierGeoObjets.extension:
                        f = new FichierGeoObjets(fichier);
                        GeoObjets_nbr++;
                        break;
                    default:
                        f = null;
                        break;
                }

                if (f == null)
                    break;

                fichiers.Add(f);

                f.t_absolute = GetTimeFromName(fichier.Name);
                f.index = index;
                index++;
                times.Add(f.t_absolute);
            }

            ComputeDurations();

            Vide = fichiers.Count == 0;
            if (Vide)
                return;
        }

        long GetTimeFromName(string t_string)
        {
            t_string = t_string.Replace(FichierGeoImage.extension, "");
            t_string = t_string.Replace(FichierInferences.extension, "");
            t_string = t_string.Replace(FichierGeoFlips.extension, "");
            t_string = t_string.Replace(FichierGeoObjets.extension, "");
            return long.Parse(t_string);
        }

        void ComputeDurations()
        {
            times.Sort();
            if (times.Count > 0)
            {
                duration_raw = times[times.Count - 1] - times[0];
                duration = (float)duration_raw / 10_000_000;
                times_f = new List<double>();
                timesfiles = new Dictionary<double, Fichier>();
                foreach (Fichier fichier in fichiers)
                {
                    fichier.ComputeTime(times[0]);
                    times_f.Add(fichier.t_s);
                    timesfiles.Add(fichier.t_s, fichier);
                }
            }
        }

        public void ExploreFile() { ExploreFile(folder); }

        void ExploreFile(string folderPath)
        {
            if (!Directory.Exists(folderPath)) return;
            folderPath = System.IO.Path.GetFullPath(folderPath);
            System.Diagnostics.Process.Start("explorer.exe", string.Format("/select,\"{0}\"", folderPath));
        }

        public void DeleteFolder(bool messagedalerte = true)
        {
            if (fichiers.Count > 0)
            {
                if (messagedalerte)
                {

                    MessageBoxResult result = MessageBox.Show(
                        "Le dossier sélectionné n'est pas vide.\n" +
                        "Confirmer la suppression.", "Attention",
                        MessageBoxButton.OKCancel,
                        MessageBoxImage.Warning);
                    if (result != MessageBoxResult.OK)
                        return;
                }
            }
            directoryInfo.Delete(true);
        }
    }
}
