using MySqlConnector;
using FitnessTracker.Areas.Identity;
using FitnessTracker.Data;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.EntityFrameworkCore;
using Blazorise;
using Blazorise.Bootstrap5;
using Blazorise.Icons.FontAwesome;
using MySqlConnector;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContextFactory<FitnessTrackerContext>(options =>
{
    var connectionStringBuilder = new MySqlConnectionStringBuilder
    {
        ConnectionString = builder.Configuration.GetConnectionString("FitnessTracker"),
        Password = builder.Configuration["FitnessTracker:RootPassword"]
    };
    options
        .UseMySql(connectionStringBuilder.ConnectionString,
            new MySqlServerVersion(builder.Configuration["MySqlVersion"]));
});
builder.Services.AddDatabaseDeveloperPageExceptionFilter();
builder.Services.AddDefaultIdentity<FitnessTrackerUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<FitnessTrackerContext>();
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services
    .AddBlazorise(options => options.Immediate = true)
    .AddBootstrap5Providers()
    .AddFontAwesomeIcons();
builder.Services
    .AddScoped<AuthenticationStateProvider, RevalidatingIdentityAuthenticationStateProvider<FitnessTrackerUser>>();
builder.Services.AddScoped<FitnessPlanService>();
builder.Services.AddScoped<FitnessInterestService>();
builder.Services.AddScoped<UserService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
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

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();
