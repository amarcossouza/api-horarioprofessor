using API_HorarioProfessor.Models;
using Microsoft.EntityFrameworkCore;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;

namespace API_HorarioProfessor.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
  
        public DbSet<ParametroDiarioOficial> ParametrosDiarioOficial { get; set; }
        public DbSet<Escola> Escolas { get; set; }
        public DbSet<UsuarioAcesso> UsuariosAcesso { get; set; }
        public DbSet<PesquisaLog> PesquisaLogs { get; set; }  // Adicione a nova tabela aqui
        public DbSet<ConfiguracaoAcao> ConfiguracoesAcao { get; set; }  // Adicionando a nova DbSet

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseMySql("Server=mysql5044.site4now.net;Port=3306;Database=db_a657ca_dbhorario_1;User=a657ca_dbhorario_1;Password=dbhorario123!@#;Pooling=true;Max Pool Size=100;",
                    new MySqlServerVersion(new Version(8, 0, 21)),
                    mySqlOptions => mySqlOptions.EnableRetryOnFailure(
                        maxRetryCount: 5,
                        maxRetryDelay: TimeSpan.FromSeconds(30),
                        errorNumbersToAdd: null
                    ));
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Escola>().ToTable("hprof_escolas");
            modelBuilder.Entity<UsuarioAcesso>().ToTable("hprof_usuariosacesso");
            modelBuilder.Entity<PesquisaLog>().ToTable("hprof_pesquisalog");
            modelBuilder.Entity<ConfiguracaoAcao>().ToTable("hprof_configuracaoacao");

            // Configurar índices
            modelBuilder.Entity<PesquisaLog>()
                .HasIndex(p => p.EscolaId)
                .HasDatabaseName("idx_pesquisa_log_escola_id");

            modelBuilder.Entity<PesquisaLog>()
                .HasIndex(p => p.UsuarioId)
                .HasDatabaseName("idx_pesquisa_log_usuario_id");

            modelBuilder.Entity<PesquisaLog>()
                .HasIndex(p => p.DataHoraPesquisa)
                .HasDatabaseName("idx_pesquisa_log_datahora_pesquisa");

            modelBuilder.Entity<PesquisaLog>()
                .HasIndex(p => p.QuantidadePesquisas)
                .HasDatabaseName("idx_pesquisa_log_quantidade_pesquisas");
        }
    }
}
