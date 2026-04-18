using AthenaUnionAI.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddServices();

builder.Services.AddControllers();
builder.Services.AddOpenApi();

builder.Services.AddCors(options =>
{
    options.AddPolicy("LocalFile", policy =>
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod());
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

//app.UseHttpsRedirection();

app.UseCors("LocalFile");

app.UseAuthorization();

app.MapControllers();

app.Run();
