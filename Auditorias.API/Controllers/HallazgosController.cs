using Auditorias.API.Infrastructure;
using Auditorias.Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Serilog.Core;

namespace Auditorias.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HallazgosController : ControllerBase
    {
        private readonly AuditoriasDbContext _context;
        private readonly ILogger<HallazgosController> _logger;

        public HallazgosController(AuditoriasDbContext context, ILogger<HallazgosController> logger)
        {
            _context = context;
            _logger = logger;
        }

        ////// -------------------------------------------------------------------
        ////// Crea un nuevo Hallazgo y lo asocia a una Auditoría existente mediante el AuditoriaId
        ////// -------------------------------------------------------------------

        ///// <summary>
        ///// Campos: descripción, tipo, severidad, fechaDeteccion, AuditoriaId.
        ///// </summary>
        ///// <param name="hallazgo">Datos del Hallazgo a crear.</param>
        ///// <returns>El nuevo Hallazgo creado con mensaje de éxito (201 Created).</returns>
        //// POST: api/Hallazgos
        //[HttpPost]
        //[ProducesResponseType(StatusCodes.Status201Created)]
        //[ProducesResponseType(StatusCodes.Status404NotFound)]
        //public async Task<ActionResult<object>> PostHallazgo([FromBody] Hallazgos hallazgo)
        //{
        //    // 1. Validación de clave foránea (AuditoriaId)
        //    var auditoriaExistente = await _context.Auditorias.FindAsync(hallazgo.AuditoriaId);

        //    if (auditoriaExistente == null)
        //    {
        //        _logger.LogWarning("Creación de Hallazgo fallida. Auditoría ID {AuditoriaId} no encontrada.", hallazgo.AuditoriaId);
        //        return NotFound(new { Mensaje = $"Error: No se puede asociar el hallazgo. La Auditoría con ID {hallazgo.AuditoriaId} no existe." });
        //    }

        //    // 2. Agregar y Guardar
        //    _context.Hallazgos.Add(hallazgo);
        //    await _context.SaveChangesAsync();

        //    _logger.LogInformation("Hallazgo ID {HallazgoId} creado y asociado a Auditoría ID {AuditoriaId}.", hallazgo.Id, hallazgo.AuditoriaId);

        //    // 3. Respuesta Exitosa (201 Created)
        //    return CreatedAtAction(
        //        "GetHallazgoById", // Asume que existe un GET por ID
        //        new { id = hallazgo.Id },
        //        new
        //        {
        //            Mensaje = $"Hallazgo creado exitosamente. ID generado: {hallazgo.Id} y asociado a Auditoría ID {hallazgo.AuditoriaId}.",
        //            Hallazgo = hallazgo
        //        }
        //    );
        //}

        //// -------------------------------------------------------------------
        //// Consulta hallazgos, permitiendo filtrar por el ID de Auditoría y/o por Severidad
        //// -------------------------------------------------------------------

        /// <summary>
        /// Consulta hallazgos, permitiendo filtrar por el ID de Auditoría y/o por Severidad.
        /// </summary>
        /// <param name="auditoriaId">ID de la Auditoría a la que pertenece el hallazgo (opcional).</param>
        /// <param name="severidad">Nivel de severidad del hallazgo ("Baja", "Media", "Alta") (opcional).</param>
        /// <returns>Una lista filtrada de Hallazgos envuelta en un mensaje de éxito.</returns>

        [HttpGet("ConsultarHallazgosPorAuditoria")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<object>> GetHallazgosFiltrados(
            [FromQuery] int? auditoriaId,
            [FromQuery] string? severidad)
        {
            IQueryable<Hallazgos> hallazgos = _context.Hallazgos;

            // 1. Aplicar filtro por AuditoriaId
            if (auditoriaId.HasValue)
            {
                hallazgos = hallazgos.Where(h => h.AuditoriaId == auditoriaId.Value);
            }

            // 2. Aplicar filtro por Severidad
            if (!string.IsNullOrEmpty(severidad))
            {
                string severidadNormalizada = severidad.Trim();
                hallazgos = hallazgos.Where(h => h.Severidad == severidadNormalizada);
            }

            // 3. Ejecutar la consulta, incluyendo la Auditoría para contexto
            var resultado = await hallazgos
                .Include(h => h.Auditoria)
                .ToListAsync();

            // 4. Devolver 200 OK con mensaje de éxito
            return Ok(new
            {
                Mensaje = $"Consulta exitosa. Se encontraron {resultado.Count} hallazgos con los filtros aplicados.",
                Hallazgos = resultado
            });
        }

        ////// -------------------------------------------------------------------
        ////// Elimina un Hallazgo por ID. Solo permitido si la Auditoría asociada está en estado "En Proceso".
        ////// -------------------------------------------------------------------

        ///// <summary>
        ///// </summary>
        ///// <param name="id">ID del Hallazgo a eliminar.</param>
        ///// <returns>200 OK con mensaje de éxito, o 400/404 si hay restricciones.</returns>
        ///// 

        //// DELETE: api/Hallazgos/5
        //[HttpDelete("EliminarEnProceso/{id}")]
        //[ProducesResponseType(StatusCodes.Status200OK)]
        //[ProducesResponseType(StatusCodes.Status400BadRequest)]
        //[ProducesResponseType(StatusCodes.Status404NotFound)]
        //public async Task<IActionResult> DeleteHallazgo(int id)
        //{
        //    // 1. Buscar el hallazgo, incluyendo la auditoría para la validación
        //    var hallazgo = await _context.Hallazgos
        //        .Include(h => h.Auditoria)
        //        .FirstOrDefaultAsync(h => h.Id == id);

        //    if (hallazgo == null)
        //    {
        //        return NotFound(new { Mensaje = $"Error: Hallazgo ID {id} no encontrado." });
        //    }

        //    // 2. ⚠️ RESTRICCIÓN DE NEGOCIO: Validar el estado de la Auditoría
        //    string estadoAuditoria = hallazgo.Auditoria?.Estado ?? "Desconocido";

        //    if (estadoAuditoria != "En Proceso")
        //    {
        //        _logger.LogWarning("Eliminación de Hallazgo denegada. Auditoría ID {AuditoriaId} está en estado '{Estado}', no 'En Proceso'.",
        //            hallazgo.AuditoriaId, estadoAuditoria);

        //        return BadRequest(new { Mensaje = $"No se puede eliminar el hallazgo. La Auditoría asociada debe estar en estado 'En Proceso' (Estado actual: '{estadoAuditoria}')." });
        //    }

        //    // 3. Eliminar y guardar
        //    _context.Hallazgos.Remove(hallazgo);
        //    await _context.SaveChangesAsync();

        //    _logger.LogInformation("Hallazgo ID {HallazgoId} eliminado exitosamente. Auditoría: {AuditoriaId}.", id, hallazgo.AuditoriaId);

        //    // 4. Respuesta Exitosa
        //    return Ok(new { Mensaje = $"Hallazgo ID {id} eliminado exitosamente." });
        //}

    }
}
