using AutoMapper;
using Core.Extensions;
using DinkToPdf;
using DinkToPdf.Contracts;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SpaServices.Webpack;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Converters;
using Rzdppk.Core;
using Rzdppk.Core.Extensions;
using Rzdppk.Core.Options;
using Rzdppk.Core.Repositoryes;
using Rzdppk.Core.Repositoryes.Base;
using Rzdppk.Core.Repositoryes.Base.Interface;
using Rzdppk.Core.Repositoryes.Interfaces;
using Rzdppk.Core.Services;
using Rzdppk.Core.Services.Interfaces;
using Rzdppk.Model;
using Rzdppk.Model.Raspisanie;
using System;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.Loader;
using System.Threading.Tasks;
using static Rzdppk.Core.Services.ScheduleCycleService;
using static Rzdppk.Core.Services.TripOnRoutesService;

namespace Rzdppk
{
    public class Startup
    {

        private readonly IHostingEnvironment _hostingEnvironment;

        public Startup(IConfiguration configuration, IHostingEnvironment env)
        {
            Configuration = configuration;

            var cultureInfo = new CultureInfo("ru-RU");
            _hostingEnvironment = env;

            CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
            CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;
        }

        public Startup(IHostingEnvironment env)
        {
            var cultureInfo = new CultureInfo("ru-RU");


            CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
            CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;
        }


        public IConfiguration Configuration { get; }
        private IServiceProvider _provider;

        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            AppSettings.SetConfiguration(Configuration);

            services.AddDbContext<RzdContext>(options => options.UseSqlServer(AppSettings.ConnectionString));

            services.AddMemoryCache();

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(_jwtBearerOptions);

            services.AddScoped<IDb, Db>();
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IFileService, FileService>();
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<ITvPanelRepository, TvPanelRepository>();
            services.AddScoped<ITvPanelSetupRepository, TvPanelSetupRepository>();
            services.AddScoped<ICheckListEquipmentRepository, CheckListEquipmentRepository>();
            services.AddScoped<ICategoryEquipmentRepository, CategoryEquipmentRepository>();
            services.AddScoped<IEquipmentRepository, EquipmentRepository>();
            services.AddScoped<IDepoEventsRepository, DepoEventsRepository>();
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddScoped<IExternalRepository, ExternalRepository>();
            services.AddScoped<ICommonService, CommonService>();
            services.AddScoped<ITechPassRepository, TechPassRepository>();

            var architectureFolder = (IntPtr.Size == 8) ? "64" : "32";
            var wkHtmlToPdfPath = Path.Combine(_hostingEnvironment.ContentRootPath, $"Libs\\{architectureFolder}\\libwkhtmltox");
            CustomAssemblyLoadContext context = new CustomAssemblyLoadContext();
            context.LoadUnmanagedLibrary(wkHtmlToPdfPath);

            services.AddSingleton(typeof(IConverter), new SynchronizedConverter(new PdfTools()));
            services.AddCors();
            services.AddAutoMapper();
            services.AddMvc(options => { options.UseGridOptionsModelBinding(_provider.GetService<IMemoryCache>()); })
                .AddJsonOptions(options =>
                {
                    options.SerializerSettings.Converters.Add(new IsoDateTimeConverter { DateTimeFormat = "yyyy-MM-ddTHH:mm:ssZ" });
                });

            _provider = services.BuildServiceProvider();

