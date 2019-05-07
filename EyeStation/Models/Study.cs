using EyeStation.PACSDAO;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace EyeStation.Model
{
    public class Study
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public string FilePath { get; set; }

        public string Description { get; set; }

        public string Angles { get; set; }

        public string Lengths { get; set; }

        public string Markers { get; set; }

        public BitmapImage ImageSource { get; set; }

        public Study(string id, string name, string description, string angles, string lengths, string markers, string filePath)
        {
            this.Id = id;
            this.Name = name;
            this.FilePath = filePath;
            this.Angles = angles;
            this.Lengths = lengths;
            this.Markers = markers;
            this.Description = description;
            try
            {
                this.ImageSource = new BitmapImage(new Uri(filePath+".jpg"));
            }
            catch
            {
                this.ImageSource = null;
            }
        }
        public static bool EditAngles(PACSObj serwer, EyeStation.Model.Study studyToEdit, string value)
        {
            return EditStudy(serwer, studyToEdit, "8,1030", value);
        }

        public static bool EditLengths(PACSObj serwer, EyeStation.Model.Study studyToEdit, string value)
        {
            return EditStudy(serwer, studyToEdit, "10,4000", value);
        }

        public static bool EditMarkers(PACSObj serwer, EyeStation.Model.Study studyToEdit, string value)
        {
            return EditStudy(serwer, studyToEdit, "20,4000", value);
        }

        public static bool EditDescription(PACSObj serwer, EyeStation.Model.Study studyToEdit,string value)
        {            
            return EditStudy(serwer, studyToEdit, "8,1080", value);
        }

        private static bool EditStudy(PACSObj serwer, EyeStation.Model.Study studyToEdit, string tag, string value)
        {
            //Zmiana DICOMA
            DCMTK.GDCMANON(studyToEdit.FilePath, tag, replacePolishSymbols(value));
            //Zapis do PACS
            bool result = serwer.Store(studyToEdit.FilePath+".dcm");

            return result;
        }

        private static string replacePolishSymbols(string s)
        {
            return s.Replace('ą', 'a').Replace('ę', 'e').Replace('ż', 'z').Replace('ź', 'z').Replace('ń', 'n').Replace('ł', 'l').Replace('ó', 'o').Replace('ć', 'c').Replace('ś', 's').Replace('Ą', 'A').Replace('Ę', 'E').Replace('Ż', 'Z').Replace('Ź', 'Z').Replace('Ń', 'N').Replace('Ł', 'L').Replace('Ó', 'O').Replace('Ć', 'C').Replace('Ś', 'S');
        }

    }
}
