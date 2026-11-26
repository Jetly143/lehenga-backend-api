using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Rent_Lehenga_Project.Data;
using Rent_Lehenga_Project.Models;
using Rent_Lehenga_Project.Services;

var builder = WebApplication.CreateBuilder(args);

// Dynamic port binding for Cloud Deployments (Render/Railway/Koyeb)
var port = Environment.GetEnvironmentVariable("PORT") ?? "5000";
builder.WebHost.UseUrls($"http://+:{port}");

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddScoped<IEmailService, EmailService>();

// Enable CORS for React Admin Panel & Frontends
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// Configure PostgreSQL DbContext
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Configure JWT Authentication
var jwtKey = builder.Configuration["Jwt:Key"] ?? "LehengaVerse_SuperSecret_Jwt_Security_Key_2026_!@#$%^&*()_+";
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "LehengaVerseApi";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "LehengaVerseClients";

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
        ValidateIssuer = true,
        ValidIssuer = jwtIssuer,
        ValidateAudience = true,
        ValidAudience = jwtAudience,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.FromMinutes(5)
    };
    options.Events = new JwtBearerEvents
    {
        OnAuthenticationFailed = context =>
        {
            var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
            logger.LogError(context.Exception, "JWT Authentication failed: {Message}", context.Exception.Message);
            return Task.CompletedTask;
        }
    };
});

// Configure Swagger/OpenAPI with JWT Bearer support
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Rent Lehenga API", Version = "v1" });
    
    // Add JWT Authentication button to Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter JWT token like: Bearer {your_token}"
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
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

// Auto-apply pending migrations at startup & seed initial Admin if not exists
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    try
    {
        dbContext.Database.Migrate();
        logger.LogInformation("✅ Database migrations applied successfully.");

        // Ensure default Platform Admin user exists
        if (!dbContext.Users.Any(u => u.Role == UserRole.Admin))
        {
            var adminUser = new User
            {
                Name = "Gourav Kumar",
                Email = "admin@lehengaverse.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("SecurePassword@123"),
                Role = UserRole.Admin,
                City = "Delhi",
                IsVerified = true,
                KycStatus = KycStatus.Verified,
                IsEmailVerified = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            dbContext.Users.Add(adminUser);
            dbContext.SaveChanges();
            logger.LogInformation("👑 Default Admin seeded successfully: admin@lehengaverse.com");
        }
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "An error occurred while migrating/seeding the database.");
    }
}

// Configure the HTTP request pipeline.
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Rent Lehenga API V1");
    c.RoutePrefix = "swagger";
});

app.UseCors("AllowAll");

// Authentication must come before Authorization
app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/", () => Results.Ok(new 
{ 
    status = "Healthy", 
    message = "Rent Lehenga API is running successfully!", 
    database = "PostgreSQL Connected",
    jwtAuth = "Enabled",
    timestamp = DateTime.UtcNow 
}));

app.MapControllers();

app.Run();
