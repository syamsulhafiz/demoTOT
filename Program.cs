using Demo1.Authentication;
using Demo1.Data;
using Demo1.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Reflection;
using System.Text;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// =====================================================
// Pendaftaran Controller   
// =====================================================

builder.Services.AddControllers();

// =====================================================
// Konfigurasi Swagger / OpenAPI
// =====================================================

builder.Services.AddSwaggerGen(options =>
{
    // Muatkan fail XML comments untuk dipaparkan dalam Swagger UI
    var xmlFilename =
        $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";

    var xmlPath =
        Path.Combine(
            AppContext.BaseDirectory,
            xmlFilename);

    if (File.Exists(xmlPath))
    {
        options.IncludeXmlComments(xmlPath);
    }

    // -------------------------------------------------
    // Definisi keselamatan API Key
    // -------------------------------------------------

    options.AddSecurityDefinition(
        "ApiKey",
        new OpenApiSecurityScheme
        {
            Name = "X-API-KEY",
            Type = SecuritySchemeType.ApiKey,
            In = ParameterLocation.Header,

            Description =
                "Masukkan API Key untuk endpoint yang menggunakan API Key."
        });

    // -------------------------------------------------
    // Definisi keselamatan JWT Bearer
    // -------------------------------------------------

    options.AddSecurityDefinition(
        "Bearer",
        new OpenApiSecurityScheme
        {
            Name = "Authorization",
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,

            Description =
                "Masukkan JWT access token sahaja. " +
                "Tidak perlu masukkan perkataan 'Bearer'."
        });

    // -------------------------------------------------
    // Keperluan keselamatan lalai dalam Swagger:
    // gunakan skema Bearer (selari dengan fallback policy JWT)
    // -------------------------------------------------

    options.AddSecurityRequirement(
        new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference =
                        new OpenApiReference
                        {
                            Type =
                                ReferenceType.SecurityScheme,

                            Id = "Bearer"
                        }
                },

                Array.Empty<string>()
            }
        });

    // -------------------------------------------------
    // Benarkan override skema auth mengikut endpoint
    // -------------------------------------------------

    options.OperationFilter<AuthOperationFilter>();
});

// =====================================================
// Ambil konfigurasi aplikasi
// =====================================================

var jwtKey =
    builder.Configuration["Jwt:Key"]
    ?? throw new InvalidOperationException(
        "Jwt:Key configuration is missing.");

var jwtIssuer =
    builder.Configuration["Jwt:Issuer"]
    ?? throw new InvalidOperationException(
        "Jwt:Issuer configuration is missing.");

var jwtAudience =
    builder.Configuration["Jwt:Audience"]
    ?? throw new InvalidOperationException(
        "Jwt:Audience configuration is missing.");

var connectionString =
    builder.Configuration
        .GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException(
        "DefaultConnection is missing.");

// =====================================================
// Konfigurasi Authentication
// =====================================================

builder.Services
    .AddAuthentication(options =>
    {
        // Skema auth lalai ialah JWT
        options.DefaultAuthenticateScheme =
            JwtBearerDefaults.AuthenticationScheme;

        options.DefaultChallengeScheme =
            JwtBearerDefaults.AuthenticationScheme;

        options.DefaultScheme =
            JwtBearerDefaults.AuthenticationScheme;
    })

    // -------------------------------------------------
    // Tambah skema API Key
    // -------------------------------------------------

    .AddScheme<
        AuthenticationSchemeOptions,
        ApiKeyAuthenticationHandler>(
            "ApiKey",
            options => { })

    // -------------------------------------------------
    // Tambah skema JWT
    // -------------------------------------------------

    .AddJwtBearer(
        JwtBearerDefaults.AuthenticationScheme,
        options =>
        {
            options.TokenValidationParameters =
                new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,

                    ValidIssuer = jwtIssuer,
                    ValidAudience = jwtAudience,

                    IssuerSigningKey =
                        new SymmetricSecurityKey(
                            Encoding.UTF8.GetBytes(jwtKey)),

                    ClockSkew = TimeSpan.Zero
                };

            // =============================================
            // Diagnostik JWT (untuk tujuan debug)
            // =============================================

            options.Events = new JwtBearerEvents
            {
                OnMessageReceived = context =>
                {
                    Console.WriteLine(
                        "=== JWT MESSAGE RECEIVED ===");

                    Console.WriteLine(
                        "Authorization Header: " +
                        context.Request.Headers
                            .Authorization
                            .ToString());

                    Console.WriteLine(
                        "Token: " +
                        (string.IsNullOrEmpty(context.Token)
                            ? "[EMPTY]"
                            : "[TOKEN RECEIVED]"));

                    return Task.CompletedTask;
                },

                OnAuthenticationFailed = context =>
                {
                    Console.WriteLine(
                        "=== JWT AUTH FAILED ===");

                    Console.WriteLine(
                        context.Exception.ToString());

                    return Task.CompletedTask;
                },

                OnTokenValidated = context =>
                {
                    Console.WriteLine(
                        "=== JWT VALIDATED ===");

                    Console.WriteLine(
                        "User: " +
                        context.Principal?
                            .Identity?
                            .Name);

                    return Task.CompletedTask;
                },

                OnChallenge = context =>
                {
                    Console.WriteLine(
                        "=== JWT CHALLENGE ===");

                    Console.WriteLine(
                        $"Error: {context.Error}");

                    Console.WriteLine(
                        $"Description: " +
                        $"{context.ErrorDescription}");

                    Console.WriteLine(
                        "Authorization Header: " +
                        context.Request.Headers
                            .Authorization
                            .ToString());

                    return Task.CompletedTask;
                }
            };
        });

