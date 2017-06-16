using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Platibus.UnitTests.Serialization
{
    [DataContract(Namespace = "urn:platibus:unittests", Name = "data-contract-message")]
    public class DataContractMessage
    {
        [DataMember(Name = "guid-value", EmitDefaultValue = false)]
        public Guid GuidValue { get; set; }

        [DataMember(Name = "date-value", EmitDefaultValue = false)]
        public DateTime DateValue { get; set; }
        
        [DataMember(Name = "int-value", EmitDefaultValue = false)]
        public int IntValue { get; set; }

        [DataMember(Name = "float-value", EmitDefaultValue = false)]
        public float FloatValue { get; set; }

        [DataMember(Name = "double-value", EmitDefaultValue = false)]
        public double DoubleValue { get; set; }

        [DataMember(Name = "decimal-value", EmitDefaultValue = false)]
        public decimal DecimalValue { get; set; }

        [DataMember(Name = "string-value", EmitDefaultValue = false)]
        public string StringValue { get; set; }

        [DataMember(Name = "bool-value", EmitDefaultValue = false)]
        public bool BoolValue { get; set; }

        [DataMember(Name = "nested-messages", EmitDefaultValue = false)]
        public List<NestedDataContractMessage> NestedMessages { get; set; }
    }
}
