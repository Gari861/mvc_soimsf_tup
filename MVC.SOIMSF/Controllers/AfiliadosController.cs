using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using MVC.SOIMSF.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;

namespace MVC.SOIMSF.Controllers
{
    [Authorize]
    public class AfiliadosController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiBaseUrl;

        public AfiliadosController()
        {
            _httpClient = new HttpClient();
            //_apiBaseUrl = "https://localhost:7086/api/Afiliados";
            _apiBaseUrl = "https://soimsf.somee.com/api/Afiliados";
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

        // GET: Afiliados
        public async Task<IActionResult> Index(string? nombre, bool? discapacidad, int? idEmpresa)
        {
            AddJwtToken(); // Agregar token JWT

            var afiliadosList = new List<AfiliadosViewModel>();
            var estadosList = new List<EstadosDeAfiliadosViewModel>();
            var situacionesList = new List<SituacionesDeRevistaViewModel>();
            var empresasList = new List<EmpresasViewModel>();

            // Validaciones
            if (nombre?.Length > 100)
            {
                ModelState.AddModelError("nombre", "El nombre no puede exceder los 100 caracteres.");
                return View(afiliadosList);
            }
            if (idEmpresa.HasValue && idEmpresa <= 0)
            {
                ModelState.AddModelError("idEmpresa", "El ID de la empresa debe ser un número positivo.");
                return View(afiliadosList);
            }

            // Construir los parámetros de la consulta
            var queryParams = new Dictionary<string, string>();
            if (!string.IsNullOrEmpty(nombre))
                queryParams.Add("nombre", nombre);
            if (discapacidad.HasValue)
                queryParams.Add("discapacidad", discapacidad.Value.ToString().ToLower());
            if (idEmpresa.HasValue)
                queryParams.Add("idEmpresa", idEmpresa.Value.ToString());

            // Construir la URL con los filtros
            var queryString = string.Join("&", queryParams.Select(kvp => $"{kvp.Key}={Uri.EscapeDataString(kvp.Value)}"));
            var url = string.IsNullOrEmpty(queryString) ? _apiBaseUrl : $"{_apiBaseUrl}?{queryString}";

            // Obtener la lista de afiliados con los filtros
            HttpResponseMessage afiliadosResponse = await _httpClient.GetAsync(url);
            if (afiliadosResponse.IsSuccessStatusCode)
            {
                string data = await afiliadosResponse.Content.ReadAsStringAsync();
                afiliadosList = JsonConvert.DeserializeObject<List<AfiliadosViewModel>>(data);
            }
            else if (afiliadosResponse.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                return RedirectToAction("Login", "Authentication");
            }

            if (!afiliadosResponse.IsSuccessStatusCode)
            {
                string errorData = await afiliadosResponse.Content.ReadAsStringAsync();
                Console.WriteLine($"Error en API de Afiliados: {afiliadosResponse.StatusCode} - {errorData}");
            }

            // Obtener la lista de EstadosDeAfiliados
            //HttpResponseMessage estadosResponse = await _httpClient.GetAsync("https://localhost:7086/api/EstadosDeAfiliados");
            HttpResponseMessage estadosResponse = await _httpClient.GetAsync("https://soimsf.somee.com/api/EstadosDeAfiliados");
            if (estadosResponse.IsSuccessStatusCode)
            {
                string data = await estadosResponse.Content.ReadAsStringAsync();
                estadosList = JsonConvert.DeserializeObject<List<EstadosDeAfiliadosViewModel>>(data);
            }

            // Obtener la lista de SituacionesDeRevista
            //HttpResponseMessage situacionesResponse = await _httpClient.GetAsync("https://localhost:7086/api/SituacionesDeRevista");
            HttpResponseMessage situacionesResponse = await _httpClient.GetAsync("https://soimsf.somee.com/api/SituacionesDeRevista");
            if (situacionesResponse.IsSuccessStatusCode)
            {
                string data = await situacionesResponse.Content.ReadAsStringAsync();
                situacionesList = JsonConvert.DeserializeObject<List<SituacionesDeRevistaViewModel>>(data);
            }

            // Obtener la lista de Empresas
            //HttpResponseMessage empresasResponse = await _httpClient.GetAsync("https://localhost:7086/api/Empresas");
            HttpResponseMessage empresasResponse = await _httpClient.GetAsync("https://soimsf.somee.com/api/Empresas");
            if (empresasResponse.IsSuccessStatusCode)
            {
                string data = await empresasResponse.Content.ReadAsStringAsync();
                empresasList = JsonConvert.DeserializeObject<List<EmpresasViewModel>>(data);
            }

            // Crear diccionarios para los nombres relacionados
            ViewBag.EstadosDeAfiliados = estadosList.ToDictionary(e => e.Id, e => e.Nombre);
            ViewBag.SituacionesDeRevista = situacionesList.ToDictionary(s => s.Id, s => s.Nombre);
            ViewBag.Empresas = empresasList.ToDictionary(emp => emp.Id, emp => emp.Nombre);

            // Verificar si se aplicaron filtros
            if (!string.IsNullOrEmpty(nombre) || discapacidad.HasValue || idEmpresa.HasValue)
            {
                TempData["AlertMessage"] = "¡Filtros aplicados con éxito!";
                TempData["AlertType"] = "primary"; // Azul (Bootstrap)
            }

            return View(afiliadosList);
        }

        // GET: Afiliados/Details/5
        public async Task<IActionResult> Details(int id)
        {
            AddJwtToken(); // Agregar token JWT

            AfiliadosViewModel afiliado = null;
            var estadosList = new List<EstadosDeAfiliadosViewModel>();
            var situacionesList = new List<SituacionesDeRevistaViewModel>();
            var empresasList = new List<EmpresasViewModel>();

            // Obtener los detalles del afiliado
            HttpResponseMessage response = await _httpClient.GetAsync($"{_apiBaseUrl}/{id}");
            if (response.IsSuccessStatusCode)
            {
                string data = await response.Content.ReadAsStringAsync();
                afiliado = JsonConvert.DeserializeObject<AfiliadosViewModel>(data);
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                return RedirectToAction("Login", "Authentication");
            }

            if (afiliado == null)
            {
                return NotFound();
            }

            // Obtener la lista de EstadosDeAfiliados
            //HttpResponseMessage estadosResponse = await _httpClient.GetAsync("https://localhost:7086/api/EstadosDeAfiliados");
            HttpResponseMessage estadosResponse = await _httpClient.GetAsync("https://soimsf.somee.com/api/EstadosDeAfiliados");
            if (estadosResponse.IsSuccessStatusCode)
            {
                string data = await estadosResponse.Content.ReadAsStringAsync();
                estadosList = JsonConvert.DeserializeObject<List<EstadosDeAfiliadosViewModel>>(data);
            }

            // Obtener la lista de SituacionesDeRevista
            //HttpResponseMessage situacionesResponse = await _httpClient.GetAsync("https://localhost:7086/api/SituacionesDeRevista");
            HttpResponseMessage situacionesResponse = await _httpClient.GetAsync("https://soimsf.somee.com/api/SituacionesDeRevista");
            if (situacionesResponse.IsSuccessStatusCode)
            {
                string data = await situacionesResponse.Content.ReadAsStringAsync();
                situacionesList = JsonConvert.DeserializeObject<List<SituacionesDeRevistaViewModel>>(data);
            }

            // Obtener la lista de Empresas
            //HttpResponseMessage empresasResponse = await _httpClient.GetAsync("https://localhost:7086/api/Empresas");
            HttpResponseMessage empresasResponse = await _httpClient.GetAsync("https://soimsf.somee.com/api/Empresas");
            if (empresasResponse.IsSuccessStatusCode)
            {
                string data = await empresasResponse.Content.ReadAsStringAsync();
                empresasList = JsonConvert.DeserializeObject<List<EmpresasViewModel>>(data);
            }

            // Obtener nombres de las relaciones
            ViewBag.IdEstadoAfiliado = estadosList.FirstOrDefault(e => e.Id == afiliado.IdEstadoAfiliado)?.Nombre ?? "Desconocido";
            ViewBag.IdSituacionDeRevista = situacionesList.FirstOrDefault(s => s.Id == afiliado.IdSituacionDeRevista)?.Nombre ?? "Desconocido";
            ViewBag.IdEmpresa = empresasList.FirstOrDefault(emp => emp.Id == afiliado.IdEmpresa)?.Nombre ?? "Desconocido";

            return View(afiliado);
        }


        // GET: Afiliados/Create
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> Create()
        {
            try
            {
                AddJwtToken(); // Agregar token JWT

                // Cargar Estados de Afiliado
                var estadosList = new List<EstadosDeAfiliadosViewModel>();
                //HttpResponseMessage estadosResponse = await _httpClient.GetAsync("https://localhost:7086/api/EstadosDeAfiliados");
                HttpResponseMessage estadosResponse = await _httpClient.GetAsync("https://soimsf.somee.com/api/EstadosDeAfiliados");

                if (estadosResponse.IsSuccessStatusCode)
                {
                    string data = await estadosResponse.Content.ReadAsStringAsync();
                    estadosList = JsonConvert.DeserializeObject<List<EstadosDeAfiliadosViewModel>>(data);
                }

                ViewBag.IdEstadoAfiliado = new SelectList(estadosList, "Id", "Nombre")
                    .Prepend(new SelectListItem { Text = "Ninguna", Value = "" }); // Opción para null

                // Cargar Situaciones de Revista
                var situacionesList = new List<SituacionesDeRevistaViewModel>();
                //HttpResponseMessage situacionesResponse = await _httpClient.GetAsync("https://localhost:7086/api/SituacionesDeRevista");
                HttpResponseMessage situacionesResponse = await _httpClient.GetAsync("https://soimsf.somee.com/api/SituacionesDeRevista");

                if (situacionesResponse.IsSuccessStatusCode)
                {
                    string data = await situacionesResponse.Content.ReadAsStringAsync();
                    situacionesList = JsonConvert.DeserializeObject<List<SituacionesDeRevistaViewModel>>(data);
                }

                ViewBag.IdSituacionDeRevista = new SelectList(situacionesList, "Id", "Nombre")
                    .Prepend(new SelectListItem { Text = "Ninguna", Value = "" }); // Opción para null

                var empresasList = new List<EmpresasViewModel>();
                //HttpResponseMessage empresasResponse = await _httpClient.GetAsync("https://localhost:7086/api/Empresas");
                HttpResponseMessage empresasResponse = await _httpClient.GetAsync("https://soimsf.somee.com/api/Empresas");

                if (empresasResponse.IsSuccessStatusCode)
                {
                    string data = await empresasResponse.Content.ReadAsStringAsync();
                    empresasList = JsonConvert.DeserializeObject<List<EmpresasViewModel>>(data);
                }

                ViewBag.IdEmpresa = new SelectList(empresasList, "Id", "Nombre")
                    .Prepend(new SelectListItem { Text = "Ninguna", Value = "" }); // Opción para null

                return View();
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error al cargar datoas: {ex.Message}");
                return View();
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(AfiliadosViewModel afiliado)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    AddJwtToken(); // Agregar token JWT

                    // Convertir el modelo de afiliado a JSON
                    string jsonData = JsonConvert.SerializeObject(afiliado);
                    StringContent content = new StringContent(jsonData, Encoding.UTF8, "application/json");

                    HttpResponseMessage response = await _httpClient.PostAsync(_apiBaseUrl, content);

                    if (response.IsSuccessStatusCode)
                    {
                        TempData["AlertMessage"] = "¡Afiliado creado con éxito!";
                        TempData["AlertType"] = "success";
                        return RedirectToAction(nameof(Index));
                    }
                    else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    {
                        return RedirectToAction("Login", "Authentication");
                    }
                    else
                    {
                        ModelState.AddModelError("", "Error al guardar el afiliado.");
                    }
                }

                // Cargar Estados de Afiliado
                var estadosList = new List<EstadosDeAfiliadosViewModel>();
                //HttpResponseMessage estadosResponse = await _httpClient.GetAsync("https://localhost:7086/api/EstadosDeAfiliados");
                HttpResponseMessage estadosResponse = await _httpClient.GetAsync("https://soimsf.somee.com/api/EstadosDeAfiliados");

                if (estadosResponse.IsSuccessStatusCode)
                {
                    string data = await estadosResponse.Content.ReadAsStringAsync();
                    estadosList = JsonConvert.DeserializeObject<List<EstadosDeAfiliadosViewModel>>(data);
                }

                ViewBag.IdEstadoAfiliado = new SelectList(estadosList, "Id", "Nombre", afiliado.IdEstadoAfiliado)
                    .Prepend(new SelectListItem { Text = "Ninguna", Value = "" });

                // Cargar Situaciones de Revista
                var situacionesList = new List<SituacionesDeRevistaViewModel>();
                //HttpResponseMessage situacionesResponse = await _httpClient.GetAsync("https://localhost:7086/api/SituacionesDeRevista");
                HttpResponseMessage situacionesResponse = await _httpClient.GetAsync("https://soimsf.somee.com/api/SituacionesDeRevista");

                if (situacionesResponse.IsSuccessStatusCode)
                {
                    string data = await situacionesResponse.Content.ReadAsStringAsync();
                    situacionesList = JsonConvert.DeserializeObject<List<SituacionesDeRevistaViewModel>>(data);
                }

                ViewBag.IdSituacionDeRevista = new SelectList(situacionesList, "Id", "Nombre", afiliado.IdSituacionDeRevista)
                    .Prepend(new SelectListItem { Text = "Ninguna", Value = "" });

                // Cargar Empresas
                var empresasList = new List<EmpresasViewModel>();
                //HttpResponseMessage empresasResponse = await _httpClient.GetAsync("https://localhost:7086/api/Empresas");
                HttpResponseMessage empresasResponse = await _httpClient.GetAsync("https://soimsf.somee.com/api/Empresas");

                if (empresasResponse.IsSuccessStatusCode)
                {
                    string data = await empresasResponse.Content.ReadAsStringAsync();
                    empresasList = JsonConvert.DeserializeObject<List<EmpresasViewModel>>(data);
                }

                ViewBag.IdEmpresa = new SelectList(empresasList, "Id", "Nombre", afiliado.IdEmpresa)
                    .Prepend(new SelectListItem { Text = "Ninguna", Value = "" });

                return View(afiliado);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error: {ex.Message}");
                return View(afiliado);
            }
        }

