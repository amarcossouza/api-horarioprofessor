using API_HorarioProfessor.Models;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace API_HorarioProfessor.Data
{
    public class HorarioProfessorDbContext : DbContext
    {
        public HorarioProfessorDbContext(DbContextOptions<HorarioProfessorDbContext> options) : base(options) { }

        public DbSet<Professor> Professores { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Professor>()
                .HasKey(p => p.Id);

            modelBuilder.Entity<Professor>()
                .Property(p => p.DistribuicaoTurmas)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null),
                    v => JsonSerializer.Deserialize<Dictionary<string, List<string>>>(v, (JsonSerializerOptions)null));
        }
    }
}
