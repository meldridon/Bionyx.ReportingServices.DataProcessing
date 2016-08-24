using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Threading;
using System.Threading.Tasks;
using Bionyx.WebApi.ReportingServices.Common;
using Microsoft.ReportingServices.DataProcessing;
using Newtonsoft.Json;

namespace Bionyx.ReportingServices.DataProcessing.WebApi
{
    /// <summary>
    /// A command object for querying web api datasets.
    /// </summary>
    /// <remarks>
    /// Set the CommandText property in the data set property panel to the relative uri of the web api dataset.
    /// </remarks>
    public class WebApiCommand : IDbCommand, IDbCommandAnalysis
    {
        public WebApiCommand(WebApiConnection webApiConnection)
        {
            _webApiConnection = webApiConnection;
        }

        #region IDbCommand

        public string CommandText { get; set; }

        public int CommandTimeout { get; set; }

        public CommandType CommandType { get; set; }

        public IDataParameterCollection Parameters => _parameters;

        public IDbTransaction Transaction
        {
            get { return null; }
            set { }
        }

        public void Cancel()
        {
            _cancellationTokenSource?.Cancel();
        }

        public IDataParameter CreateParameter()
        {
            return new WebApiDataParameter();
        }

        public IDataReader ExecuteReader(CommandBehavior behavior)
        {
            var stream = SendRequest(behavior);
            return new WebApiDataReader(stream);
        }

        #endregion

        #region IDbCommandAnalysis

        public IDataParameterCollection GetParameters()
        {
            // Use the query string ?behavior=schemaOnly to return only the parameters and columns
            // of the web api data set.
            var query = new Dictionary<string, string>
            {
                {"behavior", "schemaOnly"}
            };

            var uri = BuildUri(query);
            var client = _webApiConnection.Client;
            var response = ExecuteAndWait(() => client.PostAsJsonAsync<object>(uri, null));
            var responseContent = ExecuteAndWait(() => response.Content.ReadAsStringAsync());
            var reportResponse = JsonConvert.DeserializeObject<ReportResponse>(responseContent);

            var parameters = new WebApiDataParameterCollection();
            parameters.AddRange(reportResponse.Parameters.Select(s => new WebApiDataParameter { ParameterName = s }));
            return parameters;
        }

        #endregion

        #region IDisposable

        public void Dispose()
        {
        }

        #endregion

        #region Non-public Members

        /// <summary>
        /// Executes an async delegate and waits for it to complete.
        /// </summary>
        /// <remarks>
        /// This method handles the signal raised from the Cancel() method as well as the CommandTimeout.
        /// </remarks>
        /// <typeparam name="T">The expected async result type.</typeparam>
        /// <param name="asyncFunc">The async delegate to execute.</param>
        /// <returns>The resulting return value of the async delegate.</returns>
        protected T ExecuteAndWait<T>(Func<Task<T>> asyncFunc)
        {
            if (_cancellationTokenSource != null)
            {
                throw new InvalidOperationException("Command is already running.");
            }
            _cancellationTokenSource = new CancellationTokenSource();
            try
            {
                var task = asyncFunc();
                if (CommandTimeout != 0)
                {
                    if (!task.Wait(TimeSpan.FromSeconds(CommandTimeout).Milliseconds, _cancellationTokenSource.Token))
                    {
                        throw new TimeoutException();
                    }
                }
                else
                {
                    task.Wait(_cancellationTokenSource.Token);
                }
                return task.Result;
            }
            finally
            {
                _cancellationTokenSource.Dispose();
                _cancellationTokenSource = null;
            }
        }

        private CancellationTokenSource _cancellationTokenSource;
        private readonly WebApiDataParameterCollection _parameters = new WebApiDataParameterCollection();
        private readonly WebApiConnection _webApiConnection;

        /// <summary>
        /// Handles the concatenation of the base connection uri with the relative command uri,
        /// and build and appends the query string.
        /// </summary>
        /// <param name="query">A dictionary of name/value pairs used to build the query string of the uri.</param>
        /// <returns>An absolute Uri object.</returns>
        private Uri BuildUri(Dictionary<string, string> query)
        {
            var uriBuilder = new UriBuilder(_webApiConnection.ConnectionString);
            uriBuilder.Path += CommandText;
            uriBuilder.Query = string.Join("&", query.Select(pair => Uri.EscapeUriString(pair.Key) + "=" + Uri.EscapeUriString(pair.Value)));
            return uriBuilder.Uri;
        }

        /// <summary>
        /// Sends the request to the web api to get the report data set.
        /// </summary>
        /// <remarks>
        /// The request is sent as an HTTP post. The arguments for the data set
        /// are supplied as JSON request content. The response content is streamed
        /// so that the report can being rendering as soon as possible.
        /// </remarks>
        /// <param name="behavior"></param>
        /// <returns>The response content as a stream.</returns>
        private Stream SendRequest(CommandBehavior behavior)
        {
            var query = new Dictionary<string, string>();
            switch (behavior)
            {
                case CommandBehavior.SchemaOnly:
                    query["behavior"] = "schemaOnly";
                    break;
                case CommandBehavior.SingleResult:
                    // This just means that the command behavior only returns a single data set.
                    // SSRS commands cannot return multiple results.
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(behavior), behavior, null);
            }
            var uri = BuildUri(query);
            var requestContent = new ObjectContent<Dictionary<string, object>>(_parameters.ToDictionary(p => p.ParameterName, p => p.Value), new JsonMediaTypeFormatter());
            var request = new HttpRequestMessage(HttpMethod.Post, uri)
            {
                Content = requestContent
            };
            var client = _webApiConnection.Client;
            // The post request must be sent using SendAsync so that the ResponseHeadersRead option can be specified.
            // This allows the response content to be streamed.
            var response = ExecuteAndWait(() => client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead));
            return ExecuteAndWait(() => response.Content.ReadAsStreamAsync());
        }

        #endregion
    }
}