        // GET: Afiliados/Edit/5
        [Authorize(Roles = "Admin,Manager")]
        public async Task<ActionResult> Edit(int id)
        {
            AddJwtToken(); // Agregar token JWT

            AfiliadosViewModel afiliado = null;
            var estadosList = new List<EstadosDeAfiliadosViewModel>();
            var situacionesList = new List<SituacionesDeRevistaViewModel>();
            var empresasList = new List<EmpresasViewModel>();

            // Obtener datos del afiliado
            HttpResponseMessage afiliadoResponse = await _httpClient.GetAsync($"{_apiBaseUrl}/{id}");
            if (afiliadoResponse.IsSuccessStatusCode)
            {
                string afiliadoData = await afiliadoResponse.Content.ReadAsStringAsync();
                afiliado = JsonConvert.DeserializeObject<AfiliadosViewModel>(afiliadoData);
            }

            if (afiliado == null)
            {
                return NotFound();
            }

            // Obtener la lista de EstadosDeAfiliados
            //HttpResponseMessage estadosResponse = await _httpClient.GetAsync("https://localhost:7086/api/EstadosDeAfiliados");
            HttpResponseMessage estadosResponse = await _httpClient.GetAsync("https://soimsf.somee.com/api/EstadosDeAfiliados");
            if (estadosResponse.IsSuccessStatusCode)
            {
                string estadosData = await estadosResponse.Content.ReadAsStringAsync();
                estadosList = JsonConvert.DeserializeObject<List<EstadosDeAfiliadosViewModel>>(estadosData);
            }

            // Obtener la lista de SituacionesDeRevista
            //HttpResponseMessage situacionesResponse = await _httpClient.GetAsync("https://localhost:7086/api/SituacionesDeRevista");
            HttpResponseMessage situacionesResponse = await _httpClient.GetAsync("https://soimsf.somee.com/api/SituacionesDeRevista");
            if (situacionesResponse.IsSuccessStatusCode)
            {
                string situacionesData = await situacionesResponse.Content.ReadAsStringAsync();
                situacionesList = JsonConvert.DeserializeObject<List<SituacionesDeRevistaViewModel>>(situacionesData);
            }

            // Obtener la lista de Empresas
            //HttpResponseMessage empresasResponse = await _httpClient.GetAsync("https://localhost:7086/api/Empresas");
            HttpResponseMessage empresasResponse = await _httpClient.GetAsync("https://soimsf.somee.com/api/Empresas");
            if (empresasResponse.IsSuccessStatusCode)
            {
                string empresasData = await empresasResponse.Content.ReadAsStringAsync();
                empresasList = JsonConvert.DeserializeObject<List<EmpresasViewModel>>(empresasData);
            }

            // Asignar listas al ViewBag
            ViewBag.IdEstadoAfiliado = new SelectList(estadosList, "Id", "Nombre", afiliado.IdEstadoAfiliado);
            ViewBag.IdSituacionDeRevista = new SelectList(situacionesList, "Id", "Nombre", afiliado.IdSituacionDeRevista);
            ViewBag.IdEmpresa = new SelectList(empresasList, "Id", "Nombre", afiliado.IdEmpresa);

            return View(afiliado);
        }

