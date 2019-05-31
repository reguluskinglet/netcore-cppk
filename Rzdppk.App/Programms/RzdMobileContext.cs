using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Rzdppk.App.Models;
using Rzdppk.Core;
using Rzdppk.Model;
using Rzdppk.Model.Base;
using Rzdppk.Model.Raspisanie;
using Rzdppk.Model.Terminal;

namespace Rzdppk.App.Programms
{
    public class RzdMobileContext : RzdContext
    {
        public DbSet<InspectionTagScaned> InspectionTagScaneds { get; set; }
        public DbSet<TrainTaskTerminal> TrainTaskTerminals { get; set; }
        public DbSet<TrainTaskExecutorTerminal> TrainTaskExecutorTerminals { get; set; }
        public DbSet<TrainTaskCommentTerminal> TrainTaskCommentTerminals { get; set; }
        public DbSet<TrainTaskStatusTerminal> TrainTaskStatusTerminals { get; set; }
        public DbSet<InspectionTerminal> InspectionTerminals { get; set; }
        public DbSet<InspectionDataTerminal> InspectionDataTerminals { get; set; }
        public DbSet<DocumentTerminal> DocumentTerminals { get; set; }
        public DbSet<SignatureTerminal> SignatureTerminals { get; set; }
        public DbSet<MeterageTerminal> MeterageTerminals { get; set; }
        public DbSet<TrainTaskAttributeTerminal> TrainTaskAttributeTerminals { get; set; }
        public DbSet<TerminalBrigadeRouteTrain> TerminalBrigadeTrains { get; set; }
        public DbSet<TerminalInspectionRouteTrain> TerminalInspectionRouteTrains { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var configure = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .AddEnvironmentVariables()
                .Build();

            optionsBuilder.UseSqlite(configure.GetConnectionString("SQLiteConnection"));
        }

        public override void SetCommonModelProperties<TEntity>(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TEntity>().Property(e => e.UpdateDate).HasDefaultValueSql("datetime('now')");
        }

        protected override void SingleConfiguration(ModelBuilder modelBuilder, bool mobile = false)
        {
            base.SingleConfiguration(modelBuilder, true);

            modelBuilder.Ignore(typeof(TrainTask));
            modelBuilder.Ignore(typeof(TrainTaskAttribute));
            modelBuilder.Ignore(typeof(TrainTaskComment));
            modelBuilder.Ignore(typeof(TrainTaskExecutor));
            modelBuilder.Ignore(typeof(TrainTaskStatus));
            modelBuilder.Ignore(typeof(Inspection));
            modelBuilder.Ignore(typeof(InspectionData));
            modelBuilder.Ignore(typeof(Document));
            modelBuilder.Ignore(typeof(Signature));
            modelBuilder.Ignore(typeof(Meterage));
            modelBuilder.Ignore(typeof(RemovedEntity));
            modelBuilder.Ignore(typeof(DeviceHistory));
            modelBuilder.Ignore(typeof(DeviceValue));


            modelBuilder.Ignore(typeof(ChangedPlanedInspectionRoute));
            modelBuilder.Ignore(typeof(ChangePlaneBrigadeTrain));
            modelBuilder.Ignore(typeof(ChangePlaneStantionOnTrip));
            modelBuilder.Ignore(typeof(ChangePlaneStantionOnTrip));
        }
    }
}
