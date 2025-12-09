using Microsoft.EntityFrameworkCore;
using PluginMarket.Api.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowDev", policy =>
        policy.WithOrigins("https://bp.idvevent.cn", "http://127.0.0.1:5173")
              .AllowAnyHeader()
              .AllowAnyMethod());
});
builder.Services.AddDbContext<PluginDbContext>(options =>
{
    var cs = builder.Configuration.GetConnectionString("Default") ?? "Data Source=pluginmarket.db";
    options.UseSqlite(cs);
});

builder.Services.Configure<StorageOptions>(builder.Configuration.GetSection("Storage"));
builder.Services.Configure<AdminOptions>(builder.Configuration.GetSection("Admin"));

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<PluginDbContext>();
    db.Database.Migrate();
}

if (app.Environment.IsDevelopment())
{
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseCors("AllowDev");
app.UseAuthorization();
app.MapControllers();
app.Run();
