using System.Collections.Generic;
using Models;
using Newtonsoft.Json;

namespace DynamoDbServerless.Models
{
    public class ActorsSearchResponse
    {
        [JsonProperty("actors")]
        public ICollection<Actor> Actors { get; set; }

        [JsonProperty("count")]
        public int Count { get; set; }
    }
}