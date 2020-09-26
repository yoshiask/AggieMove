using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Flurl;
using Flurl.Http;
using TamuBusFeed.Models;

namespace TamuBusFeed
{
	public class TamuBusFeedApi
	{
		private const string HOST_URL = "https://transport.tamu.edu/BusRoutesFeed/api";

		public static async Task<List<Route>> GetRoutes()
		{
			return await HOST_URL
				.AppendPathSegments("routes")
				.GetJsonAsync<List<Route>>();
		}

		public static async Task<List<PatternElement>> GetPattern(string shortname, DateTimeOffset date)
		{
			return await HOST_URL
				.AppendPathSegments("route", shortname, "pattern", date.ToString("yyyy-MM-dd"))
				.GetJsonAsync<List<PatternElement>>();
		}
		public static async Task<List<PatternElement>> GetPattern(string shortname)
		{
			return await GetPattern(shortname, DateTimeOffset.Now);
		}
	}
}
