using System.Collections.Generic;

namespace Example.NetCore5._0.CommonUsage.Models
{
    public class Vendor
    {
        public long Id { get; set; }

        public string Name { get; set; }

        public List<Product> Products { get; set; }
    }
}
