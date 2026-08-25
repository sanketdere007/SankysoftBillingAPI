using System.Diagnostics;
using System.Text;
using System.Text.Json;
using Billing_Software_Api.Data;
using Billing_Software_Api.Helpers;
using Billing_Software_Api.Middleware;
using Billing_Software_Api.Models;
using Billing_Software_Api.Repository;
using Billing_Software_Api.Repository.Interfaces;
using Billing_Software_Api.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// ==========================================
// 1. Dependency Injection Registration
// ==========================================

// Core ADO.NET Data Helper & Security
builder.Services.AddSingleton<DbHelper>();
builder.Services.AddSingleton<IJwtHelper, JwtHelper>();

// Repositories
builder.Services.AddScoped<ICompanyRepository, CompanyRepository>();
builder.Services.AddScoped<IBranchRepository, BranchRepository>();
builder.Services.AddScoped<IEmployeeRepository, EmployeeRepository>();
builder.Services.AddScoped<IAuthRepository, AuthRepository>();
builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
builder.Services.AddScoped<IAreaRepository, AreaRepository>();
builder.Services.AddScoped<ICityRepository, CityRepository>();
builder.Services.AddScoped<IStateRepository, StateRepository>();
builder.Services.AddScoped<IDatabaseBackupRepository, DatabaseBackupRepository>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<ISubCategoryRepository, SubCategoryRepository>();
builder.Services.AddScoped<IBrandRepository, BrandRepository>();
builder.Services.AddScoped<IUnitRepository, UnitRepository>();
builder.Services.AddScoped<IGSTTaxRepository, GSTTaxRepository>();
builder.Services.AddScoped<ISupplierRepository, SupplierRepository>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IPurchaseEntryRepository, PurchaseEntryRepository>();
builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();

// Gmail SMTP bulk email (MailKit). No database is used for sending.
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection(EmailSettings.SectionName));
builder.Services.AddScoped<IEmailService, EmailService>();

// ==========================================
// 2. JWT Authentication Configuration
// ==========================================
var jwtSecretKey = builder.Configuration["JwtSettings:SecretKey"] 
    ?? "BillingSoftwareSuperSecretKeyForJwtTokenGeneration2026!#SecureKey";
var jwtIssuer = builder.Configuration["JwtSettings:Issuer"] ?? "BillingSoftwareAPI";
var jwtAudience = builder.Configuration["JwtSettings:Audience"] ?? "BillingSoftwareClients";

var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecretKey));

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = signingKey,
        ValidateIssuer = true,
        ValidIssuer = jwtIssuer,
        ValidateAudience = true,
        ValidAudience = jwtAudience,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };

    // Custom JSON responses on 401 Unauthorized and 403 Forbidden
    options.Events = new JwtBearerEvents
    {
        OnChallenge = async context =>
        {
            context.HandleResponse();
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            context.Response.ContentType = "application/json";

            var response = ApiResponse<object>.FailureResult(
                message: "Unauthorized. Access token is missing, invalid, or expired.",
                error: "Please provide a valid Bearer token in the Authorization header.");

            var json = JsonSerializer.Serialize(response, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            await context.Response.WriteAsync(json);
        },
        OnForbidden = async context =>
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            context.Response.ContentType = "application/json";

            var response = ApiResponse<object>.FailureResult(
                message: "Forbidden. You do not have permission to access this resource.",
                error: "Insufficient role or privilege.");

            var json = JsonSerializer.Serialize(response, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            await context.Response.WriteAsync(json);
        }
    };
});

builder.Services.AddAuthorization();

// ==========================================
// 3. CORS Configuration
// ==========================================
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// ==========================================
// 4. Controllers & Custom Model Validation
// ==========================================
builder.Services.AddControllers()
    .ConfigureApiBehaviorOptions(options =>
    {
        // Format model state validation errors into standardized ApiResponse
        options.InvalidModelStateResponseFactory = context =>
        {
            var errors = context.ModelState
                .Where(e => e.Value?.Errors.Count > 0)
                .SelectMany(kvp => kvp.Value!.Errors.Select(err => err.ErrorMessage))
                .ToList();

            var response = ApiResponse<object>.FailureResult(
                message: "One or more validation errors occurred.",
                error: string.Join("; ", errors));

            return new BadRequestObjectResult(response);
        };
    })
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
    });

// ==========================================
// 5. Swagger / OpenAPI with JWT Authorize
// ==========================================
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Billing Software - Employee Management Web API",
        Version = "v1",
        Description = "Production-ready ASP.NET Core Web API built with Clean Architecture, ADO.NET, Stored Procedures, and JWT Bearer Authentication."
    });

    // Configure JWT Bearer Security Scheme in Swagger
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter your JWT Bearer token.\r\n\r\nExample: `eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...`"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
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

// ==========================================
// 6. HTTP Request Pipeline Configuration
// ==========================================

// Global Exception Handler Middleware
app.UseMiddleware<ExceptionMiddleware>();

// Enable Swagger UI (both /swagger and root /)
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Billing Software API v1");
    c.RoutePrefix = "swagger";
    c.DocumentTitle = "Billing Software API Documentation";
});

app.UseRouting();

app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();

// Automatically redirect root URL "/" directly to "/swagger"
app.MapGet("/", () => Results.Redirect("/swagger"));

app.MapControllers();

// Auto-launch browser to Swagger in Development when started
if (app.Environment.IsDevelopment())
{
    app.Lifetime.ApplicationStarted.Register(() =>
    {
        try
        {
            var url = app.Urls.FirstOrDefault(u => u.StartsWith("http://")) ?? "http://localhost:5213";
            Process.Start(new ProcessStartInfo
            {
                FileName = $"{url}/swagger",
                UseShellExecute = true
            });
        }
        catch
        {
            // Silently ignore if running on headless environment or without GUI shell
        }
    });
}

app.Run();
