using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace DynamoDbLambdas.Tests
{
    public class IgnoreEmptyValuesResolver : DefaultContractResolver
    {
        private readonly string[] _types =
        {
            "System.Collections.Generic.IEnumerable",
            "System.Collections.Generic.List",
            "System.Collections.ICollection",
            "System.Collections.IList"
        };

        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            var property = base.CreateProperty(member, memberSerialization);

            property.ShouldSerialize = instance =>
            {
                var instanceProperty = instance?.GetType().GetProperty(property.PropertyName);
                if (_types.Any(x => instanceProperty.PropertyType.FullName.StartsWith(x))
                    || instanceProperty.PropertyType.FullName.Contains("[]"))
                {
                    var value = instanceProperty.GetValue(instance) as IEnumerable<object>;
                    return value.Any();
                }
                else
                {
                    var value = instanceProperty.GetValue(instance);
                    return value != null;
                }
            };

            return property;
        }
    }
}