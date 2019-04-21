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
        
        public BitmapImage ImageSource { get; set; }

        public Study(string id, string name, string description,  string filePath)
        {
            this.Id = id;
            this.Name = name;
            this.FilePath = filePath;
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

        public static bool EditStudy(PACSObj serwer, EyeStation.Model.Study studyToEdit, string tag, string value)
        {
            //Zmiana DICOMA
            DCMTK.GDCMANON(studyToEdit.FilePath, tag, value);
            //Zapis do PACS
            bool result = serwer.Store(studyToEdit.FilePath+".dcm");

            return result;
        }

    }
}
