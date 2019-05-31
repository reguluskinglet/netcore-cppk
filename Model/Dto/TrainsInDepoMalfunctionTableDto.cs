namespace Rzdppk.Model.Dto
{
    public class TrainsInDepoMalfunctionTableDto
    {
        public int TrainInTripCount { get; set; }

        public int TrainDepoCount { get; set; }

        public int GraphViolationCount { get; set; }

        public TrainsInDepoMalfunctionTableItemDto[] Items { get; set; }
    }
}
