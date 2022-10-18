using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace PlantDiary2022FS001.Pages
{
    public class IndexModel : PageModel
    {
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
            int yearStarted = 2006;
            ViewData["Brand"] = brand + " Year Started " + yearStarted;
        }
    }
}