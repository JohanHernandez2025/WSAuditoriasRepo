using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Auditorias.Domain.Entities;
using Auditorias.API.Infrastructure;
using Microsoft.Extensions.Logging;

namespace Auditorias.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ResponsablesController : ControllerBase
    {
        private readonly AuditoriasDbContext _context;
        private readonly ILogger<ResponsablesController> _logger;

        public ResponsablesController(AuditoriasDbContext context, ILogger<ResponsablesController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // -----------------------------------------------------------
        // GET: Consultar TODOS los Responsables
        // -----------------------------------------------------------

        /// <summary>
        /// Obtiene una lista de todos los Responsables registrados.
        /// </summary>
        /// <returns>Una lista de entidades Responsable.</returns>
        // GET: api/Responsables
       
        [HttpGet()]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<object>> GetResponsables()
        {
            var responsables = await _context.Responsables.ToListAsync();

            _logger.LogInformation("Consulta exitosa de {Count} responsables.", responsables.Count);

            return Ok(new
            {
                Mensaje = $"Consulta exitosa. Se encontraron {responsables.Count} responsables.",
                Responsables = responsables
            });
        }

        // -----------------------------------------------------------
        // GET: Consultar Responsable por ID
        // -----------------------------------------------------------

        /// <summary>
        /// Obtiene los detalles de un Responsable específico por su ID.
        /// </summary>
        /// <param name="id">El ID del Responsable a consultar.</param>
        /// <returns>La entidad Responsable o un código 404 si no se encuentra.</returns>
 
        // GET: api/Responsables/5
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<object>> GetResponsable(int id)
        {
            var responsable = await _context.Responsables.FindAsync(id);

            if (responsable == null)
            {
                _logger.LogWarning("Consulta de Responsable fallida. ID {ResponsableId} no encontrado.", id);
                return NotFound(new { Mensaje = $"Error: Responsable ID {id} no encontrado." });
            }

            _logger.LogInformation("Consulta de Responsable ID {ResponsableId} exitosa.", id);

            return Ok(new
            {
                Mensaje = $"Consulta exitosa. Se encontró el Responsable ID {id}.",
                Responsable = responsable
            });
        }


        // -----------------------------------------------------------
        // GET: Eliminar Responsable por ID
        // -----------------------------------------------------------

        /// <summary>
        /// Elimina un Responsable específico por su ID.
        /// </summary>
        /// <param name="id">El ID del Responsable a eliminar.</param>
        /// <returns>200 OK con mensaje de éxito o 404 si no se encuentra.</returns>
        /// 

        // DELETE: api/Responsables/5
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteResponsable(int id)
        {
            // 1. Buscar el responsable
            var responsable = await _context.Responsables.FindAsync(id);

            if (responsable == null)
            {
                _logger.LogWarning("Intento de eliminación de Responsable fallida. ID {ResponsableId} no encontrado.", id);
                return NotFound(new { Mensaje = $"Error: Responsable ID {id} no encontrado." });
            }

            // 2. Eliminar y guardar
         
            _context.Responsables.Remove(responsable);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Responsable ID {ResponsableId} eliminado exitosamente.", id);

            // 3. Respuesta Exitosa
            return Ok(new { Mensaje = $"Responsable ID {id} ('{responsable.Nombre}') eliminado exitosamente." });
        }

        // -----------------------------------------------------------
        // GET: Registrar responsables con nombre, correo y área.
        // -----------------------------------------------------------

        /// <summary>
        /// Registra un nuevo Responsable con su nombre, correo y área.
        /// </summary>
        /// <param name="responsable">Datos del Responsable a crear.</param>
        /// <returns>El nuevo Responsable creado con mensaje de éxito (201 Created).</returns>
        // POST: api/Responsables
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<object>> PostResponsable([FromBody] Responsable responsable)
        {
            // 1. Validaciones opcionales (ej: si ya existe el correo, etc.)
            // Puedes agregar validaciones aquí antes de guardar si son necesarias.

            // 2. Agregar al contexto y guardar
            _context.Responsables.Add(responsable);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Responsable ID {ResponsableId} registrado exitosamente.", responsable.Id);

            // 3. Respuesta Exitosa (201 Created)
            // Se asume la existencia de un método GET para Responsables (ej: GetResponsable)
            return CreatedAtAction(
                nameof(GetResponsable),
                new { id = responsable.Id },
                new
                {
                    Mensaje = $"Responsable '{responsable.Nombre}' registrado exitosamente. ID generado: {responsable.Id}.",
                    Responsable = responsable // Devuelve el objeto recién creado con el ID
                }
            );
        }

        // -----------------------------------------------------------
        // Put: Asigna o actualiza el Responsable principal de una Auditoría específica.
        // -----------------------------------------------------------

        /// <summary>
        /// Asigna o actualiza el Responsable principal de una Auditoría específica.
        /// </summary>
        /// <param name="auditoriaId">El ID de la Auditoría a modificar.</param>
        /// <param name="responsableId">El ID del Responsable a asignar.</param>
        /// <returns>La Auditoría modificada con el nuevo Responsable.</returns>

        // PUT: api/Auditorias/AsignarResponsable/5?responsableId=2
        [HttpPut("AsignarResponsable/{auditoriaId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<object>> AsignarResponsable(int auditoriaId, [FromQuery] int responsableId)
        {
            // 1. Buscar la Auditoría
            var auditoria = await _context.Auditorias.FindAsync(auditoriaId);
            if (auditoria == null)
            {
                _logger.LogWarning("Asignación de Responsable fallida. Auditoría ID {AuditoriaId} no encontrada.", auditoriaId);
                return NotFound(new { Mensaje = $"Error: Auditoría ID {auditoriaId} no encontrada." });
            }

            // 2. Validar que el Responsable exista
            var responsable = await _context.Responsables.FindAsync(responsableId);
            if (responsable == null)
            {
                _logger.LogWarning("Asignación de Responsable fallida. Responsable ID {ResponsableId} no encontrado.", responsableId);
                return NotFound(new { Mensaje = $"Error: Responsable ID {responsableId} no encontrado." });
            }

            // 3. Aplicar la asignación y guardar
            auditoria.ResponsableId = responsableId; // Asignar la FK
            auditoria.Responsable = responsable;     // (Opcional) Asignar el objeto para seguimiento inmediato

            // EF Core detectará el cambio en el campo ResponsableId y lo actualizará
            await _context.SaveChangesAsync();

            _logger.LogInformation("Responsable ID {ResponsableId} asignado a Auditoría ID {AuditoriaId} con éxito.", responsableId, auditoriaId);

            // 4. Devolver la Auditoría actualizada
            return Ok(new
            {
                Mensaje = $"Responsable '{responsable.Nombre}' asignado a la Auditoría ID {auditoriaId}.",
                Auditoria = auditoria
            });
        }

        // -----------------------------------------------------------
        // Get: Consulta todas las Auditorías asignadas a un Responsable específico.
        // -----------------------------------------------------------

        /// <summary>
        /// Consulta todas las Auditorías asignadas a un Responsable específico.
        /// </summary>
        /// <param name="responsableId">El ID del Responsable para el cual se buscan las Auditorías (parámetro de consulta).</param>
        /// <returns>Una lista de Auditorías relacionadas con el Responsable.</returns>

        // PUNTO DE ACCESO: GET: api/Auditorias/PorResponsable?responsableId=2
        [HttpGet("PorResponsable")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<object>> GetAuditoriasPorResponsable([FromQuery] int responsableId)
        {
            // 1. Validar que el Responsable exista (buena práctica para mensajes claros)
            var responsable = await _context.Responsables.FindAsync(responsableId);
            if (responsable == null)
            {
                // Devolver 404 si el ID del responsable no existe
                _logger.LogWarning("Consulta de Auditorías fallida. Responsable ID {ResponsableId} no encontrado.", responsableId);
                return NotFound(new { Mensaje = $"Error: Responsable ID {responsableId} no encontrado." });
            }

            // 2. Filtrar las Auditorías por el ResponsableId, incluyendo el objeto Responsable para el contexto
            var auditorias = await _context.Auditorias
                .Where(a => a.ResponsableId == responsableId)
                .Include(a => a.Responsable) // Incluye el Responsable asociado si lo necesitas en el detalle
                .ToListAsync();

            _logger.LogInformation("Consulta exitosa. {Count} auditorías encontradas para Responsable ID {ResponsableId}.", auditorias.Count, responsableId);

            // 3. Devolver el resultado con un mensaje amigable
            return Ok(new
            {
                Mensaje = $"Se encontraron {auditorias.Count} Auditorías a cargo del Responsable ID {responsableId} ('{responsable.Nombre}').",
                Auditorias = auditorias
            });
        }
    }
}
