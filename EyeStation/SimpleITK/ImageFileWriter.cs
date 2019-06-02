using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using sitk = itk.simple;

namespace EyeStation.SimpleITK
{
    public class ImageFileWriter
    {
        public static void WriteImage(sitk.Image image, string fileName)
        {
            sitk.ImageFileWriter imageFileWriter = new sitk.ImageFileWriter();
            imageFileWriter.SetFileName(fileName);
            imageFileWriter.Execute(image);
        }
    }
}
