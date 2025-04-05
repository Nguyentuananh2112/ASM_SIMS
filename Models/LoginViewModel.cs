using System.ComponentModel.DataAnnotations;

namespace ASM_SIMS.Models
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Email can not empty")]
        [EmailAddress(ErrorMessage = "Email is not valid")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }
}
