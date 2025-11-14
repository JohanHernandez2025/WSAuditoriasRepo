using Microsoft.EntityFrameworkCore;
using Auditorias.Domain.Entities;

namespace Auditorias.API.Infrastructure
{
    public class AuditoriasDbContext : DbContext
    {
        public AuditoriasDbContext(DbContextOptions<AuditoriasDbContext> options)
            : base(options)
        {
        }

        // Mapeos de las Tablas (DbSets)

        public DbSet<Auditoria> Auditorias { get; set; } = default!;
        public DbSet<Responsable> Responsables { get; set; } = default!; // 👈 AGREGADO
        public DbSet<Hallazgos> Hallazgos { get; set; } = default!;       // 👈 AGREGADO

        // Configuración de Mapeo y Relaciones
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // 1. Configuración de la Entidad Auditoria
            modelBuilder.Entity<Auditoria>()
                .HasOne(a => a.Responsable)
                .WithMany(r => r.Auditorias)
                .HasForeignKey(a => a.ResponsableId)
                .IsRequired();

            // Restricción CHECK para el campo Estado (Pendiente, En Proceso, Finalizada)
            modelBuilder.Entity<Auditoria>()
                .ToTable(t => t.HasCheckConstraint("CK_Auditorias_Estado", "[Estado] IN ('Pendiente', 'En Proceso', 'Finalizada')"));


            // 2. Configuración de la Entidad Hallazgo
            modelBuilder.Entity<Hallazgos>()
                .HasOne(h => h.Auditoria)
                .WithMany(a => a.Hallazgos)
                .HasForeignKey(h => h.AuditoriaId)
                .OnDelete(DeleteBehavior.Cascade) // Implementa ON DELETE CASCADE de tu script SQL
                .IsRequired();

            // Restricciones CHECK para Tipo y Severidad
            modelBuilder.Entity<Hallazgos>()
                .ToTable(t => t.HasCheckConstraint("CK_Hallazgos_Tipo", "[Tipo] IN ('Observacion', 'No Conformidad')"));

            modelBuilder.Entity<Hallazgos>()
                .ToTable(t => t.HasCheckConstraint("CK_Hallazgos_Severidad", "[Severidad] IN ('Baja', 'Media', 'Alta')"));
        }
    }
}