using ChatOnline.Models;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;

namespace ChatOnline.Database
{
    public static class Seed
    {
        public static void Initialize(IServiceProvider serviceProvider)
        {
            var context = serviceProvider.GetRequiredService<ApplicationDbContext>();
            if (!context.Rooms.Any())
            {
                context.Rooms.Add(new Room { Name = "FFA" });
                context.Rooms.Add(new Room { Name = "FFA1" });
                context.Rooms.Add(new Room { Name = "FFA2" });
                context.SaveChanges();
            }
        }
    }
}