            return _provider;
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, IServiceProvider serviceProvider, ILoggerFactory logging)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseWebpackDevMiddleware(new WebpackDevMiddlewareOptions
                {
                    HotModuleReplacement = true,
                    ReactHotModuleReplacement = true
                });
            }

            AppSettings.CreateProjectDirectories(env);

            //Accept All HTTP Request Methods from all origins
            app.UseCors(builder =>
                builder.AllowAnyHeader().AllowAnyOrigin().AllowAnyMethod());

            InitializeDatabase(serviceProvider);

            app.UseAuthentication();
            app.UseStaticFiles();

            app.UseMiddleware(typeof(ErrorHandlingMiddleware));
            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");

                routes.MapSpaFallbackRoute("spa-fallback", new { controller = "Home", action = "Index" });
            });
            //add NLog to ASP.NET Core
            //logging.AddNLog();

            ////add NLog.Web
            //app.AddNLogWeb();
            //env.ConfigureNLog("nlog.config");

        }

        private readonly Action<JwtBearerOptions> _jwtBearerOptions = options =>
        {
            options.RequireHttpsMetadata = false;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateLifetime = true,
                IssuerSigningKey = Constants.GetSymmetricSecurityKey(),
                ValidateIssuerSigningKey = true,
            };

            //Необходимо для передаче файлов
            options.Events = new JwtBearerEvents
            {
                OnMessageReceived = (context) =>
                {
                    if (context.Request.QueryString.HasValue && !context.Request.Headers.ContainsKey("Authorization"))
                    {
                        var query = QueryHelpers.ParseQuery(context.Request.QueryString.Value);

                        StringValues token;

                        if (query.TryGetValue("access_token", out token))
                            context.Token = token;
                    }

                    return Task.FromResult(0);
                }
            };
        };

        private void InitializeDatabase(IServiceProvider serviceProvider)
        {
            using (var context = serviceProvider.GetService<RzdContext>())
            {
                context.Database.Migrate();
                context.EnsureSeedData();
            }
        }

        public class MappingProfile : Profile
        {
            public MappingProfile()
            {
                // Add as many of these lines as you need to map your objects
                CreateMap<TripOnRouteWithStationsDto, TripOnRoute>();
                CreateMap<TripOnRoute, TripOnRouteWithStationsDto>();
                CreateMap<Trip, TripWithDateTimeStations>();
                CreateMap<TripWithDateTimeStations, Trip>();
                CreateMap<StantionOnTrip, StationOnTripWithStringDateTime>();
                CreateMap<StationOnTripWithStringDateTime, StantionOnTrip>();
                CreateMap<StantionOnTrip, StationOnTripWithStringDateTime>();
                CreateMap<StationOnTripWithStringDateTime, StantionOnTrip>();
                CreateMap<StantionOnTrip, PlaneStantionOnTrip>();
                CreateMap<PlaneStantionOnTrip, StantionOnTrip>();
                CreateMap<Stantion, PlaneStantionOnTrip>();
                CreateMap<PlaneStantionOnTrip, Stantion>();
                CreateMap<ModelService.EquipmentModelsWithOldIds, EquipmentModel>();
                CreateMap<EquipmentModel, ModelService.EquipmentModelsWithOldIds>();
                CreateMap<ChangePlaneStantionOnTrip, PlaneStantionOnTrip>();
                CreateMap<PlaneStantionOnTrip, ChangePlaneStantionOnTrip>();
                CreateMap<TurnoverWithDays, Turnover>();
                CreateMap<Turnover, TurnoverWithDays>();
                CreateMap<Trip, TripWithTripOnRouteId>();
                CreateMap<TripWithTripOnRouteId, Trip>();
                CreateMap<Trip, TripWithStartEndTimeAndDays>();
                CreateMap<TripWithStartEndTimeAndDays, Trip>();
                CreateMap<Stantion, ScheludePlanedService.StantionQq>();
                CreateMap<ScheludePlanedService.StantionQq, Stantion>();


            }
        }

        internal class CustomAssemblyLoadContext : AssemblyLoadContext
        {
            public IntPtr LoadUnmanagedLibrary(string absolutePath)
            {
                return LoadUnmanagedDll(absolutePath);
            }

            protected override IntPtr LoadUnmanagedDll(String unmanagedDllName)
            {
                return LoadUnmanagedDllFromPath(unmanagedDllName);
            }

            protected override Assembly Load(AssemblyName assemblyName)
            {
                throw new NotImplementedException();
            }
        }


    }
}

