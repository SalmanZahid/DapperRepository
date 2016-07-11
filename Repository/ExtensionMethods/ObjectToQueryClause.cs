using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Repository
{
    public static class ObjectToQueryClause
    {
        public static string ToUpdateQuery(this object Obj, string primaryKeyName = "Id")
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
                    if (item.Name != primaryKeyName)
                    {
                        string propertyNameWithValue = item.Name + " = @" + item.Name;
                        listOfProperties.Add(propertyNameWithValue);
                    }
                }
            }

            return string.Join<string>(",".ToString(), listOfProperties);
        }

        public static string ToClauseColumnNamesWithoutPK(this object Obj, string primaryKeyName)
        {
            if (Obj == null)
                return string.Empty;

            return string.Join<string>(",", Obj.GetType().GetProperties().Where(x => x.Name != primaryKeyName).Select(x => x.Name).ToList());
        }

        public static string ToClauseValuesWithoutPK(this object Obj, string primaryKeyName)
        {
            if (Obj == null)
                return string.Empty;

            return string.Join<string>(",", Obj.GetType().GetProperties().Where(x => x.Name != primaryKeyName).Select(x => string.Concat("@", x.Name)).ToList());
        }
    }
}