        // POST: Afiliados/Edit/5
        [Authorize(Roles = "Admin,Manager")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(AfiliadosViewModel afiliado)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    AddJwtToken(); // Agregar token JWT

                    // Serializar el objeto afiliado y enviarlo a la API
                    string jsonData = JsonConvert.SerializeObject(afiliado);
                    StringContent content = new StringContent(jsonData, Encoding.UTF8, "application/json");

                    HttpResponseMessage response = await _httpClient.PutAsync($"{_apiBaseUrl}/{afiliado.Id}", content);

                    if (response.IsSuccessStatusCode)
                    {
                        TempData["AlertMessage"] = "¡Afiliado editado con éxito!";
                        TempData["AlertType"] = "warning";
                        return RedirectToAction(nameof(Index));
                    }
                    else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    {
                        return RedirectToAction("Login", "Authentication");
                    }
                    else
                    {
                        ModelState.AddModelError("", "Error al actualizar el afiliado. Por favor, intenta nuevamente.");
                    }
                }

                // Si el modelo no es válido o hubo un error, recargar datos relacionados
                var situacionesList = new List<SituacionesDeRevistaViewModel>();
                var estadosList = new List<EstadosDeAfiliadosViewModel>();
                var empresasList = new List<EmpresasViewModel>();

                //HttpResponseMessage situacionesResponse = await _httpClient.GetAsync("https://localhost:7086/api/SituacionesDeRevista");
                HttpResponseMessage situacionesResponse = await _httpClient.GetAsync("https://soimsf.somee.com/api/SituacionesDeRevista");
                if (situacionesResponse.IsSuccessStatusCode)
                {
                    string situacionesData = await situacionesResponse.Content.ReadAsStringAsync();
                    situacionesList = JsonConvert.DeserializeObject<List<SituacionesDeRevistaViewModel>>(situacionesData);
                }

