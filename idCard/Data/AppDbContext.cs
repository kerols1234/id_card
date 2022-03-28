using idCard.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace idCard.Data
{
    public class AppDbContext : IdentityDbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> op) : base(op)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<IdCard> IdCards { get; set; }
        public DbSet<Attachment> Attachments { get; set; }
    }
}