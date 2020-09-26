namespace TamuBusFeed.Models
{
	public class PatternElement
	{
		public string Key { get; set; }
		public string Name { get; set; }
		public string Description { get; set; }
		public int Rank { get; set; }
		public double Longitude { get; set; }
		public double Latitude { get; set; }
		public int PointTypeCode { get; set; }
		public int RouteHeaderRank { get; set; }
		public Stop Stop { get; set; }
	}
}
