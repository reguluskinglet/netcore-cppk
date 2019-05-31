using Microsoft.AspNetCore.Http;

namespace Rzdppk.Core.Services.Interfaces
{
    public interface IFileService
    {
        int Save(IFormFile imageFile, string fileName);
    }
}