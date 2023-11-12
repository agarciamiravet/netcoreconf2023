using System.Diagnostics.Metrics;


namespace NetCoreConfMad2023.Observability.Metrics
{
    public class NetCoreConf2023Metrics
    {
        //Catalog meters
        private Counter<int> CatalogsViewsCounter { get; }

        //ShoppingCart meters
        private Counter<int> ShoppingCartProductAddedCounter { get; }
        private Counter<int> ShoppingCartViewCounter { get; }
      

        public string MetricName { get; }

        public NetCoreConf2023Metrics(string meterName = "NetCoreConf2023")
        {
            var meter = new Meter(meterName);
            MetricName = meterName;

            CatalogsViewsCounter = meter.CreateCounter<int>("netcoreconf2023-catalog-view", "Catalog");
            ShoppingCartProductAddedCounter = meter.CreateCounter<int>("netcoreconf2023-shoppingcart-product-added", "ShoppingCart");
            ShoppingCartViewCounter = meter.CreateCounter<int>("netcoreconf2023-shoppingcart-view", "ShoppingCart");           
        }

        //Catalog meters
        public void AddViewCatalog() => CatalogsViewsCounter.Add(1);
        public void AddShoppingCartProductAdded() => ShoppingCartProductAddedCounter.Add(1);
        public void AddShoppingCartView() => ShoppingCartViewCounter.Add(1);
    }
}
