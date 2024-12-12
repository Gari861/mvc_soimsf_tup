using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using MVC.SOIMSF.Models;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;

namespace MVC.SOIMSF.Controllers
{
    public class AuthenticationController : Controller
    {
        private readonly HttpClient _client;
        private readonly string _apiBaseUrl;

        public AuthenticationController()
        {
            _client = new HttpClient();
            //_apiBaseUrl = "https://localhost:7086/api/Authentication";
            _apiBaseUrl = "https://soimsf.somee.com/api/Authentication";
        }

        // GET: AuthenticationController/Login
        public ActionResult Login(string returnUrl = null)
        {
            TempData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(UsersViewModel model, string returnUrl = null)
        {
            if (!ModelState.IsValid)
            {
                TempData["AlertMessage"] = "Por favor, completa los campos correctamente.";
                TempData["AlertType"] = "danger";
                TempData["ShowModal"] = "false";
                return View(model);
            }

            // Crear la solicitud al API para autenticación
            string jsonData = JsonConvert.SerializeObject(model);
            StringContent content = new StringContent(jsonData, Encoding.UTF8, "application/json");
            HttpResponseMessage response = await _client.PostAsync($"{_apiBaseUrl}/login", content);

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var responseObject = JsonConvert.DeserializeObject<dynamic>(responseContent);

                // Guarda el token JWT en la sesión o una cookie segura
                HttpContext.Session.SetString("JwtToken", (string)responseObject.token);

                // Configura los claims (puedes mantener autenticación basada en cookies para MVC)
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, (string)responseObject.email)
                };

                foreach (var role in responseObject.roles)
                {
                    claims.Add(new Claim(ClaimTypes.Role, (string)role));
                }

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, claimsPrincipal);

                TempData["AlertMessage"] = "Inicio de sesión exitoso.";
                TempData["AlertType"] = "success";
                TempData["ShowModal"] = "true"; // Para mostrar el modal de éxito
                TempData["UserEmail"] = (string)responseObject.email;

                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                {
                    return Redirect(returnUrl);
                }

                return RedirectToAction("Login");
            }
            else
            {
                TempData["AlertMessage"] = "Inicio de sesión fallido. Verifica tus credenciales.";
                TempData["AlertType"] = "danger";
                TempData["ShowModal"] = "false";
                return View(model);
            }
        }

        // POST: AuthenticationController/Logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            HttpResponseMessage response = await _client.PostAsync($"{_apiBaseUrl}/logout", null);

            TempData["AlertMessage"] = "Has cerrado sesión correctamente.";
            TempData["AlertType"] = "info";

            return RedirectToAction("Login");
        }

        public IActionResult AccessDenied()
        {
            TempData["AlertMessage"] = "No tienes permiso para acceder a esta página.";
            TempData["AlertType"] = "warning";
            return View();
        }
    }
}
