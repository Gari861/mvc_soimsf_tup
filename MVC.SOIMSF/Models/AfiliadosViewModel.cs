using Microsoft.IdentityModel.Tokens;
using Microsoft.VisualBasic;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using static System.Runtime.InteropServices.JavaScript.JSType;
namespace MVC.SOIMSF.Models
{
    // AfiliadosViewModel se utiliza para capturar y validar los datos de
    // afiliados, facilitando la transferencia segura de información entre la interfaz
    // MVC y la API. Incluye validaciones para asegurar datos correctos.
    public class AfiliadosViewModel
    {
        [JsonProperty("id")]
        public int Id { get; set; }
        [JsonProperty("nombre")]
        [Required(ErrorMessage = "El nombre es obligatorio")]
        public string Nombre { get; set; }
        [JsonProperty("apellido")]
        [Required(ErrorMessage = "El apellido es obligatorio")]
        public string Apellido { get; set; }

        [JsonProperty("cuil")]
        [Required(ErrorMessage = "El CUIL es obligatorio")]
        [RegularExpression(@"^[0-9\- ]+$", ErrorMessage = "El CUIL solo puede contener números, guiones y espacios")]
        public string Cuil { get; set; }

        [JsonProperty("telefono")]
        [RegularExpression(@"^[0-9\-\+]+$", ErrorMessage = "El teléfono solo puede contener números, guiones y el signo '+'")]
        public string? Telefono { get; set; }

        [JsonProperty("email")]
        [EmailAddress(ErrorMessage = "El correo electrónico no tiene un formato válido")]
        public string? Email { get; set; }

        [JsonProperty("fechaNacimiento")]
        [DataType(DataType.Date)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy-MM-dd}")]
        public DateTime? FechaNacimiento { get; set; }

        [JsonProperty("direccion")]
        public string? Direccion { get; set; }

        [JsonProperty("discapacidad")]
        public bool? Discapacidad { get; set; }

        // Relaciones con IDs para selección
        [JsonProperty("idEstadoAfiliado")]
        public int? IdEstadoAfiliado { get; set; }

        [JsonProperty("idSituacionDeRevista")]
        public int? IdSituacionDeRevista { get; set; }

        [JsonProperty("idEmpresa")]
        public int? IdEmpresa { get; set; }
    }
}
