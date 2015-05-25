using System;

namespace Platibus.SampleWebApp.Models
{
    public class ReceivedMessage
    {
        public string SenderPrincipal { get; set; }

        public string MessageId { get; set; }
        public string MessageName { get; set; }
        public string Origination { get; set; }
        public string Destination { get; set; }
        public string RelatedTo { get; set; }
        public DateTime Sent { get; set; }
        public DateTime Received { get; set; }
        public DateTime? Expires { get; set; }
        public string ContentType { get; set; }
        public string Content { get; set; }
    }
}