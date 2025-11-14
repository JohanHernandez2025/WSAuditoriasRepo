
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Auditorias.Domain.Entities
{
    [Table("Hallazgos")]
    public class Hallazgos
    {
        // PK: Id INT IDENTITY(1,1) PRIMARY KEY
        [Key]
      
        public int Id { get; set; }

        [Required]
        public string Descripcion { get; set; } = string.Empty;

        public DateTime FechaDeteccion { get; set; } = DateTime.Now;

        [Required]
        [MaxLength(20)]
        public string Tipo { get; set; } = string.Empty; // 'Observacion' o 'No Conformidad'

        [Required]
        [MaxLength(10)]
        public string Severidad { get; set; } = string.Empty; // 'Baja', 'Media', o 'Alta'

        // --- Clave Foránea (FK) ---

        [Required]
        [ForeignKey("Auditoria")]
        public int AuditoriaId { get; set; }

        public Auditoria Auditoria { get; set; } = null!;

    }
}
