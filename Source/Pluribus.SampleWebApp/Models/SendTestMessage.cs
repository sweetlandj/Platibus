
using System.Collections.Generic;
using System.Linq;

namespace Pluribus.SampleWebApp.Models
{
    public class SendTestMessage
    {
        public string MessageId { get; set; }
        public string Destination { get; set; }
        public string ContentType { get; set; }

        public IList<string> ContentTypeOptions
        {
            get
            {
                return new[]
                {
                    "application/json",
                    "application/xml"
                };
            }
        }

        public string MessageText { get; set; }

        public bool UseDurableTransport { get; set; }

        public bool ErrorsOccurred { get; set; }
        public string ErrorMessage { get; set; }

        public bool MessageSent { get; set; }
        public string SentMessageId { get; set; }

        public SendTestMessage()
        {
            ContentType = ContentTypeOptions.FirstOrDefault();
        }
    }
}