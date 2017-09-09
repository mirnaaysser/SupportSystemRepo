using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace BizSparkSupport.MVC.Repositories
{
    interface IGenericRepository<TEntity> where TEntity : class
    {
        IEnumerable<TEntity> GetAll();
        IEnumerable<TEntity> GetALLByPage(int pageNumber, int pageSize);
        IEnumerable<TEntity> GetBy(Expression<Func<TEntity, bool>> filter = null);

        TEntity Add(TEntity entity);

        TEntity Edit(TEntity entity);
        
        TEntity Delete(TEntity entity);
    }
}
