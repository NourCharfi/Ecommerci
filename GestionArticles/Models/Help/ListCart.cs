using GestionArticles.Models;
using GestionArticles.Models.Orders;
using System.Linq;

namespace GestionArticles.Models.Help
{
    public class ListeCart
    {
        public List<Item> Items { get; private set; }
        public static readonly ListeCart Instance;
        static ListeCart()
        {
            Instance = new ListeCart();
            Instance.Items = new List<Item>();
        }
        protected ListeCart() { }

        // Retourne true si l'article a été ajouté / incrémenté, false si le stock est insuffisant
        public bool AddItem(Product prod)
        {
            if (prod == null) return false;
            // recherche d'un item existant
            var existing = Items.FirstOrDefault(a => a.Prod.ProductId == prod.ProductId);
            var currentQty = existing != null ? existing.quantite : 0;
            // empêcher d'ajouter plus que le stock disponible
            if (prod.QteStock <= currentQty)
            {
                return false;
            }
            if (existing != null)
            {
                existing.quantite++;
                return true;
            }
            Item newItem = new Item(prod);
            newItem.quantite = 1;
            Items.Add(newItem);
            return true;
        }
        public void setToNUll() { }
        public void SetLessOneItem(Product prod)
        {
            foreach (Item a in Items)
            {
                if (a.Prod.ProductId == prod.ProductId)
                {
                    // si la quantité actuelle est 1 ou moins, supprimer l'item
                    if (a.quantite <= 1)
                    {
                        RemoveItem(a.Prod);
                        return;
                    }
                    else
                    {
                        a.quantite--;
                        return;
                    }
                }
            }
        }
        public void SetItemQuantity(Product prod, int quantity)
        {
            if (quantity == 0)
            {
                RemoveItem(prod);
                return;
            }
            foreach (Item a in Items)
            {
                if (a.Prod.ProductId == prod.ProductId)
                {
                    a.quantite = quantity;
                    return;
                }
            }
        }
        public void RemoveItem(Product prod)
        {
            Item t = null;
            foreach (Item a in Items)
            {
                if (a.Prod.ProductId == prod.ProductId)
                {
                    t = a;
                }
            }
            if (t != null)
            {
                Items.Remove(t);
            }
        }
        public float GetSubTotal()
        {
            float subTotal = 0;
            foreach (Item i in Items)
                subTotal += i.TotalPrice;
            return (float)subTotal;
        }
    }
}


