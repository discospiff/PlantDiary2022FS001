using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PlantPlacesSpecimens;

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
            if (inBrand != null && inBrand.Length >0)
            {
                brand = inBrand;
            }
            ViewData["Brand"] = brand;

            var task = client.GetAsync("http://plantplaces.com/perl/mobile/specimenlocations.pl?Lat=39.14455&Lng=-84.50939&Range=0.5&Source=location");
            HttpResponseMessage result = task.Result; 
            result.EnsureSuccessStatusCode();
            Task<string> readString = result.Content.ReadAsStringAsync();
            string jsonString = readString.Result;
            List<Specimen> specimens = Specimen.FromJson(jsonString);
            ViewData["Specimens"] = specimens;
        }
    }
}