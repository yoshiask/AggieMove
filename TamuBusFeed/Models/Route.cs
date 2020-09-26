namespace TamuBusFeed.Models
{
	public class Route
	{
		public Group Group { get; set; }
		public string Icon { get; set; }
		public TimeTableOverride TimeTableOverride { get; set; }
		public bool WebLink { get; set; }
		public string Color { get; set; }
		public Pattern Pattern { get; set; }
		public string Key { get; set; }
		public string Name { get; set; }
		public string ShortName { get; set; }
		public string Description { get; set; }
		public RouteType RouteType { get; set; }
	}
}
