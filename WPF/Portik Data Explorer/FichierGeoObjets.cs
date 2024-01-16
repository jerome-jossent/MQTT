using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Portik_Data_Explorer
{
    internal class FichierGeoObjets : Fichier
    {
        public override string mqtt_topic { get => "structurecore/filtered-objects/ok"; }

        public new const string extension = ".portik.geoobjets";

        public FichierGeoObjets(FileInfo fileInfo)
        {
            this.fileInfo = fileInfo;
            name = fileInfo.Name;
            color = Brushes.Yellow;
        }

        internal static FichierGeoObjets[]? GetFiles(DirectoryInfo directoryInfo)
        {
            List<FichierGeoObjets> fichierGeoObjets = new List<FichierGeoObjets>();
            var fichiers = directoryInfo.GetFiles("*" + extension);
            foreach (FileInfo fileInfo in fichiers)
            {
                fichierGeoObjets.Add(new FichierGeoObjets(fileInfo));
            }
            return fichierGeoObjets.ToArray();
        }
    }
}
