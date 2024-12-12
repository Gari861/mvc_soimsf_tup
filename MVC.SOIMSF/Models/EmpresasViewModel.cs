using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace MVC.SOIMSF.Models
{
    // EmpresasViewModel se utiliza para capturar y validar los datos de una
    // empresa, facilitando la transferencia segura de información entre la interfaz
    // MVC y la API. Incluye validaciones para asegurar datos correctos.
    public class EmpresasViewModel
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("nombre")]
        [Required(ErrorMessage = "El nombre es obligatorio")]
        public string Nombre { get; set; }

        [JsonProperty("direccion")]
        [Required(ErrorMessage = "La dirección es obligatoria")]
        public string Direccion { get; set; }

        [JsonProperty("telefono")]
        [Required(ErrorMessage = "El teléfono es obligatorio")]
        [RegularExpression(@"^[0-9\-\+]+$", ErrorMessage = "El teléfono solo puede contener números, guiones y el signo '+'")]
        public string Telefono { get; set; }

        [JsonProperty("email")]
        [Required(ErrorMessage = "El email es obligatorio")]
        [EmailAddress(ErrorMessage = "El correo electrónico no tiene un formato válido")]
        public string Email { get; set; }

        [JsonProperty("cuit")]
        [Required(ErrorMessage = "El CUIT es obligatorio")]
        [RegularExpression(@"^[0-9\-]+$", ErrorMessage = "El CUIT solo puede contener números y guiones")]
        public string Cuit { get; set; }

        [JsonProperty("idLocalidad")]
        public int? IdLocalidad { get; set; }
    }
}
