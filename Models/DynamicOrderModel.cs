using System;
using System.Collections.Generic;

namespace KDSAPI.Models
{
    public class DynamicOrderModel
    {
        public long Id { get; set; }
        public string CustomerName { get; set; }
        public string Station { get; set; }
        public string Timestamp { get; set; }
        public int Users_id { get; set; }
        public List<OrderItem> Items { get; set; } = new List<OrderItem>();

        public class OrderItem
        {
            public string Name { get; set; }
            public List<ItemProperty> Properties { get; set; } = new List<ItemProperty>();

            public OrderItem() { }

            public OrderItem(string name, List<ItemProperty> properties = null)
            {
                Name = name;
                Properties = properties ?? new List<ItemProperty>();
            }
        }

        public class ItemProperty
        {
            public string Key { get; set; }
            public string Value { get; set; }

            public ItemProperty() { }

            public ItemProperty(string key, string value)
            {
                Key = key;
                Value = value;
            }
        }
    }
}
