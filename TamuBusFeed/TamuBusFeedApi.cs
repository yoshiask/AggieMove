using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Flurl;
using Flurl.Http;
using Flurl.Http.Configuration;
using TamuBusFeed.Models;

namespace TamuBusFeed
{
	public class TamuBusFeedApi
	{
		// TODO: For some reason requests fail on Android [8.1] if https is used
		private const string HOST_URL = "https://transport.tamu.edu/BusRoutesFeed/api";

		public static async Task<List<Route>> GetRoutes()
		{
			FlurlHttp.Configure(settings => {
				settings.HttpClientFactory = new MyCustomHttpClientFactory();
			});

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

	public class MyCustomHttpClientFactory : DefaultHttpClientFactory
	{

		// override to customize how HttpMessageHandler is created/configured
		public override HttpMessageHandler CreateMessageHandler()
        {
			return new HttpClientHandler()
			{
				AllowAutoRedirect = false
			};
        }
	}
}
