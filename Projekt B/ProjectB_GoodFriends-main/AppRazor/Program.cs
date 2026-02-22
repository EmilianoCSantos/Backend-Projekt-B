using Configuration;
using Configuration.Extensions;
using Configuration.Options;
using DbContext.Extensions;
using DbRepos;
using Encryption.Extensions;
using Services;
using Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// För Razor Pages
builder.Services.AddRazorPages();

// Secrets och configuration
builder.Configuration.AddSecrets(builder.Environment);

// Encryption, database, services
builder.Services.AddEncryptions(builder.Configuration);
builder.Services.AddJwtToken(builder.Configuration);
builder.Services.AddDatabaseConnections(builder.Configuration);
builder.Services.AddUserBasedDbContext();

builder.Services.AddVersionInfo();
builder.Services.AddEnvironmentInfo();
builder.Services.AddInMemoryLogger();

// Registrera DbRepos och Services
builder.Services.AddScoped<AdminDbRepos>();
builder.Services.AddScoped<FriendsDbRepos>();
builder.Services.AddScoped<AddressesDbRepos>();
builder.Services.AddScoped<PetsDbRepos>();
builder.Services.AddScoped<QuotesDbRepos>();

builder.Services.AddScoped<IAdminService, AdminServiceDb>();
builder.Services.AddScoped<IFriendsService, FriendsServiceDb>();
builder.Services.AddScoped<IAddressesService, AddressesServiceDb>();
builder.Services.AddScoped<IPetsService, PetsServiceDb>();
builder.Services.AddScoped<IQuotesService, QuotesServiceDb>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();
app.MapRazorPages();

app.Run();