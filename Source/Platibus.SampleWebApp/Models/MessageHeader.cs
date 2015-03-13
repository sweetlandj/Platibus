
namespace Platibus.SampleWebApp.Models
{
    public class MessageHeader
    {
        public string Name { get; set; }
        public string Value { get; set; }

        public MessageHeader()
        {
        }

        public MessageHeader(string name, string value)
        {
            Name = name;
            Value = value;
        }
    }
}