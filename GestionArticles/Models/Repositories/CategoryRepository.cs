using Microsoft.Extensions.Configuration;
using System;

namespace GestionArticles.Models.Repositories
{
    public class CategoryRepository : ICategorieRepository
    {
        readonly AppDbContext context;
        public CategoryRepository(AppDbContext context)
        {
            this.context = context;
        }
        public IList<Category> GetAll()
        {
            return context.Categories
            .OrderBy(c => c.CategoryName).ToList();
        }
        public Category GetById(int id)
        {
            // Toujours retourner une instance non-nulle
            return context.Categories.Find(id) ?? new Category { CategoryName = string.Empty, Products = new List<Product>() };
        }
        public void Add(Category c)
        {
            if (c == null) throw new ArgumentNullException(nameof(c));
            context.Categories.Add(c);
            context.SaveChanges();
        }
        public Category Update(Category c)
        {
            if (c == null) throw new ArgumentNullException(nameof(c));
            Category c1 = context.Categories.Find(c.CategoryId);
            if (c1 != null)
            {
                c1.CategoryName = c.CategoryName;
                // Persist image filename if provided
                if (!string.IsNullOrEmpty(c.Image))
                {
                    c1.Image = c.Image;
                }
                context.SaveChanges();
            }
            return c1 ?? new Category { CategoryName = string.Empty, Products = new List<Product>() };
        }
        public void Delete(int CategoryId)
        {
            Category c1 = context.Categories.Find(CategoryId);
            if (c1 != null)
            {
                context.Categories.Remove(c1);
                context.SaveChanges();
            }
        }
}
}
