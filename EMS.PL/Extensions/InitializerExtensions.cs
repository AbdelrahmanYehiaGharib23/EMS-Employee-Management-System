using EMS.DAL.Contracts;

namespace EMS.PL.Extensions
{
    public static class InitializerExtensions
    {
        public static void InitializeDatabase(this IApplicationBuilder app) { 
        using var Scope=app.ApplicationServices.CreateScope();
        var Services=Scope.ServiceProvider;
        var dbInitializer = Services.GetRequiredService<IDbInitalizer>(); //Ask Explicitly 
        dbInitializer.Initilaize();
        dbInitializer.Seed();
        
        }
    }
}
