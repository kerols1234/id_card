using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace idCard.Models
{
    public class User : IdentityUser
    {
        public string Name { get; set; }
    }
}