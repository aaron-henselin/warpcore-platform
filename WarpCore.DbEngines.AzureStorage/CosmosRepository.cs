namespace WarpCore.DbEngines.AzureStorage
{
    public abstract class CosmosRepository<T> where T : CosmosEntity, new()
    {
        protected readonly ICosmosOrm _orm;

        protected CosmosRepository():this(Dependency.Resolve<ICosmosOrm>())
        {
        }

        protected CosmosRepository(ICosmosOrm orm)
        {
            _orm = orm;
        }

        public void Save(T item) 
        {
            _orm.Save(item);
        }



        //public Task<IReadOnlyCollection<T>> Find(string condition = null) 
        //{
        //    return _orm.FindContentVersions<T>(condition);
        //}
    }
}