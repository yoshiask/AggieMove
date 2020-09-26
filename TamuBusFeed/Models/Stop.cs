namespace TamuBusFeed.Models
{
	public class Stop
	{
		public string Key { get; set; }
		public int Rank { get; set; }
		public string Name { get; set; }
		public string StopCode { get; set; }
		public bool IsTimeCode{ get; set; }
	}
}
