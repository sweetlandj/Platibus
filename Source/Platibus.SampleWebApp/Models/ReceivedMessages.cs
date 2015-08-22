using System.Collections.Generic;

namespace Platibus.SampleWebApp.Models
{
    public class ReceivedMessages : List<ReceivedMessage>
    {
        public ReceivedMessages(IEnumerable<ReceivedMessage> collection) : base(collection)
        {
        }

        public ReceivedMessages()
        {
        }
    }
}