using System;
using Newtonsoft.Json;

namespace SquareRecommender.Models
{
    public class Merchant
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        public string MerchantId { get; set; }

        public string AccessToken { get; set; }

        public string RefreshToken { get; set; }

        public bool Enabled { get; set; }

        public string Domain { get; set; }

        public bool RecommendationsAvailable { get; set; }
    }
}
