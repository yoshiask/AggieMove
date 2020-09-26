namespace TamuBusFeed.Models
{
	public class Pattern
	{
		public string Key { get; set; }
		public string Name { get; set; }
		public string ShortName { get; set; }
		public string Description { get; set; }
		public Direction Direction { get; set; }
		public Info LineInfo { get; set; }
		public Info TimePointInfo { get; set; }
		public Info BusStopInfo { get; set; }
		public bool IsDisplay { get; set; }
		public bool IsPrimary { get; set; }
	}
}
