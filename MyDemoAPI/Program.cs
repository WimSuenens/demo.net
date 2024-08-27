using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.OpenApi.Models;
using MudBlazor.Services;
using MyDemoAPI.Components;
using MyDemoAPI.Data;
using MyDemoAPI.Services;
using MyDemoAPI.Utils;

// https://learn.microsoft.com/en-us/aspnet/core/tutorials/first-mongo-app?view=aspnetcore-8.0&tabs=visual-studio
// https://stackoverflow.com/questions/63304279/re-use-mongodb-mongoclient-in-asp-net-core-service

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddBrowserTimeProvider();

builder.Services.SetupMongoDbContext(builder.Configuration);

builder.Services.AddHttpClient();

// Add services to the container.
builder.Services.AddSingleton<IConversationService, ConversationService>();
builder.Services.AddSingleton<IAnthropicService, AnthropicService>();

builder.Services
    .AddControllers()
    .AddJsonOptions(options => {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    });

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options => {
    // using System.Reflection;
    options.SwaggerDoc("v1", new OpenApiInfo {
        Version = "v1",
        Title = "Demo Title",
        Description = "Demo Description",
    });
    var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
});

// Add MudBlazor services
builder.Services.AddMudServices();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// app.UseHttpsRedirection();

app.MapControllers();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();

