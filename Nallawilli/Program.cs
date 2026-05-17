using Microsoft.EntityFrameworkCore;
using Nallawilli.Data;
using Microsoft.AspNetCore.Identity;
using Nallawilli.Options;
using Nallawilli.Services.Interfaces;
using Nallawilli.Services.Admin.Implementations;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddRazorPages();

builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});

builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IPostService, PostService>();
builder.Services.AddScoped<ICmsPageService, CmsPageService>();
builder.Services.AddScoped<ICmsSectionService, CmsSectionService>();
builder.Services.AddScoped<ICmsSectionContentService, CmsSectionContentService>();
builder.Services.AddScoped<ICmsManageService, CmsManageService>();
builder.Services.Configure<CmsAdminOptions>(
    builder.Configuration.GetSection(CmsAdminOptions.SectionName));

builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = false)
    .AddEntityFrameworkStores<AppDbContext>();

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

app.UseAuthentication();

app.UseAuthorization();

app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

app.Run();
