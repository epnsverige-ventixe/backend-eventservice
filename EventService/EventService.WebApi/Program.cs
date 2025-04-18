using Microsoft.AspNetCore.Diagnostics;

var builder = WebApplication.CreateBuilder(args);

string[] allowedOrigins;
try
{
    allowedOrigins = builder.Configuration
        .GetSection("CORS:AllowedOrigins")
        .Get<string[]>() ?? throw new Exception("CORS:AllowedOrigins saknas i konfigurationen.");
}
catch (Exception ex)
{
    Console.WriteLine($"[Startup Error] CORS-konfiguration misslyckades: {ex.Message}");
    throw;
}

// Services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy
            .WithOrigins(allowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();

app.UseExceptionHandler("/error");

// Utvecklingsläge
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "EventService API v.1");
        options.RoutePrefix = string.Empty;
    });

    app.UseDeveloperExceptionPage();
}

app.UseCors("AllowFrontend");

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Map("/error", (HttpContext context) =>
{
    var feature = context.Features.Get<IExceptionHandlerFeature>();
    var exception = feature?.Error;

    Console.WriteLine($"[Global Error] {exception?.Message}");
    return Results.Problem(
        title: "Ett oväntat fel har inträffat",
        detail: exception?.Message,
        statusCode: 500);
});

app.Run();