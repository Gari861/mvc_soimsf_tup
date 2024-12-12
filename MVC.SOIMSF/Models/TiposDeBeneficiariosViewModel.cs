using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace MVC.SOIMSF.Models
{
    // TiposDeBeneficiariosViewModel se utiliza para capturar y validar los datos de
    // tipos de beneficiarios, facilitando la transferencia segura de información entre la
    // interfaz MVC y la API. Incluye validaciones para asegurar datos correctos.
    public class TiposDeBeneficiariosViewModel
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("nombre")]
        [Required(ErrorMessage = "El nombre del tipo de beneficiario es obligatorio")]
        public string Nombre { get; set; }
    }
}
