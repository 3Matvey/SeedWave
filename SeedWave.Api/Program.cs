using SeedWave.Api.Endpoints.Catalog;
using SeedWave.Api.Endpoints.Regions;
using SeedWave.Api.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSeedWaveServices(builder.Environment);

var app = builder.Build();

app.UseSeedWave();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapGetCatalogEndpoint();
app.MapGetRegionsEndpoint();
app.MapGetSongDetailsEndpoint();
app.MapGetSongPreviewEndpoint();

app.Run();