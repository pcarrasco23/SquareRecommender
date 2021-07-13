using System;
using Microsoft.ML.Data;
using Newtonsoft.Json;

namespace SquareRecommender.Models
{
    public class ProductAssociation
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        public float Label { get; set; }

        [LoadColumn(1)]
        [KeyType(count: 262111)]
        public uint ProductId { get; set; }

        [LoadColumn(2)]
        [KeyType(count: 262111)]
        public uint CoPurchaseProductID { get; set; }
    }
}
