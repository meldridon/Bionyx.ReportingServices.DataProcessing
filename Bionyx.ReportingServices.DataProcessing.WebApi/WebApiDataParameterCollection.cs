using System.Collections.Generic;
using Microsoft.ReportingServices.DataProcessing;

namespace Bionyx.ReportingServices.DataProcessing.WebApi
{
    /// <summary>
    /// A trivial implementation of IDataParameterCollection because a default implementation is not provided by the SDK.
    /// </summary>
    public class WebApiDataParameterCollection : List<IDataParameter>, IDataParameterCollection
    {
        int IDataParameterCollection.Add(IDataParameter parameter)
        {
            Add(parameter);
            return Count - 1;
        }
    }
}