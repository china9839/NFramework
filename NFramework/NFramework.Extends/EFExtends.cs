using NFramework.Common;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NFramework.Extends
{
    /***************************************************
    * Author  :jony
    * Date    :2017-12
    * Describe:定义一个ef扩展类
    * 
    * 
    ***************************************************/
    public static class EFExtends
    {

        /// <summary>
        /// 更新实体的相关属性
        /// </summary>
        /// <typeparam name="E"></typeparam>
        /// <param name="dbContext"></param>
        /// <param name="entity"></param>
        /// <param name="cols"></param>
        public static void UpdateEntity<E>(this DbContext dbContext, E entity, params string[] cols)
            where E : class
        {
            dbContext.Configuration.ValidateOnSaveEnabled = false;
            DbEntityEntry<E> entry = dbContext.Entry<E>(entity);
            if (entry.State == EntityState.Detached)
            {
                dbContext.Set<E>().Attach(entity);
            }
            else
            {
                entry.State = System.Data.Entity.EntityState.Unchanged;
            }
            foreach (var str in cols)
            {
                dbContext.Entry<E>(entity).Property(str).IsModified = true;
            }
        }

        /// <summary>
        /// 删除一个实体(标记一个实体的状态为删除。实体必须是为跟踪的)
        /// </summary>
        /// <typeparam name="E"></typeparam>
        /// <param name="dbContext"></param>
        /// <param name="entity"></param>
        public static void DeleteEntity<E>(this DbContext dbContext, E entity)
            where E : class
        {
            dbContext.Set<E>().Attach(entity);
            dbContext.Entry<E>(entity).State = EntityState.Deleted;
        }

        /// <summary>
        /// 处理EF保存时引发的数据库唯一索引异常
        /// </summary>
        /// <param name="dbContext"></param>
        /// <returns></returns>
        public static int SaveChangesDbUpdateException(this DbContext dbContext)
        {
            try
            {
                return dbContext.SaveChanges();
            }
            catch (System.Data.Entity.Infrastructure.DbUpdateException innerException)
            {
                LogHelper.Error("SaveChangesDbUpdateException", innerException);
                var exception = (System.Data.Entity.Infrastructure.DbUpdateException)innerException;
                if (exception.InnerException is System.Data.Entity.Core.UpdateException)
                {
                    LogHelper.Error("SaveChangesDbUpdateException", exception.InnerException);
                    var exception1 = (System.Data.Entity.Core.UpdateException)exception.InnerException;
                    if (exception1.InnerException is System.Data.SqlClient.SqlException)
                    {
                        LogHelper.Error("SaveChangesDbUpdateException", exception1.InnerException);
                        var exception2 = (System.Data.SqlClient.SqlException)exception1.InnerException;
                        if (exception2.Number == 2601)
                        {
                            return -1001;
                        }
                    }
                }
                return -1000;
            }
        }
    }
}
