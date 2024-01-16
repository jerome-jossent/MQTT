using Geoimage;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Portik_Data_Explorer
{
    public class FichierGeoImage : Fichier
    {
        public override string mqtt_topic { get => "ackisition/geoimage/viewer"; }
        public FichierGeoImage(FileInfo fileInfo)
        {
            this.fileInfo = fileInfo;
            name = fileInfo.Name;
            color = Brushes.Red;
        }

        public new const string extension = ".portik.geoimage";

        internal static FichierGeoImage[] GetFiles(DirectoryInfo directoryInfo)
        {
            List<FichierGeoImage> fichierGeoImages = new List<FichierGeoImage>();
            var fichiers = directoryInfo.GetFiles("*" + extension);
            foreach (FileInfo fileInfo in fichiers)
            {
                fichierGeoImages.Add(new FichierGeoImage(fileInfo));
            }
            return fichierGeoImages.ToArray();
        }

        internal static void SaveToDiskImage(byte[] data, string fileName)
        {
            GeoImage geoImage = LoadGeoImage(data);
            GeoImage_to_Mat(geoImage).SaveImage(fileName);
        }

        internal static GeoImage LoadGeoImage(string fullPath)
        {
            return LoadGeoImage(File.ReadAllBytes(fullPath));
        }
        internal static GeoImage LoadGeoImage(byte[] bytes)
        {
            return GeoImage.Parser.ParseFrom(bytes);
        }

        internal static Mat GeoImage_to_Mat(GeoImage gi)
        {
            byte[] matdata = gi.ImageData.ToArray<byte>();
            Mat mat;

            switch (gi.Format)
            {
                //case "bgr":
                //    mat = Cv2.ImDecode(matdata, ImreadModes.Color);
                //    Cv2.CvtColor(mat, mat, ColorConversionCodes.BGR2RGB);
                //    break;

                default:
                    mat = Cv2.ImDecode(matdata, ImreadModes.Color);
                    break;
            }
            return mat;
        }
    }
}
