using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Windows.Input;
using AirMonitor.Models;
using AirMonitor.Views;
using Newtonsoft.Json;
using Xamarin.Essentials;
using Xamarin.Forms;



namespace AirMonitor.ViewModels
{
    public class HomeViewModel : BaseViewModel
    {
        private readonly INavigation _navigation;

        public HomeViewModel(INavigation navigation)
        {
            _navigation = navigation;

            Initialize();
        }

        private List<Installation> _items;
        public List<Installation> Items
        {
            get => _items;
            set => SetProperty(ref _items, value);
        }
        private List<Measurement> _details;
        public List<Measurement> Details
        {
            get => _details;
            set => SetProperty(ref _details, value);
        }

        private async Task Initialize()
        {
            var location = await GetLocation();
            var installations = await GetInstallations(location, maxResults: 7);
            Items = new List<Installation>(installations);
            var measurements = await GetMeasurements(Items);
            Details = new List<Measurement>(measurements);
        }
        private async Task<IEnumerable<Installation>> GetInstallations(Location location, double maxDistanceInKm = 3, int maxResults = -1)
        {
            if (location == null)
            {
                System.Diagnostics.Debug.WriteLine("No location data.");
                return null;
            }

            var query = GetQuery(new Dictionary<string, object>
            {
                { "lat", location.Latitude },
                { "lng", location.Longitude },
                { "maxDistanceKM", maxDistanceInKm },
                { "maxResults", maxResults }
            });
            var url = GetAirlyApiUrl(App.AirlyApiInstallationUrl, query);

            var response = await GetHttpResponseAsync<IEnumerable<Installation>>(url);
            return response;
        }

        private async Task<IEnumerable<Measurement>> GetMeasurements(IEnumerable<Installation> installations)
        {
            var measurements = new List<Measurement>();
            foreach (var installation in installations)
            {
                var query = GetQuery(new Dictionary<string, object>
                {
                    {"installationId", installation.Id }
                });
                var url = GetAirlyApiUrl(App.AirlyApiMeasurementUrl, query);
                var response = await GetHttpResponseAsync<Measurement>(url);
                measurements.Add(response);
            }
            return measurements;
        }

        private string GetAirlyApiUrl(string path, string query)
        {
            var builder = new UriBuilder(App.AirlyApiUrl);
            builder.Port = -1;
            builder.Path += path;
            builder.Query = query;
            string url = builder.ToString();

            return url;
        }



        private string GetQuery(IDictionary<string, object> args)
        {
            if (args == null) return null;

            var query = HttpUtility.ParseQueryString(string.Empty);

            foreach (var arg in args)
            {
                if (arg.Value is double number)
                {
                    query[arg.Key] = number.ToString(CultureInfo.InvariantCulture);
                }
                else
                {
                    query[arg.Key] = arg.Value?.ToString();
                }
            }

            return query.ToString();
        }

        private static HttpClient GetHttpClient()
        {
            var client = new HttpClient();
            client.BaseAddress = new Uri(App.AirlyApiUrl);

            client.DefaultRequestHeaders.Add("Accept", "application/json");
            client.DefaultRequestHeaders.Add("Accept-Encoding", "gzip");
            client.DefaultRequestHeaders.Add("apikey", App.AirlyApiKey);
            return client;
        }

        private async Task<T> GetHttpResponseAsync<T>(string url)
        {
            try
            {
                var client = GetHttpClient();
                var response = await client.GetAsync(url);

                if (response.Headers.TryGetValues("X-RateLimit-Limit-day", out var dayLimit) &&
                    response.Headers.TryGetValues("X-RateLimit-Remaining-day", out var dayLimitRemaining))
                {
                    System.Diagnostics.Debug.WriteLine($"Day limit: {dayLimit?.FirstOrDefault()}, remaining: {dayLimitRemaining?.FirstOrDefault()}");
                }

                switch ((int)response.StatusCode)
                {
                    case 200:
                        var content = await response.Content.ReadAsStringAsync();
                        var result = JsonConvert.DeserializeObject<T>(content);
                        return result;
                    case 429: // too many requests
                        System.Diagnostics.Debug.WriteLine("Too many requests");
                        break;
                    default:
                        var errorContent = await response.Content.ReadAsStringAsync();
                        System.Diagnostics.Debug.WriteLine($"Response error: {errorContent}");
                        return default;
                }
            }
            catch (JsonReaderException ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
            }
            catch (WebException ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
            }



            return default;
        }



        private async Task<Location> GetLocation()
        {
            Location location = await Geolocation.GetLastKnownLocationAsync();
            return location;
        }



        private ICommand _goToDetailsCommand;
        public ICommand GoToDetailsCommand => _goToDetailsCommand ?? (_goToDetailsCommand = new Command(OnGoToDetails));



        private void OnGoToDetails()
        {
            _navigation.PushAsync(new DetailsPage());
        }
    }
}