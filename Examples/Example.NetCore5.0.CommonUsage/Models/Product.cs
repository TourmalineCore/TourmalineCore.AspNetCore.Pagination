using System;

namespace Example.NetCore5._0.CommonUsage.Models
{
    public class Product
    {
        public long Id { get; set; }

        public string Name { get; set; }

        public DateTime ExpirationDate { get; set; }

        public int Cost { get; set; }

        public long VendorId { get; set; }
        public Vendor Vendor { get; set; }
    }
}
