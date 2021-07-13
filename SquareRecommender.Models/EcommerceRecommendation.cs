using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace SquareRecommender.Models
{
    public class EcommerceRecommendation
    {
        [JsonProperty(PropertyName = "product_id")]
        public string ProductId { get; set; }

        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "page_url")]
        public string PageUrl { get; set; }

        [JsonProperty(PropertyName = "image_url")]
        public string ImageUrl { get; set; }
    }
}
