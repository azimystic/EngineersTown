using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using EngineersTown.Data;
using EngineersTown.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Add Entity Framework
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Register services
builder.Services.AddScoped<IZKTecoService, ZKTecoService>();
builder.Services.AddScoped<IAttendanceService, AttendanceService>();

// Register background service
builder.Services.AddHostedService<AttendanceBackgroundService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Attendance}/{action=Index}/{id?}");

// Ensure database is created and migrated
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    try
    {
        context.Database.Migrate();
    }
    catch
    {
        context.Database.EnsureCreated();
    }
}

// Run server in background
var runTask = app.RunAsync();

// 🔥 Auto open browser on publish/run
try
{
    // You can configure this port in Properties/launchSettings.json
    var url = "http://localhost:5000"; // or "http://localhost:5000"
    Process.Start(new ProcessStartInfo
    {
        FileName = url,
        UseShellExecute = true
    });
}
catch (Exception ex)
{
    Console.WriteLine("Could not open browser: " + ex.Message);
}

// Wait for shutdown
await runTask;
