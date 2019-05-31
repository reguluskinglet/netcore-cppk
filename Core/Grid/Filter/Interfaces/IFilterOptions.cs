namespace Core.Grid
{
    public interface IFilterOptions<T>
    {
        T Filter { get; set; }
    }
}