                //HttpResponseMessage estadosResponse = await _httpClient.GetAsync("https://localhost:7086/api/EstadosDeAfiliados");
                HttpResponseMessage estadosResponse = await _httpClient.GetAsync("https://soimsf.somee.com/api/EstadosDeAfiliados");
                if (estadosResponse.IsSuccessStatusCode)
                {
                    string estadosData = await estadosResponse.Content.ReadAsStringAsync();
                    estadosList = JsonConvert.DeserializeObject<List<EstadosDeAfiliadosViewModel>>(estadosData);
                }

                //HttpResponseMessage empresasResponse = await _httpClient.GetAsync("https://localhost:7086/api/Empresas");
                HttpResponseMessage empresasResponse = await _httpClient.GetAsync("https://soimsf.somee.com/api/Empresas");
                if (empresasResponse.IsSuccessStatusCode)
                {
                    string empresasData = await empresasResponse.Content.ReadAsStringAsync();
                    empresasList = JsonConvert.DeserializeObject<List<EmpresasViewModel>>(empresasData);
                }

                ViewBag.IdSituacionDeRevista = new SelectList(situacionesList, "Id", "Nombre", afiliado.IdSituacionDeRevista);
                ViewBag.IdEstadoDeAfiliado = new SelectList(estadosList, "Id", "Nombre", afiliado.IdEstadoAfiliado);
                ViewBag.IdEmpresa = new SelectList(empresasList, "Id", "Nombre", afiliado.IdEmpresa);

