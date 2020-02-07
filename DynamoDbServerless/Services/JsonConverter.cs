using System.IO;
using Amazon.Lambda.Core;
using Newtonsoft.Json;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]
namespace DynamoDbServerless.Services
{
    public class JsonConverter : IJsonConverter
    {
        private readonly JsonSerializer _jsonSerializer;

        public JsonConverter()
        {
            _jsonSerializer = new JsonSerializer();
        }

        public string SerializeObject(object obj)
        {
            using (var writer = new StringWriter())
            {
                _jsonSerializer.Serialize(writer, obj);
                return writer.ToString();
            }
        }

        public T DeserializeObject<T>(string content)
        {
            var stringReader = new StringReader(content);
            var jsonReader = new JsonTextReader(stringReader);
            return _jsonSerializer.Deserialize<T>(jsonReader);
        }
    }
}