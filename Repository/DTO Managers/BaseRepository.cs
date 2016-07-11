namespace Repository.DTO_Managers
{
    using Dapper;
    using Repository.DTO_Interfaces;
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Data;
    using System.Data.SqlClient;
    using System.Linq;

    public class BaseRepository<TModel> : IRepository<TModel>, IDisposable where TModel : class
    {

        private string _tableName, query;

        private string _connectionString = ConfigurationManager.ConnectionStrings["dbConnection"].ConnectionString;

        private IsolationLevel isolationLevel = IsolationLevel.ReadCommitted;

        protected IDbConnection db;
        protected IDbTransaction _transaction;

        protected string connectionString
        {
            get { return this._connectionString; }
            set { this._connectionString = value; }
        }

        public BaseRepository()
        {
            if (db == null)
                db = new SqlConnection(connectionString);

            this._tableName = typeof(TModel).Name;
        }

        public List<TModel> SelectAll()
        {

            List<TModel> listOfTModel = new List<TModel>();

            try
            {
                this.OpenConnection();
                query = string.Format("Select * from {0}", _tableName);
                listOfTModel.AddRange(db.Query<TModel>(query));
            }
            catch (Exception ex)
            {

            }
            finally
            {
                this.CloseConnection();
            }

            return listOfTModel;
        }

        public List<TModel> SelectAll(System.Linq.Expressions.Expression<Func<TModel, bool>> predicate)
        {
            throw new NotImplementedException();
        }

        public TModel Find(int id)
        {
            TModel model = default(TModel);

            try
            {
                this.OpenConnection();
                string primaryKeyColumnName = TablePrimaryKey();
                query = string.Format("Select * from {0} where {1} = {2}", _tableName, primaryKeyColumnName, id);
                model = db.Query<TModel>(query).FirstOrDefault();
            }
            catch (Exception ex)
            {

            }
            finally
            {
                this.CloseConnection();
            }

            return model;
        }

        private string TablePrimaryKey()
        {
            return db.Query<string>("select column_name from information_schema.KEY_COLUMN_USAGE where table_name =@TableName", new { TableName = this._tableName }).FirstOrDefault();
        }

        public int Insert(TModel entity)
        {
            int id = 0;

            try
            {
                this.OpenConnection();
                DynamicParameters dynamicParam = new DynamicParameters();
                string primaryKey = TablePrimaryKey();
                foreach (var item in entity.GetType().GetProperties())
                {
                    if (item.Name != primaryKey)
                        dynamicParam.Add("@" + item.Name, item.GetValue(entity));
                }

                query = string.Format("Insert into {0}({1}) Values({2}); Select CAST(SCOPE_IDENTITY() as int)", _tableName, entity.ToClauseColumnNamesWithoutPK(primaryKey), entity.ToClauseValuesWithoutPK(primaryKey));
                id = db.Query<int>(query, dynamicParam).FirstOrDefault();
            }
            catch (Exception ex)
            {

            }
            finally
            {
                this.CloseConnection();
            }

            return id;
        }

        public bool Update(TModel entity)
        {
            bool successful = false;

            try
            {
                this.OpenConnection();

                DynamicParameters dynamicParam = new DynamicParameters();
                string primaryKey = TablePrimaryKey();
                int primaryKeyValue = 0;

                foreach (var item in entity.GetType().GetProperties())
                {
                    if (item.Name == primaryKey)
                        primaryKeyValue = Convert.ToInt32(item.GetValue(entity));

                    dynamicParam.Add("@" + item.Name, item.GetValue(entity));
                }

                query = string.Format("Update {0} set {1} where {2} = {3}", _tableName, entity.ToUpdateQuery(primaryKey), primaryKey, primaryKeyValue);
                db.Query(query, dynamicParam);
                successful = true;
            }
            catch (Exception ex)
            {
                successful = false;
            }
            finally
            {
                this.CloseConnection();
            }

            return successful;
        }

        public bool Update(int id, TModel entity)
        {
            bool successful = false;

            try
            {

                this.OpenConnection();

                DynamicParameters dynamicParam = new DynamicParameters();
                string primaryKey = TablePrimaryKey();

                dynamicParam.Add("@Id", id);

                foreach (var item in entity.GetType().GetProperties())
                {
                    dynamicParam.Add("@" + item.Name, item.GetValue(entity));
                }

                query = string.Format("Update {0} set {1} where Id = @Id", _tableName, entity.ToUpdateQuery(primaryKey));
                db.Query(query, dynamicParam);
                successful = true;
            }
            catch (Exception ex)
            {
                successful = false;
            }
            finally
            {
                this.CloseConnection();
            }

            return successful;
        }

        public bool Remove(int id)
        {
            bool successful = false;

            try
            {
                this.OpenConnection();
                string primaryKey = TablePrimaryKey();
                query = string.Format("Delete from {0} where {1} = {2}", _tableName, primaryKey, id);
                db.Query(query);
                successful = true;
            }
            catch (Exception ex)
            {
                successful = false;
            }
            finally
            {
                this.CloseConnection();
            }

            return successful;
        }

        protected virtual void RollBack()
        {
            this._transaction.Rollback();
            _transaction = null;
        }

        protected virtual void Commit()
        {
            this._transaction.Commit();
            _transaction = null;
        }

        public void Dispose()
        {
            db.Dispose();
        }

        private void OpenConnection()
        {
            if (db.State != ConnectionState.Open)
                db.Open();
        }

        private void CloseConnection()
        {
            if (db.State == ConnectionState.Open)
                db.Close();
        }


    }
}
