namespace Rzdppk.Core.Services.Interfaces
{
    public interface ISqlQueryStorage
    {
        string GetAllPaging(int skip, int limit);
        string Select();
        string Count();
        string ById(int id);

    }
}