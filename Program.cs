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
// Controllers
// =====================================================

builder.Services.AddControllers();


// =====================================================
// Swagger / OpenAPI
// =====================================================

builder.Services.AddSwaggerGen(options =>
{
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
    // API Key
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
    // JWT Bearer
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
    // Bearer = default security requirement in Swagger
    // Matches our JWT fallback policy
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
    // Override security per endpoint
    // -------------------------------------------------

    options.OperationFilter<AuthOperationFilter>();
});


// =====================================================
// Configuration
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
// Authentication
// =====================================================

builder.Services
    .AddAuthentication(options =>
    {
        // Default authentication = JWT
        options.DefaultAuthenticateScheme =
            JwtBearerDefaults.AuthenticationScheme;

        options.DefaultChallengeScheme =
            JwtBearerDefaults.AuthenticationScheme;

        options.DefaultScheme =
            JwtBearerDefaults.AuthenticationScheme;
    })

    // -------------------------------------------------
    // API Key authentication
    // -------------------------------------------------

    .AddScheme<
        AuthenticationSchemeOptions,
        ApiKeyAuthenticationHandler>(
            "ApiKey",
            options => { })

    // -------------------------------------------------
    // JWT authentication
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
            // JWT Diagnostics
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
// Authorization
// =====================================================

builder.Services.AddAuthorization(options =>
{
    // -------------------------------------------------
    // Fallback Policy
    //
    // Semua endpoint require JWT secara default.
    //
    // Untuk public endpoint:
    // [AllowAnonymous]
    //
    // Untuk API Key:
    // [Authorize(AuthenticationSchemes = "ApiKey")]
    // -------------------------------------------------

    options.FallbackPolicy =
        new AuthorizationPolicyBuilder()
            .AddAuthenticationSchemes(
                JwtBearerDefaults.AuthenticationScheme)
            .RequireAuthenticatedUser()
            .Build();


    // -------------------------------------------------
    // ProfileRead
    // Requires JWT + scope=profile.read
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
    // AdminOnly
    // Requires JWT + role=Admin
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
// Rate Limiting
// =====================================================

builder.Services.AddRateLimiter(options =>
{
    options.GlobalLimiter =
        PartitionedRateLimiter
            .Create<HttpContext, string>(
                httpContext =>
                {
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

                                    Window =
                                        TimeSpan
                                            .FromMinutes(1),

                                    QueueLimit = 0,

                                    QueueProcessingOrder =
                                        QueueProcessingOrder
                                            .OldestFirst,

                                    AutoReplenishment =
                                        true
                                });
                });

    options.RejectionStatusCode =
        StatusCodes.Status429TooManyRequests;
});


// =====================================================
// Entity Framework
// =====================================================

builder.Services.AddDbContext<TestDbContext>(
    options =>
        options.UseSqlServer(
            connectionString));


// =====================================================
// Application Services
// =====================================================

builder.Services.AddScoped<
    IDataService,
    DataService>();


// =====================================================
// Build Application
// =====================================================

var app = builder.Build();


// =====================================================
// Swagger
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
// HTTP Pipeline
// =====================================================

app.UseHttpsRedirection();


// -----------------------------------------------------
// Rate limit before authentication
// so abusive requests can be rejected earlier.
// -----------------------------------------------------

app.UseRateLimiter();


// -----------------------------------------------------
// Authentication BEFORE Authorization
// -----------------------------------------------------

app.UseAuthentication();

app.UseAuthorization();


// -----------------------------------------------------
// Controllers
// -----------------------------------------------------

app.MapControllers();

app.Run();