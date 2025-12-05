using DotNetEnv;
using QLCSV.Extensions;
using QLCSV.Middleware;

// Load .env file
Env.Load();

var builder = WebApplication.CreateBuilder(args);

// dependency injection
// bật api controller
builder.Services.AddControllers();
// cho phép khám phá endpoint cho swagger
builder.Services.AddEndpointsApiExplorer();

// viết theo kiểu fluent api
// services
//     .AddDatabaseServices()
//     .AddAuthenticationServices()
//     .AddSwaggerServices();

builder.Services.AddDatabaseServices(builder.Configuration);
builder.Services.AddAuthenticationServices(builder.Configuration);
builder.Services.AddApplicationServices();
builder.Services.AddSwaggerServices();

// cấu hình web server
builder.WebHost.ConfigureWebServer();

// build ứng dụng
var app = builder.Build();

// Tự động chạy EF Core migrations khi app khởi động Generate DB và seed data mặc định
await app.UseDatabaseMigrationAsync();

// cấu hình middleware trong pipeline
// global error handler
app.UseGlobalExceptionHandler();
// rate limiting
app.UseRateLimiting();
// Bật Swagger UI chỉ trong môi trường phù hợp.
app.UseSwaggerConfiguration(app.Environment);


/*

Thêm headers bảo mật như:

X-Frame-Options

X-Content-Type-Options

Content-Security-Policy

HSTS
→ Chỉ bật khi không phải development để dev đỡ bị vướng
*/
if (!app.Environment.IsDevelopment())
{
    app.UseSecurityHeaders();
}


// ASP.Net middleware
// chuyển hướng HTTP sang HTTPS
app.UseHttpsRedirection();

// xác thực và phân quyền
app.UseAuthentication();
app.UseAuthorization();

//Bật API routing theo attribute như [HttpGet], [Route("api/...")]
app.MapControllers();

app.Run();
