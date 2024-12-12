using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using MVC.SOIMSF.Models;
using Microsoft.AspNetCore.Authorization;

namespace MVC.SOIMSF.Controllers
{
    [Authorize]
    public class LocalidadesController : Controller
    {
        private readonly HttpClient _client;
        private readonly string _apiBaseUrl;

        public LocalidadesController()
        {
            _client = new HttpClient();
            //_apiBaseUrl = "https://localhost:7086/api/Localidades";
            _apiBaseUrl = "https://soimsf.somee.com/api/Localidades";
        }

        private void AddJwtToken()
        {
            var token = HttpContext.Session.GetString("JwtToken");
            if (!string.IsNullOrEmpty(token))
            {
                _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }
        }

        // GET: LocalidadesController
        public async Task<ActionResult> Index(string? nombre)
        {
            AddJwtToken(); // Agregar token JWT

            var localidadesList = new List<LocalidadesViewModel>();

            // Validaciones
            if (nombre?.Length > 100)
            {
                ModelState.AddModelError("nombre", "El nombre no puede exceder los 100 caracteres.");
                return View(localidadesList);
            }

            // Construir los parámetros de la consulta
            var queryParams = new Dictionary<string, string>();
            if (!string.IsNullOrEmpty(nombre))
                queryParams.Add("nombre", nombre);

            // Construir la URL con los filtros
            var queryString = string.Join("&", queryParams.Select(kvp => $"{kvp.Key}={Uri.EscapeDataString(kvp.Value)}"));
            var url = string.IsNullOrEmpty(queryString) ? _apiBaseUrl : $"{_apiBaseUrl}?{queryString}";

            HttpResponseMessage response = await _client.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                string data = await response.Content.ReadAsStringAsync();
                localidadesList = JsonConvert.DeserializeObject<List<LocalidadesViewModel>>(data);
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                return RedirectToAction("Login", "Authentication");
            }

            // Verificar si se aplicaron filtros
            if (!string.IsNullOrEmpty(nombre))
            {
                TempData["AlertMessage"] = "¡Filtros aplicados con éxito!";
                TempData["AlertType"] = "primary"; // Azul (Bootstrap)
            }

            return View(localidadesList);
        }

        // GET: LocalidadesController/Details/5
        public async Task<ActionResult> Details(int id)
        {
            AddJwtToken();
            LocalidadesViewModel localidad = null;
            HttpResponseMessage response = await _client.GetAsync($"{_apiBaseUrl}/{id}");

            if (response.IsSuccessStatusCode)
            {
                string data = await response.Content.ReadAsStringAsync();
                localidad = JsonConvert.DeserializeObject<LocalidadesViewModel>(data);
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                return RedirectToAction("Login", "Authentication");
            }

            if (localidad == null)
            {
                return NotFound();
            }

            return localidad == null ? NotFound() : View(localidad);
        }

        // GET: LocalidadesController/Create
        [Authorize(Roles = "Admin")]
        public ActionResult Create()
        {
            return View();
        }

        // POST: LocalidadesController/Create
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(LocalidadesViewModel localidad)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    AddJwtToken();
                    string jsonData = JsonConvert.SerializeObject(localidad);
                    StringContent content = new StringContent(jsonData, Encoding.UTF8, "application/json");

                    HttpResponseMessage response = await _client.PostAsync(_apiBaseUrl, content);

                    if (response.IsSuccessStatusCode)
                    {
                        TempData["AlertMessage"] = "¡Localidad creado con éxito!";
                        TempData["AlertType"] = "success";
                        return RedirectToAction(nameof(Index));
                    }
                }

                return View(localidad);
            }
            catch
            {
                return View(localidad);
            }
        }

        // GET: LocalidadesController/Edit/5
        [Authorize(Roles = "Admin,Manager")]
        public async Task<ActionResult> Edit(int id)
        {
            AddJwtToken(); // Agregar token JWT aquí

            LocalidadesViewModel localidad = null;
            HttpResponseMessage response = await _client.GetAsync($"{_apiBaseUrl}/{id}");

            if (response.IsSuccessStatusCode)
            {
                string data = await response.Content.ReadAsStringAsync();
                localidad = JsonConvert.DeserializeObject<LocalidadesViewModel>(data);
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                return RedirectToAction("Login", "Authentication");
            }


            return localidad == null ? NotFound() : View(localidad);

        }

        // POST: LocalidadesController/Edit/5
        [Authorize(Roles = "Admin,Manager")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(LocalidadesViewModel localidad)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    AddJwtToken();
                    string jsonData = JsonConvert.SerializeObject(localidad);
                    StringContent content = new StringContent(jsonData, Encoding.UTF8, "application/json");

                    HttpResponseMessage response = await _client.PutAsync($"{_apiBaseUrl}/{localidad.Id}", content);

                    if (response.IsSuccessStatusCode)
                    {
                        TempData["AlertMessage"] = "¡Localidad editado con éxito!";
                        TempData["AlertType"] = "warning";
                        return RedirectToAction(nameof(Index));
                    }
                    else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    {
                        return RedirectToAction("Login", "Authentication");
                    }

                }

                return View(localidad);
            }
            catch
            {
                return View(localidad);
            }
        }

        // GET: LocalidadesController/Delete/5
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> Delete(int id)
        {
            AddJwtToken(); // Agregar token JWT aquí

            LocalidadesViewModel localidad = null;
            HttpResponseMessage response = await _client.GetAsync($"{_apiBaseUrl}/{id}");

            if (response.IsSuccessStatusCode)
            {
                string data = await response.Content.ReadAsStringAsync();
                localidad = JsonConvert.DeserializeObject<LocalidadesViewModel>(data);
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                return RedirectToAction("Login", "Authentication");
            }


            return View(localidad);
        }

        // POST: LocalidadesController/Delete/5
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Delete(int id, IFormCollection collection)
        {
            try
            {
                AddJwtToken();
                HttpResponseMessage response = await _client.DeleteAsync($"{_apiBaseUrl}/{id}");

                if (response.IsSuccessStatusCode)
                {
                    TempData["AlertMessage"] = "¡Localidad eliminado con éxito!";
                    TempData["AlertType"] = "danger"; // Clase para la alerta de eliminación (Bootstrap)
                    return RedirectToAction(nameof(Index));
                }

                return View();
            }
            catch
            {
                return View();
            }
        }
    }
}
