using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace idCard.Models
{
    public class IdCard
    {
        public IdCard()
        {
            Attachments = new HashSet<Attachment>();
        }

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
        public int Sex { get; set; }

        [Required]
        public String BirthDate { get; set; }

        [Required]
        public String ExpiryDate { get; set; }

        public virtual ICollection<Attachment> Attachments { get; set; }
    }
}
