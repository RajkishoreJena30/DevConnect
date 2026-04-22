using Microsoft.EntityFrameworkCore;
using DevConnect.Models;

namespace DevConnect.Data
{
    public class DevConnectDbContext:DbContext
    {
        public DevConnectDbContext(DbContextOptions<DevConnectDbContext> options) : base(options) { }
        public DbSet<User> Users { get; set; }

    }
}