// =====================================================
// Konfigurasi Authorization
// =====================================================

builder.Services.AddAuthorization(options =>
{
    // -------------------------------------------------
    // Fallback Policy:
    // Semua endpoint perlukan JWT secara lalai.
    // Guna [AllowAnonymous] untuk endpoint awam.
    // Guna [Authorize(AuthenticationSchemes = "ApiKey")]
    // untuk endpoint API Key.
    // -------------------------------------------------

    options.FallbackPolicy =
        new AuthorizationPolicyBuilder()
            .AddAuthenticationSchemes(
                JwtBearerDefaults.AuthenticationScheme)
            .RequireAuthenticatedUser()
            .Build();

    // -------------------------------------------------
    // Policy: ProfileRead
    // Syarat: JWT + scope=profile.read
    // -------------------------------------------------

    options.AddPolicy(
        "ProfileRead",
        policy =>
        {
            policy
                .AddAuthenticationSchemes(
                    JwtBearerDefaults.AuthenticationScheme)
                .RequireAuthenticatedUser()
                .RequireClaim(
                    "scope",
                    "profile.read");
        });

    // -------------------------------------------------
    // Policy: AdminOnly
    // Syarat: JWT + role=Admin
    // -------------------------------------------------

    options.AddPolicy(
        "AdminOnly",
        policy =>
        {
            policy
                .AddAuthenticationSchemes(
                    JwtBearerDefaults.AuthenticationScheme)
                .RequireAuthenticatedUser()
                .RequireRole("Admin");
        });
});

// =====================================================
// Konfigurasi Rate Limiting
// =====================================================

builder.Services.AddRateLimiter(options =>
{
    options.GlobalLimiter =
        PartitionedRateLimiter
            .Create<HttpContext, string>(
                httpContext =>
                {
                    // Partisi limiter berdasarkan alamat IP klien
                    var clientIp =
                        httpContext
                            .Connection
                            .RemoteIpAddress?
                            .ToString()
                        ?? "unknown";

                    return RateLimitPartition
                        .GetFixedWindowLimiter(
                            partitionKey: clientIp,

                            factory: _ =>
                                new FixedWindowRateLimiterOptions
                                {
                                    PermitLimit = 10,
                                    Window = TimeSpan.FromMinutes(1),
                                    QueueLimit = 0,
                                    QueueProcessingOrder =
                                        QueueProcessingOrder.OldestFirst,
                                    AutoReplenishment = true
                                });
                });

    // Kod status bila rate limit melebihi had
    options.RejectionStatusCode =
        StatusCodes.Status429TooManyRequests;
});

// =====================================================
// Entity Framework DbContext
// =====================================================

builder.Services.AddDbContext<TestDbContext>(
    options =>
        options.UseSqlServer(
            connectionString));

// =====================================================
// Pendaftaran servis aplikasi
// =====================================================

builder.Services.AddScoped<
    IDataService,
    DataService>();

// =====================================================
// Bina aplikasi
// =====================================================

var app = builder.Build();

// =====================================================
// Middleware Swagger
// =====================================================

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();

    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint(
            "/swagger/v1/swagger.json",
            "Demo1 API V1");

        options.RoutePrefix =
            "swagger";
    });
}

// =====================================================
// HTTP Request Pipeline
// =====================================================

app.UseHttpsRedirection();

// Letak rate limiter sebelum authentication supaya
// permintaan berlebihan boleh ditolak lebih awal.
app.UseRateLimiter();

// Urutan wajib: Authentication dahulu, kemudian Authorization.
app.UseAuthentication();
app.UseAuthorization();

// Peta endpoint controller.
app.MapControllers();

app.Run();