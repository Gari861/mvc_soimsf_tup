using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace MVC.SOIMSF.Models
{
    // SituacionesDeRevistaViewModel se utiliza para capturar y validar los datos de
    // situaciones de revista, facilitando la transferencia segura de información entre la
    // interfaz MVC y la API. Incluye validaciones para asegurar datos correctos.
    public class SituacionesDeRevistaViewModel
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("nombre")]
        [Required(ErrorMessage = "El nombre de la situación es obligatorio")]
        public string Nombre { get; set; }
    }
}
