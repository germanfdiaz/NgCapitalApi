using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NgCapitalApi.Models;
using Pomelo.EntityFrameworkCore;

namespace NgCapitalApi.Data
{
    public class NgCapitalApiDbContext : DbContext
    {
        public NgCapitalApiDbContext(DbContextOptions<NgCapitalApiDbContext> options) : base(options)
        {
        }

        public DbSet<Usuario> Usuarios   
        { get; set; }
    }
}