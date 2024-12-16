using System.ComponentModel.DataAnnotations;

namespace ProniaApplication.ViewModels
{
    public class RegisterVM
    {
        [MinLength(3, ErrorMessage = "Name must contain at least 3 letters!")]
        [MaxLength(30, ErrorMessage = "Name must be less than 30 letters!")]
        public string Name { get; set; }

        [MinLength(3, ErrorMessage = "Surname must contain at least 3 letters!")]
        [MaxLength(50, ErrorMessage = "Surname must be less than 50 letters!")]
        public string Surname { get; set; }

        [MinLength(5, ErrorMessage = "Username must contain at least 5 symbols!")]
        [MaxLength(50, ErrorMessage = "Username must be less than 50 symbols!")]
        public string UserName { get; set; }

        [MaxLength(256, ErrorMessage = "Email must be less than 256 symbols!")]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }

        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Compare(nameof(Password), ErrorMessage = "Confirm Password doesn't match with Password, Try again!")]
        public string ConfirmPassword { get; set; }

    }
}
