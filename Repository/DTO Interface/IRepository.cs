namespace Repository.DTO_Interfaces
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;

    public interface IRepository<TModel> where TModel : class
    {
        // Selects all from table
        List<TModel> SelectAll();

        // Selects all from table with where or select expression provided
        List<TModel> SelectAll(Expression<Func<TModel, bool>> predicate);

        // Return single entity and find entity by Id
        TModel Find(int id);

        // Returns entity after successful or unsuccessful insertion
        int Insert(TModel entity);

        // Returns boolean after successful or unsuccessful updation
        bool Update(TModel entity);
        bool Update(int id, TModel entity);

        // Returns boolean after successful or unsuccessful delete
        bool Remove(int id);
    }
}
