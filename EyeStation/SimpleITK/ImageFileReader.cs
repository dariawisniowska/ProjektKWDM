using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using sitk = itk.simple;

namespace EyeStation.SimpleITK
{
    public class ImageFileReader
    {
        public static sitk.Image ReadImage(string fileName)
        {
            sitk.ImageFileReader imageFileReader = new sitk.ImageFileReader();
            imageFileReader.SetFileName(fileName);
            sitk.Image image = imageFileReader.Execute();
            return image;
        }
    }
}
