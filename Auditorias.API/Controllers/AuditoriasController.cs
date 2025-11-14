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
    public class AuditoriasController : ControllerBase
    {

        private readonly AuditoriasDbContext _context;
        private readonly ILogger<AuditoriasController> _logger;

        public AuditoriasController(AuditoriasDbContext context,ILogger<AuditoriasController> logger) // 👈 INYECCIÓN
        {
            _context = context;
            _logger = logger; 
        }

        //// -------------------------------------------------------------------
        //// Consultar Auditoría por ID (Soporte para POST)
        //// -------------------------------------------------------------------

        /// <summary>
        /// Obtiene los detalles de una Auditoría específica por su ID.
        /// </summary>
        /// <param name="id">El ID de la auditoría a consultar.</param>
        /// <returns>La entidad Auditoría o un código 404 si no se encuentra.</returns>
        // GET: api/Auditorias/5
       
        [HttpGet("ConsultarAuditoria/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<Auditoria>> GetAuditoriaById(int id)
        {
            var auditoria = await _context.Auditorias
                                 .Include(a => a.Responsable)
                                 // Usamos el .FirstOrDefaultAsync para no fallar si no existe
                                 .FirstOrDefaultAsync(a => a.Id == id);

            if (auditoria == null)
            {
                _logger.LogWarning("Consulta de Auditoría fallida. ID {AuditoriaId} no encontrado.", id);
                return NotFound(new
                { 
                    Mensaje = $"Error: La Auditoría con ID {id} no fue encontrada."
                });
            }

            _logger.LogInformation("Consulta de Auditoría exitosa. ID {AuditoriaId} encontrado. Estado actual: {Estado}",
                auditoria.Id,
                auditoria.Estado);

            return Ok(new
            {
                Mensaje = $"Consulta exitosa. Se encontró la Auditoría con ID {auditoria.Id}.",
                Auditoria = auditoria 
            });
        }

        //// -------------------------------------------------------------------
        //// Actualiza una Auditoría existente si la Auditoría es estado "Pendiente".
        //// -------------------------------------------------------------------

        /// <summary>
        /// Actualiza una Auditoría existente. Solo se permite la actualización si la Auditoría está en estado "Pendiente".
        /// </summary>
        /// <param name="id">El ID de la auditoría a actualizar.</param>
        /// <param name="auditoria">El objeto Auditoría con los nuevos datos.</param>
        /// <returns>Un código 204 No Content si es exitoso, o 400/404 si hay errores.</returns>
        // PUT: api/Auditorias/5
     
        [HttpPut("ActualizaAuditoria/{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> PutAuditoria(int id, [FromBody] Auditoria auditoria)
        {
            // 1. Verificar consistencia de ID
            if (id != auditoria.Id)
            {
                _logger.LogWarning("Intento de actualización con ID inconsistente. Ruta ID: {RouteId}, Body ID: {BodyId}", id, auditoria.Id);
                return BadRequest("El ID de la ruta no coincide con el ID del cuerpo de la petición.");
            }

            // 2. Buscar la auditoría existente para VALIDAR SU ESTADO
            // 💡 Recomendación: ELIMINAR _context.Entry(auditoriaExistente).State = EntityState.Modified; del paso 5.
            var auditoriaExistente = await _context.Auditorias.FindAsync(id);

            if (auditoriaExistente == null)
            {
                _logger.LogWarning("Intento de actualización de Auditoría fallido. ID {AuditoriaId} no encontrado.", id);
                return NotFound();
            }

            // 3. ⚠️ RESTRICCIÓN DE NEGOCIO: Solo se actualiza si está en estado "Pendiente"
            if (auditoriaExistente.Estado != "Pendiente")
            {
                _logger.LogWarning(
                    "Actualización de Auditoría denegada. ID {AuditoriaId} está en estado '{EstadoActual}' y no 'Pendiente'.",
                    id,
                    auditoriaExistente.Estado
                );
                return BadRequest($"Solo se pueden actualizar las auditorías que están en estado 'Pendiente'. El estado actual es '{auditoriaExistente.Estado}'.");
            }

            // 4. Actualizar las propiedades de la entidad existente con los nuevos datos
            auditoriaExistente.Titulo = auditoria.Titulo;
            auditoriaExistente.FechaInicio = auditoria.FechaInicio;
            auditoriaExistente.FechaFin = auditoria.FechaFin;
            auditoriaExistente.AreaAuditada = auditoria.AreaAuditada;
            auditoriaExistente.ResponsableId = auditoria.ResponsableId;

            // 5. Marcar la entidad como Modificada y Guardar
            // ⚠️ Esta línea puede ser ELIMINADA si FindAsync ya rastreó el objeto y solo se actualizaron propiedades.
            // La dejaremos si la necesitas, pero recuerda que puede ser redundante.
            _context.Entry(auditoriaExistente).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogError(ex, "Error de concurrencia al actualizar Auditoría ID {AuditoriaId}.", id);
                throw;
            }

            // 6. LOGGING ESTRUCTURADO y Respuesta Exitosa
            _logger.LogInformation("Auditoría ID {AuditoriaId} actualizada exitosamente. Nuevo Título: {Titulo}", id, auditoria.Titulo);

            // 💡 CAMBIO AQUÍ: Retornamos 200 OK con un objeto anónimo
            return Ok(new
            {
                Mensaje = $"Auditoría ID {id} actualizada exitosamente. El título fue cambiado a '{auditoria.Titulo}'.",
                AuditoriaId = id
            });
        }

        //// -------------------------------------------------------------------
        //// Cambia el estado de una Auditoría.
        //// -------------------------------------------------------------------

        /// <summary>
        /// Cambia el estado de una Auditoría siguiendo el flujo: Pendiente → En Proceso → Finalizada.
        /// </summary>
        /// <param name="id">El ID de la auditoría a modificar.</param>
        /// <param name="nuevoEstado">El nuevo estado al que se desea mover la auditoría.</param>
        /// <returns>Un código 200 OK con mensaje de éxito, o 400/404 si hay errores.</returns>
        /// 

        // POST: api/Auditorias/CambiarEstado/5
        [HttpPost("CambiarEstado/{id}")] 
        [ProducesResponseType(StatusCodes.Status200OK)] 
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> PostCambiarEstado(int id, [FromBody] string nuevoEstado)
        {
            // 1. Buscar la auditoría
            var auditoria = await _context.Auditorias.FindAsync(id);

            if (auditoria == null)
            {
                _logger.LogWarning("Intento de cambio de estado fallido. Auditoría ID {AuditoriaId} no encontrada.", id);
                return NotFound();
            }

            string estadoActual = auditoria.Estado;
            string estadoDeseado = nuevoEstado.Trim();

            // 2. RESTRICCIÓN DE NEGOCIO: Lógica de la Máquina de Estados
            bool transicionValida = (estadoActual, estadoDeseado) switch
            {
                ("Pendiente", "En Proceso") => true,
                ("En Proceso", "Finalizada") => true,
                _ => false
            };

            if (!transicionValida)
            {
                _logger.LogWarning(
                    "Transición de estado denegada para Auditoría ID {AuditoriaId}. Intento mover de '{EstadoActual}' a '{EstadoDeseado}'.",
                    id, estadoActual, estadoDeseado
                );
                return BadRequest($"Transición de estado inválida. No se puede pasar de '{estadoActual}' a '{estadoDeseado}'. Las transiciones permitidas son: Pendiente → En Proceso → Finalizada.");
            }

            // 3. Actualizar el estado y guardar
            auditoria.Estado = estadoDeseado;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al guardar el cambio de estado para Auditoría ID {AuditoriaId}.", id);
                throw;
            }

            // 4. LOGGING ESTRUCTURADO y Respuesta Exitosa (200 OK con Mensaje)
            _logger.LogInformation("Auditoría ID {AuditoriaId} cambió de estado de '{EstadoAnterior}' a '{EstadoNuevo}' exitosamente.",
                id, estadoActual, estadoDeseado);

            return Ok(new
            { // ⬅️ Retornamos 200 OK con el mensaje
                Mensaje = $"El estado de la Auditoría ID {id} se cambió exitosamente de '{estadoActual}' a '{estadoDeseado}'.",
                NuevoEstado = estadoDeseado
            });
        }

        //// -------------------------------------------------------------------
        //// Cambia el estado de una Auditoría.
        //// -------------------------------------------------------------------

        /// <summary>
        /// Consulta auditorías, permitiendo filtrar por un rango de fechas y/o por estado.
        /// </summary>
        /// <param name="fechaInicio">Fecha de inicio para el rango de búsqueda (opcional).</param>
        /// <param name="fechaFin">Fecha de fin para el rango de búsqueda (opcional).</param>
        /// <param name="estado">Estado de la auditoría ("Pendiente", "En Proceso", "Finalizada") (opcional).</param>
        /// <returns>Una lista filtrada de Auditorías envuelta en un mensaje de éxito.</returns>
        
        // GET: api/Auditorias/Filtrar?fechaInicio=2025-10-01&estado=Pendiente
        [HttpGet("Filtrar")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<object>> GetAuditoriasFiltradas(
            [FromQuery] DateTime? fechaInicio,
            [FromQuery] DateTime? fechaFin,
            [FromQuery] string? estado)
        {
            IQueryable<Auditoria> auditorias = _context.Auditorias
                .Include(a => a.Responsable); 

            // 1. Aplicar filtro por Rango de Fechas
            if (fechaInicio.HasValue)
            {
                auditorias = auditorias.Where(a => a.FechaInicio >= fechaInicio.Value.Date);
            }

            if (fechaFin.HasValue)
            {
                auditorias = auditorias.Where(a => a.FechaFin <= fechaFin.Value.Date);
            }

            // 2. Aplicar filtro por Estado
            if (!string.IsNullOrEmpty(estado))
            {
                string estadoNormalizado = estado.Trim();
                auditorias = auditorias.Where(a => a.Estado == estadoNormalizado);
            }

            // 3. Ejecutar la consulta a la base de datos
            var resultado = await auditorias.ToListAsync();

            _logger.LogInformation("Consulta de auditorías filtradas exitosa. {Count} resultados obtenidos.", resultado.Count);

            // 4. Devolver 200 OK con mensaje de éxito y los datos
            return Ok(new
            {
                Mensaje = $"Consulta exitosa. Se encontraron {resultado.Count} auditorías con los filtros aplicados.",
                Auditorias = resultado // La lista de auditorías
            });
        }

        //// -------------------------------------------------------------------
        ////  Crea una nueva Auditoría
        //// -------------------------------------------------------------------

        /// <summary>
        /// Crea una nueva Auditoría. El estado inicial se establece automáticamente como "Pendiente".
        /// </summary>
        /// <param name="auditoria">Datos de la Auditoría a crear.</param>
        /// <returns>La nueva Auditoría creada junto con un mensaje de éxito (201 Created).</returns>
        // PUNTO DE ACCESO: POST: api/Auditorias

        [HttpPost("NuevaAuditoria")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<object>> PostAuditoria([FromBody] Auditoria auditoria)
        {
            // 1. ⚠️ REGLA DE NEGOCIO: Establecer el estado inicial
            auditoria.Estado = "Pendiente";

            // 2. Agregar al contexto de la base de datos
            _context.Auditorias.Add(auditoria);

            try
            {
                // 3. Guardar cambios (el ID será generado aquí)
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear nueva Auditoría.");
                return StatusCode(StatusCodes.Status500InternalServerError, new { Mensaje = "Error interno del servidor al intentar guardar la Auditoría." });
            }

            // 4. LOGGING y Respuesta Exitosa (201 Created)
            _logger.LogInformation("Auditoría ID {AuditoriaId} creada exitosamente con el título: {Titulo}",
                auditoria.Id, auditoria.Titulo);

            // Retornamos 201 Created, usando CreatedAtAction para incluir la ubicación del nuevo recurso
            return CreatedAtAction(
                // Asumiendo que tienes un método GET llamado GetAuditoriaById (o similar) para la consulta
                nameof(GetAuditoriaById),
                new { id = auditoria.Id },
                new
                {
                    Mensaje = $"Auditoría creada exitosamente. ID generado: {auditoria.Id}. Estado inicial: Pendiente.",
                    Auditoria = auditoria // Devuelve el objeto recién creado con el ID
                }
            );
         }
    }
}
