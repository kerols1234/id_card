using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace idCard.Models
{
    public class RegisterModel
    {
        [Required]
        public string Name { get; set; }

        [EmailAddress]
        [Required]
        public string Email { get; set; }

        [Required]
        [StringLength(11, ErrorMessage = "Invalid Mobile Number", MinimumLength = 11)]
        [RegularExpression(@"^01([0-9]{9})", ErrorMessage = "Invalid Mobile Number")]
        public string PhoneNumber { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }
}