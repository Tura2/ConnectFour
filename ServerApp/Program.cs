using ServerApp.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// ? ????? ????? ??Controllers (Web API)
builder.Services.AddControllers(); // ? ?? ??? ????

builder.Services.AddRazorPages();

builder.Services.AddDbContext<ConnectFourContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

// ? ????? ????? ??API Controllers
app.MapControllers(); // ? ?? ?? ??? ???

app.MapRazorPages();

app.Run();
