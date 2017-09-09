using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using BizSparkSupport.DAL;
using System.Data.Entity.Infrastructure;

namespace BizSparkSupport.MVC.Repositories
{
    public class GenericUnitOfWork
    {
        private BizSparkSupportEntities dbContext = new BizSparkSupportEntities();
        public Type TheType { get; set; }

        public GenericRepository<TEntityType> GetRepoInstance<TEntityType>() where TEntityType : class
        {
            return new GenericRepository<TEntityType>(dbContext);
        }

        public void SaveChanges()
        {
            bool saveFailed;
            do
            {
                saveFailed = false;

                try
                {
                    this.dbContext.SaveChanges();
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    saveFailed = true;

                    // Update the values of the entity that failed to save from the store 
                    ex.Entries.Single().Reload();
                }

            } while (saveFailed);
        }

    }
}
