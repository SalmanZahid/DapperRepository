namespace DapperDataAnnotation
{
    using System;

    public class ForeignTable : Attribute
    {
        public string _columnName;

        public ForeignTable(string columnName)
        {
            this._columnName = columnName;
        }
    }
}
