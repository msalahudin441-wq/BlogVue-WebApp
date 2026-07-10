using System.ComponentModel.DataAnnotations;

namespace BlogVue.Models.ViewModels
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage="Email is required")]
        [EmailAddress(ErrorMessage ="Format not correct")]
        public string Email  { get; set; }
        [Required(ErrorMessage = "Password is required")]
        [DataType(DataType.Password)]
        public string Password {  get; set; }
        [Compare("Password",ErrorMessage ="Password must match the confirm Password")]
        [DataType(DataType.Password)]
        public string ConfirmPassword {  get; set; }
    }
}
