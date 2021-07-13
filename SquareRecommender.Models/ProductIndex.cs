using System;
using Newtonsoft.Json;

namespace SquareRecommender.Models
{
    public class ProductIndex
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        public string ProductId { get; set; }

        public uint Index { get; set; }

        public string PageUrl { get; set; }

        public string ProductName { get; set; }

        public string ImageUrl { get; set; }
    }
}
