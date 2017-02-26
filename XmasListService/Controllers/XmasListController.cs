using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using XmasListService.Models;

namespace XmasListService.Controllers
{
    [Authorize]
    public class XmasListController : ApiController
    {
        //
        // Xmas items list for all users. Since the list is stored in memory, it will go away if the service is cycled.
        //
        static readonly ConcurrentBag<GiftItem> GiftBag = new ConcurrentBag<GiftItem>();

        /// <summary>
        /// Retrieve a list of gifts for an authenticated giftee.
        /// </summary>
        /// <returns>An enumerable list of gifts.</returns>
        public IEnumerable<GiftItem> Get()
        {
            //
            // The Scope claim tells you what permissions the client application has in the service.
            // In this case we look for a scope value of user_impersonation, or full access to the service as the user.
            //
            if (!ClaimsPrincipal.Current.FindFirst("http://schemas.microsoft.com/identity/claims/scope").Value.Contains("user_impersonation"))
            {
                throw new HttpResponseException(new HttpResponseMessage { StatusCode = HttpStatusCode.Unauthorized, ReasonPhrase = "The Scope claim does not contain 'user_impersonation' or scope claim not found" });
            }

            // A user's Xmas list is keyed off of the NameIdentifier claim, which contains an immutable, unique identifier for the user.
            Claim subject = ClaimsPrincipal.Current.FindFirst(ClaimTypes.NameIdentifier);

            return from gift in GiftBag
                   where gift.Owner == subject.Value
                   select gift;
        }

        /// <summary>
        /// Add a gift to the list for an authenticated giftee.
        /// </summary>
        /// <param name="gift">A gift.</param>
        public void Post(GiftItem gift)
        {
            if (!ClaimsPrincipal.Current.FindFirst("http://schemas.microsoft.com/identity/claims/scope").Value.Contains("user_impersonation"))
            {
                throw new HttpResponseException(new HttpResponseMessage { StatusCode = HttpStatusCode.Unauthorized, ReasonPhrase = "The Scope claim does not contain 'user_impersonation' or scope claim not found" });
            }

            if (!string.IsNullOrWhiteSpace(gift?.Title))
            {
                GiftBag.Add(new GiftItem { Title = gift.Title, Owner = ClaimsPrincipal.Current.FindFirst(ClaimTypes.NameIdentifier).Value });
            }
        }
    }
}
