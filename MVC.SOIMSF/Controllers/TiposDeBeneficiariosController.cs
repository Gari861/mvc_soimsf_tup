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
    public class TiposDeBeneficiariosController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiBaseUrl;

        public TiposDeBeneficiariosController()
        {
            _httpClient = new HttpClient();
            //_apiBaseUrl = "https://localhost:7086/api/TiposDeBeneficiarios";
            _apiBaseUrl = "https://soimsf.somee.com/api/TiposDeBeneficiarios";
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

        // GET: TiposDeBeneficiariosController
        public async Task<ActionResult> Index(string? nombre)
        {
            AddJwtToken(); // Agregar token JWT

            var tiposList = new List<TiposDeBeneficiariosViewModel>();

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
                tiposList = JsonConvert.DeserializeObject<List<TiposDeBeneficiariosViewModel>>(data);
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

        // GET: TiposDeBeneficiariosController/Details/5
        public async Task<ActionResult> Details(int id)
        {
            AddJwtToken(); // Agregar token JWT

            TiposDeBeneficiariosViewModel tipo = null;
            HttpResponseMessage response = await _httpClient.GetAsync($"{_apiBaseUrl}/{id}");

            if (response.IsSuccessStatusCode)
            {
                string data = await response.Content.ReadAsStringAsync();
                tipo = JsonConvert.DeserializeObject<TiposDeBeneficiariosViewModel>(data);
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


        // GET: TiposDeBeneficiariosController/Create
        [Authorize(Roles = "Admin")]
        public ActionResult Create()
        {
            return View();
        }

        // POST: TiposDeBeneficiariosController/Create
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(TiposDeBeneficiariosViewModel model)
        {
            if (ModelState.IsValid)
            {
                AddJwtToken(); // Agregar token JWT
                
                var jsonData = JsonConvert.SerializeObject(model);
                var content = new StringContent(jsonData, Encoding.UTF8, "application/json");
                HttpResponseMessage response = await _httpClient.PostAsync(_apiBaseUrl, content);

                if (response.IsSuccessStatusCode)
                {
                    TempData["AlertMessage"] = "¡Tipo de Beneficiario creado con éxito!";
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

        // GET: TiposDeBeneficiariosController/Edit/5
        [Authorize(Roles = "Admin,Manager")]
        public async Task<ActionResult> Edit(int id)
        {
            AddJwtToken(); // Agregar token JWT

            TiposDeBeneficiariosViewModel tipo = null;
            HttpResponseMessage response = await _httpClient.GetAsync($"{_apiBaseUrl}/{id}");

            if (response.IsSuccessStatusCode)
            {
                string json = await response.Content.ReadAsStringAsync();
                tipo = JsonConvert.DeserializeObject<TiposDeBeneficiariosViewModel>(json);
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

        // POST: TiposDeBeneficiariosController/Edit/5
        [Authorize(Roles = "Admin,Manager")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(int id, TiposDeBeneficiariosViewModel model)
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
                    TempData["AlertMessage"] = "¡Tipo de Beneficiario editado con éxito!";
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

        // GET: TiposDeBeneficiariosController/Delete/5
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> Delete(int id)
        {
            AddJwtToken(); // Agregar token JWT

            var response = await _httpClient.GetAsync($"{_apiBaseUrl}/{id}");
            if (!response.IsSuccessStatusCode)
            {
                return View("Error", "No se encontró el tipo de beneficiario.");
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                return RedirectToAction("Login", "Authentication");
            }


            var jsonData = await response.Content.ReadAsStringAsync();
            var tipo = JsonConvert.DeserializeObject<TiposDeBeneficiariosViewModel>(jsonData);
            return View(tipo);
        }

        // POST: TiposDeBeneficiariosController/Delete/5
        [Authorize(Roles = "Admin")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            AddJwtToken(); // Agregar token JWT

            HttpResponseMessage response = await _httpClient.DeleteAsync($"{_apiBaseUrl}/{id}");

            if (response.IsSuccessStatusCode)
            {
                TempData["AlertMessage"] = "¡Tipo de Beneficiario eliminado con éxito!";
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
