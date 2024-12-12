using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace MVC.SOIMSF.Models
{
    // LocalidadesViewModel se utiliza para capturar y validar los datos de
    // localidades , facilitando la transferencia segura de información entre la
    // interfaz MVC y la API. Incluye validaciones para asegurar datos correctos.
    public class LocalidadesViewModel
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("nombre")]
        [Required(ErrorMessage = "El nombre de la localidad es obligatorio")]
        public string Nombre { get; set; }
    }
}
