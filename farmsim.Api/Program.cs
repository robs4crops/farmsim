using NetTopologySuite.IO.Converters;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services
    .AddControllers()
    .AddJsonOptions(options =>
        {
            // this constructor is overloaded.  see other overloads for options.
            var geoJsonConverterFactory = new GeoJsonConverterFactory();
            options.JsonSerializerOptions.Converters.Add(geoJsonConverterFactory);
        });
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
// if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//  app.UseHttpsRedirection();

// app.UseAuthorization();
Console.WriteLine("Starting the app");
app.MapControllers();

app.Run();
