using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Windows.Data.Json;
using YellowBuses_Sketch.App.PresentationModel;
using YellowBuses_Sketch.App.Repositories;

namespace YellowBuses_Sketch.App.DataModel
{
    public sealed class StopsDataSource
    {
        private static readonly StopsDataSource _stopsDataSource = new StopsDataSource();
        private static IRepository<Direction> _repository = new XmlRepository<Direction>("Stops.xml");

        private readonly ObservableCollection<Direction> _directions = new ObservableCollection<Direction>();

        public ObservableCollection<Direction> Directions => _directions;

        public static IEnumerable<Direction> GetDirectionsAsync()
        {
            return _repository.OrderByDescending(x => x.Name);
        }

        public static Direction GetDirectionAsync(string name)
        {
            var matches = _repository.Where(x => x.Name.Equals(name));

            return matches.Count() == 1 ? matches.First() : null;
        }

        public static async Task<IEnumerable<Stop>> GetStopsAsync()
        {
            var uri = "https://www.bybus.co.uk/api/v2/jp/locationsDatalist.cache.js";

            using (var client = new HttpClient())
            {
                var result = await client
                    .GetAsync(uri)
                    .Result
                    .Content
                    .ReadAsStringAsync();
                var t =  result.Skip(58);
                var f = string.Join("", t.Take(t.Count() - 59));

                return f.Split('|').Select(x => new Stop() {Name = x});
            }
        }

        public static IEnumerable<Stop> GetStopDetails(string stop)
        {
            using (var client = new HttpClient())
            {
                var jsonText = client.GetAsync($"https://www.bybus.co.uk/api/v2/jp/stopSearch.json?searchQuery={stop}").Result.Content.ReadAsStringAsync().Result;
                var stopObject = JsonObject.Parse(jsonText);
                var list = new List<Stop>();

                foreach (JsonValue item in stopObject["data"].GetArray())
                {
                    var o = item.GetObject();
                    var stopItem = new Stop
                    {
                        Name = o["title"].GetString(),
                        StopId = o["id"].GetString(),
                        StopCode = o["atcoCode"].GetString(),
                        NaptanCode = o["naptanCode"].ValueType == JsonValueType.Null ? string.Empty : o["naptanCode"].GetString()
                    };
                    list.Add(stopItem);

                    Debug.WriteLine($"Title: {stopItem.Name}; Id: {stopItem.StopId}; Code: {stopItem.StopCode}; NaptanCode: {stopItem.NaptanCode}");
                }

                return list;
            }
        }

        public static async Task<StopSchedule> GetStopScheduleByIdAsync(string id)
        {
            var stop = _repository.SelectMany(d => d.Stops).Where(s => s.StopId.Equals(id)).First();

            Debug.WriteLine($"Name: {stop.Name}; Id: {stop.StopId}; Code: {stop.StopCode}");

            using (var client = new HttpClient())
            {
                var jsonText = await client.GetAsync($"https://www.bybus.co.uk/api/v2/jp/stopDepartures.json?stopId={stop.StopId}&stopCode={stop.StopCode}").Result.Content.ReadAsStringAsync();
                var stopObject = JsonObject.Parse(jsonText);
                Debug.WriteLine(jsonText);
                var jsonArray = stopObject["data"].GetArray();

                var routes = new ObservableCollection<Tuple<string, string>>();

                foreach (var routeValue in jsonArray)
                {
                    var routeObject = routeValue.GetObject();
                    routes.Add(new Tuple<string, string>(routeObject["route"].GetString(), routeObject["time"].GetString()));
                }

                return new StopSchedule()
                {
                    Name = stop.Name,
                    Id = id,
                    Routes = routes
                };
            }
        }

        public static void AddStop(string direction, Stop stop)
        {
            var d = _repository.Find(x => x.Name == direction).FirstOrDefault();

            if (d != null)
            {
                d.Stops.Add(new StopEntity(stop.Name, direction, stop.StopId, stop.StopCode));
                _repository.Update(d);
            }
            else
            {
                var dir = new Direction(direction);
                dir.Stops.Add(new StopEntity(stop.Name, dir.Name, stop.StopId, stop.StopCode));

                _repository.Add(dir);
            }
        }

        public static void RemoveStop(string direction, string stop)
        {
            var d = _repository.Find(x => x.Name == direction).First();
            var s = d.Stops.Single(x => x.StopId == stop);
            d.Stops.Remove(s);

            _repository.Update(d);
        }
    }
}