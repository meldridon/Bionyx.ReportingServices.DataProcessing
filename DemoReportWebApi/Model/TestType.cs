using System;
using System.Runtime.Serialization;

namespace DemoReportWebApi.Model
{
    [DataContract]
    public class TestType
    {
        [DataMember]
        public int Id { get; set; }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public decimal Value { get; set; }

        [DataMember]
        public DateTime Date { get; set; }
    }
}
