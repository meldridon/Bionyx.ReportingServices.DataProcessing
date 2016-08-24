using System;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Xml;
using Bionyx.WebApi.ReportingServices.Common;
using Newtonsoft.Json;
using IDataReader = Microsoft.ReportingServices.DataProcessing.IDataReader;

namespace Bionyx.ReportingServices.DataProcessing.WebApi
{
    /// <summary>
    /// A data reader for web api data sets.
    /// </summary>
    /// <remarks>
    /// An instance of this object is returned from the WebApiCommand.ExecuteReader() method.
    /// </remarks>
    public class WebApiDataReader : IDataReader
    {
        public WebApiDataReader(Stream stream)
        {
            // Use a single instance of a JSON serializer for this reader.
            Json = JsonSerializer.CreateDefault();

            // The JsonTextReader requires a StreamReader.
            var streamReader = new StreamReader(stream);
            try
            {
                // In order to be responsive in making rows available to reports,
                // the response JSON is read using the JsonTextReader (forward only)
                // instead of trying to read in the entire JSON file.
                _reader = new JsonTextReader(streamReader)
                {
                    CloseInput = true
                };
            }
            catch (Exception)
            {
                streamReader.Dispose();
                throw;
            }
            try
            {
                // The response must be a JSON object (not a primitive or array).
                _reader.Read();
                if (_reader.TokenType != JsonToken.StartObject)
                {
                    throw new InvalidDataException("Response JSON must be an object.");
                }

                if (!SkipToSection("@columns"))
                {
                    throw new InvalidDataException("Expected a \"@columns\" property in the response object.");
                }
                _columns = Json.Deserialize<WebApiColumnSchema[]>(_reader);

                if (!SkipToSection("value")) return;
                BeginValueSection();

            }
            catch (Exception)
            {
                _reader.Close();
                throw;
            }
        }

        #region IDataReader

        public int FieldCount
        {
            get
            {
                if (_reader == null)
                {
                    throw new ObjectDisposedException(nameof(WebApiDataReader));
                }
                return _columns.Length;
            }
        }

