using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace MVC.SOIMSF.Models
{
    // EstadosDePracticasViewModel se utiliza para capturar y validar los datos de
    // Estados de las prácticas , facilitando la transferencia segura de información entre la
    // interfaz MVC y la API. Incluye validaciones para asegurar datos correctos.
    public class EstadosDePracticasViewModel
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("nombre")]
        [Required(ErrorMessage = "El nombre del estado es obligatorio")]
        public string Nombre { get; set; }
    }
}
