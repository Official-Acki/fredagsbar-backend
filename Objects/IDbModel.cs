public interface IDbModel<T>
{
    static abstract Task<T?> CreateObj(params object[] args);
    static abstract Task<T?> ReadObj(int id);
    static abstract Task<IEnumerable<T>> GetAll();
}