using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using PlantPlacesPlants;
using PlantPlacesSpecimens;
using PlantPlacesWeather;
using System.Xml.Serialization;

namespace PlantDiary2022FS001.Pages
{
    public class IndexModel : PageModel
    {
        private const double PRECIPITAION_THRESHOLD = 0.5;
        static readonly HttpClient client = new HttpClient();

        private readonly ILogger<IndexModel> _logger;

        public IndexModel(ILogger<IndexModel> logger)
        {
            _logger = logger;
        }

        public void OnGet()
        {
            GenerateBrand();

            Task<List<Specimen>> task = GetData();
            List<Specimen> waterLovingSpecimens = task.Result;
            ViewData["Specimens"] = waterLovingSpecimens;

        }

        private void GenerateBrand()
        {
            string inBrand = Request.Query["Brand"];
            string brand = "My Plant Diary";
            if (inBrand != null && inBrand.Length > 0)
            {
                brand = inBrand;
            }
            ViewData["Brand"] = brand;
        }

        private async Task<List<Specimen>> GetData()
        {
            return await Task.Run(async () =>
            {
                // get plant specimens at this location.
                var task = client.GetAsync("http://plantplaces.com/perl/mobile/specimenlocations.pl?Lat=39.14455&Lng=-84.50939&Range=0.5&Source=location");
                Task<HttpResponseMessage> plantTask = client.GetAsync("http://plantplaces.com/perl/mobile/viewplantsjsonarray.pl?WetTolerant=on");

                // grab our API key from THE SECRET STORE
                var config = new ConfigurationBuilder()
                    .AddUserSecrets<Program>()
                    .Build();

                string apiKey = config["weatherapikey"];

                String weatherEndpoint = "https://api.weatherbit.io/v2.0/current?&city=Cincinnati&country=USA&key=" + apiKey;
                Task<HttpResponseMessage> weatherTask = client.GetAsync(weatherEndpoint);

                HttpResponseMessage result = task.Result;
                result.EnsureSuccessStatusCode();
                Task<string> readString = result.Content.ReadAsStringAsync();
                string jsonString = readString.Result;

                // read in the schema for specimens, so we can validate them.
                string specimenSchema = System.IO.File.ReadAllText("specimen-schema.json");
                JSchema schema = JSchema.Parse(specimenSchema);

                // Parse our JSON against the schema.
                JArray jsonObject = JArray.Parse(jsonString);

                // this collection will hold any errors that occur when parsing the JSON against the schema.
                IList<string> validationEvents  = new List<string>();

                List<Specimen> specimens = new List<Specimen>();
                if (jsonObject.IsValid(schema, out validationEvents))
                {
                  specimens = Specimen.FromJson(jsonString);
                } 
                else
                {
                    foreach (var validationEvent in validationEvents)
                    {
                        ViewData["Error"] = ViewData["Error"] + validationEvent;
                    }
                    return specimens;
                }

                // get the data for water loving plants.
                HttpResponseMessage plantResult = plantTask.Result;
                Task<string> plantTaskString = plantResult.Content.ReadAsStringAsync();
                string plantJson = plantTaskString.Result;
                List<Plant> plants = Plant.FromJson(plantJson);

                // Combine our data together.
                IDictionary<long, Plant> waterLovingPlants = new Dictionary<long, Plant>();
                foreach (Plant plant in plants)
                {
                    waterLovingPlants[plant.Id] = plant;
                }
                List<Specimen> waterLovingSpecimens = new List<Specimen>();
                foreach (Specimen specimen in specimens)
                {
                    if (waterLovingPlants.ContainsKey(specimen.PlantId))
                    {
                        // if we're here, we have a specimen that likes water
                        waterLovingSpecimens.Add(specimen);
                    }
                }

                // read in weather data for our locale.
                HttpResponseMessage weatherResult = await weatherTask;
                Task<string> weatherTaskString = weatherResult.Content.ReadAsStringAsync();
                string weatherJson = weatherTaskString.Result;

                double precip = 0;
                Weather weather = Weather.FromJson(weatherJson);
                foreach (Datum weatherDatum in weather.Data)
                { 
                    precip = weatherDatum.Precip;
                 
                }
                if (precip < PRECIPITAION_THRESHOLD)
                {
                    ViewData["Message"] = "It's dry!  Water plants.";
                }
                else
                {
                    ViewData["Message"] = "Rain expected.  No need to water.";
                }

                return waterLovingSpecimens;
            });
        }
    }
}