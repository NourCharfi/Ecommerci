namespace GestionArticles.Models.Repositories
{
    public interface IProductRepository
    {
        Product GetById(int Id);
        IList<Product> GetAll();
        // ✅ NOUVEAU: Récupérer produits en corbeille
        IList<Product> GetDeleted();
        // ✅ NOUVEAU: Récupérer by ID même si supprimé
        Product GetByIdIncludeDeleted(int id);
        void Add(Product t);
        Product Update(Product t);
        void Delete(int Id, string deletedBy = null);    // ✅ MODIFIÉ: Soft delete
        // ✅ NOUVEAU: Restaurer
        void Restore(int ProductId);
        // ✅ NOUVEAU: Supprimer définitivement
        void PermanentDelete(int ProductId);
        public IList<Product> GetProductsByCategID(int? CategId);
        public IList<Product> FindByName(string name);
        public IQueryable<Product> GetAllProducts();
    }
}
