using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using System.Collections.Concurrent;
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
            if (!string.IsNullOrWhiteSpace(gift?.Title))
            {
                GiftBag.Add(new GiftItem { Title = gift.Title, Owner = ClaimsPrincipal.Current.FindFirst(ClaimTypes.NameIdentifier).Value });
            }
        }
    }
}
