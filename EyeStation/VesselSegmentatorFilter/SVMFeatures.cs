using libsvm;

namespace EyeStation.VesselSegmentatorFilter
{
	/// <summary>
	/// Propably temporaty class for store data for svm
	/// </summary>
	public class SVMFeatures
	{
		/// <summary>
		/// Difference between average gray scale level of main filter line and average gray scale of window
		/// </summary>
		public double PixelPowerOfMainLine;

		/// <summary>
		/// Difference between average gray scale level of line small filter line and average gray scale of window
		/// </summary>
		public double PixelPowerOfSmallLine;

		/// <summary>
		/// Gray scale value of pixel
		/// </summary>
		public double PixelGrayLevel;

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="normalize">flag on which depending normalization</param>
		public SVMFeatures(double pixelPowerOfMainLine, double pixelPowerOfSmallLine, double pixelGrayLevel, bool normalize = true)
		{
			if (normalize)
			{
				PixelPowerOfMainLine = (pixelPowerOfMainLine + byte.MaxValue) / (byte.MaxValue * 2);
				PixelPowerOfSmallLine = (pixelPowerOfSmallLine + byte.MaxValue) / (byte.MaxValue * 2);
				PixelGrayLevel = pixelGrayLevel / byte.MaxValue;
			}
			else
			{
				PixelPowerOfMainLine = pixelPowerOfMainLine;
				PixelPowerOfSmallLine = pixelPowerOfSmallLine;
				PixelGrayLevel = pixelGrayLevel;
			}
		}

		/// <summary>
		/// Convert class object to array of svm nodes
		/// </summary>
		/// <returns>array of svm nodes</returns>
		public svm_node[] ToSVMNodesArray()
		{
			return new svm_node[]
			{
				new svm_node(){ value = PixelPowerOfMainLine, index = 1 },
				new svm_node(){ value = PixelPowerOfSmallLine, index = 2 },
				new svm_node(){ value = PixelGrayLevel, index = 3 }
			};
		}
	}
}
