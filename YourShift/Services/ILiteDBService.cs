using LiteDB;
using System.Collections.Generic;

namespace YourShift.Services
{
    public interface ILiteDBService<T>
    {
        void Save(T entity);

        void Delete(T entity);
        void Delete(int id);

        void Update(T entity);

        T Get(T entity);

        T Get(int id);

        List<T> GetAll();
    }
}
