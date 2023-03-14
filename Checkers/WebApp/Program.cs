// ====================================== SET UP DEPENDENCY INJECTION ============================

using DAL;
using DAL.Db;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();

// Add services in the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlite(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddScoped<IGameRepository,GameRepositoryDb>();
builder.Services.AddScoped<IGameOptionsRepository,GameOptionsRepositoryDb>();
builder.Services.AddScoped<IGameStateRepository,GameStateRepositoryDb>();
// ====================================== PIPELINE SETUP ========================================
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    if (app.Environment.IsDevelopment())
    {
        app.UseMigrationsEndPoint();
    } 
    else
    {
        app.UseExceptionHandler("/Error");
        // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
        app.UseHsts();
    }
    
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();


// ===================================== RUN THE WEB SERVER ===================================

app.Run();