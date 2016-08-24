using System.Data;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Bionyx.WebApi.ReportingServices.Common
{
    /// <summary>
    /// Describes the translation of a web api response property into a dataset column.
    /// </summary>
    [DataContract]
    public class WebApiColumnSchema
    {
        [DataMember]
        [JsonConverter(typeof(StringEnumConverter))]
        public DbType DbType { get; set; }

        [DataMember]
        public string Name { get; set; }
    }
}
