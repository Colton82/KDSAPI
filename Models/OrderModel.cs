﻿namespace KDSAPI.Models
{
    public class OrderModel
    {
        public int Id { get; set; }
        public string CustomerName { get; set; }
        public DateTime Timestamp { get; set; }
        public int Users_id { get; set; }
    }
}
