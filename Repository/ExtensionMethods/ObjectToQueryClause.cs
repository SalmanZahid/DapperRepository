using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;

namespace Repository
{
    public static class ObjectToQueryClause
    {
        public static string ToUpdateQuery(this object Obj)
        {
            string queryString = string.Empty;
            List<string> listOfProperties = new List<string>();


            if (Obj == null)
                return string.Empty;

            bool isDictionary = Obj.GetType().IsGenericType;

            if (!isDictionary)
            {
                PropertyInfo[] propertyInfo = Obj.GetType().GetProperties();

                foreach (var item in propertyInfo)
                {
                    if (item.GetType() != typeof(KeyAttribute))
                    {
                        string propertyNameWithValue = item.Name + " = @" + item.Name;
                        listOfProperties.Add(propertyNameWithValue);
                    }
                }
            }

            return string.Join<string>(",".ToString(), listOfProperties);
        }

        public static string ToInsertQueryColumnNames(this object Obj)
        {
            if (Obj == null)
                return string.Empty;

            return string.Join<string>(",", Obj.GetType().GetProperties().Where(x => !x.IsDefined(typeof(KeyAttribute)) && x.PropertyType.IsGenericType == false).Select(x => x.Name).ToList());
        }

        public static string ToInsertQueryColumnValues(this object Obj)
        {
            if (Obj == null)
                return string.Empty;

            return string.Join<string>(",", Obj.GetType().GetProperties().Where(x => !x.IsDefined(typeof(KeyAttribute)) && x.PropertyType.IsGenericType == false).Select(x => string.Concat("@", x.Name)).ToList());
        }
    }
}
