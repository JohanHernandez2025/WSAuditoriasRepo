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
    }
}
