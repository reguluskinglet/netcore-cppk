using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Rzdppk.Core.Options;
using Rzdppk.Core.Services.Interfaces;
using Rzdppk.Model.Enums;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing.Filters;
using SixLabors.ImageSharp.Processing.Transforms;

namespace Rzdppk.Core.Services
{
    public class FileService : IFileService
    {
        private const int ImageWidth = 120;
        private const int ImageHeight = 80;

        public int Save(IFormFile imageFile, string fileName)
        {
            var extension = Path.GetExtension(imageFile.FileName).ToLower();

            string originalPath;

            var documentTypeInt = 0;

            if (_fileTypeName.TryGetValue(extension, out DocumentType documentType))
            {
                if (documentType == DocumentType.Image)
                {
                    originalPath = AppSettings.ImagePath(fileName);
                    documentTypeInt = 1;
                    if (imageFile.Length <= 0)
                    {
                        if (File.Exists(originalPath))
                            File.Delete(originalPath);
                        var thPath = AppSettings.ImageThumbsPath(fileName);
                        if (File.Exists(thPath))
                            File.Delete(thPath);
                        return documentTypeInt;
                    }
                }
                else
                {
                    if (documentType == DocumentType.Sound)
                    {
                        originalPath = AppSettings.SoundPath(fileName);
                        documentTypeInt = 2;
                    }
                    else
                        throw new Exception("DocumentType Not File Initializer");
                }
            }
            else
            {
                originalPath = AppSettings.DocumentPath(fileName);
            }
            
           

            if (documentType == DocumentType.Image)
            {
                var imageThumbsPath = AppSettings.ImageThumbsPath(fileName);

                using (Stream imageStream = imageFile.OpenReadStream())
                {
                    using (Image<Rgba32> image = Image.Load(imageStream))
                    {
                        image.Save(originalPath);
                        image.Mutate(x => x
                            .Resize(ImageWidth, ImageHeight)
                            .Grayscale());
                        image.Save(imageThumbsPath);
                    }
                }
            }
            else
            {
                using (var fileStream = new FileStream(originalPath, FileMode.OpenOrCreate, FileAccess.ReadWrite))
                {
                    imageFile.CopyTo(fileStream);//TODO Не менять на async!!!

                }
            }
            return documentTypeInt;
        }

        private readonly Dictionary<string, DocumentType> _fileTypeName = new Dictionary<string, DocumentType>
        {
            {".mp3", DocumentType.Sound},

            {".jpg",  DocumentType.Image},
            {".jpeg", DocumentType.Image},
            {".png",  DocumentType.Image},
            {".gif",  DocumentType.Image},
        };
    }
}
