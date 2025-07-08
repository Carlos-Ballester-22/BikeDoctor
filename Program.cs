using BikeDoctor.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using BikeDoctor.Repository;
using BikeDoctor.Service;
using BikeDoctor.Repositories;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Agregar servicios al contenedor
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    });

// Configurar el DbContext con la cadena de conexión desde appsettings.json
builder.Services.AddDbContext<BikeDoctorContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))
           .EnableSensitiveDataLogging()
           .LogTo(Console.WriteLine, LogLevel.Information));

// Configuración de Repositories y Services de Clientes y Motocicletas
builder.Services.AddScoped<IClientRepository, ClientRepository>();
builder.Services.AddScoped<IClientService, ClientService>();
builder.Services.AddScoped<IMotorcycleRepository, MotorcycleRepository>();
builder.Services.AddScoped<IMotorcycleService, MotorcycleService>();

// Configuración de Repositories y Services del board de seguimiento
builder.Services.AddScoped<IReceptionRepository, ReceptionRepository>();
builder.Services.AddScoped<IReceptionService, ReceptionService>();
builder.Services.AddScoped<IDiagnosisRepository, DiagnosisRepository>();
builder.Services.AddScoped<IDiagnosisService, DiagnosisService>();
builder.Services.AddScoped<ISparePartsRepository, SparePartsRepository>();
builder.Services.AddScoped<ISparePartsService, SparePartsService>();
builder.Services.AddScoped<ICostApprovalRepository, CostApprovalRepository>();
builder.Services.AddScoped<ICostApprovalService, CostApprovalService>();
builder.Services.AddScoped<IRepairRepository, RepairRepository>();
builder.Services.AddScoped<IRepairService, RepairService>();
builder.Services.AddScoped<IQualityControlRepository, QualityControlRepository>();
builder.Services.AddScoped<IQualityControlService, QualityControlService>();
builder.Services.AddScoped<IDeliveryRepository, DeliveryRepository>();
builder.Services.AddScoped<IDeliveryService, DeliveryService>();

// Configuración de Repositories y Services del Tablero general del board
builder.Services.AddScoped<IFlowAttentionRepository, FlowAttentionRepository>();
builder.Services.AddScoped<IFlowAttentionService, FlowAttentionService>();

// Configurar autenticación JWT
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = "auth_service",
        ValidAudience = "auth_service_users",
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("ClaveSuperSeguraConMasDe32Caracteres123!"))
    };
    options.MapInboundClaims = false;
    options.TokenValidationParameters.RoleClaimType = "Role";
});

// Agregar Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo 
    { 
        Title = "BikeDoctor API", 
        Version = "v1",
        Description = "API para la gestión de BikeDoctor",
        Contact = new OpenApiContact
        {
            Name = "Tu Nombre",
            Email = "tuemail@ejemplo.com"
        }
    });

    // Configurar autenticación JWT en Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Ingresa el token JWT en el formato: Bearer {token}"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});
var app = builder.Build();

// Configurar el pipeline de la aplicación
app.UseHttpsRedirection();

// Agregar middleware de Swagger
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "BikeDoctor API V1");
    c.RoutePrefix = string.Empty; // Hace que Swagger esté disponible en la raíz (/)
});

// Agregar middleware de autenticación antes de autorización
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
