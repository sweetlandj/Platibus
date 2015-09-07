
using System.Security.Principal;
using System.Threading.Tasks;

namespace Platibus.Security
{
    /// <summary>
    /// A service that identifies whether requestors are authorized to perform
    /// certain operations or access certain resources
    /// </summary>
    public interface IAuthorizationService
    {
        /// <summary>
        /// Indicates whether the requestor is authorized to send messages
        /// </summary>
        /// <param name="principal">The principal</param>
        /// <returns>Returns <c>true</c> if the principal is authorized to send messages;
        /// <c>false</c> otherwise</returns>
        Task<bool> IsAuthorizedToSendMessages(IPrincipal principal);

        /// <summary>
        /// Indicates whether the requestor is authorized to subscribe to the specified topic
        /// </summary>
        /// <param name="principal">The principal</param>
        /// <param name="topic">The topic</param>
        /// <returns>Returns <c>true</c> if the principal is authorized to subscribe to the
        /// topic; <c>false</c> otherwise</returns>
        Task<bool> IsAuthorizedToSubscribe(IPrincipal principal, TopicName topic);
    }
}
