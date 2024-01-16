using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Portik_Data_Explorer
{
    public class FichierInferences : Fichier
    {
        public override string mqtt_topic { get => "structurecore/inference-response"; }

        public FichierInferences(FileInfo fileInfo)
        {
            this.fileInfo = fileInfo;
            name = fileInfo.Name;
            color = Brushes.Orange;
        }

        public new const string extension = ".portik.inference";

        internal static FichierInferences[]? GetFiles(DirectoryInfo directoryInfo)
        {
            List<FichierInferences> fichierGeoImages = new List<FichierInferences>();
            var fichiers = directoryInfo.GetFiles("*" + extension);
            foreach (FileInfo fileInfo in fichiers)
            {
                fichierGeoImages.Add(new FichierInferences(fileInfo));
            }
            return fichierGeoImages.ToArray();
        }
    }
}
