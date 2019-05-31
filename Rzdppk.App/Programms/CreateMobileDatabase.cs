using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Rzdppk.Core;
using Rzdppk.Core.Extensions;
using Rzdppk.Core.Options;

namespace Rzdppk.App.Programms
{
    public class CreateMobileDatabase
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("Start Migration");

            using (var context = new RzdMobileContext())
            {
                context.Database.Migrate();
                //context.EnsureSeedData();
            }

            Console.WriteLine("End Migration");
            Console.ReadKey();
        }
    }
}
