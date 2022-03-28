using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace idCard.Models
{
    public class Filesmodel
    {
        [Required]
        public string NationalId { get; set; }

        public List<IFormFile> files { get; set; }
    }
}
