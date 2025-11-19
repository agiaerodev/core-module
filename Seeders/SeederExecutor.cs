using Core.Exceptions;
using Idata.Data;
using Idata.Data.Entities.Isite;
using Ihelpers.Interfaces;
using Microsoft.AspNetCore.Builder;

public static class SeederExecutor
{
    public static void ExecuteSeeder(Action<IApplicationBuilder> seederMethod, string seederName, IApplicationBuilder app)
    {
#if DEBUG
        Console.WriteLine($"[Seeder Start] {seederName}");
#endif
        try
        {
            DoSomethingBefore(app);
            seederMethod(app);
#if DEBUG
            Console.WriteLine($"[Seeder Success] {seederName}");
#endif
        }
        catch (Exception ex)
        {
            ExceptionBase.HandleException(ex, $"An error has occurred executing the {seederName} seeder at: {DateTime.UtcNow}");
        }

        //DoSomethingAfter(seederName);
    }

    //private static void DoSomethingAfter(string seederName)
    //{
    //    Console.WriteLine($"[After Action] {seederName}");
    //}

    private static void DoSomethingBefore(IApplicationBuilder app)
    {
        using (var serviceScope = app.ApplicationServices.CreateScope())
        {
            var cacheBase = serviceScope.ServiceProvider.GetService<ICacheBase>();
            cacheBase.RemoveStartingWith("config_settings").GetAwaiter().GetResult();
            cacheBase.RemoveStartingWith(typeof(Setting).FullName!).GetAwaiter().GetResult();
        }
    }
}
