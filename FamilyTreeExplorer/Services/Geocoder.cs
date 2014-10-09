using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using RestSharp;

namespace FamilyTreeExplorer.Services
{
    public class Geocoder
    {
        object _locker = new object();
        public Geocoder(string filePath)
        {
            _filePath = filePath;
            if (File.Exists(_filePath))
            {
                var json = File.ReadAllText(_filePath);
                _cache = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, IEnumerable<GeocodePlace>>>(json);
            }
        }

        public Geocoder()
        {

        }

        RestClient client = new RestClient("http://open.mapquestapi.com/nominatim/v1/");

        Dictionary<string, IEnumerable<GeocodePlace>> _cache = new Dictionary<string, IEnumerable<GeocodePlace>>();
        private string _filePath;

        public IEnumerable<GeocodePlace> Lookup(string address)
        {
            if (!_cache.ContainsKey(address))
            {
                var request = new RestRequest("search.php", Method.GET);
                request.AddParameter("q", address);
                request.AddParameter("format", "json");

                var results = client.Execute(request);

                IEnumerable<GeocodePlace> places = Enumerable.Empty<GeocodePlace>();
                if (!string.IsNullOrWhiteSpace(results.Content))
                {
                    places = Newtonsoft.Json.JsonConvert.DeserializeObject<IEnumerable<GeocodePlace>>(results.Content);
                }

                lock (_locker)
                {
                    if (!_cache.ContainsKey(address))
                    {
                        _cache.Add(address, places);


                        if (!string.IsNullOrWhiteSpace(_filePath))
                        {
                            File.WriteAllText(_filePath, Newtonsoft.Json.JsonConvert.SerializeObject(_cache));
                        }
                    }
                }
            }

            return _cache[address];
        }

        public IReadOnlyDictionary<string, IEnumerable<GeocodePlace>> Lookup(IEnumerable<string> addresses)
        {
            int limitedMissesPerBatch = 10;
            int miss = 0;
            addresses = addresses.Distinct();
            Dictionary<string, IEnumerable<GeocodePlace>> returnValue = new Dictionary<string, IEnumerable<GeocodePlace>>();

            var added = false;
            foreach (var address in addresses)
            {
                if (miss >= limitedMissesPerBatch)
                    break;

                if (!_cache.ContainsKey(address))
                {
                    miss++;

                    var request = new RestRequest("search.php", Method.GET);
                    request.AddParameter("q", address);
                    request.AddParameter("format", "json");

                    var results = client.Execute(request);

                    if (!_cache.ContainsKey(address))
                    {
                        IEnumerable<GeocodePlace> places = Enumerable.Empty<GeocodePlace>();
                        if (!string.IsNullOrWhiteSpace(results.Content))
                        {
                            places = Newtonsoft.Json.JsonConvert.DeserializeObject<IEnumerable<GeocodePlace>>(results.Content);
                        }

                        lock (_locker)
                        {
                            if (!_cache.ContainsKey(address))
                            {
                                _cache.Add(address, places);
                                added = true;
                            }
                        }
                    }
                }

                returnValue.Add(address, _cache[address]);
            }

            if (added && !string.IsNullOrWhiteSpace(_filePath))
            {
                File.WriteAllText(_filePath, Newtonsoft.Json.JsonConvert.SerializeObject(_cache));
            }

            return returnValue;
        }
    }

    public class GeocodePlace
    {
        public GeocodePlace() { }

        [Newtonsoft.Json.JsonProperty("place_id")]
        public string Id { get; set; }

        public string Licence { get; set; }

        //public string osm_type { get; set; }
        //public string osm_id { get; set; }
        public string[] boundingbox { get; set; }

        [Newtonsoft.Json.JsonProperty("lat")]
        public float Latitude { get; set; }

        [Newtonsoft.Json.JsonProperty("lon")]
        public float Longitude { get; set; }

        [Newtonsoft.Json.JsonProperty("display_name")]
        public string Name { get; set; }
        //public string _class { get; set; }
        //public string type { get; set; }
        //public float importance { get; set; }
    }
}