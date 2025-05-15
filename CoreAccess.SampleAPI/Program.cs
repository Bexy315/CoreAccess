using CoreAccess.SampleAPI.Services;

namespace CoreAccess.SampleAPI;

public static class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        
        builder.Services.AddControllers();
        
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        builder.Services.AddSingleton<ProductService>();
        
        builder.Services.AddCoreAccess(options =>
        {
            options.DatabaseType = DatabaseType.SQLiteFile;
            options.ConnectionString = "Data Source=coreaccess.db";
        });
        
        var app = builder.Build();

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();

        app.UseAuthorization();
        

        app.MapControllers();

        app.Run();
    }
}