using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Auditorias.Domain.Entities
{
    [Table("Responsables")]
    public class Responsable
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Nombre { get; set; } = string.Empty;

        public string Correo { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string Area { get; set; } = string.Empty;

        [JsonIgnore]
        public ICollection<Auditoria> Auditorias { get; set; } = new List<Auditoria>();
    }
}
