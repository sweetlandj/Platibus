using Newtonsoft.Json;

namespace Platibus.Http.Models
{
    /// <summary>
    /// A model representing an error
    /// </summary>
    /// <remarks>
    /// Used for serializing and deserializing HTTP content for error responses
    /// </remarks>
    public class ErrorModel
    {
        /// <summary>
        /// The error message
        /// </summary>
        [JsonProperty("errmsg")]
        public string ErrorMessage { get; set; }

        /// <summary>
        /// The name of the invalid parameter, if applicable
        /// </summary>
        [JsonProperty("param")]
        public string Parameter { get; set; }

        /// <summary>
        /// Initializes a new empty <see cref="ErrorModel"/>
        /// </summary>
        public ErrorModel()
        {
        }

        /// <summary>
        /// Initializes a new <see cref="ErrorModel"/> with the specified 
        /// <paramref name="parameter"/> and <paramref name="errorMessage"/>
        /// </summary>
        /// <param name="errorMessage">The error message</param>
        /// <param name="parameter">The parameter</param>
        public ErrorModel(string errorMessage, string parameter = null)
        {
            ErrorMessage = errorMessage;
            Parameter = parameter;
        }
    }
}
