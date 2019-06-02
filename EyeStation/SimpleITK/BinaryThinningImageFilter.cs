using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using sitk = itk.simple;

namespace EyeStation.SimpleITK
{
    public class BinaryThinningImageFilter
    {
        public static sitk.Image BinaryThinning(sitk.Image inputImage)
        {
            sitk.VectorIndexSelectionCastImageFilter vectorIndexSelectionCastImageFilter = new sitk.VectorIndexSelectionCastImageFilter();
            sitk.CastImageFilter castImageFilter = new sitk.CastImageFilter();
            sitk.Image image = vectorIndexSelectionCastImageFilter.Execute(inputImage, 0, castImageFilter.GetOutputPixelType());

            sitk.BinaryThresholdImageFilter binaryThresholdImageFilter = new sitk.BinaryThresholdImageFilter();
            binaryThresholdImageFilter.SetInsideValue(0);
            binaryThresholdImageFilter.SetOutsideValue(255);
            binaryThresholdImageFilter.SetLowerThreshold(0);
            binaryThresholdImageFilter.SetUpperThreshold(100);
            sitk.Image binaryThresholdImage = binaryThresholdImageFilter.Execute(image);

            sitk.BinaryThinningImageFilter binaryThinningImageFilter = new sitk.BinaryThinningImageFilter();
            sitk.Image binaryThinningImage = binaryThinningImageFilter.Execute(binaryThresholdImage);

            sitk.RescaleIntensityImageFilter rescaleIntensityImageFilter = new sitk.RescaleIntensityImageFilter();
            rescaleIntensityImageFilter.SetOutputMinimum(0);
            rescaleIntensityImageFilter.SetOutputMaximum(255);
            sitk.Image rescaleIntensityImage = rescaleIntensityImageFilter.Execute(binaryThinningImage);

            return rescaleIntensityImage;
        }
    }
}
