using QuanLyDatVeMayBay.Models.Entities;
using System.Text;
using System.Net.Http.Headers;
using System.Text.Json;

namespace QuanLyDatVeMayBay.Services.PaypalService
{
    public interface IPaypal
    {
        Task<string> TaoDonHang(decimal amount, string state);
        Task<bool> CaptureOrder(string orderId);
    }
    public class Paypal : IPaypal
    {
        private readonly ThinhContext _context;
        private readonly HttpClient _httpClient;
        private string ClientId;
        private string Secret;
        private string baseUrl;
        public Paypal(ThinhContext thinhContext, HttpClient client)
        {
            _context = thinhContext;
            _httpClient = client;
            var setting = _context.Paypalsettings.FirstOrDefault();
            ClientId = setting.ClientId;
            Secret = setting.SecretId;
            baseUrl = setting.BaseUrl;
        }
        public async Task<string> GetAccessToken()
        {
            var byteArr = Encoding.UTF8.GetBytes($"{ClientId}:{Secret}");
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArr));
            var body = new Dictionary<string, string>
            {
                { "grant_type", "client_credentials" }
            };
            var respone = await _httpClient.PostAsync($"{baseUrl}/v1/oauth2/token", new FormUrlEncodedContent(body));
            var json = await respone.Content.ReadAsStringAsync();
            var token = JsonSerializer.Deserialize<JsonElement>(json).GetProperty("access_token").GetString();
            return token!;
        }
        public async Task<string> TaoDonHang(decimal amount,string state)
        {
            var token = await GetAccessToken();
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var body = new
            {
                intent = "CAPTURE",
                purchase_units = new[]
                {
                    new
                    {
                        amount = new
                        {
                            currency_code = "USD",
                            value = amount.ToString("F2")
                        },
                        description = "Payment test order"
                    }
                },
                application_context = new
                {
                    return_url = $"https://audrina-subultimate-ghostily.ngrok-free.dev/api/PayPal/capture-order?state={state}",
                    cancel_url = "https://audrina-subultimate-ghostily.ngrok-free.dev/api/PayPal/cancel-order?"
                }
            };
            var content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");
            var respone = await _httpClient.PostAsync($"{baseUrl}/v2/checkout/orders", content);
            var json = await respone.Content.ReadAsStringAsync();

            var order = JsonSerializer.Deserialize<JsonElement>(json);
            var approveLink = order.GetProperty("links").EnumerateArray()
                .FirstOrDefault(x => x.GetProperty("rel").GetString() == "approve").GetProperty("href").GetString();
            return approveLink!;
        }

        public async Task<bool> CaptureOrder(string orderId)
        {
            var token = await GetAccessToken();
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            // tạo request 
            var request = new HttpRequestMessage(HttpMethod.Post, $"{baseUrl}/v2/checkout/orders/{orderId}/capture"); 
            request.Content = new StringContent("", Encoding.UTF8, "application/json");
            
            var respone = await _httpClient.SendAsync(request);
            string result = await respone.Content.ReadAsStringAsync();
            return respone.IsSuccessStatusCode;

        }
    }
}
