using System;
using Newtonsoft.Json;

namespace SquareRecommender.Models
{
    public class ProductRecommendations
    {

        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        public string ProductId { get; set; }

        public string Recommendations { get; set; }
    }
}
