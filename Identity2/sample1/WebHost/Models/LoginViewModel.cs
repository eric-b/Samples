using System.ComponentModel.DataAnnotations;

namespace Identity2.Sample1.WebHost.Models
{
    public class LoginViewModel
    {
        [Required]
        [Display(Name = "User name")]
        public string Username { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [Display(Name = "Remember ?")]
        public bool RememberMe { get; set; }
    }
}