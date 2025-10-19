using Microsoft.EntityFrameworkCore;
using ReclamacoesBank.Domain.Entites;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ReclamacoesBank.Infra.Persistence
{
    public class ReclamationContext : DbContext
    {
        public DbSet<Reclamation> Reclamations { get; set; }

        public ReclamationContext(DbContextOptions<ReclamationContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Reclamation>().ToTable("Reclamacoes");

            modelBuilder.Entity<Reclamation>().HasKey(r => r.Id);

            modelBuilder.Entity<Reclamation>()
                .Property(r => r.ClassifiedCategories)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null),
                    v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions)null)
                );
        }
    }
}
