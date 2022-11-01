using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PlantPlacesPlants;
using PlantPlacesSpecimens;
using PlantPlacesWeather;
using System.Xml.Serialization;

namespace PlantDiary2022FS001.Pages
{
    public class IndexModel : PageModel
    {
        static readonly HttpClient client = new HttpClient();

        private readonly ILogger<IndexModel> _logger;

        public IndexModel(ILogger<IndexModel> logger)
        {
            _logger = logger;
        }

        public void OnGet()
        {
            string inBrand = Request.Query["Brand"];
            string brand = "My Plant Diary";
            if (inBrand != null && inBrand.Length > 0)
            {
                brand = inBrand;
            }
            ViewData["Brand"] = brand;

            Task<List<Specimen>> task = GetData();
            List<Specimen> waterLovingSpecimens = task.Result;
            ViewData["Specimens"] = waterLovingSpecimens;

        }

        private async Task<List<Specimen>> GetData()
        {
            return await Task.Run(async () =>
            {
                // get plant specimens at this location.
                var task = client.GetAsync("http://plantplaces.com/perl/mobile/specimenlocations.pl?Lat=39.14455&Lng=-84.50939&Range=0.5&Source=location");
                HttpResponseMessage result = task.Result;
                result.EnsureSuccessStatusCode();
                Task<string> readString = result.Content.ReadAsStringAsync();
                string jsonString = readString.Result;
                List<Specimen> specimens = Specimen.FromJson(jsonString);


                // get the data for water loving plants.
                Task<HttpResponseMessage> plantTask = client.GetAsync("http://plantplaces.com/perl/mobile/viewplantsjsonarray.pl?WetTolerant=on");
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
                

                // grab our API key from THE SECRET STORE
                var config = new ConfigurationBuilder()
                    .AddUserSecrets<Program>()
                    .Build();

                string apiKey = config["weatherapikey"];

                // read in weather data for our locale.
                String weatherEndpoint = "https://api.weatherbit.io/v2.0/current?&city=Cincinnati&country=USA&key=" + apiKey;
                Task<HttpResponseMessage> weatherTask = client.GetAsync(weatherEndpoint);
                HttpResponseMessage weatherResult = await weatherTask;
                Task<string> weatherTaskString = weatherResult.Content.ReadAsStringAsync();
                string weatherJson = weatherTaskString.Result;

                double precip = 0;
                Weather weather = Weather.FromJson(weatherJson);
                foreach (Datum weatherDatum in weather.Data)
                {
                    precip = weatherDatum.Precip;
                }
                if (precip < 0.5)
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