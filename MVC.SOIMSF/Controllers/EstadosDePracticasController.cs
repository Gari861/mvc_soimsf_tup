using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using MVC.SOIMSF.Models;
using Microsoft.AspNetCore.Authorization;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace MVC.SOIMSF.Controllers
{
    [Authorize]
    public class EstadosDePracticasController : Controller
    {
        private readonly HttpClient _client;
        private readonly string _apiBaseUrl;

        public EstadosDePracticasController()
        {
            _client = new HttpClient();
            //_apiBaseUrl = "https://localhost:7086/api/EstadosDePracticas";
            _apiBaseUrl = "https://soimsf.somee.com/api/EstadosDePracticas";
        }

        // Método para agregar el token JWT al encabezado de autorización
        private void AddJwtToken()
        {
            var token = HttpContext.Session.GetString("JwtToken");
            if (!string.IsNullOrEmpty(token))
            {
                _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }
        }

        // GET: EstadosDePracticasController
        public async Task<ActionResult> Index(string? nombre)
        {
            AddJwtToken(); // Agregar token JWT

            var estadosList = new List<EstadosDePracticasViewModel>();

            // Validaciones
            if (nombre?.Length > 100)
            {
                ModelState.AddModelError("nombre", "El nombre no puede exceder los 100 caracteres.");
                return View(estadosList);
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
                estadosList = JsonConvert.DeserializeObject<List<EstadosDePracticasViewModel>>(data);
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

            return View(estadosList);
        }

        // GET: EstadosDePracticasController/Details/5
        public async Task<ActionResult> Details(int id)
        {
            AddJwtToken(); // Agregar token JWT

            EstadosDePracticasViewModel estado = null;
            HttpResponseMessage response = await _client.GetAsync($"{_apiBaseUrl}/{id}");

            if (response.IsSuccessStatusCode)
            {
                string data = await response.Content.ReadAsStringAsync();
                estado = JsonConvert.DeserializeObject<EstadosDePracticasViewModel>(data);
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                return RedirectToAction("Login", "Authentication");
            }


            if (estado == null)
            {
                return NotFound();
            }

            return View(estado);
        }

        // GET: EstadosDePracticasController/Create
        [Authorize(Roles = "Admin")]
        public ActionResult Create()
        {
            return View();
        }

        // POST: EstadosDePracticasController/Create
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(EstadosDePracticasViewModel estado)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    AddJwtToken(); // Agregar token JWT

                    string jsonData = JsonConvert.SerializeObject(estado);
                    StringContent content = new StringContent(jsonData, Encoding.UTF8, "application/json");

                    HttpResponseMessage response = await _client.PostAsync(_apiBaseUrl, content);

                    if (response.IsSuccessStatusCode)
                    {
                        TempData["AlertMessage"] = "¡Estado de Práctica creado con éxito!";
                        TempData["AlertType"] = "success";
                        return RedirectToAction(nameof(Index));
                    }
                    else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    {
                        return RedirectToAction("Login", "Authentication");
                    }

                }

                return View(estado);
            }
            catch
            {
                return View(estado);
            }
        }

        // GET: EstadosDePracticasController/Edit/5
        [Authorize(Roles = "Admin,Manager")]
        public async Task<ActionResult> Edit(int id)
        {
            AddJwtToken(); // Agregar token JWT

            EstadosDePracticasViewModel estado = null;
            HttpResponseMessage response = await _client.GetAsync($"{_apiBaseUrl}/{id}");

            if (response.IsSuccessStatusCode)
            {
                string data = await response.Content.ReadAsStringAsync();
                estado = JsonConvert.DeserializeObject<EstadosDePracticasViewModel>(data);
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                return RedirectToAction("Login", "Authentication");
            }

            if (estado == null)
            {
                return NotFound();
            }

            return View(estado);
        }

        // POST: EstadosDePracticasController/Edit/5
        [Authorize(Roles = "Admin,Manager")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(EstadosDePracticasViewModel estado)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    AddJwtToken(); // Agregar token JWT

                    string jsonData = JsonConvert.SerializeObject(estado);
                    StringContent content = new StringContent(jsonData, Encoding.UTF8, "application/json");

                    HttpResponseMessage response = await _client.PutAsync($"{_apiBaseUrl}/{estado.Id}", content);

                    if (response.IsSuccessStatusCode)
                    {
                        TempData["AlertMessage"] = "¡Estado de Práctica editado con éxito!";
                        TempData["AlertType"] = "warning";
                        return RedirectToAction(nameof(Index));
                    }
                    else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    {
                        return RedirectToAction("Login", "Authentication");
                    }

                }

                return View(estado);
            }
            catch
            {
                return View(estado);
            }
        }

        // GET: EstadosDePracticasController/Delete/5
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> Delete(int id)
        {
            AddJwtToken(); // Agregar token JWT

            EstadosDePracticasViewModel estado = null;
            HttpResponseMessage response = await _client.GetAsync($"{_apiBaseUrl}/{id}");

            if (response.IsSuccessStatusCode)
            {
                string data = await response.Content.ReadAsStringAsync();
                estado = JsonConvert.DeserializeObject<EstadosDePracticasViewModel>(data);
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                return RedirectToAction("Login", "Authentication");
            }


            return View(estado);
        }

        // POST: EstadosDePracticasController/Delete/5
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Delete(int id, IFormCollection collection)
        {
            try
            {
                AddJwtToken(); // Agregar token JWT

                HttpResponseMessage response = await _client.DeleteAsync($"{_apiBaseUrl}/{id}");

                if (response.IsSuccessStatusCode)
                {
                    TempData["AlertMessage"] = "¡Estado de Práctica eliminado con éxito!";
                    TempData["AlertType"] = "danger"; // Clase para la alerta de eliminación (Bootstrap)
                    return RedirectToAction(nameof(Index));
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    return RedirectToAction("Login", "Authentication");
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