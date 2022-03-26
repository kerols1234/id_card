using System;
using System.ComponentModel.DataAnnotations;

namespace idCard.Models
{
    public class IdCard
    {
        [Key]
        public string NationalId { get; set; }
        [Required]
        public string FirstName { get; set; }

        [Required]
        public string FullName { get; set; }

        [Required]
        public string Address { get; set; }

        [Required]
        public string Governorate { get; set; }

        [Required]
        public DateTime BirthDate { get; set; }

        [Required]
        public DateTime ExpiryDate { get; set; }

    }
}
