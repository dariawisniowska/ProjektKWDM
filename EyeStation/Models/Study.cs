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

        public string Description { get; set; }

        public Image Image { get; set; }

        public BitmapImage ImageSource { get; set; }

        public Study(string id, string name, string description, Image image, string filePath)
        {
            this.Id = id;
            this.Name = name;
            this.Description = description;
            this.ImageSource = new BitmapImage(new Uri(filePath));
        }        
    }
}
