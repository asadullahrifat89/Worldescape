using WorldescapeServer;

var builder = WebApplication.CreateBuilder(args);
builder.MapServices();

var app = builder.Build();
app.MapEndpoints();
app.MapFeatures();
app.Run();