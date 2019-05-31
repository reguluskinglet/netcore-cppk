using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Text;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

namespace Rzdppk.Core.Options
{
    public static class AppSettings
    {
        private static IConfiguration _configuration;

        public static void SetConfiguration(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        //public static string WebRootPath => "C:\\docs\\Rzd_bitbucket\\Rzdppk\\wwwroot";
        public static string WebRootPath => "wwwroot\\";

        //TODO убрать хардкод конекшн стринга. Из тестов нихуя не видит.
        public static string ConnectionString => _configuration.GetConnectionString("DefaultConnection");

        public static string ApiKey => _configuration.GetSection("ApiKey").Value;
        public static bool DbInit => _configuration.GetValue<bool>("DbInit");
        //public static string ConnectionString => @"Data Source=.\SQLEXPRESS;Initial Catalog=rzd;Integrated Security=True; MultipleActiveResultSets=True";
        //public static string ConnectionString => _configuration.GetConnectionString("DefaultConnection");
        //public static string ConnectionString => @"Data Source=.\SQLEXPRESS;Initial Catalog=cppk;Integrated Security=True; MultipleActiveResultSets=True";
        //public static string ConnectionString =>
        //   @"Data Source = 109.69.72.102\SQL2012;Initial Catalog = cppk_last; user ID = sa; password=Xabaxa72;MultipleActiveResultSets=true;";

        //Data Source =.\SQLEXPRESS;Initial Catalog = rzd; Integrated Security = True; MultipleActiveResultSets=True

        public const string ProjectFileDirectory = "project";
        public const string ImageFolder = "images";
        public const string ImageThumbsFolder = "thumbs";
        public const string SoundFolder = "sounds";
        public const string DocumentFolder = "docs";

        public static void CreateProjectDirectories(IHostingEnvironment hostingEnvironment)
        {
            var projectFolder = Path.Combine(hostingEnvironment.WebRootPath, ProjectFileDirectory);
            _imageFolder = Path.Combine(projectFolder, ImageFolder);
            _imageThumbsFolder = Path.Combine(_imageFolder, ImageThumbsFolder);
            _documentFolder = Path.Combine(projectFolder, DocumentFolder);
            _soundFolder = Path.Combine(projectFolder, SoundFolder);

            if (!Directory.Exists(_imageThumbsFolder))
                Directory.CreateDirectory(_imageThumbsFolder);

          
            if (!Directory.Exists(_documentFolder))
                Directory.CreateDirectory(_documentFolder);


            if (!Directory.Exists(_soundFolder))
                Directory.CreateDirectory(_soundFolder);
        }

        private static string _imageFolder;
        private static string _imageThumbsFolder;
        private static string _documentFolder;
        private static string _soundFolder;

        public static Func<string, string> ImagePath = fileName => Path.Combine(_imageFolder, fileName);
        public static Func<string, string> ImageThumbsPath = fileName => Path.Combine(_imageThumbsFolder, fileName);
        public static Func<string, string> DocumentPath = fileName => Path.Combine(_documentFolder, fileName);
        public static Func<string, string> SoundPath = fileName => Path.Combine(_soundFolder, fileName);

        public static Func<string, string> ImagePathUi = fileName => Path.Combine($"/{ProjectFileDirectory}/{ImageFolder}/", fileName);
        public static Func<string, string> ImageThumbsPathUi = fileName => Path.Combine($"/{ProjectFileDirectory}/{ImageFolder}/{ImageThumbsFolder}/", fileName);
        public static Func<string, string> DocumentPathUi = fileName => Path.Combine($"/{ProjectFileDirectory}/{DocumentFolder}/", fileName);
        public static Func<string, string> SoundPathUi = fileName => Path.Combine($"/{ProjectFileDirectory}/{SoundFolder}/", fileName);
    }
}
