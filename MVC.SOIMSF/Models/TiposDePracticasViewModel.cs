using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace MVC.SOIMSF.Models
{
    // TiposDePracticasViewModel se utiliza para capturar y validar los datos de
    // tipos de Prácticas, facilitando la transferencia segura de información entre la
    // interfaz MVC y la API. Incluye validaciones para asegurar datos correctos.
    public class TiposDePracticasViewModel
    {
        [JsonProperty("id")]
        public int Id { get; set; }
        
        [JsonProperty("nombre")]
        [Required(ErrorMessage = "El nombre del tipo de práctica es obligatorio")]
        public string Nombre { get; set; }

        [JsonProperty("descripcion")]
        public string? Descripcion { get; set; }
    }
}
