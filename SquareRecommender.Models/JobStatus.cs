using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace SquareRecommender.Models
{
    public class JobStatus
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        public DateTime Timestamp { get; set; }

        public string Message { get; set; }
    }
}
