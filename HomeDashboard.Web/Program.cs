using HomeDashboard.Web.MQTT;
using HomeDashboard.Web.Services;

var builder = WebApplication.CreateBuilder(args);

// Add optional config file for containerized deployments
builder.Configuration.AddJsonFile("/config.json", optional: true, reloadOnChange: true);

builder.AddMqttInfrastructure();

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddSingleton<ILightControlService, LightControlService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
	app.UseExceptionHandler("/Error");
	// The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
	app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages()
   .WithStaticAssets();

await app.RunAsync();
