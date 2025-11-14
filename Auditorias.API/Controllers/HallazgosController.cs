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

     

    }
}
