using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace stock_management_system.Models.common
{
    [Serializable]
    public class CustomExtention : Exception
    {
        public CustomExtention() : base() { }
        public CustomExtention(string message) : base(message) { }
        public CustomExtention(string message, Exception inner) : base(message, inner) { }

        // A constructor is needed for serialization when an
        // exception propagates from a remoting server to the client.
        protected CustomExtention(System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
