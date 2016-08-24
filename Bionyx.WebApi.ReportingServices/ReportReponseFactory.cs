using System;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Reflection;
using System.Runtime.Serialization;
using Newtonsoft.Json.Serialization;
using System.Linq;
using Bionyx.WebApi.ReportingServices.Common;

namespace Bionyx.WebApi.ReportingServices
{
    public class ReportReponseFactory
    {
        public ReportResponse CreateSchemaOnlyReportResponse<TEntity, TParameters>()
        {
            var response = new ReportResponse();
            FillParameters<TParameters>(response);
            FillColumns<TEntity>(response);
            return response;
        }

        public ReportResponse<TEntity[]> CreateMultipleRowReportResponse<TEntity, TParameters>()
        {
            var response = new ReportResponse<TEntity[]>();
            FillParameters<TParameters>(response);
            FillColumns<TEntity>(response);
            return response;
        }

        private readonly NamingStrategy _namingStrategy = new CamelCaseNamingStrategy();

        private static DbType DbTypeForProperty(PropertyInfo property)
        {
            var dataTypeAttribute = property.GetCustomAttribute<DataTypeAttribute>();
            if (dataTypeAttribute != null)
            {
                switch (dataTypeAttribute.DataType)
                {
                    case DataType.Custom:
                        return DbTypeForString(dataTypeAttribute.CustomDataType) ?? DbType.Object;
                    case DataType.DateTime:
                        return DbType.DateTime;
                    case DataType.Date:
                        return DbType.Date;
                    case DataType.Time:
                        return DbType.Time;
                    case DataType.Duration:
                        return DbType.Int64;
                    case DataType.PhoneNumber:
                        return DbType.String;
                    case DataType.Currency:
                        return DbType.Currency;
                    case DataType.Text:
                        return DbType.String;
                    case DataType.Html:
                        return DbType.String;
                    case DataType.MultilineText:
                        return DbType.String;
                    case DataType.EmailAddress:
                        return DbType.String;
                    case DataType.Password:
                        return DbType.String;
                    case DataType.Url:
                        return DbType.String;
                    case DataType.ImageUrl:
                        return DbType.String;
                    case DataType.CreditCard:
                        return DbType.String;
                    case DataType.PostalCode:
                        return DbType.String;
                    case DataType.Upload:
                        return DbType.Binary;
                }
            }
            if (property.PropertyType == typeof(String)) return DbType.String;
            if (property.PropertyType == typeof(Byte)) return DbType.Byte;
            if (property.PropertyType == typeof(Boolean)) return DbType.Boolean;
            if (property.PropertyType == typeof(Decimal)) return DbType.Decimal;
            if (property.PropertyType == typeof(DateTime)) return DbType.DateTime;
            if (property.PropertyType == typeof(Double)) return DbType.Double;
            if (property.PropertyType == typeof(Byte[])) return DbType.Binary;
            if (property.PropertyType == typeof(Guid)) return DbType.Guid;
            if (property.PropertyType == typeof(Int16)) return DbType.Int16;
            if (property.PropertyType == typeof(Int32)) return DbType.Int32;
            if (property.PropertyType == typeof(Int64)) return DbType.Int64;
            if (property.PropertyType == typeof(SByte)) return DbType.SByte;
            if (property.PropertyType == typeof(Single)) return DbType.Single;
            if (property.PropertyType == typeof(UInt16)) return DbType.UInt16;
            if (property.PropertyType == typeof(UInt32)) return DbType.UInt32;
            if (property.PropertyType == typeof(UInt64)) return DbType.UInt64;
            if (property.PropertyType == typeof(DateTimeOffset)) return DbType.DateTimeOffset;
            return DbType.Object;
        }