        public Type GetFieldType(int fieldIndex)
        {
            if (_reader == null)
            {
                throw new ObjectDisposedException(nameof(WebApiDataReader));
            }

            // The return types are using the .NET framework types instead of the 
            // C# types just to be explicit in the intent.
            switch (_columns[fieldIndex].DbType)
            {
                case DbType.AnsiString:
                    return typeof(String);
                case DbType.Binary:
                    return typeof(Byte[]);
                case DbType.Byte:
                    return typeof(Byte);
                case DbType.Boolean:
                    return typeof(Boolean);
                case DbType.Currency:
                    return typeof(Decimal);
                case DbType.Date:
                    return typeof(DateTime);
                case DbType.DateTime:
                    return typeof(DateTime);
                case DbType.Decimal:
                    return typeof(Decimal);
                case DbType.Double:
                    return typeof(Double);
                case DbType.Guid:
                    return typeof(Guid);
                case DbType.Int16:
                    return typeof(Int16);
                case DbType.Int32:
                    return typeof(Int32);
                case DbType.Int64:
                    return typeof(Int64);
                case DbType.Object:
                    return typeof(Object);
                case DbType.SByte:
                    return typeof(SByte);
                case DbType.Single:
                    return typeof(Single);
                case DbType.String:
                    return typeof(String);
                case DbType.Time:
                    return typeof(DateTime);
                case DbType.UInt16:
                    return typeof(UInt16);
                case DbType.UInt32:
                    return typeof(UInt32);
                case DbType.UInt64:
                    return typeof(UInt64);
                case DbType.VarNumeric:
                    return typeof(Decimal);
                case DbType.AnsiStringFixedLength:
                    return typeof(String);
                case DbType.StringFixedLength:
                    return typeof(String);
                case DbType.Xml:
                    return typeof(XmlNode);
                case DbType.DateTime2:
                    return typeof(DateTime);
                case DbType.DateTimeOffset:
                    return typeof(DateTimeOffset);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public string GetName(int fieldIndex)
        {
            if (_reader == null)
            {
                throw new ObjectDisposedException(nameof(WebApiDataReader));
            }
            return _columns[fieldIndex].Name;
        }

        public int GetOrdinal(string fieldName)
        {
            if (_reader == null)
            {
                throw new ObjectDisposedException(nameof(WebApiDataReader));
            }
            return Array.FindIndex(_columns, column => string.Equals(column.Name, fieldName, StringComparison.Ordinal));
        }

        public object GetValue(int fieldIndex)
        {
            if (_reader == null)
            {
                throw new ObjectDisposedException(nameof(WebApiDataReader));
            }
            if (_values == null)
            {
                throw new InvalidOperationException("A row has not been read from the stream.");
            }
            return _values[fieldIndex];
        }

        public bool Read()
        {
            if (_reader == null)
            {
                throw new ObjectDisposedException(nameof(WebApiDataReader));
            }
            // See the BeginValueSection() method for more information on _resultType.
            switch (_resultType)
            {
                case WebApiResultType.NoResult:
                    return ReadNoResult();
                case WebApiResultType.Scalar:
                    return ReadSingleScalar();
                case WebApiResultType.SingleRow:
                    return ReadSingleRow();
                case WebApiResultType.MultipleRows:
                    return ReadNextRow();
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        #endregion

        #region IDisposable

        public void Dispose()
        {
            _reader?.Close();
            _reader = null;
        }

        #endregion

        #region Non-public Members

        private JsonSerializer Json { get; }
        private readonly WebApiColumnSchema[] _columns;
        private JsonTextReader _reader;

        /// <summary>
        /// The type of result available to be read from the response.
        /// </summary>
        private WebApiResultType _resultType;

        /// <summary>
        /// The column values of the current row in the data set.
        /// </summary>
        private object[] _values;

        /// <summary>
        /// Resets the state of the IDataReader for the next row to be read.
        /// </summary>
        private void BeginRow()
        {
            _values = new object[FieldCount];
        }

        /// <summary>
        /// Sets up the reader for reading rows from the response data set.
        /// </summary>
        private void BeginValueSection()
        {
            switch (_reader.TokenType)
            {
                case JsonToken.StartObject:
                    // The response only contains a single row (JSON object instead of JSON array).
                    _resultType = WebApiResultType.SingleRow;
                    break;
                case JsonToken.StartArray:
                    // The response contains multiple rows (JSON array).
                    _resultType = WebApiResultType.MultipleRows;
                    break;
                default:
                    // The response contains a single value (JSON primitive).
                    _resultType = WebApiResultType.Scalar;
                    break;
            }
            // When the BeginValueSection does not get called, _resultType defaults to NoResult.
        }

        /// <summary>
        /// Deserialize the next token in the JSON as the value for the specified field index.
        /// </summary>
        /// <param name="fieldIndex"></param>
        private void DeserializeField(int fieldIndex)
        {
            var fieldType = GetFieldType(fieldIndex);
            _values[fieldIndex] = Json.Deserialize(_reader, fieldType);
        }

        /// <summary>
        /// Completes the load of data into the current row.
        /// </summary>
        private void EndRow()
        {
            // This method doesn't do anything right now, but it's here as a placeholder to complete
            // the pattern.
        }

        /// <summary>
        /// Reads a row of columns that are supplied in the response content as an array of values
        /// instead of a structured object.
        /// </summary>
        private void ReadArrayRow()
        {
            BeginRow();
            for (var fieldIndex = 0; _reader.Read(); ++fieldIndex)
            {
                if (_reader.TokenType == JsonToken.EndArray) break;
                if (fieldIndex >= FieldCount)
                {
                    // extra elements in the array are just ignored
                    _reader.Skip();
                    continue;
                }
                DeserializeField(fieldIndex);
            }
            EndRow();
        }

        /// <summary>
        /// Reads the next row from the response JSON (if there are any).
        /// </summary>
        /// <returns>true if the response had another row to read; false otherwise.</returns>
        private bool ReadNextRow()
        {
            _reader.Read();
            if (_reader.TokenType == JsonToken.EndArray)
            {
                return ReadNoResult();
            }
            switch (_reader.TokenType)
            {
                case JsonToken.StartArray:
                    ReadArrayRow();
                    break;
                case JsonToken.StartObject:
                    ReadObjectRow();
                    break;
                default:
                    ReadScalarRow();
                    break;
            }
            return true;
        }

        /// <summary>
        /// Resets the state of the IDataReader to indicate there are no more rows, and clears the current row from
        /// the IDataReader state.
        /// </summary>
        /// <returns>false always</returns>
        private bool ReadNoResult()
        {
            _values = null;
            return false;
        }

        /// <summary>
        /// Reads the next token of the JSON as a field value from a structured object.
        /// </summary>
        private void ReadObjectField()
        {
            if (_reader.TokenType != JsonToken.PropertyName)
            {
                throw new InvalidDataException($"Expected a property in the response JSON but found a {_reader.TokenType} instead.");
            }

            var fieldName = (string)_reader.Value;
            var fieldIndex = GetOrdinal(fieldName);
            if (fieldIndex < 0)
            {
                _reader.Skip();
                return;
            };

            _reader.Read();
            DeserializeField(fieldIndex);
        }

        /// <summary>
        /// Reads a row of columns that are supplied in the response content as a structured object.
        /// </summary>
        private void ReadObjectRow()
        {
            BeginRow();
            while (_reader.Read())
            {
                if (_reader.TokenType == JsonToken.EndObject) break;
                ReadObjectField();
            }
            EndRow();
        }

        /// <summary>
        /// Reads a row with a single value (JSON primitive).
        /// </summary>
        private void ReadScalarRow()
        {
            BeginRow();
            var fieldType = GetFieldType(0);
            _values[0] = Json.Deserialize(_reader, fieldType);
            EndRow();
        }

        /// <summary>
        /// Reads a single row from the response JSON.
        /// </summary>
        /// <remarks>
        /// This method is used when the response only has a single row result. After the row is read,
        /// the state of the IDataReader is set to indicate there are no more rows
        /// </remarks>
        /// <returns>true always</returns>
        private bool ReadSingleRow()
        {
            ReadObjectRow();
            _resultType = WebApiResultType.NoResult;
            return true;
        }

        /// <summary>
        /// Reads a single scalar result from the response JSON.
        /// </summary>
        /// <remarks>
        /// This method is used when the response only has a single scalar result. After the result is read,
        /// the state of the IDataReader is set to indicate there are no more rows
        /// </remarks>
        /// <returns>true always</returns>
        private bool ReadSingleScalar()
        {
            ReadScalarRow();
            _resultType = WebApiResultType.NoResult;
            return true;
        }

        /// <summary>
        /// Skips extraneous properties in the response object to find the desired property. 
        /// </summary>
        /// <param name="sectionName">The name of the property to find.</param>
        /// <returns>true if the property could be found; false otherwise.</returns>
        private bool SkipToSection(string sectionName)
        {
            while (_reader.Read())
            {
                if (_reader.TokenType == JsonToken.EndObject) break;
                Debug.Assert(_reader.TokenType == JsonToken.PropertyName, $"Expected a property in the response JSON but found a {_reader.TokenType} instead.");
                var propertyName = (string)_reader.Value;
                if (propertyName == sectionName)
                {
                    _reader.Read();
                    return true;
                }
                // If the current property isn't the one being searched for, it's children must be skipped completely.
                // In other words, the method does not descend into property values.
                _reader.Skip();
            }
            return false;
        }

        #endregion
    }
}