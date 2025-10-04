using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http.Headers;
using System.Threading.Tasks;

[ApiController]
[Route("api/[controller]")]
public class OrderController : ControllerBase
{
    private readonly OrderRepository _repo;
    private readonly ILogger<OrderController> _logger;
    private readonly IConfiguration _config;
    public OrderController(OrderRepository repo, ILogger<OrderController> logger, IConfiguration config) { _repo = repo; _logger = logger; _config = config; }

    [HttpGet] public async Task<IActionResult> GetAll() => Ok(await _repo.GetAllAsync());
    [HttpGet("{id}")] public async Task<IActionResult> Get(string id) { var o = await _repo.GetByIdAsync(id); if (o == null) return NotFound(); return Ok(o); }
    [HttpPost] public async Task<IActionResult> Create([FromBody] Order o) { await _repo.CreateAsync(o); return CreatedAtAction(nameof(Get), new { id = o.Id }, o); }

    // Simple status update
    [HttpPost("{id}/status")] public async Task<IActionResult> UpdateStatus(string id, [FromBody] StatusDto dto) { await _repo.UpdateStatusAsync(id, dto.Status); return NoContent(); }

    // PayPal: Create order (server creates PayPal order - returns approval link)
    [HttpPost("{orderId}/create-paypal-order")]
    public async Task<IActionResult> CreatePaypalOrder(string orderId)
    {
        var clientId = _config["PayPal:ClientId"];
        var secret = _config["PayPal:Secret"];
        var baseUrl = _config["PayPal:ApiBase"] ?? "https://api-m.sandbox.paypal.com";

        // For simplicity, use HttpClient with Basic auth to get access token
        using var http = new HttpClient();
        var tokenRequest = new HttpRequestMessage(HttpMethod.Post, $"{baseUrl}/v1/oauth2/token");
        tokenRequest.Headers.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes($"{clientId}:{secret}")));
        tokenRequest.Content = new FormUrlEncodedContent(new[] { new KeyValuePair<string, string>("grant_type", "client_credentials") });

        var tokenResp = await http.SendAsync(tokenRequest);
        var tokenJson = await tokenResp.Content.ReadAsStringAsync();
        dynamic tokenObj = JsonConvert.DeserializeObject(tokenJson);
        string accessToken = tokenObj.access_token;

        // Create PayPal order payload based on local Order total
        var order = await _repo.GetByIdAsync(orderId);
        if (order == null) return NotFound("order not found");

        var createOrderReq = new
        {
            intent = "CAPTURE",
            purchase_units = new[] {
                new {
                    amount = new {
                        currency_code = "USD",
                        value = order.Total.ToString("F2")
                    }
                }
            },
            application_context = new
            {
                return_url = _config["App:ReturnUrl"] ?? "https://example.com/paypal/return",
                cancel_url = _config["App:CancelUrl"] ?? "https://example.com/paypal/cancel"
            }
        };

        var createReqMsg = new HttpRequestMessage(HttpMethod.Post, $"{baseUrl}/v2/checkout/orders");
        createReqMsg.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        createReqMsg.Content = new StringContent(JsonConvert.SerializeObject(createOrderReq), System.Text.Encoding.UTF8, "application/json");
        var createResp = await http.SendAsync(createReqMsg);
        var createJson = await createResp.Content.ReadAsStringAsync();
        return Content(createJson, "application/json");
    }

    // PayPal webhook endpoint - configure webhook in PayPal Sandbox to call this
    [HttpPost("paypal-webhook")]
    public async Task<IActionResult> PaypalWebhook()
    {
        var json = await new StreamReader(Request.Body).ReadToEndAsync();
        _logger.LogInformation("PayPal webhook received: {0}", json);

        // NOTE: validate webhook signature in production (PayPal provides verification endpoints/signature headers).
        dynamic ev = JsonConvert.DeserializeObject(json);
        // Example: capture completed -> update local order
        try
        {
            string eventType = ev.event_type;
            if (eventType == "CHECKOUT.ORDER.APPROVED" || eventType == "PAYMENT.CAPTURE.COMPLETED")
            {
                // Extract order id if you stored custom_id or map using reference
                // For demo, we skip mapping; in real usage include orderId in purchase_unit.reference_id
                _logger.LogInformation("Payment event {0}", eventType);
            }
        }
        catch (Exception ex) { _logger.LogError(ex, "Webhook processing error"); }

        return Ok();
    }
}

public record StatusDto(string Status);
