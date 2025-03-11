using Newtonsoft.Json;

namespace KDSAPI.Models
{
    /// <summary>
    /// Model for an order.
    /// </summary>
    public class OrderModel
    {
        public int Id { get; set; }
        public string CustomerName { get; set; }
        public string Timestamp { get; set; }
        public int Users_id { get; set; }
        public string? Station { get; set; }

        [JsonProperty("Items")]
        public string ItemsJson { get; set; }


        [JsonIgnore]
        public Dictionary<string, Dictionary<string, string>> Items
        {
            get
            {
                return string.IsNullOrEmpty(ItemsJson)
                    ? new Dictionary<string, Dictionary<string, string>>()
                    : JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, string>>>(ItemsJson);
            }
            set
            {
                ItemsJson = JsonConvert.SerializeObject(value);
            }
        }
    }
}
