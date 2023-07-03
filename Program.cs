using Hangfire;
using Hangfire.LiteDB;
using Microsoft.EntityFrameworkCore;
using HangFireExample.EFDbContext;
using HangFireExample.HangFire;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<AppDbContext>(
    opt =>
        opt.UseSqlServer(
            builder
                .Configuration
                .GetConnectionString("DbConnection")
        )
);

//using HangFire
string liteDb = AppDomain.CurrentDomain.BaseDirectory + "HangFireLite.db";
builder.Services.AddHangfire(
    config => config
    .UseLiteDbStorage(liteDb)
    );

builder.Services.AddRazorPages().AddRazorRuntimeCompilation();
builder.Services.AddScoped<HangFireService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.UseHangfireDashboard();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Blog}/{action=Index}/{id?}");

app.Run();