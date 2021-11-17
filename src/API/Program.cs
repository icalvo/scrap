using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Scrap.API;

Host.CreateDefaultBuilder(args)
    .ConfigureWebHostDefaults(webBuilder => { webBuilder.UseStartup<Startup>(); })
    .Build()
    .Run();
