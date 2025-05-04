using DATN.Configuration;
using DATN.Data;
using DATN.Services;
using DATN.Hubs;
using DATN.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authorization;
using DATN.Middleware;
using DATN.Authorization; 

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddDbContext<StrokeDbContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        new MySqlServerVersion(new Version(8, 0, 21)),
        mysqlOptions => mysqlOptions.EnableRetryOnFailure()));


// Add Email Services
builder.Services.AddSingleton<EmailService>();
builder.Services.AddSingleton<IBackgroundEmailQueue>(new BackgroundEmailQueue(100));
//builder.Services.AddScoped<INotificationService, EmailNotificationService>();
builder.Services.AddScoped<IEmailTemplateService, EmailTemplateService>();
builder.Services.AddHostedService<EmailBackgroundService>();

builder.Services.AddSignalR();

// Add Notification Service 
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<INotificationFormatterService, NotificationFormatterService>();
builder.Services.AddScoped<IMobileNotificationSenderService, MobileNotificationSenderService>();
builder.Services.AddScoped<IDoctorService, DoctorService>();
builder.Services.AddScoped<IMobileNotificationService, SignalRMobileNotificationService>();


builder.Services.AddScoped<IUserService, UserService>();

// Configure CORS for frontend and SignalR
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.SetIsOriginAllowed(_ => true) // dev only
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});


builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.Preserve;
        options.JsonSerializerOptions.MaxDepth = 64;
    });

// Configure JWT
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("Jwt"));
var jwtSettings = builder.Configuration.GetSection("Jwt").Get<JwtSettings>();
builder.Services.AddSingleton<IJwtTokenService>(new JwtTokenService(jwtSettings));

// Configure Kestrel
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(5062);
});

JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidAudience = jwtSettings.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Key)),
            RoleClaimType = ClaimTypes.Role,
            NameClaimType = ClaimTypes.NameIdentifier
        };


        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                Console.WriteLine("Authentication failed: " + context.Exception.Message);
                return Task.CompletedTask;
            },
            OnTokenValidated = context =>
            {
                Console.WriteLine("Token validated successfully.");
                foreach (var claim in context.Principal.Claims)
                {
                    Console.WriteLine($"{claim.Type}: {claim.Value}");
                }
                return Task.CompletedTask;
            },
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];
                if (!string.IsNullOrEmpty(accessToken) &&
                    context.HttpContext.Request.Path.StartsWithSegments("/notificationHub"))
                {
                    context.Token = accessToken;
                }
                return Task.CompletedTask;
            }
        };
    });


builder.Services.AddAuthorization(options =>
{
    
    options.AddPolicy("AdminOnly", policy =>
        policy.RequireRole("admin"));

  
    options.AddPolicy("DoctorOnly", policy =>
        policy.RequireRole("doctor"));


    options.AddPolicy("AdminOrDoctor", policy =>
        policy.RequireRole("admin", "doctor"));

  
    options.AddPolicy("UserAccessPolicy", policy =>
        policy.RequireAssertion(context =>
        {
            var userId = context.Resource as string;
            var currentUserId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isAdmin = context.User.IsInRole("admin");
            var isDoctor = context.User.IsInRole("doctor");

            if (isAdmin) return true;

            
            if (userId == currentUserId) return true;

            
            if (isDoctor)
            {
                
                return true;
            }

            return false;
        }));
});


builder.Services.AddScoped<IAuthorizationHandler, UserResourceAuthorizationHandler>();

builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Description = "Enter 'Bearer [space] and your token'",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT"
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
          new OpenApiSecurityScheme
          {
              Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
          },
          Array.Empty<string>()
        }
    });
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddControllers();


builder.Services.AddHttpContextAccessor();

var app = builder.Build();


if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "API v1");
    });
}

app.UseAccessLog();

app.UseCors("AllowFrontend3000");
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

if (app.Environment.IsDevelopment())
{
    app.Use(async (context, next) =>
    {
        var authHeader = context.Request.Headers["Authorization"].ToString();
        Console.WriteLine($"Authorization Header: {authHeader}");
        await next.Invoke();
        if (context.User.Identity?.IsAuthenticated == true)
        {
            var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var roles = context.User.FindAll(ClaimTypes.Role).Select(r => r.Value);
            Console.WriteLine($"Authenticated UserId: {userId}");
            Console.WriteLine($"Roles: {string.Join(", ", roles)}");
        }
        else
        {
            Console.WriteLine("User is not authenticated.");
        }
    });
}
//using (var scope = app.Services.CreateScope())
//{
//    var context = scope.ServiceProvider.GetRequiredService<StrokeDbContext>();


//    context.Database.Migrate();


//    if (!context.Roles.Any())
//    {
//        context.Roles.AddRange(new[]
//        {
//            new Role { RoleName = "user" },
//            new Role { RoleName = "admin" },
//            new Role { RoleName = "doctor" }
//        });
//        context.SaveChanges();
//        Console.WriteLine("Roles seeded successfully.");
//    }
//    else
//    {
//        var roles = context.Roles.Select(r => r.RoleName).ToArray();
//        Console.WriteLine("Roles already exist: " + string.Join(", ", roles));
//    }
//}

app.MapControllers();
app.UseCors("AllowAll");
app.UseRouting();
app.MapHub<NotificationHub>("/notificationHub");
app.UseStaticFiles();

app.Run();