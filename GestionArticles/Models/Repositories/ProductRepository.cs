using Microsoft.EntityFrameworkCore;
using System;

namespace GestionArticles.Models.Repositories
{
    public class ProductRepository : IProductRepository
    {
        readonly AppDbContext context;
        public ProductRepository(AppDbContext context)
        {
            this.context = context;
        }
        
        public IList<Product> GetAll()
        {
            // ✅ Ignorer les produits supprimés
            return context.Products
                .Where(p => !p.IsDeleted)
                .OrderBy(x => x.Name)
                .Include(x => x.Category).ToList();
        }

        // ✅ NOUVEAU: Récupérer les produits en corbeille
        public IList<Product> GetDeleted()
        {
            return context.Products
                .Where(p => p.IsDeleted)
                .OrderByDescending(x => x.DeletedAt)
                .Include(x => x.Category).ToList();
        }

        public Product GetById(int id)
        {
            return context.Products
                .Include(x => x.Category)
                .SingleOrDefault(x => x.ProductId == id && !x.IsDeleted);
        }

        // ✅ NOUVEAU: Récupérer by ID même si supprimé (pour admin)
        public Product GetByIdIncludeDeleted(int id)
        {
            return context.Products
                .Include(x => x.Category)
                .SingleOrDefault(x => x.ProductId == id);
        }

        public void Add(Product p)
        {
            context.Products.Add(p);
            context.SaveChanges();
        }

        public IList<Product> FindByName(string name)
        {
            return context.Products
                .Where(p => !p.IsDeleted && (p.Name.Contains(name) ||
                    p.Category.CategoryName.Contains(name)))
                .Include(c => c.Category).ToList();
        }

        public Product Update(Product p)
        {
            Product p1 = context.Products.Find(p.ProductId);
            if (p1 != null && !p1.IsDeleted)
            {
                p1.Name = p.Name;
                p1.Price = p.Price;
                p1.QteStock = p.QteStock;
                p1.CategoryId = p.CategoryId;
                // ✅ Mettre à jour le modificateur
                if (!string.IsNullOrEmpty(p.ModifiedBy))
                {
                    p1.ModifiedBy = p.ModifiedBy;
                    p1.ModifiedAt = p.ModifiedAt ?? DateTime.Now;
                }
                context.SaveChanges();
            }
            return p1;
        }

        // ✅ MODIFIÉ: Soft Delete au lieu de supprimer
        public void Delete(int ProductId, string deletedBy = null)
        {
            Product p1 = context.Products.Find(ProductId);
            if (p1 != null && !p1.IsDeleted)
            {
                p1.IsDeleted = true;
                p1.DeletedBy = deletedBy;
                p1.DeletedAt = DateTime.Now;
                context.SaveChanges();
            }
        }

        // ✅ NOUVEAU: Restaurer depuis corbeille
        public void Restore(int ProductId)
        {
            Product p1 = context.Products.Find(ProductId);
            if (p1 != null && p1.IsDeleted)
            {
                p1.IsDeleted = false;
                p1.DeletedBy = null;
                p1.DeletedAt = null;
                context.SaveChanges();
            }
        }

        // ✅ NOUVEAU: Supprimer définitivement
        public void PermanentDelete(int ProductId)
        {
            Product p1 = context.Products.Find(ProductId);
            if (p1 != null)
            {
                context.Products.Remove(p1);
                context.SaveChanges();
            }
        }

        public IList<Product> GetProductsByCategID(int? CategId)
        {
            return context.Products
                .Where(p => p.CategoryId.Equals(CategId) && !p.IsDeleted)
                .OrderBy(p => p.ProductId)
                .Include(p => p.Category).ToList();
        }

        public IQueryable<Product> GetAllProducts()
        {
            return context.Products
                .Where(p => !p.IsDeleted)
                .Include(p => p.Category);
        }
    }
}
