using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using AngleSharp;
using Flurl;
using Flurl.Http;
using Flurl.Http.Configuration;
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

        public static async Task<List<Route>> GetRoutes()
        {
            return await FEED_API_URL
                .AppendPathSegments("routes")
                .GetJsonAsync<List<Route>>();
        }

        public static async Task<List<PatternElement>> GetPattern(string shortname, DateTimeOffset date)
        {
            return await FEED_API_URL
                .AppendPathSegments("route", shortname, "pattern", date.ToString("yyyy-MM-dd"))
                .GetJsonAsync<List<PatternElement>>();
        }
        public static async Task<List<PatternElement>> GetPattern(string shortname)
        {
            return await GetPattern(shortname, DateTimeOffset.Now);
        }

        public static async Task<AnnouncementFeed> GetAnnouncements()
        {
            return await FEED_API_URL
                .AppendPathSegments("announcements")
                .GetJsonAsync<AnnouncementFeed>();
        }

        public static async Task<TimeTable> GetTimetable(string shortname)
        {
            var response = await HOST_BASE
                .AppendPathSegments("busroutes", "Routes.aspx").SetQueryParam("r", shortname)
                .PostAsync();
            string html = await response.GetStringAsync();

            var config = Configuration.Default.WithDefaultLoader();
            var context = BrowsingContext.New(config);
            var document = await context.OpenAsync(req => req.Content(html));

            var timeTable = new TimeTable();

            var timeTableGrid = document.QuerySelector("#TimeTableGridView").LastElementChild.Children.ToList();
            var headerRowIdx = timeTableGrid.FindLastIndex(e => e.ClassList.Contains("headRow"));
            if (headerRowIdx < 0)
                return null;

            var headerRow = timeTableGrid[headerRowIdx];
            timeTable.TimeStops = new System.Collections.ObjectModel.ObservableCollection<TimeStop>(
                headerRow.Children.Select(timeStop => new TimeStop
                {
                    Name = timeStop.TextContent, 
                    LeaveTimes = new System.Collections.ObjectModel.ObservableCollection<DateTimeOffset?>()
                })
            );

            foreach (var row in timeTableGrid.Skip(headerRowIdx + 1))
            {
                for (int i = 0; i < timeTable.TimeStops.Count; i++)
                {
                    var leaveTimes = timeTable.TimeStops[i].LeaveTimes;
                    var cell = row.Children[i].QuerySelector("time");

                    if (DateTimeOffset.TryParse(cell?.GetAttribute("dateTime"), out var time))
                        leaveTimes.Add(time);
                    else
                        leaveTimes.Add(null);
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
            bool isValid = (certificate.Subject == "CN=transport.tamu.edu, OU=Texas A&M IT, O=Texas A & M University, STREET=112 Jack K Williams Admin Building, L=College Station, S=Texas, PostalCode=77843, C=US"
                || certificate.Subject == "CN=transport.tamu.edu, OU=Texas A&M IT, O=Texas A & M University, STREET=112 Jack K Williams Admin Building, L=College Station, S=Texas, OID.2.5.4.17=77843, C=US")
                && certificate.SerialNumber == "00E4773C658B1DC2028D5BF14E162711F2"
                && certificate.Thumbprint == "01C6EFCA9730068A91E0DB01D8857575044FB6D0";
            return isValid;
        }
    }
}
