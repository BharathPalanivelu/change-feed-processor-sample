using System;
using System.Collections.Generic;
using System.Text;

namespace CosmosBackupSample.Models
{
    public class Product
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string ProductCategory { get; set; }
        public decimal Price { get; set; }
    }
}
