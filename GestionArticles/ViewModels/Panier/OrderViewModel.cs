namespace GestionArticles.ViewModels.Panier
{
    public class OrderViewModel
    {
        public string Address { get; set; }
        public string PaymentMethod { get; set; }
        public float TotalAmount { get; set; }
        public List<CartItemViewModel> CartItems { get; set; } = new List<CartItemViewModel>();
        // New customer info
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Phone { get; set; }
        // Delivery fee (10 DT)
        public float DeliveryFee { get; set; } = 10;
    }
}
