using MudBlazor.Services;
using JustABackup.Core.Extensions;
using JustABackup.Database.Repositories;
using JustABackup.Services;
using JustABackup.Database.SQLite;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddMudServices();
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

builder.Services.AddSQLiteDatabase(builder.Configuration);
builder.Services.AddJustABackupServices();
builder.Services.AddJustABackupRepositories();
builder.Services.AddQuartz();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

//app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

using (var serviceScope = app.Services.CreateScope())
{
    var init = serviceScope.ServiceProvider.GetService<IInitializationService>();

    await init.VerifyDatabase();
    await init.LoadPlugins();
    //await init.VerifyScheduledJobs();
}


app.Run();
