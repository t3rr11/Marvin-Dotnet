namespace Marvin.DbAccess.Attributes;

[AttributeUsage(AttributeTargets.Property)]
public class DapperColumnAttribute : Attribute
{
    public DapperColumnAttribute(string columnName)
    {
        ColumnName = columnName;
    }

    public string ColumnName { get; }
}