using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MTS.BackEnd.Infrastructure;
using MTS.BLL;
using MTS.BLL.Services;
using MTS.BLL.Services.QRService;
using MTS.DAL.Repositories;
using MTS.Data;
using MTS.Data.Base;
using MTS.Data.Models;
using System.Text;
using static MTS.BLL.IServiceProviders;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Tắt giữ vòng lặp object, giúp JSON sạch, không có $id, $values
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;

        // Tuỳ chọn: format đẹp hơn khi debug
        options.JsonSerializerOptions.WriteIndented = true;
    });

builder.Services.AddEndpointsApiExplorer();

// Load JWT settings from configuration
var jwtSettingsSection = builder.Configuration.GetSection("JwtSettings");
builder.Services.Configure<JWTSettings>(jwtSettingsSection);
var jwtSettings = jwtSettingsSection.Get<JWTSettings>();
builder.Services.AddSingleton(jwtSettings);
builder.Services.AddHttpClient();


// Add Swagger with JWT support
builder.Services.AddSwaggerGen(options =>
{
	options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
	{
		Description = @"JWT Authorization header using the Bearer scheme.  
                      Enter your token in the text input below. Example: 'eyJhbGciOiJIUzI1NiIsInR...'",
		Name = "Authorization",
		In = ParameterLocation.Header,
		Type = SecuritySchemeType.Http,
		Scheme = "bearer",
		BearerFormat = "JWT"
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

// Add CORS
builder.Services.AddCors(options =>
{
	options.AddDefaultPolicy(policy =>
	{
		policy.WithOrigins("http://localhost:5500", "https://localhost:7159")
			  .SetIsOriginAllowed(origin => true)
			  .AllowAnyHeader()
			  .WithMethods("GET", "POST")
			  .AllowCredentials();
	});
});

// Configure DbContext
builder.Services.AddDbContext<MTS_Context>(options =>
	options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add JWT Authentication
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
		ValidateIssuer = true,
		ValidateAudience = true,
		ValidateIssuerSigningKey = true,
		ValidIssuer = jwtSettings.Issuer,
		ValidAudience = jwtSettings.Audience,
		IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey))
	};
});

// Dependency Injection
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IGenericRepository<User>, GenericRepository<User>>();
builder.Services.AddScoped<IServiceProviders, ServiceProviders>();
builder.Services.AddScoped<IEmailSender, EmailSender>();
builder.Services.AddScoped<ITicketService, TicketService>();
builder.Services.AddScoped<QRTokenGeneratorService>();
builder.Services.AddScoped<IPriorityApplicationService, PriorityApplicationService>();
builder.Services.AddScoped<ISupabaseFileService, SupabaseFileService>();
builder.Services.AddScoped<IWalletService, WalletService>();
builder.Services.AddScoped<IRefundService, RefundService>();
builder.Services.AddScoped<IVNPayRefundGatewayService, VNPayRefundGatewayService>();


var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI();
}

app.UseRouting();
app.UseHttpsRedirection();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
