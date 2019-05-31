using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Mime;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Rzdppk.Controllers.Base;
using Rzdppk.Core.Options;
using Rzdppk.Core.Repositoryes;
using Rzdppk.Core.Repositoryes.Base.Interface;
using Rzdppk.Core.Services;
using Rzdppk.Model;
using Rzdppk.Model.Enums;

namespace Rzdppk.Controllers
{
    public class DocumentController : BaseController
    {

        private readonly ILogger _logger;

        public DocumentController
        (
            IDb db,
            IMemoryCache memoryCache,
            ILogger<DocumentController> logger
        )
        {
            base.Initialize();
            _logger = logger;
        }

        [Authorize]
        [HttpPost]
        [Route("api/[controller]/[action]")]
        public async Task<JsonResult> Add(IFormFileCollection files)
        {
            await CheckPermission();
            var cr = new DocumentRepository(_logger);
            var fileService = new FileService();
            var docs = new List<Document>();

            var thumbs = new Dictionary<string, string>();

            foreach (var uploadedFile in files)
            {
                var documentTypeInt = fileService.Save(uploadedFile, uploadedFile.FileName);

                //var name = uploadedFile.FileName;
                //var newName = Guid.NewGuid() + Path.GetExtension(name);
                //var path = "/docs/" + newName;
                //using (var fileStream = new FileStream(AppSettings.WebRootPath + path, FileMode.Create))
                //{
                //    await uploadedFile.CopyToAsync(fileStream);
                //}
                //image
                thumbs.Add(uploadedFile.FileName, uploadedFile.FileName);
                if ((DocumentType) documentTypeInt == DocumentType.Image)
                {
                    //thumbs.Add(AppSettings.ImagePathUi(uploadedFile.FileName),AppSettings.ImageThumbsPathUi(uploadedFile.FileName));
                    
                    docs.Add(new Document
                    {
                        TrainTaskCommentId = null,
                        Name = uploadedFile.FileName,
                        //Name = AppSettings.ImagePathUi(uploadedFile.FileName),
                        Description = uploadedFile.FileName,
                        DocumentType = (DocumentType) documentTypeInt
                    });
                }
                //sound
                if ((DocumentType)documentTypeInt == DocumentType.Sound)
                {
                    docs.Add(new Document
                    {
                        TrainTaskCommentId = null,
                        //Name = AppSettings.SoundPathUi(uploadedFile.FileName),
                        Name = uploadedFile.FileName,
                        Description = uploadedFile.FileName,
                        DocumentType = (DocumentType)documentTypeInt
                    });
                }
                //other
                if ((DocumentType)documentTypeInt == DocumentType.Other)
                {
                    docs.Add(new Document
                    {
                        TrainTaskCommentId = null,
                        //Name = AppSettings.DocumentPathUi(uploadedFile.FileName),
                        Name = uploadedFile.FileName,
                        Description = uploadedFile.FileName,
                        DocumentType = (DocumentType)documentTypeInt
                    });
                }


            }

            //Document[] docs1;
            //try
            //{
            var docs1 = await cr.Add(docs.ToArray());
            //}
            //catch (Exception e)
            //{
            //    //delete all uploaded files in case of db error (rollback)
            //    foreach (var doc in docs)
            //    {
            //        if (System.IO.File.Exists(AppSettings.WebRootPath + doc.Name))
            //        {
            //            System.IO.File.Delete(AppSettings.WebRootPath + doc.Name);
            //        }
            //    }
            //    throw e;
            //}

            var ret = new DocumentsUI();
            var docsUi = new List<DocumentUI>();
            foreach (var doc in docs1)
            {
                docsUi.Add(ConvertDocToDocUi(doc, thumbs[doc.Name]));
            }
            ret.Files = docsUi.ToArray();

            cr.Dispose();

            return Json(ret);
        }

        [Authorize]
        [Route("api/[controller]/[action]")]
        public async Task<JsonResult> Delete([FromBody]Document doc)
        {
            await CheckPermission();
            var dr = new DocumentRepository(_logger);
            var doc1 = await dr.GetById(doc.Id);
            if (doc1 == null)
                throw new Exception("doc not found");

            if (System.IO.File.Exists(AppSettings.WebRootPath + doc1.Name))
            {
                System.IO.File.Delete(AppSettings.WebRootPath + doc1.Name);
            }

            await dr.Delete(doc1.Id);
            dr.Dispose();
            return Json(new { message = "Delete OK" });
        }

        public DocumentUI ConvertDocToDocUi(Document doc, string thumbPath)
        {
            var tUiOther = new DocumentUI
            {
                Id = doc.Id,
                TrainTaskCommentId = doc.TrainTaskCommentId,
                ThumbPath = thumbPath,
                Path = doc.Name,
                Name = doc.Description,
                DocumentType = (int)doc.DocumentType

            };
            return tUiOther;
        }

        public class DocumentsUI
        {
            public DocumentUI[] Files { get; set; }
        }

        public class DocumentUI
        {
            public int Id { get; set; }
            public int? TrainTaskCommentId { get; set; }
            public string ThumbPath { get; set; }
            public string Path { get; set; }
            public string Name { get; set; }
            public int DocumentType { get; set;}
        }
    }
}