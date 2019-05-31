using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Rzdppk.Core.Repositoryes.Interfaces
{
    public interface IExternalRepository
    {
        Task<object> GetAllData(DateTime? date);
        bool SaveTerminalResult(ExternalRepository.TerminalResultDto model);
        bool SaveDocuments(List<ExternalRepository.UploadDocumentDto> documents);
        Task<bool> SaveDeviceValue(ExternalRepository.DeviceValueDto model);
    }
}