// Blog: https://pingfu.net/how-to-serialise-ipaddress-ipendpoint
// Source: https://gist.github.com/marcbarry/2e7a64fed2ae539cf415

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace NETworkManager.Utilities
{
    public class JsonIPAddressConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(IPAddress) || objectType == typeof(List<IPAddress>);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            // convert an ipaddress represented as a string into an IPAddress object and return it to the caller
            if (objectType == typeof(IPAddress))
                return IPAddress.Parse(JToken.Load(reader).ToString());

            // convert a json array of ipaddresses represented as strings into a List<IPAddress> object and return it to the caller
            if (objectType == typeof(List<IPAddress>))
                return JToken.Load(reader).Select(address => IPAddress.Parse((string)address)).ToList();

            throw new NotImplementedException();
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value.GetType() == typeof(IPAddress))
            {
                JToken.FromObject(value.ToString()).WriteTo(writer);
                return;
            }

            // convert a List<IPAddress> object to an array of strings of ipaddresses and write it to the serialiser
            if (value.GetType() != typeof(List<IPAddress>))
                throw new NotImplementedException();

            JToken.FromObject((from n in (List<IPAddress>)value select n.ToString()).ToList()).WriteTo(writer);
        }
    }
}
