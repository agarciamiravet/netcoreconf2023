namespace NetCoreConfMad2023_API.Models
{
    public class ShoppingCart
    {
        public string UserName { get; set; }
        public List<ShoppingCartItem> Items { get; set; } = new List<ShoppingCartItem>();
        public ShoppingCart()
        {
        }
        public ShoppingCart(string userName)
        {
            UserName = userName;
        }

        public double TotalPrice
        {
            get
            {
                double totalprice = 0;
                foreach (var item in Items)
                {
                    totalprice += item.Price * item.Quantity;
                }
                return totalprice;
            }
        }
    }
}
