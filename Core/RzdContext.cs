using System;
using System.IO;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.Extensions.Configuration;
using Rzdppk.Core.Extensions;
using Rzdppk.Core.Migrations.Configurations;
using Rzdppk.Model;
using Rzdppk.Model.Auth;
using Rzdppk.Model.Base;
using Rzdppk.Model.Enums;
using Rzdppk.Model.Raspisanie;
using Rzdppk.Model.Terminal;
using Rzdppk.Model.TV;

namespace Rzdppk.Core
{
    public class RzdContext : DbContext
    {
        public DbSet<Permission> Permissions { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }
        public DbSet<Brigade> Brigades { get; set; }

        public DbSet<Carriage> Carriages { get; set; }
        public DbSet<CarriageMigration> CarriageMigrations { get; set; }

        public DbSet<CheckListEquipment> CheckListEquipments { get; set; }
        public DbSet<Document> Documents { get; set; }
        public DbSet<Equipment> Equipments { get; set; }
        public DbSet<EquipmentCategory> EquipmentCategoryes { get; set; }
        public DbSet<EquipmentModel> EquipmentModels { get; set; }
        public DbSet<Fault> Faults { get; set; }
        public DbSet<FaultEquipment> FaultEquipments { get; set; }
        public DbSet<Inspection> Inspections { get; set; }
        public DbSet<InspectionData> InspectionData { get; set; }
        public DbSet<Label> Labels { get; set; }
        public DbSet<Meterage> Meterage { get; set; }

        public DbSet<Rzdppk.Model.Model> Models { get; set; }
        public DbSet<Signature> Signatures { get; set; }
        public DbSet<Train> Trains { get; set; }
        public DbSet<TrainTask> TrainTasks { get; set; }
        public DbSet<TrainTaskAttribute> TrainTaskAttributes { get; set; }
        public DbSet<TrainTaskComment> TrainTaskComments { get; set; }
        public DbSet<TrainTaskExecutor> TrainTaskExecutors { get; set; }
        public DbSet<TrainTaskStatus> TrainTaskStatuses { get; set; }

        public DbSet<TaskPrint> TaskPrints { get; set; }
        public DbSet<TaskPrintItem> TaskPrintItems { get; set; }
        public DbSet<TemplateLabel> TemplateLabels { get; set; }
        public DbSet<RemovedEntity> RemovedEntities { get; set; }
        public DbSet<ActCategory> ActCategories { get; set; }
        public DbSet<EquipmentAct> EquipmentActs { get; set; }

        public DbSet<Device> Devices { get; set; }
        public DbSet<DeviceFault> DeviceFaults { get; set; }
        public DbSet<DeviceHistory> DeviceHistories { get; set; }
        public DbSet<DeviceTask> DeviceTasks { get; set; }
        public DbSet<DeviceTaskComment> DeviceTaskComments { get; set; }
        public DbSet<DeviceValue> DeviceValues { get; set; }

        public DbSet<TVBox> TvBoxes { get; set; }
        public DbSet<TVPanel> TvPanels { get; set; }

        public DbSet<Stantion> Stantions { get; set; }
        public DbSet<StantionOnTrip> StantionOnTrips { get; set; }
        public DbSet<Trip> Trips { get; set; }
        public DbSet<TripOnRoute> TripOnRoutes { get; set; }
        public DbSet<Route> Routes { get; set; }
        public DbSet<ChangedPlanedInspectionRoute> ChangedPlanedInspectionRoutes { get; set; }
        public DbSet<ChangePlaneBrigadeTrain> ChangePlaneBrigadeTrains { get; set; }
        public DbSet<ChangePlaneStantionOnTrip> ChangePlaneStantionOnTrips { get; set; }
        public DbSet<DayOfRoute> DayOfRoutes { get; set; }
        public DbSet<DayOfTrip> DayOfTrips { get; set; }
        public DbSet<InspectionRoute> InspectionRoutes { get; set; }
        public DbSet<PlaneBrigadeTrain> PlaneBrigadeTrains { get; set; }
        public DbSet<PlanedInspectionRoute> PlanedInspectionRoutes { get; set; }
        public DbSet<PlanedRouteTrain> PlanedRouteTrains { get; set; }
        public DbSet<PlaneStantionOnTrip> PlaneStantionOnTrips { get; set; }
        public DbSet<Direction> Directions { get; set; }
        public DbSet<Parking> Parkings { get; set; }
        public DbSet<DepoEvent> DepoEvents { get; set; }
        public DbSet<Turnover> Turnovers { get; set; }
        public DbSet<TechPass> TechPasses { get; set; }

        public RzdContext() { }

        public RzdContext(DbContextOptions<RzdContext> options)
            : base(options)
        {

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            SetCommonInitialize(modelBuilder);

            modelBuilder.Entity<UserRole>().ToTable("auth_roles");

            modelBuilder.Entity<User>()
                .HasIndex(b => b.Login)
                .IsUnique();

            modelBuilder.Entity<Trip>().Property(x=>x.TripType).HasDefaultValue(TripType.Default);
            modelBuilder.AddConfiguration(new PlaneStantionOnTripConfiguration());
            modelBuilder.AddConfiguration(new PlanedInspectionRouteConfiguration());
            modelBuilder.AddConfiguration(new UserConfiguration());
            modelBuilder.AddConfiguration(new EquipmentModelConfiguration());
            modelBuilder.AddConfiguration(new RouteConfiguration());
            modelBuilder.AddConfiguration(new DeviceTaskConfiguration());

            SingleConfiguration(modelBuilder);
        }

        protected virtual void SingleConfiguration(ModelBuilder modelBuilder, bool mobile=false)
        {
            if (mobile)
            {
                modelBuilder.AddConfiguration(new TrainTaskTerminalConfiguration());
                modelBuilder.Entity<DocumentTerminal>().Property(x => x.DocumentType).HasDefaultValue(DocumentType.Image);
            }
            else
            {
                modelBuilder.AddConfiguration(new TrainTaskConfiguration());
                modelBuilder.Entity<Document>().Property(x => x.DocumentType).HasDefaultValue(DocumentType.Other);
            }
        }

        public void SetCommonInitialize(ModelBuilder modelBuilder)
        {
            var type = typeof(RzdContext);
            foreach (var property in type.GetProperties())
            {
                if (property.PropertyType.Name == typeof(DbSet<BaseEntity>).Name)
                {
                    SetCommonModelPropertiesDelegate<BaseEntity> del = SetCommonModelProperties<BaseEntity>;
                    var entityType = property.PropertyType.GetGenericArguments().Single();
                    var method = type.GetMethod(del.Method.Name).MakeGenericMethod(entityType);
                    method.Invoke(this, new[] { modelBuilder });
                }
            }
        }

        public virtual void SetCommonModelProperties<TEntity>(ModelBuilder modelBuilder)
            where TEntity : BaseEntity
        {
            modelBuilder.Entity<TEntity>().Property(e => e.UpdateDate).HasDefaultValueSql("GETDATE()");

            var baseType = typeof(TEntity).BaseType;

            if (baseType == typeof(BaseEntityRef))
            {

                modelBuilder.Entity(typeof(TEntity)).HasIndex("RefId").IsUnique();
                modelBuilder.Entity(typeof(TEntity)).Property("RefId").HasDefaultValueSql("NEWID()");
            }
        }

        public delegate void SetCommonModelPropertiesDelegate<TEntity>(ModelBuilder builder)
            where TEntity : BaseEntity;
    }

}
