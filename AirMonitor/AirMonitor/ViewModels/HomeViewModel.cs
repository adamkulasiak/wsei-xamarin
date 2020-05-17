using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Windows.Input;
using AirMonitor.Helpers;
using AirMonitor.Models;
using AirMonitor.Views;
using Newtonsoft.Json;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Maps;

namespace AirMonitor.ViewModels
{
    public class HomeViewModel : BaseViewModel
    {
        private readonly INavigation _navigation;
        private readonly DatabaseHelper _db;

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

        private List<MapLocation> _locations;
        public List<MapLocation> Locations
        {
            get => _locations;
            set => SetProperty(ref _locations, value);
        }

        private string _remaining;
        public string Remaining
        {
            get => _remaining;
            set => SetProperty(ref _remaining, value);
        }

        private bool _isVisible;
        public bool IsVisible
        {
            get => _isVisible;
            set => SetProperty(ref _isVisible, value);
        }
        private bool _isRunning;
        public bool IsRunning
        {
            get => _isRunning;
            set => SetProperty(ref _isRunning, value);
        }

        private async Task Initialize()
        {
            
            IsRunning = true;
            IsVisible = true;
            var location = await GetLocation();
            var installations = await GetInstallations(location, maxResults: 3);
            var installationsWithDetails = await GetMeasurementsForInstalations(installations);
            Items = new List<Installation>(installationsWithDetails);
            Locations = Items.Select(x => new MapLocation
            {
                Address = x.Address.Description,
                Description = "CAQI: " + x.Measurement.Current.Values.FirstOrDefault().Value.ToString(),
                Position = new Position(x.Location.Latitude, x.Location.Longitude)
            }).ToList();
            IsRunning = false;
            IsVisible = false;
            
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

        private async Task<IEnumerable<Installation>> GetMeasurementsForInstalations(IEnumerable<Installation> installations)
        {
            var installationsUpdated = new List<Installation>();
            foreach (var installation in installations)
            {
                var query = GetQuery(new Dictionary<string, object>
                {
                    {"installationId", installation.Id }
                });
                var url = GetAirlyApiUrl(App.AirlyApiMeasurementUrl, query);
                var response = await GetHttpResponseAsync<Measurement>(url);
                installation.Measurement = response;
                installationsUpdated.Add(installation);
            }
            return installationsUpdated;
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
            client.DefaultRequestHeaders.Add("Accept-Language", "pl");
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
                    Remaining = dayLimitRemaining.ToArray()[0];
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
        public ICommand GoToDetailsCommand => _goToDetailsCommand ?? (_goToDetailsCommand = new Command<Installation>(installation => OnGoToDetails(installation)));

        private void OnGoToDetails(Installation installation)
        {
            _navigation.PushAsync(new DetailsPage(installation));
        }

        private ICommand _goToDetailsCommandFromMap;
        public ICommand GoToDetailsCommandFromMap => _goToDetailsCommandFromMap ?? (_goToDetailsCommandFromMap = new Command<string>(address => OnGoToDetailsFromMap(address)));

        private void OnGoToDetailsFromMap(string address)
        {
            var installation = Items.First(x => x.Address.Description == address);
            _navigation.PushAsync(new DetailsPage(installation));
        }
    }
}