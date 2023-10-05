using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text;
using System.Text.Json;

namespace CalculatorFrontend.Pages
{
    [IgnoreAntiforgeryToken(Order = 1001)]
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private readonly IHttpClientFactory _httpClientFactory;

        [BindProperty]
        public int val1 { get; set; }

        [BindProperty]
        public int val2 { get; set; }

        [BindProperty]
        public string Operation { get; set; }
        public IEnumerable<HistoryItem> HistoryItems { get; set; } = new List<HistoryItem>();
        //  public string CalculationResult { get; set; }


        public IndexModel(ILogger<IndexModel> logger, IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
        }
        [IgnoreAntiforgeryToken(Order = 1001)]
        public async Task OnGetAsync()
        {
            HistoryItems = await GetHistory();
        }
        [IgnoreAntiforgeryToken(Order = 1001)]

        public async Task<IActionResult> OnPostAsync()
        {


            var httpClient = _httpClientFactory.CreateClient("Client");
            var url = Operation == "+" ? "http://calc-service/addition" : "http://calc-service/subtraction";

            var payload = new { val1, val2 };
            try
            {
                var response = await httpClient.PostAsync(url, new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json"));
                var responseObject = response.Content.ReadFromJsonAsync<HistoryItem>().Result;
                ViewData["CalculationResult"] = $"Result: {responseObject.result}";
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                _logger.LogCritical(e, "Critical error!");
            }

            HistoryItems = await GetHistory();
            return Page();
        }
        [IgnoreAntiforgeryToken(Order = 1001)]
        private async Task<IEnumerable<HistoryItem>> GetHistory()
        {
            var httpClient = _httpClientFactory.CreateClient("Client");
            var response = await httpClient.GetAsync("http://calc-service/");


            try
            {
                if (response.IsSuccessStatusCode)
                {
                    _logger.LogDebug("Sucess");
                    var content = await response.Content.ReadAsStringAsync();
                    _logger.LogDebug(content);
                    HistoryItems = JsonSerializer.Deserialize<List<HistoryItem>>(content);
                    return HistoryItems;
                }
                else
                {
                    // Log or handle the error accordingly.
                    _logger.LogError($"Failed to fetch history: {response.ReasonPhrase}");
                    return HistoryItems;
                }
            }
            catch (Exception e)
            {

                _logger.LogError(e, $"Caught an error");
            }
            return HistoryItems;


            /*
                        var content = await response.Content.ReadAsStringAsync();
                        Console.Write(content);
                        return JsonSerializer.Deserialize<List<HistoryItem>>(content);*/
        }
        [IgnoreAntiforgeryToken(Order = 1001)]
        public class HistoryItem
        {
            public string id { get; set; }
            public string operation { get; set; }
            public int val1 { get; set; }
            public int val2 { get; set; }
            public int result { get; set; }
        }
    }
}