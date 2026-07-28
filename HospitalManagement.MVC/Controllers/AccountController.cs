using HospitalManagement.MVC.Auth;
using HospitalManagement.MVC.Dtos.Requests;
using HospitalManagement.MVC.Dtos.Responses;
using HospitalManagement.MVC.Models.Account;
using HospitalManagement.MVC.Services.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace HospitalManagement.MVC.Controllers
{

    [AllowAnonymous]
    public class AccountController : Controller
    {
        private readonly IApiClient _apiClient;

        public AccountController(IApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            return View(new LoginViewModel { ReturnUrl = returnUrl });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var loginResponse = await _apiClient.PostAsync<AuthRequest, AuthResponse>(
                "api/auth/login",
                new AuthRequest(model.Username, model.Password),
                cancellationToken);

            var principal = BuildPrincipal(loginResponse);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

            if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
            {
                return Redirect(model.ReturnUrl);
            }

            return RedirectToAction("Index", "Appointments");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            return RedirectToAction(nameof(Login));
        }

        private static ClaimsPrincipal BuildPrincipal(AuthResponse loginResponse)
        {
            var claims = ExtractClaimsFromToken(loginResponse.AccessToken);

            claims.Add(new Claim(AppClaimTypes.AccessToken, loginResponse.AccessToken));
            claims.Add(new Claim(AppClaimTypes.RefreshToken, loginResponse.RefreshToken));

            var identity = new ClaimsIdentity(
                claims,
                CookieAuthenticationDefaults.AuthenticationScheme,
                ClaimTypes.Name,
                ClaimTypes.Role);

            return new ClaimsPrincipal(identity);
        }

        private static List<Claim> ExtractClaimsFromToken(string accessToken)
        {
            var payloadSegment = accessToken.Split('.')[1];

            var payloadJson = Encoding.UTF8.GetString(DecodeBase64Url(payloadSegment));

            var payload = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(payloadJson) ?? throw new InvalidOperationException("Unable to read the access token.");

            return payload
                .Select(kvp => new Claim(
                    kvp.Key,
                    kvp.Value.ValueKind == JsonValueKind.String ? kvp.Value.GetString()! : kvp.Value.GetRawText()))
                .ToList();
        }

        private static byte[] DecodeBase64Url(string value)
        {
            var base64 = value.Replace('-', '+').Replace('_', '/');

            base64 = base64.PadRight(base64.Length + (4 - base64.Length % 4) % 4, '=');

            return Convert.FromBase64String(base64);
        }
    }
}