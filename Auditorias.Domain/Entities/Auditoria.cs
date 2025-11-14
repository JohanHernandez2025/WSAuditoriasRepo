using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Auditorias.Domain.Entities
{

    [Table("Auditorias")]
    public class Auditoria
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(255)] // Ajustado de 200 a 255 según tu script SQL
        public string Titulo { get; set; } = string.Empty;

        [Required]
        public DateTime FechaInicio { get; set; }

        [Required]
        public DateTime FechaFin { get; set; }

        // Campo para el control de flujo
        [Required]
        [MaxLength(20)]
        public string Estado { get; set; } = "Pendiente"; // CHECK (Estado IN ('Pendiente', 'En Proceso', 'Finalizada'))

        [Required]
        [MaxLength(50)]
        public string AreaAuditada { get; set; } = string.Empty;

        // CLAVE FORÁNEA a Responsables
        [Required]
        public int ResponsableId { get; set; }

        public Responsable? Responsable { get; set; }

        public ICollection<Hallazgos> Hallazgos { get; set; } = new List<Hallazgos>();

    }
}
