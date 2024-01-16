using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Portik_Data_Explorer
{
    public class FichierGeoFlips : Fichier
    {
        public override string mqtt_topic { get => "plc/flips"; }

        public FichierGeoFlips(FileInfo fileInfo)
        {
            this.fileInfo = fileInfo;
            name = fileInfo.Name;
            color = Brushes.LightGreen;
        }

        public new const string extension = ".portik.geoflips";


        internal static FichierGeoFlips[]? GetFiles(DirectoryInfo directoryInfo)
        {
            List<FichierGeoFlips> fichierGeoImages = new List<FichierGeoFlips>();
            var fichiers = directoryInfo.GetFiles("*" + extension);
            foreach (FileInfo fileInfo in fichiers)
            {
                fichierGeoImages.Add(new FichierGeoFlips(fileInfo));
            }
            return fichierGeoImages.ToArray();
        }
    }
}
