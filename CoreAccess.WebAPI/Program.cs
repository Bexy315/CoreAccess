using CoreAccess.WebAPI.Repositories;
using CoreAccess.WebAPI.Services;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddOpenApi();

#region Repositories

builder.Services.AddScoped<IUserRepository, UserRepository>();

#endregion

#region Services

builder.Services.AddScoped<IUserService, UserService>();

#endregion

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();            
    app.UseSwaggerUI();
}

app.UseAuthorization();

app.MapControllers();

app.Run();