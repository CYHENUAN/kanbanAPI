using Application.Interfaces;
using Application.Services;
using Infrastructure.Data;
using Infrastructure.Data.Interface;
using Infrastructure.Data.Services;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using System.Reflection;
using VisualKanbanWebAPI.Hubs;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// 根据环境加载不同配置,设置优先级及环境变量前缀
builder.Configuration
    .AddJsonFile("appsettings.json")
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true)
    .AddEnvironmentVariables(prefix: "JWTKey_");

//连接数据库
// 配置 `DbContext` 并添加到依赖注入容器 
builder.Services.AddDbContext<MESDBContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));


builder.Services.AddSignalR();
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssemblies(
        Assembly.Load("Application"),
        Assembly.GetExecutingAssembly());
});

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyHeader().AllowAnyMethod().AllowCredentials().SetIsOriginAllowed(_ => true);
    });
});
builder.Services.AddScoped<IDeviceService, DeviceService>();
builder.Services.AddScoped<IDeviceRepository, DeviceRepository>();
builder.Services.AddScoped<IProductionService, ProductionService>();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.MapHub<SignalRHub>("/SignalHub");
app.UseCors();

// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
   
//}
app.UseSwagger();
app.UseSwaggerUI();


app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(Path.Combine(builder.Environment.ContentRootPath, "myapp")),
    ContentTypeProvider = new FileExtensionContentTypeProvider(),
    RequestPath = ""
});
app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
