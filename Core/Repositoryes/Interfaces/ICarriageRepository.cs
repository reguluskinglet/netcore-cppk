using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Rzdppk.Model;
using Rzdppk.Model.Auth;

namespace Rzdppk.Core.Repositoryes.Interfaces
{
    public interface ICarriageRepository
    {
        Task<CarriageRepository.CarriageExt[]> GetByTrain(Train train);

        Task<CarriageRepository.CarriageExt> GetById(int id);

        Task<Carriage> Add(Carriage carriage);

        Task Update(Carriage carriage);

        Task Delete(int id);
    }
}