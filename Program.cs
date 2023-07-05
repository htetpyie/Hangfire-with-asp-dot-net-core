using Hangfire;
using Hangfire.LiteDB;
using Hangfire.Storage;
using HangfireDotNetCoreExample.Features.Cron;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();


//using HangFire
string liteDb = AppDomain.CurrentDomain.BaseDirectory + "HangFireLite.db";
builder.Services.AddHangfire(
    config => config
    .UseLiteDbStorage(liteDb)
    );
builder.Services.AddHangfireServer();

builder.Services.AddRazorPages().AddRazorRuntimeCompilation();
builder.Services.AddScoped<CronService>();

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

using var connection = JobStorage.Current.GetConnection();
foreach (var recurringJob in connection.GetRecurringJobs())
{
    RecurringJob.RemoveIfExists(recurringJob.Id);
}

app.Run();