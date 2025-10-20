using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using QuanLyTreEmAPI.Data;
using QuanLyTreEmAPI.Repositories;
//using QuanLyTreEmAPI.Services;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new OpenApiInfo { Title = "Quản lý Trẻ Em API", Version = "v1" });
        c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Description = "JWT Authorization header using the Bearer scheme",
            Name = "Authorization",
            In = ParameterLocation.Header,
            Type = SecuritySchemeType.ApiKey,
            Scheme = "Bearer"
        });
        c.AddSecurityRequirement(new OpenApiSecurityRequirement {
            {
                new OpenApiSecurityScheme {
                    Reference = new OpenApiReference {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                },
                Array.Empty<string>()
            }
        });
    }
);

// Database
builder.Services.AddDbContext<QuanLyTreEmAPI.Models.QuanLyTreEmContext>(c =>
    c.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
);

// JWT Authentication
/*
 * Cấu hình JWT Authentication
*/
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var key = Encoding.ASCII.GetBytes(jwtSettings["Secret"]);

builder.Services.AddAuthentication(x =>
{
    x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(x =>
{
    x.RequireHttpsMetadata = false;
    x.SaveToken = true;
    x.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        ClockSkew = TimeSpan.Zero
    };
});

// CORS
/* 
 * Cho phép mọi domain, mọi phương thức, mọi header truy cập API.
 * Dùng khi gọi API từ frontend như Flutter
*/
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});

// Services
/*
 * Đăng ký Các Service, Repositories dạng Scoped (Duy trì trạng thái trong một yêu cầu)
*/
// Register Repositories
builder.Services.AddScoped<IAuthRepository, AuthRepository>();
builder.Services.AddScoped<ITreEmRepository, TreEmRepository>();
builder.Services.AddScoped<IKhuPhoRepository, KhuPhoRepository>();
builder.Services.AddScoped<ITruongHocRepository, TruongHocRepository>();
builder.Services.AddScoped<IHoanCanhRepository, HoanCanhRepository>();
builder.Services.AddScoped<IPhuHuynhRepository, PhuHuynhRepository>();
builder.Services.AddScoped<IPhieuHocTapRepository, PhieuHocTapRepository>();
builder.Services.AddScoped<IHoTroPhucLoiRepository, HoTroPhucLoiRepository>();
builder.Services.AddScoped<IVanDongTreEmRepository, VanDongTreEmRepository>();

// Register Services
//builder.Services.AddScoped<IAuthService, AuthService>();
//builder.Services.AddScoped<ITreEmService, TreEmService>();
//builder.Services.AddScoped<IDashboardService, DashboardService>();
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();

// Đặt UseStaticFiles trước MapControllers
app.UseStaticFiles();

app.MapControllers();

app.Run();
