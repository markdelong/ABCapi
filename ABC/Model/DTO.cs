using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace ABC.Model
{
    [DataContract]
    public class DTO<E>
    {
        [DataMember]
        public int Status { get; set; }
        [DataMember]
        public string Message { get; set; }
        [DataMember]
        public List<E> Data { get; set; } = new List<E>();

    }
}
