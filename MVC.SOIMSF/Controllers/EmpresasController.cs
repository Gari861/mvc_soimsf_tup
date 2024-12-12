using Microsoft.AspNetCore.Mvc;
using MVC.SOIMSF.Models;
using Newtonsoft.Json;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Authorization;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace MVC.SOIMSF.Controllers
{
    [Authorize]
    public class EmpresasController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiBaseUrl;

        public EmpresasController()
        {
            _httpClient = new HttpClient();
            ////_apiBaseUrl = "https://localhost:7086/api/Empresas";
            _apiBaseUrl = "https://soimsf.somee.com/api/Empresas";
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

        // GET: Empresas
        public async Task<IActionResult> Index(string? nombre, string? cuit, int? idLocalidad)
        {
            AddJwtToken(); // Agregar token JWT

            var empresasList = new List<EmpresasViewModel>();
            var localidadesList = new List<LocalidadesViewModel>();

            // Validaciones
            if (nombre?.Length > 100)
            {
                ModelState.AddModelError("nombre", "El nombre no puede exceder los 100 caracteres.");
                return View(empresasList);
            }
            if (cuit?.Length > 100)
            {
                ModelState.AddModelError("cuit", "El cuit no puede exceder los 100 caracteres.");
                return View(empresasList);
            }
            if (idLocalidad.HasValue && idLocalidad <= 0)
            {
                ModelState.AddModelError("idLocalidad", "El ID de la localidad debe ser un número positivo.");
                return View(empresasList);
            }

            // Construir los parámetros de la consulta
            var queryParams = new Dictionary<string, string>();
            if (!string.IsNullOrEmpty(nombre))
                queryParams.Add("nombre", nombre);
            if (!string.IsNullOrEmpty(cuit))
                queryParams.Add("cuit", cuit);
            if (idLocalidad.HasValue)
                queryParams.Add("idLocalidad", idLocalidad.Value.ToString());

            // Construir la URL con los filtros
            var queryString = string.Join("&", queryParams.Select(kvp => $"{kvp.Key}={Uri.EscapeDataString(kvp.Value)}"));
            var url = string.IsNullOrEmpty(queryString) ? _apiBaseUrl : $"{_apiBaseUrl}?{queryString}";

            HttpResponseMessage response = await _httpClient.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                string data = await response.Content.ReadAsStringAsync();
                empresasList = JsonConvert.DeserializeObject<List<EmpresasViewModel>>(data);
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                return RedirectToAction("Login", "Authentication");
            }

            // Obtener la lista de Localidades
            //HttpResponseMessage estadosResponse = await _httpClient.GetAsync("https://localhost:7086/api/Localidades");
            HttpResponseMessage estadosResponse = await _httpClient.GetAsync("https://soimsf.somee.com/api/Localidades");
            if (estadosResponse.IsSuccessStatusCode)
            {
                string data = await estadosResponse.Content.ReadAsStringAsync();
                localidadesList = JsonConvert.DeserializeObject<List<LocalidadesViewModel>>(data);
            }
            ViewBag.Localidades = localidadesList.ToDictionary(s => s.Id, s => s.Nombre);

            // Verificar si se aplicaron filtros
            if (!string.IsNullOrEmpty(nombre) || !string.IsNullOrEmpty(cuit) || idLocalidad.HasValue)
            {
                TempData["AlertMessage"] = "¡Filtros aplicados con éxito!";
                TempData["AlertType"] = "primary"; // Azul (Bootstrap)
            }

            return View(empresasList);
        }

        // GET: Empresas/Details/5
        public async Task<ActionResult> Details(int id)
        {
            AddJwtToken(); // Agregar token JWT

            EmpresasViewModel empresa = null;
            var localidadesList = new List<LocalidadesViewModel>();

            HttpResponseMessage response = await _httpClient.GetAsync($"{_apiBaseUrl}/{id}");

            if (response.IsSuccessStatusCode)
            {
                string data = await response.Content.ReadAsStringAsync();
                empresa = JsonConvert.DeserializeObject<EmpresasViewModel>(data);
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                return RedirectToAction("Login", "Authentication");
            }

            if (empresa == null)
            {
                return NotFound();
            }

            // Obtener la lista de EstadosDeAfiliados
            //HttpResponseMessage estadosResponse = await _httpClient.GetAsync("https://localhost:7086/api/Localidades");
            HttpResponseMessage estadosResponse = await _httpClient.GetAsync("https://soimsf.somee.com/api/Localidades");
            if (estadosResponse.IsSuccessStatusCode)
            {
                string data = await estadosResponse.Content.ReadAsStringAsync();
                localidadesList = JsonConvert.DeserializeObject<List<LocalidadesViewModel>>(data);
            }

            ViewBag.Localidades = localidadesList.FirstOrDefault(s => s.Id == empresa.IdLocalidad)?.Nombre ?? "Desconocido";

            return View(empresa);
        }

        // GET: Empresas/Create
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> Create()
        {
            try
            {
                AddJwtToken(); // Agregar token JWT

                var localidadesList = new List<LocalidadesViewModel>();
                //HttpResponseMessage response = await _httpClient.GetAsync("https://localhost:7086/api/Localidades");
                HttpResponseMessage response = await _httpClient.GetAsync("https://soimsf.somee.com/api/Localidades");

                if (response.IsSuccessStatusCode)
                {
                    string data = await response.Content.ReadAsStringAsync();
                    localidadesList = JsonConvert.DeserializeObject<List<LocalidadesViewModel>>(data);
                }

                ViewBag.IdLocalidad = new SelectList(localidadesList, "Id", "Nombre")
                    .Prepend(new SelectListItem { Text = "Ninguna", Value = "" }); // Opción para null

                return View();
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error al cargar localidades: {ex.Message}");
                return View();
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(EmpresasViewModel empresa)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    AddJwtToken(); // Agregar token JWT

                    string jsonData = JsonConvert.SerializeObject(empresa);
                    StringContent content = new StringContent(jsonData, Encoding.UTF8, "application/json");

                    HttpResponseMessage response = await _httpClient.PostAsync(_apiBaseUrl, content);

                    if (response.IsSuccessStatusCode)
                    {
                        TempData["AlertMessage"] = "¡Empresa creado con éxito!";
                        TempData["AlertType"] = "success";
                        return RedirectToAction(nameof(Index));
                    }
                    else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    {
                        return RedirectToAction("Login", "Authentication");
                    }
                    else
                    {
                        ModelState.AddModelError("", "Error al guardar la empresa.");
                    }
                }

                // Volver a cargar localidades si hay errores
                var localidadesList = new List<LocalidadesViewModel>();
                //HttpResponseMessage localidadesResponse = await _httpClient.GetAsync("https://localhost:7086/api/Localidades");
                HttpResponseMessage localidadesResponse = await _httpClient.GetAsync("https://soimsf.somee.com/api/Localidades");
                if (localidadesResponse.IsSuccessStatusCode)
                {
                    string data = await localidadesResponse.Content.ReadAsStringAsync();
                    localidadesList = JsonConvert.DeserializeObject<List<LocalidadesViewModel>>(data);
                }

                ViewBag.IdLocalidad = new SelectList(localidadesList, "Id", "Nombre", empresa.IdLocalidad)
                    .Prepend(new SelectListItem { Text = "Ninguna", Value = "" }); 
                return View(empresa);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error: {ex.Message}");
                return View(empresa);
            }
        }

        // GET: Empresas/Edit/5
        [Authorize(Roles = "Admin,Manager")]
        public async Task<ActionResult> Edit(int id)
        {
            AddJwtToken(); // Agregar token JWT

            EmpresasViewModel empresa = null;
            var localidadesList = new List<LocalidadesViewModel>();

            // Obtener los datos de la empresa
            HttpResponseMessage empresaResponse = await _httpClient.GetAsync($"{_apiBaseUrl}/{id}");
            if (empresaResponse.IsSuccessStatusCode)
            {
                string empresaData = await empresaResponse.Content.ReadAsStringAsync();
                empresa = JsonConvert.DeserializeObject<EmpresasViewModel>(empresaData);
            }

            if (empresa == null)
            {
                return NotFound();
            }

            // Obtener la lista de localidades
            //HttpResponseMessage localidadesResponse = await _httpClient.GetAsync("https://localhost:7086/api/Localidades");
            HttpResponseMessage localidadesResponse = await _httpClient.GetAsync("https://soimsf.somee.com/api/Localidades");
            if (localidadesResponse.IsSuccessStatusCode)
            {
                string localidadesData = await localidadesResponse.Content.ReadAsStringAsync();
                localidadesList = JsonConvert.DeserializeObject<List<LocalidadesViewModel>>(localidadesData);
            }

            // Asignar localidades al ViewBag
            ViewBag.IdLocalidad = new SelectList(localidadesList, "Id", "Nombre", empresa.IdLocalidad);

            return View(empresa);
        }

        [Authorize(Roles = "Admin,Manager")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(EmpresasViewModel empresa)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    AddJwtToken(); // Agregar token JWT

                    // Serializar el objeto empresa y enviarlo a la API
                    string jsonData = JsonConvert.SerializeObject(empresa);
                    StringContent content = new StringContent(jsonData, Encoding.UTF8, "application/json");

                    HttpResponseMessage response = await _httpClient.PutAsync($"{_apiBaseUrl}/{empresa.Id}", content);

                    if (response.IsSuccessStatusCode)
                    {
                        TempData["AlertMessage"] = "¡Empresa editado con éxito!";
                        TempData["AlertType"] = "warning";
                        return RedirectToAction(nameof(Index));
                    }
                    else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    {
                        return RedirectToAction("Login", "Authentication");
                    }
                    else
                    {
                        ModelState.AddModelError("", "Error al actualizar la empresa. Por favor, intenta nuevamente.");
                    }
                }

                // Si el modelo no es válido o hubo un error, recargar localidades
                var localidadesList = new List<LocalidadesViewModel>();
                //HttpResponseMessage localidadesResponse = await _httpClient.GetAsync("https://localhost:7086/api/Localidades");
                HttpResponseMessage localidadesResponse = await _httpClient.GetAsync("https://soimsf.somee.com/api/Localidades");
                if (localidadesResponse.IsSuccessStatusCode)
                {
                    string localidadesData = await localidadesResponse.Content.ReadAsStringAsync();
                    localidadesList = JsonConvert.DeserializeObject<List<LocalidadesViewModel>>(localidadesData);
                }

                ViewBag.IdLocalidad = new SelectList(localidadesList, "Id", "Nombre", empresa.IdLocalidad);
                return View(empresa);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error inesperado: {ex.Message}");
                return View(empresa);
            }
        }


        // GET: Empresas/Delete/5
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> Delete(int id)
        {
            AddJwtToken(); // Agregar token JWT

            EmpresasViewModel empresa = null;
            var localidadesList = new List<LocalidadesViewModel>();
            HttpResponseMessage response = await _httpClient.GetAsync($"{_apiBaseUrl}/{id}");

            if (response.IsSuccessStatusCode)
            {
                string data = await response.Content.ReadAsStringAsync();
                empresa = JsonConvert.DeserializeObject<EmpresasViewModel>(data);
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                return RedirectToAction("Login", "Authentication");
            }

            // Obtener la lista de Localidades
            //HttpResponseMessage localidadesResponse = await _httpClient.GetAsync("https://localhost:7086/api/Localidades");
            HttpResponseMessage localidadesResponse = await _httpClient.GetAsync("https://soimsf.somee.com/api/Localidades");
            if (localidadesResponse.IsSuccessStatusCode)
            {
                string data = await localidadesResponse.Content.ReadAsStringAsync();
                localidadesList = JsonConvert.DeserializeObject<List<LocalidadesViewModel>>(data);
            }

            // Obtener nombres de las relaciones
            ViewBag.Localidades = localidadesList.FirstOrDefault(emp => emp.Id == empresa.IdLocalidad)?.Nombre ?? "Desconocido";

            return View(empresa);
        }

        // POST: Empresas/Delete/5
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            try
            {
                AddJwtToken(); // Agregar token JWT

                HttpResponseMessage response = await _httpClient.DeleteAsync($"{_apiBaseUrl}/{id}");

                if (response.IsSuccessStatusCode)
                {
                    TempData["AlertMessage"] = "¡Empresa eliminado con éxito!";
                    TempData["AlertType"] = "danger"; // Clase para la alerta de eliminación (Bootstrap)
                    return RedirectToAction(nameof(Index));
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    return RedirectToAction("Login", "Authentication");
                }
                else
                {
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (Exception ex)
            {
                TempData["AlertMessage"] = $"Error: {ex.Message}";
                TempData["AlertType"] = "danger";
                return RedirectToAction(nameof(Index));
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public IActionResult Import()
        {
            AddJwtToken(); // Agregar token JWT

            return View();
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> ImportarDatosEsenciales(IFormFile excelFile)
        {
            if (excelFile == null || excelFile.Length == 0)
            {
                ViewData["Error"] = "Seleccione un archivo válido.";
                return View("Import");
            }

            try
            {
                AddJwtToken(); // Agregar token JWT

                using (var content = new MultipartFormDataContent())
                {
                    // Agregar el archivo al contenido del formulario
                    using (var stream = new MemoryStream())
                    {
                        await excelFile.CopyToAsync(stream);
                        content.Add(new ByteArrayContent(stream.ToArray()), "excelFile", excelFile.FileName);
                    }

                    // Llamar a la API
                    var response = await _httpClient.PostAsync(_apiBaseUrl + "/ImportarDatosEsenciales", content);

                    if (response.IsSuccessStatusCode)
                    {
                        TempData["Success"] = "Datos esenciales importados exitosamente.";
                        return RedirectToAction("Index");
                    }
                    else
                    {
                        var error = await response.Content.ReadAsStringAsync();
                        ViewData["Error"] = $"Error al importar datos: {error}";
                        return View("Import");
                    }
                }
            }
            catch (Exception ex)
            {
                ViewData["Error"] = $"Error inesperado: {ex.Message}";
            }

            return View("Import");
        }

        [Authorize(Roles = "Admin, Manager")]
        [HttpGet]
        public async Task<IActionResult> ExportarEmpresas()
        {
            try
            {
                AddJwtToken();

                // Llamar a la API para exportar empresas
                var response = await _httpClient.GetAsync($"{_apiBaseUrl}/ExportarEmpresas");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsByteArrayAsync();
                    var fileName = "Empresas.xlsx";

                    // Retorna el archivo para su descarga
                    return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
                }
                else
                {
                    TempData["Error"] = "No se pudo exportar el archivo. Intenta nuevamente.";
                    return RedirectToAction("Index");
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Ocurrió un error: {ex.Message}";
                return RedirectToAction("Index");
            }
        }
        [Authorize(Roles = "Admin, Manager")]
        [HttpGet]
        public async Task<IActionResult> ExportarEmpresasPDF()
        {
            try
            {
                // Asegúrate de que el token JWT está en el encabezado
                AddJwtToken();

                // Llamar a la API para exportar empresas en PDF
                var response = await _httpClient.GetAsync($"{_apiBaseUrl}/ExportarEmpresasPDF");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsByteArrayAsync();
                    var fileName = "Empresas.pdf";

                    // Retorna el archivo PDF para su descarga
                    return File(content, "application/pdf", fileName);
                }
                else
                {
                    TempData["Error"] = "No se pudo exportar el archivo. Intenta nuevamente.";
                    return RedirectToAction("Index");
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Ocurrió un error: {ex.Message}";
                return RedirectToAction("Index");
            }
        }

    }
}

