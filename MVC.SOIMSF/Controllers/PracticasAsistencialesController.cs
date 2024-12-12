using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using MVC.SOIMSF.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Authorization;
using static System.Runtime.InteropServices.JavaScript.JSType;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace MVC.SOIMSF.Controllers
{
    [Authorize]
    public class PracticasAsistencialesController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiBaseUrl;


        public PracticasAsistencialesController()
        {
            _httpClient = new HttpClient();
            //_apiBaseUrl = "https://localhost:7086/api/PracticasAsistenciales"; 
            _apiBaseUrl = "https://soimsf.somee.com/api/PracticasAsistenciales"; 
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

        // GET: PracticasAsistenciales
        public async Task<IActionResult> Index(DateTime? Fecha, int? idTipo, int? IdAfiliado, int? IdTipoDeBeneficiario, int? IdEstadoDePractica)
        {
            AddJwtToken(); // Agregar token JWT

            var practicasList = new List<PracticasAsistencialesViewModel>();
            var tiposDePracticaList = new List<TiposDePracticasViewModel>();
            var tiposDeBeneficiariosList = new List<TiposDeBeneficiariosViewModel>();
            var estadosDePracticasList = new List<EstadosDePracticasViewModel>();
            var afiliadosList = new List<AfiliadosViewModel>();

            // Validaciones
            if (idTipo.HasValue && idTipo <= 0)
            {
                ModelState.AddModelError("idTipo", "El ID del tipo de práctica debe ser un número positivo.");
                return View(practicasList);
            }
            if (IdTipoDeBeneficiario.HasValue && IdTipoDeBeneficiario <= 0)
            {
                ModelState.AddModelError("IdTipoDeBeneficiario", "El ID del tipo de beneficiario debe ser un número positivo.");
                return View(practicasList);
            }
            if (IdAfiliado.HasValue && IdAfiliado <= 0)
            {
                ModelState.AddModelError("IdAfiliado", "El ID del afiliado debe ser un número positivo.");
                return View(practicasList);
            }
            if (IdEstadoDePractica.HasValue && IdEstadoDePractica <= 0)
            {
                ModelState.AddModelError("IdEstadoDePractica", "El ID del estado de práctica debe ser un número positivo.");
                return View(practicasList);
            }

            // Construir los parámetros de la consulta
            var queryParams = new Dictionary<string, string>();
            if (Fecha.HasValue)
                queryParams.Add("Fecha", Fecha.Value.ToString("yyyy-MM-dd"));
            if (idTipo.HasValue)
                queryParams.Add("idTipo", idTipo.Value.ToString());
            if (IdTipoDeBeneficiario.HasValue)
                queryParams.Add("IdTipoDeBeneficiario", IdTipoDeBeneficiario.Value.ToString());
            if (IdAfiliado.HasValue)
                queryParams.Add("IdAfiliado", IdAfiliado.Value.ToString());
            if (IdEstadoDePractica.HasValue)
                queryParams.Add("IdEstadoDePractica", IdEstadoDePractica.Value.ToString());

            // Construir la URL con los filtros
            var queryString = string.Join("&", queryParams.Select(kvp => $"{kvp.Key}={Uri.EscapeDataString(kvp.Value)}"));
            var url = string.IsNullOrEmpty(queryString) ? _apiBaseUrl : $"{_apiBaseUrl}?{queryString}";

            HttpResponseMessage response = await _httpClient.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                string json = await response.Content.ReadAsStringAsync();
                practicasList = JsonConvert.DeserializeObject<List<PracticasAsistencialesViewModel>>(json);
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                return RedirectToAction("Login", "Authentication");
            }

            // Obtener la lista de TiposDePrácticas
            //HttpResponseMessage tipoDePracticaResponse = await _httpClient.GetAsync("https://localhost:7086/api/TiposDePracticas");
            HttpResponseMessage tipoDePracticaResponse = await _httpClient.GetAsync("https://soimsf.somee.com/api/TiposDePracticas");
            if (tipoDePracticaResponse.IsSuccessStatusCode)
            {
                string data = await tipoDePracticaResponse.Content.ReadAsStringAsync();
                tiposDePracticaList = JsonConvert.DeserializeObject<List<TiposDePracticasViewModel>>(data);
            }

            // Obtener la lista de TiposDeBeneficiarios
            //HttpResponseMessage tiposDeBeneficiariosResponse = await _httpClient.GetAsync("https://localhost:7086/api/TiposDeBeneficiarios");
            HttpResponseMessage tiposDeBeneficiariosResponse = await _httpClient.GetAsync("https://soimsf.somee.com/api/TiposDeBeneficiarios");
            if (tiposDeBeneficiariosResponse.IsSuccessStatusCode)
            {
                string data = await tiposDeBeneficiariosResponse.Content.ReadAsStringAsync();
                tiposDeBeneficiariosList = JsonConvert.DeserializeObject<List<TiposDeBeneficiariosViewModel>>(data);
            }

            // Obtener la lista de estadosDePracticas
            //HttpResponseMessage estadosDePracticasResponse = await _httpClient.GetAsync("https://localhost:7086/api/EstadosDePracticas");
            HttpResponseMessage estadosDePracticasResponse = await _httpClient.GetAsync("https://soimsf.somee.com/api/EstadosDePracticas");
            if (estadosDePracticasResponse.IsSuccessStatusCode)
            {
                string data = await estadosDePracticasResponse.Content.ReadAsStringAsync();
                estadosDePracticasList = JsonConvert.DeserializeObject<List<EstadosDePracticasViewModel>>(data);
            }

            // Obtener la lista de afiliados
            //HttpResponseMessage afiliadosResponse = await _httpClient.GetAsync("https://localhost:7086/api/Afiliados");
            HttpResponseMessage afiliadosResponse = await _httpClient.GetAsync("https://soimsf.somee.com/api/Afiliados");
            if (afiliadosResponse.IsSuccessStatusCode)
            {
                string data = await afiliadosResponse.Content.ReadAsStringAsync();
                afiliadosList = JsonConvert.DeserializeObject<List<AfiliadosViewModel>>(data);
            }

            // Crear diccionarios para los nombres relacionados
            ViewBag.TiposDePracticas = tiposDePracticaList.ToDictionary(e => e.Id, e => e.Nombre);
            ViewBag.TiposDeBeneficiarios = tiposDeBeneficiariosList.ToDictionary(s => s.Id, s => s.Nombre);
            ViewBag.EstadosDePracticas = estadosDePracticasList.ToDictionary(emp => emp.Id, emp => emp.Nombre);
            ViewBag.Afiliados = afiliadosList.ToDictionary(
                emp => emp.Id,                         // Clave: Id
                emp => $"{emp.Nombre} {emp.Apellido}"  // Valor: Nombre completo
            );

            // Verificar si se aplicaron filtros
            if (Fecha.HasValue || idTipo.HasValue || IdTipoDeBeneficiario.HasValue || IdAfiliado.HasValue || IdEstadoDePractica.HasValue)
            {
                TempData["AlertMessage"] = "¡Filtros aplicados con éxito!";
                TempData["AlertType"] = "primary"; // Azul (Bootstrap)
            }

            return View(practicasList);
        }

        // GET: PracticasAsistenciales/Details/5
        public async Task<IActionResult> Details(int id)
        {
            AddJwtToken(); // Agregar token JWT

            PracticasAsistencialesViewModel practica = null;
            var tiposDePracticasList = new List<TiposDePracticasViewModel>();
            var tiposDeBeneficiariosList = new List<TiposDeBeneficiariosViewModel>();
            var estadosDePracticasList = new List<EstadosDePracticasViewModel>();
            var afiliadosList = new List<AfiliadosViewModel>();

            // Obtener los detalles de la práctica asistencial
            HttpResponseMessage response = await _httpClient.GetAsync($"{_apiBaseUrl}/{id}");
            if (response.IsSuccessStatusCode)
            {
                string data = await response.Content.ReadAsStringAsync();
                practica = JsonConvert.DeserializeObject<PracticasAsistencialesViewModel>(data);
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                return RedirectToAction("Login", "Authentication");
            }

            if (practica == null)
            {
                return NotFound();
            }

            // Obtener la lista de TiposDePracticas
            //HttpResponseMessage tiposDePracticasResponse = await _httpClient.GetAsync("https://localhost:7086/api/TiposDePracticas");
            HttpResponseMessage tiposDePracticasResponse = await _httpClient.GetAsync("https://soimsf.somee.com/api/TiposDePracticas");
            if (tiposDePracticasResponse.IsSuccessStatusCode)
            {
                string data = await tiposDePracticasResponse.Content.ReadAsStringAsync();
                tiposDePracticasList = JsonConvert.DeserializeObject<List<TiposDePracticasViewModel>>(data);
            }

            // Obtener la lista de TiposDeBeneficiarios
            //HttpResponseMessage tiposDeBeneficiariosResponse = await _httpClient.GetAsync("https://localhost:7086/api/TiposDeBeneficiarios");
            HttpResponseMessage tiposDeBeneficiariosResponse = await _httpClient.GetAsync("https://soimsf.somee.com/api/TiposDeBeneficiarios");
            if (tiposDeBeneficiariosResponse.IsSuccessStatusCode)
            {
                string data = await tiposDeBeneficiariosResponse.Content.ReadAsStringAsync();
                tiposDeBeneficiariosList = JsonConvert.DeserializeObject<List<TiposDeBeneficiariosViewModel>>(data);
            }

            // Obtener la lista de EstadosDePracticas
            //HttpResponseMessage estadosDePracticasResponse = await _httpClient.GetAsync("https://localhost:7086/api/EstadosDePracticas");
            HttpResponseMessage estadosDePracticasResponse = await _httpClient.GetAsync("https://soimsf.somee.com/api/EstadosDePracticas");
            if (estadosDePracticasResponse.IsSuccessStatusCode)
            {
                string data = await estadosDePracticasResponse.Content.ReadAsStringAsync();
                estadosDePracticasList = JsonConvert.DeserializeObject<List<EstadosDePracticasViewModel>>(data);
            }

            // Obtener la lista de Afiliados
            //HttpResponseMessage afiliadosResponse = await _httpClient.GetAsync("https://localhost:7086/api/Afiliados");
            HttpResponseMessage afiliadosResponse = await _httpClient.GetAsync("https://soimsf.somee.com/api/Afiliados");
            if (afiliadosResponse.IsSuccessStatusCode)
            {
                string data = await afiliadosResponse.Content.ReadAsStringAsync();
                afiliadosList = JsonConvert.DeserializeObject<List<AfiliadosViewModel>>(data);
            }

            // Obtener los nombres relacionados
            ViewBag.IdTipoDePractica = tiposDePracticasList.FirstOrDefault(tp => tp.Id == practica.IdTipo)?.Nombre ?? "Desconocido";
            ViewBag.IdTipoDeBeneficiario = tiposDeBeneficiariosList.FirstOrDefault(tb => tb.Id == practica.IdTipoDeBeneficiario)?.Nombre ?? "Desconocido";
            ViewBag.IdEstadoPractica = estadosDePracticasList.FirstOrDefault(ep => ep.Id == practica.IdEstadoDePractica)?.Nombre ?? "Desconocido";
            ViewBag.IdAfiliado = afiliadosList.FirstOrDefault(a => a.Id == practica.IdAfiliado)?.Nombre ?? "Desconocido";

            return View(practica);
        }

        // GET: PracticasAsistenciales/Create
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> Create()
        {
            try
            {
                AddJwtToken(); // Agregar token JWT

                // Cargar Tipos de Prácticas
                var tiposDePracticasList = new List<TiposDePracticasViewModel>();
                //HttpResponseMessage tiposDePracticasResponse = await _httpClient.GetAsync("https://localhost:7086/api/TiposDePracticas");
                HttpResponseMessage tiposDePracticasResponse = await _httpClient.GetAsync("https://soimsf.somee.com/api/TiposDePracticas");

                if (tiposDePracticasResponse.IsSuccessStatusCode)
                {
                    string data = await tiposDePracticasResponse.Content.ReadAsStringAsync();
                    tiposDePracticasList = JsonConvert.DeserializeObject<List<TiposDePracticasViewModel>>(data);
                }
                ViewBag.IdTipoDePractica = new SelectList(tiposDePracticasList, "Id", "Nombre");

                // Cargar Tipos de Beneficiarios
                var tiposDeBeneficiariosList = new List<TiposDeBeneficiariosViewModel>();
                //HttpResponseMessage tiposDeBeneficiariosResponse = await _httpClient.GetAsync("https://localhost:7086/api/TiposDeBeneficiarios");
                HttpResponseMessage tiposDeBeneficiariosResponse = await _httpClient.GetAsync("https://soimsf.somee.com/api/TiposDeBeneficiarios");

                if (tiposDeBeneficiariosResponse.IsSuccessStatusCode)
                {
                    string data = await tiposDeBeneficiariosResponse.Content.ReadAsStringAsync();
                    tiposDeBeneficiariosList = JsonConvert.DeserializeObject<List<TiposDeBeneficiariosViewModel>>(data);
                }
                ViewBag.IdTipoDeBeneficiario = new SelectList(tiposDeBeneficiariosList, "Id", "Nombre");

                // Cargar Estados de Prácticas
                var estadosDePracticasList = new List<EstadosDePracticasViewModel>();
                //HttpResponseMessage estadosDePracticasResponse = await _httpClient.GetAsync("https://localhost:7086/api/EstadosDePracticas");
                HttpResponseMessage estadosDePracticasResponse = await _httpClient.GetAsync("https://soimsf.somee.com/api/EstadosDePracticas");

                if (estadosDePracticasResponse.IsSuccessStatusCode)
                {
                    string data = await estadosDePracticasResponse.Content.ReadAsStringAsync();
                    estadosDePracticasList = JsonConvert.DeserializeObject<List<EstadosDePracticasViewModel>>(data);
                }
                ViewBag.IdEstadoPractica = new SelectList(estadosDePracticasList, "Id", "Nombre");

                // Cargar Afiliados
                var afiliadosList = new List<AfiliadosViewModel>();
                //HttpResponseMessage afiliadosResponse = await _httpClient.GetAsync("https://localhost:7086/api/Afiliados");
                HttpResponseMessage afiliadosResponse = await _httpClient.GetAsync("https://soimsf.somee.com/api/Afiliados");

                if (afiliadosResponse.IsSuccessStatusCode)
                {
                    string data = await afiliadosResponse.Content.ReadAsStringAsync();
                    afiliadosList = JsonConvert.DeserializeObject<List<AfiliadosViewModel>>(data);

                    // Combinar Nombre y Apellido
                    var afiliadosDisplayList = afiliadosList
                        .Select(a => new
                        {
                            Id = a.Id,
                            NombreCompleto = $"{a.Nombre} {a.Apellido}"
                        })
                        .ToList();

                    ViewBag.IdAfiliado = new SelectList(afiliadosDisplayList, "Id", "NombreCompleto");
                }
                else
                {
                    ViewBag.IdAfiliado = new SelectList(Enumerable.Empty<SelectListItem>());
                }

                return View();
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error al cargar datos: {ex.Message}");
                return View();
            }
        }


        // POST: PracticasAsistenciales/Create
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(PracticasAsistencialesViewModel practica)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    AddJwtToken(); // Agregar token JWT

                    // Convertir el modelo de práctica asistencial a JSON
                    string jsonData = JsonConvert.SerializeObject(practica);
                    StringContent content = new StringContent(jsonData, Encoding.UTF8, "application/json");

                    HttpResponseMessage response = await _httpClient.PostAsync(_apiBaseUrl, content);

                    if (response.IsSuccessStatusCode)
                    {
                        TempData["AlertMessage"] = "¡Práctica Asistencial creado con éxito!";
                        TempData["AlertType"] = "success";
                        return RedirectToAction(nameof(Index));
                    }
                    else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    {
                        return RedirectToAction("Login", "Authentication");
                    }
                    else
                    {
                        ModelState.AddModelError("", "Error al guardar la práctica asistencial.");
                    }
                }

                // Cargar Tipos de Prácticas
                var tiposDePracticasList = new List<TiposDePracticasViewModel>();
                //HttpResponseMessage tiposDePracticasResponse = await _httpClient.GetAsync("https://localhost:7086/api/TiposDePracticas");
                HttpResponseMessage tiposDePracticasResponse = await _httpClient.GetAsync("https://soimsf.somee.com/api/TiposDePracticas");

                if (tiposDePracticasResponse.IsSuccessStatusCode)
                {
                    string data = await tiposDePracticasResponse.Content.ReadAsStringAsync();
                    tiposDePracticasList = JsonConvert.DeserializeObject<List<TiposDePracticasViewModel>>(data);
                }
                ViewBag.IdTipoDePractica = new SelectList(tiposDePracticasList, "Id", "Nombre", practica.IdTipo);

                // Cargar Tipos de Beneficiarios
                var tiposDeBeneficiariosList = new List<TiposDeBeneficiariosViewModel>();
                //HttpResponseMessage tiposDeBeneficiariosResponse = await _httpClient.GetAsync("https://localhost:7086/api/TiposDeBeneficiarios");
                HttpResponseMessage tiposDeBeneficiariosResponse = await _httpClient.GetAsync("https://soimsf.somee.com/api/TiposDeBeneficiarios");

                if (tiposDeBeneficiariosResponse.IsSuccessStatusCode)
                {
                    string data = await tiposDeBeneficiariosResponse.Content.ReadAsStringAsync();
                    tiposDeBeneficiariosList = JsonConvert.DeserializeObject<List<TiposDeBeneficiariosViewModel>>(data);
                }
                ViewBag.IdTipoDeBeneficiario = new SelectList(tiposDeBeneficiariosList, "Id", "Nombre", practica.IdTipoDeBeneficiario);

                // Cargar Estados de Prácticas
                var estadosDePracticasList = new List<EstadosDePracticasViewModel>();
                //HttpResponseMessage estadosDePracticasResponse = await _httpClient.GetAsync("https://localhost:7086/api/EstadosDePracticas");
                HttpResponseMessage estadosDePracticasResponse = await _httpClient.GetAsync("https://soimsf.somee.com/api/EstadosDePracticas");

                if (estadosDePracticasResponse.IsSuccessStatusCode)
                {
                    string data = await estadosDePracticasResponse.Content.ReadAsStringAsync();
                    estadosDePracticasList = JsonConvert.DeserializeObject<List<EstadosDePracticasViewModel>>(data);
                }
                ViewBag.IdEstadoPractica = new SelectList(estadosDePracticasList, "Id", "Nombre", practica.IdEstadoDePractica);

                // Cargar Afiliados
                var afiliadosList = new List<AfiliadosViewModel>();
                //HttpResponseMessage afiliadosResponse = await _httpClient.GetAsync("https://localhost:7086/api/Afiliados");
                HttpResponseMessage afiliadosResponse = await _httpClient.GetAsync("https://soimsf.somee.com/api/Afiliados");

                if (afiliadosResponse.IsSuccessStatusCode)
                {
                    string data = await afiliadosResponse.Content.ReadAsStringAsync();
                    afiliadosList = JsonConvert.DeserializeObject<List<AfiliadosViewModel>>(data);
                }
                ViewBag.IdAfiliado = new SelectList(afiliadosList, "Id", "Nombre", practica.IdAfiliado);

                return View(practica);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error: {ex.Message}");
                return View(practica);
            }
        }

        // GET: PracticasAsistenciales/Edit/5
        [Authorize(Roles = "Admin,Manager")]
        public async Task<ActionResult> Edit(int id)
        {
            AddJwtToken(); // Agregar token JWT

            PracticasAsistencialesViewModel practica = null;
            var tiposDePracticasList = new List<TiposDePracticasViewModel>();
            var tiposDeBeneficiariosList = new List<TiposDeBeneficiariosViewModel>();
            var estadosDePracticasList = new List<EstadosDePracticasViewModel>();
            var afiliadosList = new List<AfiliadosViewModel>();

            // Obtener datos de la práctica asistencial
            HttpResponseMessage practicaResponse = await _httpClient.GetAsync($"{_apiBaseUrl}/{id}");
            if (practicaResponse.IsSuccessStatusCode)
            {
                string practicaData = await practicaResponse.Content.ReadAsStringAsync();
                practica = JsonConvert.DeserializeObject<PracticasAsistencialesViewModel>(practicaData);
            }
            else if (practicaResponse.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                return RedirectToAction("Login", "Authentication");
            }

            if (practica == null)
            {
                return NotFound();
            }

            // Obtener la lista de Tipos de Prácticas
            //HttpResponseMessage tiposDePracticasResponse = await _httpClient.GetAsync("https://localhost:7086/api/TiposDePracticas");
            HttpResponseMessage tiposDePracticasResponse = await _httpClient.GetAsync("https://soimsf.somee.com/api/TiposDePracticas");
            if (tiposDePracticasResponse.IsSuccessStatusCode)
            {
                string tiposDePracticasData = await tiposDePracticasResponse.Content.ReadAsStringAsync();
                tiposDePracticasList = JsonConvert.DeserializeObject<List<TiposDePracticasViewModel>>(tiposDePracticasData);
            }

            // Obtener la lista de Tipos de Beneficiarios
            //HttpResponseMessage tiposDeBeneficiariosResponse = await _httpClient.GetAsync("https://localhost:7086/api/TiposDeBeneficiarios");
            HttpResponseMessage tiposDeBeneficiariosResponse = await _httpClient.GetAsync("https://soimsf.somee.com/api/TiposDeBeneficiarios");
            if (tiposDeBeneficiariosResponse.IsSuccessStatusCode)
            {
                string tiposDeBeneficiariosData = await tiposDeBeneficiariosResponse.Content.ReadAsStringAsync();
                tiposDeBeneficiariosList = JsonConvert.DeserializeObject<List<TiposDeBeneficiariosViewModel>>(tiposDeBeneficiariosData);
            }

            // Obtener la lista de Estados de Prácticas
            //HttpResponseMessage estadosDePracticasResponse = await _httpClient.GetAsync("https://localhost:7086/api/EstadosDePracticas");
            HttpResponseMessage estadosDePracticasResponse = await _httpClient.GetAsync("https://soimsf.somee.com/api/EstadosDePracticas");
            if (estadosDePracticasResponse.IsSuccessStatusCode)
            {
                string estadosDePracticasData = await estadosDePracticasResponse.Content.ReadAsStringAsync();
                estadosDePracticasList = JsonConvert.DeserializeObject<List<EstadosDePracticasViewModel>>(estadosDePracticasData);
            }

            // Obtener la lista de Afiliados
            //HttpResponseMessage afiliadosResponse = await _httpClient.GetAsync("https://localhost:7086/api/Afiliados");
            HttpResponseMessage afiliadosResponse = await _httpClient.GetAsync("https://soimsf.somee.com/api/Afiliados");
            if (afiliadosResponse.IsSuccessStatusCode)
            {
                string afiliadosData = await afiliadosResponse.Content.ReadAsStringAsync();
                afiliadosList = JsonConvert.DeserializeObject<List<AfiliadosViewModel>>(afiliadosData);
            }

            // Asignar listas al ViewBag
            ViewBag.IdTipoDePractica = new SelectList(tiposDePracticasList, "Id", "Nombre", practica.IdTipo);
            ViewBag.IdTipoDeBeneficiario = new SelectList(tiposDeBeneficiariosList, "Id", "Nombre", practica.IdTipoDeBeneficiario);
            ViewBag.IdEstadoPractica = new SelectList(estadosDePracticasList, "Id", "Nombre", practica.IdEstadoDePractica);
            ViewBag.IdAfiliado = new SelectList(afiliadosList, "Id", "Nombre", practica.IdAfiliado);

            return View(practica);
        }

        // POST: PracticasAsistenciales/Edit/5
        [Authorize(Roles = "Admin,Manager")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(PracticasAsistencialesViewModel practica)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    AddJwtToken(); // Agregar token JWT

                    // Serializar el objeto práctica asistencial y enviarlo a la API
                    string jsonData = JsonConvert.SerializeObject(practica);
                    StringContent content = new StringContent(jsonData, Encoding.UTF8, "application/json");

                    HttpResponseMessage response = await _httpClient.PutAsync($"{_apiBaseUrl}/{practica.Id}", content);

                    if (response.IsSuccessStatusCode)
                    {
                        TempData["AlertMessage"] = "¡Práctica Asistencial editado con éxito!";
                        TempData["AlertType"] = "warning";
                        return RedirectToAction(nameof(Index));
                    }
                    else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    {
                        return RedirectToAction("Login", "Authentication");
                    }
                    else
                    {
                        ModelState.AddModelError("", "Error al actualizar la práctica asistencial. Por favor, intenta nuevamente.");
                    }
                }

                // Si el modelo no es válido o hubo un error, recargar datos relacionados
                var tiposDePracticasList = new List<TiposDePracticasViewModel>();
                var tiposDeBeneficiariosList = new List<TiposDeBeneficiariosViewModel>();
                var estadosDePracticasList = new List<EstadosDePracticasViewModel>();
                var afiliadosList = new List<AfiliadosViewModel>();

                // Cargar Tipos de Prácticas
                //HttpResponseMessage tiposDePracticasResponse = await _httpClient.GetAsync("https://localhost:7086/api/TiposDePracticas");
                HttpResponseMessage tiposDePracticasResponse = await _httpClient.GetAsync("https://soimsf.somee.com/api/TiposDePracticas");
                if (tiposDePracticasResponse.IsSuccessStatusCode)
                {
                    string tiposDePracticasData = await tiposDePracticasResponse.Content.ReadAsStringAsync();
                    tiposDePracticasList = JsonConvert.DeserializeObject<List<TiposDePracticasViewModel>>(tiposDePracticasData);
                }

                // Cargar Tipos de Beneficiarios
                //HttpResponseMessage tiposDeBeneficiariosResponse = await _httpClient.GetAsync("https://localhost:7086/api/TiposDeBeneficiarios");
                HttpResponseMessage tiposDeBeneficiariosResponse = await _httpClient.GetAsync("https://soimsf.somee.com/api/TiposDeBeneficiarios");
                if (tiposDeBeneficiariosResponse.IsSuccessStatusCode)
                {
                    string tiposDeBeneficiariosData = await tiposDeBeneficiariosResponse.Content.ReadAsStringAsync();
                    tiposDeBeneficiariosList = JsonConvert.DeserializeObject<List<TiposDeBeneficiariosViewModel>>(tiposDeBeneficiariosData);
                }

                // Cargar Estados de Prácticas
                //HttpResponseMessage estadosDePracticasResponse = await _httpClient.GetAsync("https://localhost:7086/api/EstadosDePracticas");
                HttpResponseMessage estadosDePracticasResponse = await _httpClient.GetAsync("https://soimsf.somee.com/api/EstadosDePracticas");
                if (estadosDePracticasResponse.IsSuccessStatusCode)
                {
                    string estadosDePracticasData = await estadosDePracticasResponse.Content.ReadAsStringAsync();
                    estadosDePracticasList = JsonConvert.DeserializeObject<List<EstadosDePracticasViewModel>>(estadosDePracticasData);
                }

                // Cargar Afiliados
                //HttpResponseMessage afiliadosResponse = await _httpClient.GetAsync("https://localhost:7086/api/Afiliados");
                HttpResponseMessage afiliadosResponse = await _httpClient.GetAsync("https://soimsf.somee.com/api/Afiliados");
                if (afiliadosResponse.IsSuccessStatusCode)
                {
                    string afiliadosData = await afiliadosResponse.Content.ReadAsStringAsync();
                    afiliadosList = JsonConvert.DeserializeObject<List<AfiliadosViewModel>>(afiliadosData);
                }

                // Asignar listas al ViewBag
                ViewBag.IdTipoDePractica = new SelectList(tiposDePracticasList, "Id", "Nombre", practica.IdTipo);
                ViewBag.IdTipoDeBeneficiario = new SelectList(tiposDeBeneficiariosList, "Id", "Nombre", practica.IdTipoDeBeneficiario);
                ViewBag.IdEstadoPractica = new SelectList(estadosDePracticasList, "Id", "Nombre", practica.IdEstadoDePractica);
                ViewBag.IdAfiliado = new SelectList(afiliadosList, "Id", "Nombre", practica.IdAfiliado);

                return View(practica);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error inesperado: {ex.Message}");
                return View(practica);
            }
        }

        // GET: PracticasAsistenciales/Delete/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            AddJwtToken(); // Agregar token JWT

            PracticasAsistencialesViewModel practica = null;
            var tiposDePracticasList = new List<TiposDePracticasViewModel>();
            var tiposDeBeneficiariosList = new List<TiposDeBeneficiariosViewModel>();
            var estadosDePracticasList = new List<EstadosDePracticasViewModel>();
            var afiliadosList = new List<AfiliadosViewModel>();

            // Obtener los detalles de la práctica asistencial
            HttpResponseMessage response = await _httpClient.GetAsync($"{_apiBaseUrl}/{id}");
            if (response.IsSuccessStatusCode)
            {
                string data = await response.Content.ReadAsStringAsync();
                practica = JsonConvert.DeserializeObject<PracticasAsistencialesViewModel>(data);
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                return RedirectToAction("Login", "Authentication");
            }


            if (practica == null)
            {
                return NotFound();
            }

            // Obtener la lista de Tipos de Prácticas
            //HttpResponseMessage tiposDePracticasResponse = await _httpClient.GetAsync("https://localhost:7086/api/TiposDePracticas");
            HttpResponseMessage tiposDePracticasResponse = await _httpClient.GetAsync("https://soimsf.somee.com/api/TiposDePracticas");
            if (tiposDePracticasResponse.IsSuccessStatusCode)
            {
                string data = await tiposDePracticasResponse.Content.ReadAsStringAsync();
                tiposDePracticasList = JsonConvert.DeserializeObject<List<TiposDePracticasViewModel>>(data);
            }

            // Obtener la lista de Tipos de Beneficiarios
            //HttpResponseMessage tiposDeBeneficiariosResponse = await _httpClient.GetAsync("https://localhost:7086/api/TiposDeBeneficiarios");
            HttpResponseMessage tiposDeBeneficiariosResponse = await _httpClient.GetAsync("https://soimsf.somee.com/api/TiposDeBeneficiarios");
            if (tiposDeBeneficiariosResponse.IsSuccessStatusCode)
            {
                string data = await tiposDeBeneficiariosResponse.Content.ReadAsStringAsync();
                tiposDeBeneficiariosList = JsonConvert.DeserializeObject<List<TiposDeBeneficiariosViewModel>>(data);
            }

            // Obtener la lista de Estados de Prácticas
            //HttpResponseMessage estadosDePracticasResponse = await _httpClient.GetAsync("https://localhost:7086/api/EstadosDePracticas");
            HttpResponseMessage estadosDePracticasResponse = await _httpClient.GetAsync("https://soimsf.somee.com/api/EstadosDePracticas");
            if (estadosDePracticasResponse.IsSuccessStatusCode)
            {
                string data = await estadosDePracticasResponse.Content.ReadAsStringAsync();
                estadosDePracticasList = JsonConvert.DeserializeObject<List<EstadosDePracticasViewModel>>(data);
            }

            // Obtener la lista de Afiliados
            //HttpResponseMessage afiliadosResponse = await _httpClient.GetAsync("https://localhost:7086/api/Afiliados");
            HttpResponseMessage afiliadosResponse = await _httpClient.GetAsync("https://soimsf.somee.com/api/Afiliados");
            if (afiliadosResponse.IsSuccessStatusCode)
            {
                string data = await afiliadosResponse.Content.ReadAsStringAsync();
                afiliadosList = JsonConvert.DeserializeObject<List<AfiliadosViewModel>>(data);
            }

            // Obtener nombres de las relaciones
            ViewBag.IdTipoDePractica = tiposDePracticasList.FirstOrDefault(t => t.Id == practica.IdTipo)?.Nombre ?? "Desconocido";
            ViewBag.IdTipoDeBeneficiario = tiposDeBeneficiariosList.FirstOrDefault(b => b.Id == practica.IdTipoDeBeneficiario)?.Nombre ?? "Desconocido";
            ViewBag.IdEstadoPractica = estadosDePracticasList.FirstOrDefault(e => e.Id == practica.IdEstadoDePractica)?.Nombre ?? "Desconocido";
            ViewBag.IdAfiliado = afiliadosList.FirstOrDefault(a => a.Id == practica.IdAfiliado)?.Nombre ?? "Desconocido";

            return View(practica);
        }

        // POST: PracticasAsistenciales/Delete/5
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
                    TempData["AlertMessage"] = "¡Práctica Asistencial eliminado con éxito!";
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

        [Authorize(Roles = "Admin, Manager")]
        [HttpGet]
        public async Task<IActionResult> ExportarPracticasAsistenciales()
        {
            try
            {
                // Asegúrate de que el token JWT está en el encabezado
                AddJwtToken();

                // Llamar a la API para exportar empresas
                var response = await _httpClient.GetAsync($"{_apiBaseUrl}/ExportarPracticasAsistenciales");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsByteArrayAsync();
                    var fileName = "PracticasAsistenciales.xlsx";

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
        public async Task<IActionResult> ExportarPracticasAsistencialesPDF()
        {
            try
            {
                // Asegúrate de que el token JWT está en el encabezado
                AddJwtToken();

                // Llamar a la API para exportar las prácticas asistenciales en PDF
                var response = await _httpClient.GetAsync($"{_apiBaseUrl}/ExportarPracticasAsistencialesPDF");

                if (response.IsSuccessStatusCode)
                {
                    // Leer el contenido del archivo PDF
                    var content = await response.Content.ReadAsByteArrayAsync();
                    var fileName = "PracticasAsistenciales.pdf";

                    // Retorna el archivo como una descarga
                    return File(content, "application/pdf", fileName);
                }
                else
                {
                    // Manejar errores en la respuesta de la API
                    TempData["Error"] = "No se pudo exportar el archivo PDF. Intenta nuevamente.";
                    return RedirectToAction("Index");
                }
            }
            catch (Exception ex)
            {
                // Captura errores y los muestra en TempData
                TempData["Error"] = $"Ocurrió un error al exportar: {ex.Message}";
                return RedirectToAction("Index");
            }
        }
    }
}