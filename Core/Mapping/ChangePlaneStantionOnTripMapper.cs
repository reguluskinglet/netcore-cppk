using Rzdppk.Model.Raspisanie;
using System.Collections.Generic;
using System.Linq;
using static Rzdppk.Api.ScheludeChangedDtos;

namespace Rzdppk.Core.Mapping
{
    public class ChangePlaneStantionOnTripMapper
    {

        public ChangePlaneStantionOnTrip ToEntity(ChangePlaneStantionOnTripDto input)
        {
            var entity = new ChangePlaneStantionOnTrip
            {
                TrainId = input.TrainId,
                InTime = input.StartFact,
                OutTime = input.EndFact,
                Droped = input.Canceled,
                PlaneStantionOnTripId = input.PlaneStationOnTripId
            };
            return entity;
        }

        public ChangePlaneStantionOnTripDto ToDto(ChangePlaneStantionOnTrip input)
        {
            var dto = new ChangePlaneStantionOnTripDto
            {
                TrainId = input.TrainId,
                StartFact = input.InTime,
                EndFact = input.OutTime,
                Canceled = input.Droped,
                PlaneStationOnTripId = input.PlaneStantionOnTripId
            };
            return dto;
        }

        public List<ChangePlaneStantionOnTrip> ToEntity(List<ChangePlaneStantionOnTripDto> input)
        {
            return input.Select(ToEntity).ToList();
        }

        public List<ChangePlaneStantionOnTripDto> ToDto(List<ChangePlaneStantionOnTrip> input)
        {
            return input.Select(ToDto).ToList();
        }

    }
}