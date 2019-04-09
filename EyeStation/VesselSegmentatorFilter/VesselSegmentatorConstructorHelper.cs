namespace EyeStation.VesselSegmentatorFilter
{
	/// <summary>
	/// Helper class for creating VesselSegmentator objects with the same parameters
	/// </summary>
	public class VesselSegmentatorConstructorHelper
	{
		public int? windowRadius { get; set; }
		public int? smallLineLenght { get; set; }
		public double? Threshold { get; set; }
		public VesselSegmentatioMethod? VesselSegmentatioMethodType { get; set; }
	}
}
