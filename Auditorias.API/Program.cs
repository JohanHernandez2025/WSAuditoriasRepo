using Serilog;
using Microsoft.EntityFrameworkCore;
using Auditorias.API.Infrastructure;

// 1. CONFIGURACIÓN DE SERILOG
var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json")
    .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}.json", optional: true)
    .Build();

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/log-.txt", rollingInterval: RollingInterval.Day, retainedFileCountLimit: 7)
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseSerilog();

// 2. CONFIGURACIÓN DE DEPENDENCIAS
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Inyección del DbContext
builder.Services.AddDbContext<AuditoriasDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("AuditoriasDbConnection"));
});


// 3. CONFIGURACIÓN DEL PIPELINE HTTP
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseSerilogRequestLogging();
app.UseAuthorization();
app.MapControllers();

// Bloque de VALIDACIÓN DE CONEXIÓN (Temporal)
//try
//{
//    using (var scope = app.Services.CreateScope())
//    {
//        var context = scope.ServiceProvider.GetRequiredService<AuditoriasDbContext>();
//        context.Database.OpenConnection();
//        context.Database.CloseConnection();
//        Log.Information("✅ La conexión a la base de datos GestorAuditoriasDB fue exitosa.");
//    }
//}
//catch (Exception ex)
//{
//    Log.Fatal(ex, "❌ ERROR FATAL: No se pudo conectar a la base de datos GestorAuditoriasDB.");
//    // Opcional: Detener la aplicación si la conexión falla en el inicio
//    // throw; 
//}
//// Fin del bloque de VALIDACIÓN DE CONEXIÓN (Temporal)
app.Run();