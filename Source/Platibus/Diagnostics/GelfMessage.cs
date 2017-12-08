// The MIT License (MIT)
// 
// Copyright (c) 2017 Jesse Sweetland
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System;
using Newtonsoft.Json;

namespace Platibus.Diagnostics
{
    /// <summary>
    /// Graylog Extended Log Format (GELF) message
    /// </summary>
    /// <remarks>
    /// This class is intended to be extended if/when new fields are added.  This class is 
    /// serialized directly using the <see cref="JsonConvert"/> class.  Derived classes must
    /// include <see cref="JsonPropertyAttribute"/>s where field name overrides are expected.
    /// </remarks>
    public class GelfMessage
    {
        private UnixTime _timestamp;

        /// <summary>
        /// The GELF version (1.1)
        /// </summary>
        [JsonProperty("version")]
        public string Version { get; set; }

        /// <summary>
        /// The application hostname
        /// </summary>
        /// <remarks>
        /// Defaults to <see cref="Environment.MachineName"/>
        /// </remarks>
        [JsonProperty("host")]
        public string Host { get; set; }

        /// <summary>
        /// The message timestamp as a .NET <see cref="DateTime"/>
        /// </summary>
        [JsonIgnore]
        public DateTime Timestamp
        {
            get => _timestamp.ToDateTime();
            set => _timestamp = value;
        }

        /// <summary>
        /// The message timestamp as the number of seconds since the UNIX Epoch with optional
        /// decimal places denoting milliseconds
        /// </summary>
        [JsonProperty("timestamp")]
        public double GelfTimestamp
        {
            get => Math.Round(_timestamp.Seconds, 3);
            set => _timestamp = UnixTime.FromSeconds(value);
        }
    
        /// <summary>
        /// The syslog level
        /// </summary>
        [JsonProperty("level")]
        public int Level { get; set; }

        /// <summary>
        /// The short message
        /// </summary>
        [JsonProperty("short_message")]
        public string ShortMessage { get; set; }

        /// <summary>
        /// The full message
        /// </summary>
        [JsonProperty("full_message")]
        public string FullMessage { get; set; }

        /// <summary>
        /// The facility (component) that emitted the log event
        /// </summary>
        [JsonProperty("_facility")]
        public string Facility { get; set; }

        /// <summary>
        /// The type of diagnostic event
        /// </summary>
        [JsonProperty("_event_type")]
        public string EventType { get; set; }

        /// <summary>
        /// The exception and stack trace related to the event
        /// </summary>
        [JsonProperty("_exception")]
        public string Exception { get; set; }

        /// <summary>
        /// The ID of the message
        /// </summary>
        [JsonProperty("_message_id")]
        public string MessageId { get; set; }

        /// <summary>
        /// The message name
        /// </summary>
        [JsonProperty("_message_name")]
        public string MessageName { get; set; }

        /// <summary>
        /// The instance from which the message originated
        /// </summary>
        [JsonProperty("_origination")]
        public string Origination { get; set; }

        /// <summary>
        /// The intended destination of the message
        /// </summary>
        [JsonProperty("_destination")]
        public string Destination { get; set; }

        /// <summary>
        /// The instance to which replies should be addressed
        /// </summary>
        [JsonProperty("_reply_to")]
        public string ReplyTo { get; set; }

        /// <summary>
        /// The ID of the message to which this message is a reply
        /// </summary>
        [JsonProperty("_related_to")]
        public string RelatedTo { get; set; }

        /// <summary>
        /// The queue name to which the event pertains
        /// </summary>
        [JsonProperty("_queue")]
        public string Queue { get; set; }

        /// <summary>
        /// The topic to which the event pertains
        /// </summary>
        [JsonProperty("_topic")]
        public string Topic { get; set; }

        /// <summary>
        /// The HTTP method related to the event
        /// </summary>
        [JsonProperty("_http_method")]
        public string HttpMethod { get; set; }

        /// <summary>
        /// The IP or hostname of the remote client
        /// </summary>
        [JsonProperty("_remote")]
        public string Remote { get; set; }

        /// <summary>
        /// The URI related to the event
        /// </summary>
        [JsonProperty("_uri")]
        public string Uri { get; set; }

        /// <summary>
        /// The HTTP status related to the event
        /// </summary>
        [JsonProperty("_http_status")]
        public int? HttpStatus { get; set; }

        /// <summary>
        /// The UDP multicast node ID related to the event
        /// </summary>
        [JsonProperty("_mcast_node")]
        public string MulticastNode { get; set; }

        /// <summary>
        /// The path of the file or directory to which the event pertains
        /// </summary>
        [JsonProperty("_path")]
        public string Path { get; set; }
        
        /// <summary>
        /// Initializes a <see cref="GelfMessage"/> object
        /// </summary>
        public GelfMessage()
        {
            Version = "1.1";
            Host = Environment.MachineName;
            Level = 7;
        }
    }
}
