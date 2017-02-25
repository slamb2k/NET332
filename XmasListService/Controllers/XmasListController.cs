using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using System.Collections.Concurrent;
using XmasListService.Models;

namespace XmasListService.Controllers
{
    public class XmasListController : ApiController
    {
        //
        // Xmas items list for all users. Since the list is stored in memory, it will go away if the service is cycled.
        //
        static readonly ConcurrentBag<GiftItem> GiftBag = new ConcurrentBag<GiftItem>();

        /// <summary>
        /// Retrieve a list of gifts for a specific giftee.
        /// </summary>
        /// <param name="owner">The giftee.</param>
        /// <returns>An enumerable list of gifts.</returns>
        public IEnumerable<GiftItem> Get(string owner)
        {
            return from gift in GiftBag
                   where gift.Owner == owner
                   select gift;
        }

        /// <summary>
        /// Add a gift to the list for a specific giftee.
        /// </summary>
        /// <param name="gift">A gift.</param>
        /// <param name="owner">The giftee.</param>
        public void Post(GiftItem gift, string owner)
        {
            if (!string.IsNullOrWhiteSpace(gift?.Title))
            {
                GiftBag.Add(new GiftItem { Title = gift.Title, Owner = owner });
            }
        }
    }
}
