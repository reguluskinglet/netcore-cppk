using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Rzdppk.Core.Options;
using Rzdppk.Core.Repositoryes;
using Rzdppk.Core.Repositoryes.Base;
using Rzdppk.Core.Repositoryes.Base.Interface;
using Rzdppk.Model;

namespace Rzdppk.Controllers
{
    public class TestController : Controller
    {
        //TestController()
        //{
        //    base.i();
        //}

        [Authorize]
        [Route("api/[controller]/[action]")]
        public IActionResult GetTest()
        {
            return Ok(new
            {
                message = "Idi naxyi"
            });
        }

        [Authorize]
        [Route("api/[controller]/[action]")]
        public IActionResult GetException()
        {
            var tR = new TestRepository();
            tR.Exception();
            return Ok(new
            {
                message = "Idi naxyi"
            });
        }


        [Authorize]
        [HttpPost]
        [Route("api/[controller]/[action]")]
        public async Task<IActionResult> SendFile(DocComment doc)
        {
            foreach (var uploadedFile in doc.files)
            {
                var name = uploadedFile.FileName;
                var path = "/docs/" + name;
                using (var fileStream = new FileStream(AppSettings.WebRootPath + path, FileMode.Create))
                {
                    await uploadedFile.CopyToAsync(fileStream);
                }
            }

            return Ok(new
            {
                message = "Idi naxyi"
            });
        }

        public class DocComment
        {
            public int CommentId { get; set; }
            public IFormFileCollection files { get; set; }
        }

        //    [Authorize]
        //    [Route("api/[controller]/[action]")]
        //    public async Task<string> GetEquipment()
        //    {
        //        var db = new Db();
        //        var cache = new MemoryCache(new MemoryCacheOptions());

        //        var eq = new EquipmentRepository(_logger);
        //        var result = await eq.GetAll(10, 10);
        //        var json = JsonConvert.SerializeObject(result);
        //        return json;
        //    }
        //}
    }
}