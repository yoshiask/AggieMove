using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Flurl;
using Flurl.Http;
using Flurl.Http.Configuration;
using TamuBusFeed.Models;

namespace TamuBusFeed
{
	public class TamuBusFeedApi
	{
		private const string HOST_URL = "https://transport.tamu.edu/BusRoutesFeed/api";

		public static async Task<List<Route>> GetRoutes()
		{
            FlurlHttp.Configure(settings =>
            {
                settings.HttpClientFactory = new TAMUTransportHttpClientFactory();
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

	public class TAMUTransportHttpClientFactory : DefaultHttpClientFactory
	{
		// override to customize how HttpMessageHandler is created/configured
		public override HttpMessageHandler CreateMessageHandler()
		{
			HttpClientHandler httpClientHandler = new HttpClientHandler();

			// TODO: THIS IS VERY DANGEROUS! This callback makes it so that any SSL certificate is considered valid, even if it's
			// malicious or self-signed. This opens the app to man-in-the-middle attacks. See the following issue:
			// https://github.com/xamarin/xamarin-android/issues/4688
			httpClientHandler.ServerCertificateCustomValidationCallback += ValidateTAMUTransportSSLCert;
			return httpClientHandler;
		}

		private static bool ValidateTAMUTransportSSLCert(HttpRequestMessage sender, X509Certificate2 certificate, X509Chain chain, SslPolicyErrors sslPolicy)
		{
			bool isValid = (certificate.Subject == "CN=transport.tamu.edu, OU=Texas A&M IT, O=Texas A & M University, STREET=112 Jack K Williams Admin Building, L=College Station, S=Texas, PostalCode=77843, C=US"
				|| certificate.Subject == "CN=transport.tamu.edu, OU=Texas A&M IT, O=Texas A & M University, STREET=112 Jack K Williams Admin Building, L=College Station, S=Texas, OID.2.5.4.17=77843, C=US")
				&& certificate.SerialNumber == "00E4773C658B1DC2028D5BF14E162711F2"
				&& certificate.Thumbprint == "01C6EFCA9730068A91E0DB01D8857575044FB6D0";
			return isValid;
		}
	}
}