                return View(afiliado);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error inesperado: {ex.Message}");
                return View(afiliado);
            }
        }

        // GET: Afiliados/Delete/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            AddJwtToken(); // Agregar token JWT

            AfiliadosViewModel afiliado = null;
            var estadosList = new List<EstadosDeAfiliadosViewModel>();
            var situacionesList = new List<SituacionesDeRevistaViewModel>();
            var empresasList = new List<EmpresasViewModel>();

            // Obtener los detalles del afiliado
            HttpResponseMessage response = await _httpClient.GetAsync($"{_apiBaseUrl}/{id}");
            if (response.IsSuccessStatusCode)
            {
                string data = await response.Content.ReadAsStringAsync();
                afiliado = JsonConvert.DeserializeObject<AfiliadosViewModel>(data);
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                return RedirectToAction("Login", "Authentication");
            }

            if (afiliado == null)
            {
                return NotFound();
            }

            // Obtener la lista de EstadosDeAfiliados
            //HttpResponseMessage estadosResponse = await _httpClient.GetAsync("https://localhost:7086/api/EstadosDeAfiliados");
            HttpResponseMessage estadosResponse = await _httpClient.GetAsync("https://soimsf.somee.com/api/EstadosDeAfiliados");
            if (estadosResponse.IsSuccessStatusCode)
            {
                string data = await estadosResponse.Content.ReadAsStringAsync();
                estadosList = JsonConvert.DeserializeObject<List<EstadosDeAfiliadosViewModel>>(data);
            }

            // Obtener la lista de SituacionesDeRevista
            //HttpResponseMessage situacionesResponse = await _httpClient.GetAsync("https://localhost:7086/api/SituacionesDeRevista");
            HttpResponseMessage situacionesResponse = await _httpClient.GetAsync("https://soimsf.somee.com/api/SituacionesDeRevista");
            if (situacionesResponse.IsSuccessStatusCode)
            {
                string data = await situacionesResponse.Content.ReadAsStringAsync();
                situacionesList = JsonConvert.DeserializeObject<List<SituacionesDeRevistaViewModel>>(data);
            }

            // Obtener la lista de Empresas
            //HttpResponseMessage empresasResponse = await _httpClient.GetAsync("https://localhost:7086/api/Empresas");
            HttpResponseMessage empresasResponse = await _httpClient.GetAsync("https://soimsf.somee.com/api/Empresas");
            if (empresasResponse.IsSuccessStatusCode)
            {
                string data = await empresasResponse.Content.ReadAsStringAsync();
                empresasList = JsonConvert.DeserializeObject<List<EmpresasViewModel>>(data);
            }

            // Obtener nombres de las relaciones
            ViewBag.IdEstadoAfiliado = estadosList.FirstOrDefault(e => e.Id == afiliado.IdEstadoAfiliado)?.Nombre ?? "Desconocido";
            ViewBag.IdSituacionDeRevista = situacionesList.FirstOrDefault(s => s.Id == afiliado.IdSituacionDeRevista)?.Nombre ?? "Desconocido";
            ViewBag.IdEmpresa = empresasList.FirstOrDefault(emp => emp.Id == afiliado.IdEmpresa)?.Nombre ?? "Desconocido";

            return View(afiliado);
        }


        // POST: Afiliados/Delete/5
        [Authorize(Roles = "Admin")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                AddJwtToken(); // Agregar token JWT

                HttpResponseMessage response = await _httpClient.DeleteAsync($"{_apiBaseUrl}/{id}");

                if (response.IsSuccessStatusCode)
                {
                    TempData["AlertMessage"] = "¡Afiliado eliminado con éxito!";
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
        public async Task<IActionResult> ExportarAfiliados()
        {
            try
            {
                // Asegúrate de que el token JWT está en el encabezado
                AddJwtToken();

                // Llamar a la API para exportar empresas
                var response = await _httpClient.GetAsync($"{_apiBaseUrl}/ExportarAfiliados");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsByteArrayAsync();
                    var fileName = "Afiliados.xlsx";

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
        public async Task<IActionResult> ExportarAfiliadosPDF()
        {
            try
            {
                // Agrega el token JWT al encabezado
                AddJwtToken();

                // Llamar a la API para exportar afiliados en PDF
                var response = await _httpClient.GetAsync($"{_apiBaseUrl}/ExportarAfiliadosPDF");

                if (response.IsSuccessStatusCode)
                {
                    // Leer el contenido del archivo
                    var content = await response.Content.ReadAsByteArrayAsync();
                    var fileName = "Afiliados.pdf";

                    // Retorna el archivo para su descarga
                    return File(content, "application/pdf", fileName);
                }
                else
                {
                    // Si ocurre un error, manejarlo con TempData y redirigir al índice
                    TempData["Error"] = "No se pudo exportar el archivo PDF. Intenta nuevamente.";
                    return RedirectToAction("Index");
                }
            }
            catch (Exception ex)
            {
                // Captura errores y muestra el mensaje en TempData
                TempData["Error"] = $"Ocurrió un error al exportar: {ex.Message}";
                return RedirectToAction("Index");
            }
        }
    }
}


