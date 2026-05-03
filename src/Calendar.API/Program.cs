using System.Text;
using Calendar.API.Middlewares;
using Calendar.Core.Interfaces;
using Calendar.Infrastructure.Data;
using Calendar.Infrastructure.Repositories;
using Calendar.Service.Implementations;
using Calendar.Service.Interfaces;
using Calendar.Shared.Validators;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;


var builder = WebApplication.CreateBuilder(args);

// Add Database setup
builder.Services.AddDbContext<CalendarDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add DI mappings
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IAppointmentRepository, AppointmentRepository>();
builder.Services.AddScoped<IGroupMeetingRepository, GroupMeetingRepository>();
builder.Services.AddScoped<IRecurrenceRuleRepository, RecurrenceRuleRepository>();
builder.Services.AddScoped<IReminderRepository, ReminderRepository>();

builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IAppointmentService, AppointmentService>();
builder.Services.AddScoped<IEmailService, Calendar.Infrastructure.Services.EmailService>();

// Add Background Services
builder.Services.AddHostedService<Calendar.API.BackgroundServices.ReminderBackgroundService>();

// Add FluentValidation
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<RegisterRequestValidator>();

// Add Controllers
builder.Services.AddControllers();

// Add JWT authentication
var jwtSection = builder.Configuration.GetSection("Jwt");
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSection["Issuer"],
            ValidAudience = jwtSection["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSection["Key"]!))
        };
    });

// Add Swagger
builder.Services.AddEndpointsApiExplorer();

// Configure CORS for later Frontend
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowBlazor", b =>
    {
        b.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
    });
});

var app = builder.Build();

// Automatically Apply Migrations on startup
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<CalendarDbContext>();
    dbContext.Database.Migrate();
}

app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
}

app.UseCors("AllowBlazor");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
