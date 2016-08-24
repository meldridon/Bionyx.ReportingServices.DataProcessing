namespace Bionyx.ReportingServices.DataProcessing.WebApi
{
    /// <summary>
    /// Describes the result in the web api data set.
    /// </summary>
    public enum WebApiResultType
    {
        /// <summary>
        /// The web api data set has no rows, or no more rows.
        /// </summary>
        NoResult,

        /// <summary>
        /// The web api data set has a single scalar value.
        /// </summary>
        Scalar,

        /// <summary>
        /// The web api data set has a single row.
        /// </summary>
        SingleRow,
        
        /// <summary>
        /// The web api data set has an array of rows (which may still only have one element).
        /// </summary>
        MultipleRows
    }
}