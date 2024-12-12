using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages;
using System.ComponentModel.DataAnnotations;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace MVC.SOIMSF.Models
{
    // UsersViewModel en MVC sirve como intermediario entre la interfaz de usuario y el controlador,
    // validando que los datos ingresados para el login sean correctos y cumplan con los requisitos
    // como el formato de correo electrónico y la seguridad de la contraseña.
    public class UsersViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }
}
