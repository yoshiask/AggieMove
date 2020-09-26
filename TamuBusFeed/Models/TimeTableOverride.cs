namespace TamuBusFeed.Models
{
	public class TimeTableOverride
	{
		public bool Enabled { get; set; }
		public double Before { get; set; }
		public bool During { get; set; }
		public double After { get; set; }
	}
}
