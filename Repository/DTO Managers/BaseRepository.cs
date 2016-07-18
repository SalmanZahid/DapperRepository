namespace Repository.DTO_Managers
{
    using Dapper;
    using DapperDataAnnotation;
    using Repository.DTO_Interfaces;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Configuration;
    using System.Data;
    using System.Data.SqlClient;
    using System.Diagnostics;
    using System.Linq;
    using System.Reflection;

    public class BaseRepository<TModel> : IRepository<TModel>, IDisposable where TModel : class
    {

        private string _tableName, query;

        private string _connectionString = ConfigurationManager.ConnectionStrings["dbConnection"].ConnectionString;
        private TModel model = default(TModel);
        protected IDbConnection db;
        protected IDbTransaction _transaction;

        protected string connectionString
        {
            get { return this._connectionString; }
            set { this._connectionString = value; }
        }

        public bool IsTransactionEnable { get; set; }

        public BaseRepository()
        {
            if (db == null)
                db = new SqlConnection(connectionString);

            this._tableName = typeof(TModel).Name;
            this.IsTransactionEnable = true;
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

            try
            {
                this.OpenConnection();

                Type type = typeof(TModel);

                PropertyInfo[] propertyInfo = type.GetProperties();
                string pk = string.Empty;

                if (propertyInfo.Where(x => x.IsDefined(typeof(PK))).Any())
                    pk = propertyInfo.Where(x => x.IsDefined(typeof(PK))).FirstOrDefault().Name;
                else
                    pk = TablePrimaryKey();

                if (propertyInfo.Where(x => x.PropertyType.IsClass).Any())
                    Debug.Write("Codnition True Found");


                query = string.Format("Select * from {0} where {1} = {2}", _tableName, pk, id);
                model = db.Query<TModel>(query).FirstOrDefault();

                foreach (PropertyInfo item in propertyInfo.Where(x => x.PropertyType.IsGenericType && x.IsDefined(typeof(ForeignTable)) && !x.IsDefined(typeof(Ignore))))
                {
                    ForeignTable foreignTableAttribute = item.GetCustomAttribute(typeof(ForeignTable)) as ForeignTable;

                    Type childpropertyType = item.PropertyType.GetGenericArguments()[0];
                    if (string.IsNullOrEmpty(foreignTableAttribute._columnName))
                        continue;



                    PropertyInfo[] propertyInfoOfChild = item.GetType().GetProperties();


                    dynamic foreignKeyValue = model.GetType().GetProperty(foreignTableAttribute._columnName).GetValue(this.model);
                    string foreignKeyTableName = item.PropertyType.GenericTypeArguments.FirstOrDefault().Name;




                    if (propertyInfo.Where(x => x.PropertyType.IsClass).Any())
                        Debug.Write("Codnition True Found");



                    var t = typeof(SqlMapper);
                    var genericQuery = t.GetMethods().Where(x => x.Name == "Query" && x.GetGenericArguments().Length == 1).First(); // You can cache this object.
                    var concreteQuery = genericQuery.MakeGenericMethod(childpropertyType); // you can also keep a dictionary of these, for speed.
                   // var _ret = (IEnumerable)concreteQuery.Invoke(null, new object[] { db });

                    query = string.Format("Select * from {0} where {1} = {2}", foreignKeyTableName, foreignTableAttribute._columnName, foreignKeyValue);
                    model.GetType().GetProperty(foreignKeyTableName).SetValue(model, (IEnumerable) db.Query(query));
                    // possibly from a string



                }
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
            return db.Query<string>("select column_name from information_schema.KEY_COLUMN_USAGE where table_name =@TableName", new { TableName = this._tableName }, _transaction).FirstOrDefault();
        }

        public int Insert(TModel entity)
        {
            int id = 0;
            this.OpenConnection();

            try
            {
                DynamicParameters dynamicParam = new DynamicParameters();
                //  string primaryKey = TablePrimaryKey();
                PropertyInfo[] properties = entity.GetType().GetProperties();

                foreach (var item in entity.GetType().GetProperties())
                {
                    if (!item.IsDefined(typeof(KeyAttribute)) && !item.PropertyType.IsGenericType)
                        dynamicParam.Add("@" + item.Name, item.GetValue(entity));
                }

                query = string.Format("Insert into {0}({1}) Values({2}); Select CAST(SCOPE_IDENTITY() as int)", _tableName, entity.ToInsertQueryColumnNames(), entity.ToInsertQueryColumnValues());
                id = db.Query<int>(query, dynamicParam, _transaction).FirstOrDefault();
                this.Commit();
            }
            catch (Exception ex)
            {
                this.RollBack();
                throw ex;
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
                    dynamicParam.Add("@" + item.Name, item.GetValue(entity));
                }

                primaryKey = entity.GetType().GetProperties().Where(x => x.GetType() == typeof(KeyAttribute)).FirstOrDefault().Name;
                primaryKeyValue = (int)entity.GetType().GetProperties().Where(x => x.GetType() == typeof(KeyAttribute)).FirstOrDefault().GetValue(entity);
                query = string.Format("Update {0} set {1} where {2} = {3}", _tableName, entity.ToUpdateQuery(), primaryKey, primaryKeyValue);
                db.Query(query, dynamicParam);
                this.Commit();
                successful = true;
            }
            catch (Exception ex)
            {
                this.RollBack();
                successful = false;
                throw ex;
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

                query = string.Format("Update {0} set {1} where Id = @Id", _tableName, entity.ToUpdateQuery());
                db.Query(query, dynamicParam, _transaction);
                this.Commit();
                successful = true;

            }
            catch (Exception ex)
            {
                this.RollBack();
                successful = false;
                throw ex;
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
                db.Query(query, _transaction);
                this.Commit();
                successful = true;
            }
            catch (Exception ex)
            {
                this.RollBack();
                successful = false;
                throw ex;
            }
            finally
            {
                this.CloseConnection();
            }

            return successful;
        }

        protected virtual void RollBack()
        {
            if (IsTransactionEnable)
                this._transaction.Rollback();
            _transaction = null;
        }

        protected virtual void Commit()
        {
            if (IsTransactionEnable)
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

            //if (this.IsTransactionEnable)
            //    _transaction = db.BeginTransaction(IsolationLevel.ReadCommitted);
        }

        private void CloseConnection()
        {
            if (db.State == ConnectionState.Open)
                db.Close();
        }





    }
}
