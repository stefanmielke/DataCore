using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using DataCore.Attributes;

namespace DataCore
{
    public class TableDefinition
    {
        public string Name { get; set; }
        public FieldDefinition IdField { get; set; }
        public List<FieldDefinition> Fields { get; set; }

        public TableDefinition(Type type)
        {
            Name = GetTableName(type);
            IdField = GetIdFieldForType(type);
            Fields = GetPropertiesForType(type).Select(GetFieldForProperty).ToList();
        }

        private IEnumerable<PropertyInfo> GetPropertiesForType(Type type)
        {
            return type.GetProperties().Where(p => CanUseType(p.PropertyType) && p.GetCustomAttributes(typeof(IgnoreAttribute), true).Length == 0);
        }

        private bool CanUseType(Type propertyType)
        {
            return !propertyType.IsClass || propertyType == typeof(string);
        }

        private PropertyInfo GetIdPropertyForType(Type type)
        {
            return type.GetProperties()
                .FirstOrDefault(p =>
                {
                    var columnAttributes = p.GetCustomAttributes(typeof(ColumnAttribute), true);

                    return columnAttributes.Length > 0 && ((ColumnAttribute)columnAttributes[0]).IsPrimaryKey;
                }
                );
        }

        private FieldDefinition GetIdFieldForType(Type type)
        {
            return GetFieldForProperty(GetIdPropertyForType(type));
        }

        private string GetTableName(Type type)
        {
            var tableAttributes = type.GetCustomAttributes(typeof(TableAttribute), true);
            if (tableAttributes.Length > 0)
            {
                var tableAttribute = (TableAttribute)tableAttributes[0];
                return tableAttribute.TableName;
            }

            return type.Name;
        }

        private static FieldDefinition GetFieldForProperty(PropertyInfo p)
        {
            if (p == null)
                return null;

            var columnName = p.Name;
            var isPrimaryKey = false;
            var length = 255;
            var nullable = p.PropertyType.IsGenericType && p.PropertyType.Name.StartsWith("Nullable");
            var precision = 3;

            var columnAttributes = p.GetCustomAttributes(typeof(ColumnAttribute), true);
            if (columnAttributes.Length > 0)
            {
                var columnAttribute = (ColumnAttribute)columnAttributes[0];
                columnName = columnAttribute.ColumnName ?? columnName;
                isPrimaryKey = columnAttribute.IsPrimaryKey;
                length = columnAttribute.Length;
                nullable = !columnAttribute.IsRequired && !isPrimaryKey;
                precision = columnAttribute.Precision;
            }

            var field = new FieldDefinition
            {
                Name = columnName,
                Nullable = nullable,
                Size = length,
                Precision = precision,
                Type = GetTypeForProperty(p),
                IsPrimaryKey = isPrimaryKey,
                PropertyInfo = p
            };

            var identityAttributes = p.GetCustomAttributes(typeof(IdentityAttribute), true);
            if (identityAttributes.Length > 0)
            {
                var identityAttribute = (IdentityAttribute)identityAttributes[0];
                field.IsIdentity = true;
                field.IdentityStart = identityAttribute.Start;
                field.IdentityIncrement = identityAttribute.Increment;
            }

            var referenceAttributes = p.GetCustomAttributes(typeof(ReferenceAttribute), true);
            if (referenceAttributes.Length > 0)
            {
                var referenceAttribute = (ReferenceAttribute)referenceAttributes[0];
                field.IsReference = true;
                field.ReferenceName = referenceAttribute.FkName;
                field.ReferenceTable = referenceAttribute.Table;
            }

            var indexAttributes = p.GetCustomAttributes(typeof(IndexAttribute), true);
            if (indexAttributes.Length > 0)
            {
                var indexAttribute = (IndexAttribute)indexAttributes[0];
                field.HasIndex = true;
                field.IndexName = indexAttribute.Name;
                field.IndexUnique = indexAttribute.Unique;
            }

            return field;
        }

        private static DbType GetTypeForProperty(PropertyInfo propertyInfo)
        {
            return GetTypeForProperty(propertyInfo.PropertyType);
        }

        public static DbType GetTypeForProperty(Type type)
        {
            if (type.Name.StartsWith("Nullable"))
                type = type.GetGenericArguments()[0];

            switch (type.Name)
            {
                case "String":
                    return DbType.String;
                case "Int32":
                    return DbType.Int32;
                case "Boolean":
                    return DbType.Boolean;
                case "Float":
                    return DbType.Single;
                case "Decimal":
                    return DbType.Decimal;
                case "DateTime":
                    return DbType.DateTime;
                default:
                    return DbType.Int32;
            }
        }
    }
}
