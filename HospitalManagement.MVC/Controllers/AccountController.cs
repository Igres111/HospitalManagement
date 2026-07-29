using HospitalManagement.MVC.Auth;
using HospitalManagement.MVC.Dtos.Requests;
using HospitalManagement.MVC.Dtos.Responses;
using HospitalManagement.MVC.Models.Account;
using HospitalManagement.MVC.Services.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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

            var result = await _apiClient.PostAsync<AuthRequest, AuthResponse>(
                "api/auth/login",
                new AuthRequest(model.Username, model.Password),
                cancellationToken);

            if (result.IsError)
            {
                if (result.StatusCode == 401)
                {
                    ModelState.AddModelError(nameof(model.Password), "Wrong password or username.");
                    return View(model);
                }

                TempData["ErrorStatusCode"] = result.StatusCode;
                TempData["ErrorMessage"] = result.ErrorMessage;
                return RedirectToAction("Error", "Home");
            }

            var principal = ClaimsPrincipalFactory.BuildPrincipal(result.Value!);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

            if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
            {
                return Redirect(model.ReturnUrl);
            }

            return RedirectToAction("Index", "Appointments");
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View(new RegisterViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var result = await _apiClient.PostAsync<RegisterViewModel, RegisterResponse>(
                "api/auth/register",
                model,
                cancellationToken);

            if (result.IsError)
            {
                if (result.StatusCode == 409)
                {
                    ModelState.AddModelError(nameof(model.Username), result.ErrorMessage!);
                    return View(model);
                }

                TempData["ErrorStatusCode"] = result.StatusCode;
                TempData["ErrorMessage"] = result.ErrorMessage;
                return RedirectToAction("Error", "Home");
            }

            TempData["SuccessMessage"] = "Registration successful. You can now log in.";

            return RedirectToAction(nameof(Login));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout(CancellationToken cancellationToken)
        {
            var refreshToken = User.FindFirst(AppClaimTypes.RefreshToken)?.Value;

            if (!string.IsNullOrEmpty(refreshToken))
            {
                await _apiClient.PostAsync("api/auth/logout", new RefreshTokenRequest(refreshToken), cancellationToken);
            }

            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            return RedirectToAction(nameof(Login));
        }

    }
}
