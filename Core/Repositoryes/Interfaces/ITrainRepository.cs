using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Rzdppk.Model;
using Rzdppk.Model.Auth;

namespace Rzdppk.Core.Repositoryes.Interfaces
{
    public interface ITrainRepository
    {
        Task<TrainRepository.TrainPaging> GetAll(int skip, int limit);

        Task<Train> ByIdWithStations(int id);

        Task<Train> Add(Train train);

        Task Delete(int id);
    }
}