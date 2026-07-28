using HospitalManagement.MVC.Dtos.Responses;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace HospitalManagement.MVC.Auth
{
    public static class ClaimsPrincipalFactory
    {
        public static ClaimsPrincipal BuildPrincipal(AuthResponse loginResponse)
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
