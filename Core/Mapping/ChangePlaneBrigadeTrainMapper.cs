using Rzdppk.Model.Raspisanie;
using System.Collections.Generic;
using System.Linq;
using Rzdppk.Model;
using static Rzdppk.Api.ScheludeChangedDtos;

namespace Rzdppk.Core.Mapping
{
    public class ChangePlaneBrigadeTrainMapper
    {

        public ChangePlaneBrigadeTrain ToEntity(ChangePlaneBrigadeTrainDto input)
        {
            var entity = new ChangePlaneBrigadeTrain
            {
                Id = input.Id,
                StantionStartId = input.StartId,
                StantionEndId = input.EndId,
                UserId = input.UserId,
                PlaneBrigadeTrainId = input.PlaneBrigadeTrainId,
                Droped = input.Canseled
            };
            return entity;
        }

        public ChangePlaneBrigadeTrainDto ToDto(ChangePlaneBrigadeTrain input)
        {
            var dto = new ChangePlaneBrigadeTrainDto
            {
                Id = input.Id,
                StartId = input.StantionStartId,
                EndId = input.StantionEndId,
                UserId = input.UserId,
                PlaneBrigadeTrainId = input.PlaneBrigadeTrainId,
                Canseled = input.Droped
            };
            return dto;
        }

        public List<ChangePlaneBrigadeTrain> ToEntity(List<ChangePlaneBrigadeTrainDto> input)
        {
            return input.Select(ToEntity).ToList();
        }

        public List<ChangePlaneBrigadeTrainDto> ToDto(List<ChangePlaneBrigadeTrain> input)
        {
            return input.Select(ToDto).ToList();
        }

    }
}