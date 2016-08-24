using System;
using System.Net.Http;
using System.Net.Http.Headers;
using Microsoft.ReportingServices.DataProcessing;

namespace Bionyx.ReportingServices.DataProcessing.WebApi
{
    /// <summary>
    /// A connection object for web api data sets.
    /// </summary>
    /// <remarks>
    /// Set the ConnectionString property in the data source property panel to the base uri of the
    /// reports web api (including a trailing slash).
    /// </remarks>
    public class WebApiConnection : IDbConnection
    {
        #region IDbConnection

        /// <summary>
        /// Gets or sets the base uri of the reports web api (including a trailing slash).
        /// </summary>
        public string ConnectionString { get; set; }

        public int ConnectionTimeout { get; set; }

        public string LocalizedName => "Bionyx WebApi Connection";

        public IDbTransaction BeginTransaction()
        {
            throw new NotSupportedException();
        }

        public void Close()
        {
            if (_httpClient != null)
            {
                _httpClient.Dispose();
                _httpClient = null;
            }
        }

        public IDbCommand CreateCommand()
        {
            return new WebApiCommand(this);
        }

        public void Open()
        {
            // The HttpClient isn't really a connection, but Open() provides a convenient way
            // to initialize the HttpClient for shared use in the WebApiCommands.
            if (_httpClient != null)
            {
                throw new InvalidOperationException("Connection is already open.");
            }
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri(ConnectionString),
                DefaultRequestHeaders = { Accept = { new MediaTypeWithQualityHeaderValue("application/json")}}
            };
            // The data processing documentation says that ConnectionTimeout has a default value of 30,
            // but it seems to have a default value of 0.
            if (ConnectionTimeout != 0)
            {
                _httpClient.Timeout = TimeSpan.FromSeconds(ConnectionTimeout);
            }
        }

        public void SetConfiguration(string configuration)
        {
        }

        #endregion

        #region IDisposable

        public void Dispose()
        {
            Close();
        }

        #endregion

        #region Non-public members

        /// <summary>
        /// Provides the HttpClient to the WebApiCommand object.
        /// </summary>
        internal HttpClient Client
        {
            get
            {
                if (_httpClient == null)
                {
                    throw new ObjectDisposedException(nameof(WebApiConnection));
                }
                return _httpClient;
            }
        }

        private HttpClient _httpClient;

        #endregion
    }
}
