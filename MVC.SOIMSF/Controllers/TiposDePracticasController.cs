using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using MVC.SOIMSF.Models;
using Microsoft.AspNetCore.Authorization;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace MVC.SOIMSF.Controllers
{
    [Authorize]
    public class TiposDePracticasController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiBaseUrl;

        public TiposDePracticasController()
        {
            _httpClient = new HttpClient();
            //_apiBaseUrl = "https://localhost:7086/api/TiposDePracticas"; // Cambiar URL según tu API
            _apiBaseUrl = "https://soimsf.somee.com/api/TiposDePracticas"; // Cambiar URL según tu API
        }

        // Método para agregar el token JWT al encabezado de autorización
        private void AddJwtToken()
        {
            var token = HttpContext.Session.GetString("JwtToken");
            if (!string.IsNullOrEmpty(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }
        }

        // GET: TiposDePracticas
        public async Task<ActionResult> Index(string? nombre)
        {
            AddJwtToken(); // Agregar token JWT

            var tiposList = new List<TiposDePracticasViewModel>();

            // Validaciones
            if (nombre?.Length > 100)
            {
                ModelState.AddModelError("nombre", "El nombre no puede exceder los 100 caracteres.");
                return View(tiposList);
            }

            // Construir los parámetros de la consulta
            var queryParams = new Dictionary<string, string>();
            if (!string.IsNullOrEmpty(nombre))
                queryParams.Add("nombre", nombre);

            // Construir la URL con los filtros
            var queryString = string.Join("&", queryParams.Select(kvp => $"{kvp.Key}={Uri.EscapeDataString(kvp.Value)}"));
            var url = string.IsNullOrEmpty(queryString) ? _apiBaseUrl : $"{_apiBaseUrl}?{queryString}";

            HttpResponseMessage response = await _httpClient.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                string data = await response.Content.ReadAsStringAsync();
                tiposList = JsonConvert.DeserializeObject<List<TiposDePracticasViewModel>>(data);
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

            return View(tiposList);
        }

        // GET: TiposDePracticas/Details/5
        public async Task<IActionResult> Details(int id)
        {
            AddJwtToken(); // Agregar token JWT

            TiposDePracticasViewModel tipo = null;
            HttpResponseMessage response = await _httpClient.GetAsync($"{_apiBaseUrl}/{id}");

            if (response.IsSuccessStatusCode)
            {
                string json = await response.Content.ReadAsStringAsync();
                tipo = JsonConvert.DeserializeObject<TiposDePracticasViewModel>(json);
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                return RedirectToAction("Login", "Authentication");
            }


            if (tipo == null)
            {
                return NotFound();
            }

            return View(tipo);
        }

        // GET: TiposDePracticas/Create
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: TiposDePracticas/Create
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TiposDePracticasViewModel model)
        {
            if (ModelState.IsValid)
            {
                AddJwtToken(); // Agregar token JWT

                string json = JsonConvert.SerializeObject(model);
                StringContent content = new StringContent(json, Encoding.UTF8, "application/json");
                HttpResponseMessage response = await _httpClient.PostAsync(_apiBaseUrl, content);

                if (response.IsSuccessStatusCode)
                {
                    TempData["AlertMessage"] = "¡Tipo de Práctica creado con éxito!";
                    TempData["AlertType"] = "success";
                    return RedirectToAction(nameof(Index));
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    return RedirectToAction("Login", "Authentication");
                }
            }
            return View(model);
        }

        // GET: TiposDePracticas/Edit/5
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> Edit(int id)
        {
            AddJwtToken(); // Agregar token JWT

            TiposDePracticasViewModel tipo = null;
            HttpResponseMessage response = await _httpClient.GetAsync($"{_apiBaseUrl}/{id}");

            if (response.IsSuccessStatusCode)
            {
                string json = await response.Content.ReadAsStringAsync();
                tipo = JsonConvert.DeserializeObject<TiposDePracticasViewModel>(json);
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                return RedirectToAction("Login", "Authentication");
            }


            if (tipo == null)
            {
                return NotFound();
            }

            return View(tipo);
        }

        // POST: TiposDePracticas/Edit/5
        [Authorize(Roles = "Admin,Manager")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, TiposDePracticasViewModel model)
        {
            if (id != model.Id)
            {
                return BadRequest();
            }

            if (ModelState.IsValid)
            {
                AddJwtToken(); // Agregar token JWT

                string json = JsonConvert.SerializeObject(model);
                StringContent content = new StringContent(json, Encoding.UTF8, "application/json");
                HttpResponseMessage response = await _httpClient.PutAsync($"{_apiBaseUrl}/{id}", content);

                if (response.IsSuccessStatusCode)
                {
                    TempData["AlertMessage"] = "¡Tipo de Práctica editado con éxito!";
                    TempData["AlertType"] = "warning";
                    return RedirectToAction(nameof(Index));
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    return RedirectToAction("Login", "Authentication");
                }

            }

            return View(model);
        }

        // GET: TiposDePracticas/Delete/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            AddJwtToken(); // Agregar token JWT

            TiposDePracticasViewModel tipo = null;
            HttpResponseMessage response = await _httpClient.GetAsync($"{_apiBaseUrl}/{id}");

            if (response.IsSuccessStatusCode)
            {
                string json = await response.Content.ReadAsStringAsync();
                tipo = JsonConvert.DeserializeObject<TiposDePracticasViewModel>(json);
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                return RedirectToAction("Login", "Authentication");
            }


            if (tipo == null)
            {
                return NotFound();
            }

            return View(tipo);
        }

        // POST: TiposDePracticas/Delete/5
        [Authorize(Roles = "Admin")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            AddJwtToken(); // Agregar token JWT

            HttpResponseMessage response = await _httpClient.DeleteAsync($"{_apiBaseUrl}/{id}");

            if (response.IsSuccessStatusCode)
            {
                TempData["AlertMessage"] = "¡Tipo de Práctica eliminado con éxito!";
                TempData["AlertType"] = "danger"; // Clase para la alerta de eliminación (Bootstrap)
                return RedirectToAction(nameof(Index));
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                return RedirectToAction("Login", "Authentication");
            }


            return BadRequest();
        }
    }
}
