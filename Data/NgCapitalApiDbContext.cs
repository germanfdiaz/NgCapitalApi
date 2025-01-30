using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Cotizaciones;
using Microsoft.EntityFrameworkCore;
using NgCapitalApi.Models;
using Pomelo.EntityFrameworkCore;
using NgCapitalApi.Models;

namespace NgCapitalApi.Data
{
    public class NgCapitalApiDbContext : DbContext
    {
        public NgCapitalApiDbContext(DbContextOptions<NgCapitalApiDbContext> options) : base(options)
        {
        }

        public DbSet<Usuario> Usuarios { get; set; }

        public DbSet<Titulo> Cotizaciones { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder) 
        { 
            modelBuilder.Entity<Titulo>(entity => 
            { 
                entity.ToTable("Cotizaciones"); 
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Instrumento).HasColumnName("Instrumento");
                entity.Property(e => e.Simbolo).HasColumnName("Simbolo");
                entity.Property(e => e.PuntaCantidadCompra).HasColumnName("PuntaCantidadCompra");
                entity.Property(e => e.PuntaPrecioCompra).HasColumnName("PuntaPrecioCompra");
                entity.Property(e => e.PuntaPrecioVenta).HasColumnName("PuntaPrecioVenta");
                entity.Property(e => e.PuntaCantidadVenta).HasColumnName("PuntaCantidadVenta");
                entity.Property(e => e.UltimoPrecio).HasColumnName("UltimoPrecio");
                entity.Property(e => e.VariacionPorcentual).HasColumnName("VariacionPorcentual"); 
                entity.Property(e => e.Apertura).HasColumnName("Apertura"); 
                entity.Property(e => e.Maximo).HasColumnName("Maximo"); 
                entity.Property(e => e.Minimo).HasColumnName("Minimo"); 
                entity.Property(e => e.UltimoCierre).HasColumnName("UltimoCierre"); 
                entity.Property(e => e.Volumen).HasColumnName("Volumen"); 
                entity.Property(e => e.CantidadOperaciones).HasColumnName("CantidadOperaciones"); 
                entity.Property(e => e.Fecha).HasColumnName("Fecha"); 
                entity.Property(e => e.TipoOpcion).HasColumnName("TipoOpcion"); 
                entity.Property(e => e.PrecioEjercicio).HasColumnName("PrecioEjercicio"); 
                entity.Property(e => e.FechaVencimiento).HasColumnName("FechaVencimiento"); 
                entity.Property(e => e.Mercado).HasColumnName("Mercado"); 
                entity.Property(e => e.Moneda).HasColumnName("Moneda"); 
                entity.Property(e => e.Descripcion).HasColumnName("Descripcion"); 
                entity.Property(e => e.Plazo).HasColumnName("Plazo"); 
                entity.Property(e => e.LaminaMinima).HasColumnName("LaminaMinima"); 
                entity.Property(e => e.Lote).HasColumnName("Lote");
                entity.Property(e => e.FechaRegistro).HasColumnName("FechaRegistro");
            }); 
        }
    }
}