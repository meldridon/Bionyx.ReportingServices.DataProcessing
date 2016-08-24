using Microsoft.ReportingServices.DataProcessing;

namespace Bionyx.ReportingServices.DataProcessing.WebApi
{
    /// <summary>
    /// A trivial implementation of IDataParameter because a default implementation is not provided by the SDK.
    /// </summary>
    public class WebApiDataParameter : IDataParameter
    {
        public string ParameterName { get; set; }

        public object Value { get; set; }
    }
}