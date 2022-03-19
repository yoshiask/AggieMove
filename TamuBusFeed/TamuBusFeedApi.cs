using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Flurl;
using Flurl.Http;
using Flurl.Http.Configuration;
using Newtonsoft.Json.Linq;
using TamuBusFeed.Models;

namespace TamuBusFeed
{
    public static class TamuBusFeedApi
    {
        static TamuBusFeedApi()
        {
            FlurlHttp.Configure(settings =>
            {
                settings.HttpClientFactory = new TAMUTransportHttpClientFactory();
            });
        }

        private const string HOST_BASE = "https://transport.tamu.edu";
        private static readonly string FEED_API_URL = HOST_BASE.AppendPathSegments("BusRoutesFeed", "api");
        
        private static IFlurlRequest GetBase()
        {
            return FEED_API_URL.WithHeader("Accept", "application/json");
        }

        public static async Task<List<Route>> GetRoutes()
        {
            return await GetBase()
                .AppendPathSegments("routes")
                .GetJsonAsync<List<Route>>();
        }

        public static async Task<List<PatternElement>> GetPattern(string shortname, DateTimeOffset? date = null)
        {
            date ??= DateTimeOffset.Now;
            return await GetBase()
                .AppendPathSegments("route", shortname, "pattern", date.Value.ToString("yyyy-MM-dd"))
                .GetJsonAsync<List<PatternElement>>();
        }

        public static async Task<AnnouncementFeed> GetAnnouncements()
        {
            return await GetBase()
                .AppendPathSegments("announcements")
                .GetJsonAsync<AnnouncementFeed>();
        }

        public static async Task<TimeTable> GetTimetable(string shortname, DateTimeOffset? date = null)
        {
            date ??= DateTimeOffset.Now;
            var response = await GetBase()
                .AppendPathSegments("route", shortname, "TimeTable", "2022-03-24")//date.Value.ToString("yyyy-MM-dd"))
                .GetJsonAsync<List<JObject>>();

            TimeTable timeTable = new()
            {
                TimeStops = new()
            };

            // Check for no service
            if (response.Count == 1 && response[0].ContainsKey(" "))
                return timeTable;

            // Add stops
            int guidLength = Guid.Empty.ToString().Length;
            foreach (var stop in response[0])
            {
                // The JSON keys look like "e6266125-1350-4226-8413-b41869f0c313Trigon"
                string name = stop.Key.Remove(0, guidLength);
                timeTable.TimeStops.Add(new()
                {
                    Name = name,
                    LeaveTimes = new()
                });
            }

            // Populate times
            foreach (var row in response)
            {
                var rowTimes = row.Children().Select(c => c.Value<string>()).ToArray();
                for (int col = 0; col < row.Count; col++)
                {
                    DateTimeOffset? time = null;
                    string timeStr = rowTimes[col];
                    if (timeStr != null)
                        time = DateTimeOffset.Parse(timeStr);

                    timeTable.TimeStops[col].LeaveTimes.Add(time);
                }
            }

            return timeTable;
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
            bool isValid = certificate.Subject == "CN=transport.tamu.edu, OU=Texas A&M IT, O=Texas A & M University, STREET=112 Jack K Williams Admin Building, L=College Station, S=Texas, PostalCode=77843, C=US"
                || certificate.Subject == "CN=transport.tamu.edu, OU=Texas A&M IT, O=Texas A & M University, STREET=112 Jack K Williams Admin Building, L=College Station, S=Texas, OID.2.5.4.17=77843, C=US"
                || certificate.Subject == "CN=transport.tamu.edu, OU=Texas A&M IT, O=Texas A & M University, L=College Station, S=Texas, C=US";
            return isValid;
        }
    }
}
