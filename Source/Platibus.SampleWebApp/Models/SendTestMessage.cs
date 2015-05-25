using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace Platibus.SampleWebApp.Models
{
    public class SendTestMessage
    {
        public string MessageId { get; set; }
        public string Destination { get; set; }
        public string ContentType { get; set; }

        public IList<SelectListItem> ImportanceOptions
        {
            get
            {
                return new[]
                {
                    new SelectListItem
                    {
                        Value = MessageImportance.Low.ToString(),
                        Text = "Low"
                    },
                    new SelectListItem
                    {
                        Value = MessageImportance.Normal.ToString(),
                        Text = "Normal"
                    },
                    new SelectListItem
                    {
                        Value = MessageImportance.High.ToString(),
                        Text = "High"
                    },
                    new SelectListItem
                    {
                        Value = MessageImportance.Critical.ToString(),
                        Text = "Critical"
                    }
                };
            }
        }

        public int Importance { get; set; }

        public IList<SelectListItem> ContentTypeOptions
        {
            get
            {
                return new[]
                {
                    "application/json",
                    "application/xml"
                }
                    .Select(ct => new SelectListItem {Value = ct, Text = ct})
                    .ToList();
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
            ContentType = "application/json";
            Importance = MessageImportance.Normal;
        }
    }
}