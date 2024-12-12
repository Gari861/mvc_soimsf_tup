using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations;

namespace MVC.SOIMSF.Models
{
    // PracticasAsistencialesViewModel se utiliza para capturar y validar los datos de
    // Prácticas Asistenciales, facilitando la transferencia segura de información entre la
    // interfaz MVC y la API. Incluye validaciones para asegurar datos correctos.
    public class PracticasAsistencialesViewModel
    {
        [JsonProperty("id")]
        public int Id { get; set; } // ID de la práctica

        // Relación con Tipo de Práctica
        [JsonProperty("idTipo")]
        [Required(ErrorMessage = "El tipo de práctica es obligatorio")]
        public int IdTipo { get; set; }

        // Detalles de la práctica
        [JsonProperty("precio")]
        [Required(ErrorMessage = "El precio de la práctica es obligatorio")]
        [Range(0, double.MaxValue, ErrorMessage = "El precio no puede ser negativo")]
        public decimal Precio { get; set; }

        [JsonProperty("fecha")]
        [Required(ErrorMessage = "La fecha de la práctica es obligatoria")]
        [DataType(DataType.Date)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy-MM-dd}")]
        public DateTime Fecha { get; set; }

        [JsonProperty("observacion")]
        public string? Observacion { get; set; }

        // Relación con Afiliado
        [JsonProperty("idAfiliado")]
        [Required(ErrorMessage = "El afiliado es obligatorio")]
        public int IdAfiliado { get; set; }

        // Relación con Tipo de Beneficiario
        [JsonProperty("idTipoDeBeneficiario")]
        [Required(ErrorMessage = "El tipo de beneficiario es obligatorio")]
        public int IdTipoDeBeneficiario { get; set; }

        // Relación con Estado de Práctica
        [JsonProperty("idEstadoDePractica")]
        [Required(ErrorMessage = "El estado de la práctica es obligatorio")]
        public int IdEstadoDePractica { get; set; }
    }
}
