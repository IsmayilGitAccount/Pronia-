using System.ComponentModel.DataAnnotations;

namespace ProniaApplication.ViewModels
{
    public class LoginVM
    {
        [MinLength(5)]
        [MaxLength(256)]
        public string UserOrEmail { get; set; }

        [DataType(DataType.Password)]
        [MinLength(8)]
        public string Password { get; set; } 
        public bool IsPersistent { get; set; }
    }
}
