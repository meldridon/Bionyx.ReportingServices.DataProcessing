using System.Runtime.Serialization;

namespace Bionyx.WebApi.ReportingServices.Common
{
    /// <summary>
    /// An object used to provide the schema of a web api report dataset to be consumed by the web api
    /// data processing extensions.
    /// </summary>
    [DataContract]
    public class ReportResponse
    {
        [DataMember(Name = "@parameters", Order = 0)]
        public string[] Parameters { get; set; }

        [DataMember(Name = "@columns", Order = 1)]
        public WebApiColumnSchema[] Columns { get; set; }
    }

    /// <summary>
    /// An object used to provide the schema and results of a web api report dataset to be consumed by
    /// the web api data processing extension.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [DataContract]
    public class ReportResponse<T> : ReportResponse
    {
        [DataMember(Order = 2)]
        public T Value { get; set; }
    }
}
