using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;

namespace Sodes.Base
{
    public static class JsonSerialization
    {
        public static string Serialize<T>(T t)
        {
            using (var stream = new MemoryStream())
            {
                var ds = new DataContractJsonSerializer(typeof(T));
                ds.WriteObject(stream, t);
                var buffer = stream.ToArray();
                return Encoding.UTF8.GetString(buffer, 0, buffer.Length);
            }
        }

        public static T Deserialize<T>(string jsonString)
        {
            DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(T));
            using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(jsonString)))
            {
                T obj = (T)ser.ReadObject(stream);
                return obj;
            }
        }
    }
}
