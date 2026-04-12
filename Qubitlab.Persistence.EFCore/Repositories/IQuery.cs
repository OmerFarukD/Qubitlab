namespace Qubitlab.Persistence.EFCore.Repositories;

public interface IQuery<T>
{
    IQueryable<T> Query();
}