﻿using DATN.Configuration;
using DATN.Data;
using DATN.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add DbContext
builder.Services.AddDbContext<StrokeDbContext>(options =>
	options.UseMySql(
		builder.Configuration.GetConnectionString("DefaultConnection"),
		new MySqlServerVersion(new Version(8, 0, 21)),
		mysqlOptions => mysqlOptions.EnableRetryOnFailure()));

// Add Email Services
builder.Services.AddSingleton<EmailService>();
builder.Services.AddSingleton<IBackgroundEmailQueue>(new BackgroundEmailQueue(100));
builder.Services.AddHostedService<EmailBackgroundService>();

// Configure JWT
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("Jwt"));
var jwtSettings = builder.Configuration.GetSection("Jwt").Get<JwtSettings>();
builder.Services.AddSingleton<IJwtTokenService>(new JwtTokenService(jwtSettings));

// Configure Kestrel
builder.WebHost.ConfigureKestrel(options =>
{
	options.ListenAnyIP(5062); // Vẫn để backend chạy port 5062
});

// === Thêm cấu hình CORS ===
builder.Services.AddCors(options =>
{
	options.AddPolicy("AllowFrontend3000", policy =>
	{
		policy.WithOrigins("http://localhost:3000")
			  .AllowAnyHeader()
			  .AllowAnyMethod()
			  .AllowCredentials(); // nếu bạn dùng cookie hoặc gửi token qua header
	});
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
			}
		};
	});

// Swagger
builder.Services.AddSwaggerGen(options =>
{
	options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
	{
		Name = "Authorization",
		Description = "Enter 'Bearer' [space] and your token",
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

// Add Controllers
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

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

// === Thêm UseCors ===
app.UseCors("AllowFrontend3000");

app.UseAuthentication();
app.UseAuthorization();

// Log Auth Info
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

app.MapControllers();
app.Run();