        private static DbType? DbTypeForString(string customDataType)
        {
            if(string.Equals(customDataType, DbType.AnsiString.ToString(), StringComparison.Ordinal)) return DbType.AnsiString;
            if(string.Equals(customDataType, DbType.Binary.ToString(), StringComparison.Ordinal)) return DbType.Binary;
            if(string.Equals(customDataType, DbType.Byte.ToString(), StringComparison.Ordinal)) return DbType.Byte;
            if(string.Equals(customDataType, DbType.Boolean.ToString(), StringComparison.Ordinal)) return DbType.Boolean;
            if(string.Equals(customDataType, DbType.Currency.ToString(), StringComparison.Ordinal)) return DbType.Currency;
            if(string.Equals(customDataType, DbType.Date.ToString(), StringComparison.Ordinal)) return DbType.Date;
            if(string.Equals(customDataType, DbType.DateTime.ToString(), StringComparison.Ordinal)) return DbType.DateTime;
            if(string.Equals(customDataType, DbType.Decimal.ToString(), StringComparison.Ordinal)) return DbType.Decimal;
            if(string.Equals(customDataType, DbType.Double.ToString(), StringComparison.Ordinal)) return DbType.Double;
            if(string.Equals(customDataType, DbType.Guid.ToString(), StringComparison.Ordinal)) return DbType.Guid;
            if(string.Equals(customDataType, DbType.Int16.ToString(), StringComparison.Ordinal)) return DbType.Int16;
            if(string.Equals(customDataType, DbType.Int32.ToString(), StringComparison.Ordinal)) return DbType.Int32;
            if(string.Equals(customDataType, DbType.Int64.ToString(), StringComparison.Ordinal)) return DbType.Int64;
            if(string.Equals(customDataType, DbType.Object.ToString(), StringComparison.Ordinal)) return DbType.Object;
            if(string.Equals(customDataType, DbType.SByte.ToString(), StringComparison.Ordinal)) return DbType.SByte;
            if(string.Equals(customDataType, DbType.Single.ToString(), StringComparison.Ordinal)) return DbType.Single;
            if(string.Equals(customDataType, DbType.String.ToString(), StringComparison.Ordinal)) return DbType.String;
            if(string.Equals(customDataType, DbType.Time.ToString(), StringComparison.Ordinal)) return DbType.Time;
            if(string.Equals(customDataType, DbType.UInt16.ToString(), StringComparison.Ordinal)) return DbType.UInt16;
            if(string.Equals(customDataType, DbType.UInt32.ToString(), StringComparison.Ordinal)) return DbType.UInt32;
            if(string.Equals(customDataType, DbType.UInt64.ToString(), StringComparison.Ordinal)) return DbType.UInt64;
            if(string.Equals(customDataType, DbType.VarNumeric.ToString(), StringComparison.Ordinal)) return DbType.VarNumeric;
            if(string.Equals(customDataType, DbType.AnsiStringFixedLength.ToString(), StringComparison.Ordinal)) return DbType.AnsiStringFixedLength;
            if(string.Equals(customDataType, DbType.StringFixedLength.ToString(), StringComparison.Ordinal)) return DbType.StringFixedLength;
            if(string.Equals(customDataType, DbType.Xml.ToString(), StringComparison.Ordinal)) return DbType.Xml;
            if(string.Equals(customDataType, DbType.DateTime2.ToString(), StringComparison.Ordinal)) return DbType.DateTime2;
            if(string.Equals(customDataType, DbType.DateTimeOffset.ToString(), StringComparison.Ordinal)) return DbType.DateTimeOffset;
            return null;
        }

        private string NameForProperty(PropertyInfo property)
        {
            var dataMemberAttribute = property.GetCustomAttribute<DataMemberAttribute>();
            return dataMemberAttribute.Name ?? _namingStrategy.GetPropertyName(property.Name, false);
        }

        private void FillColumns<T>(ReportResponse response)
        {
            var type = typeof(T);
            var properties = type.GetTypeInfo().GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.FlattenHierarchy)
                .Where(property => property.GetCustomAttribute<DataMemberAttribute>() != null);
            response.Columns = properties
                .Select(property => 
                    new WebApiColumnSchema()
                    {
                        DbType = DbTypeForProperty(property),
                        Name = NameForProperty(property)
                    })
                .ToArray();
        }

        private void FillParameters<T>(ReportResponse response)
        {
            var type = typeof(T);
            var properties = type.GetTypeInfo().GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.FlattenHierarchy)
                .Where(property => property.GetCustomAttribute<DataMemberAttribute>() != null);
            response.Parameters = properties
                .Select(NameForProperty)
                .ToArray();
        }
    }
}