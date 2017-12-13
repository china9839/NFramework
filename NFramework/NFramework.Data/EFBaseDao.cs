using NFramework.Common;
using NFramework.Extends;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace NFramework.Data
{
    public abstract class EFBaseDao<T> 
        where T : class, IBaseEntity
    {
        public EFBaseDao(DbContext db)
        {
            this.db = db;
        }

        /// <summary>
        /// 上下文网关
        /// </summary>
        protected DbContext db = null;

        #region 1.Add


        /// <summary>
        /// 增加一条数据
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public T AddEntity(T entity)
        {

            db.Entry<T>(entity).State = EntityState.Added;
            // db.Set<T>().Add(entity);此方法同上方法
            db.SaveChanges();
            return entity;//因为可能要返回自动增长的ID，所以把整个实体返回，否则可以直接返回bool。
        }
        /// <summary>
        /// 同时增加多条数据到一张表（事务处理）
        /// </summary>
        /// <param name="entitys"></param>
        /// <returns></returns>
        public bool AddEntity(List<T> entitys)
        {
            foreach (var entity in entitys)
            {
                db.Entry<T>(entity).State = EntityState.Added;
            }
            // entitys.ForEach(c=>db.Entry<T>(c).State = EntityState.Added);//等价于上面的循环
            return db.SaveChanges() > 0;
        }

        /// <summary>
        /// 批量插入实体
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entityList"></param>
        /// <returns></returns>
        public int BatchInsert(IEnumerable<T> entityList)
        {
            try
            {
                db.Set<T>().AddRange(entityList);
                return db.SaveChanges();
            }
            catch (Exception ex)
            {
                LogHelper.Error("BatchInsert", ex);
                return -1;
            }
        }
        #endregion

        #region 2.Modify
        /// <summary>
        /// 修改一条数据，会修改所有列的值，没有赋值的属性将会被赋予属性类型的默认值**************
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public bool ModifyEntity(T entity)
        {
            db.Set<T>().Attach(entity);
            db.Entry<T>(entity).State = EntityState.Modified;//将所有属性标记为修改状态
            return db.SaveChanges() > 0;
        }
        /// <summary>
        /// 修改一条数据,会修改指定列的值
        /// </summary>
        /// <param name="entity">要修改的实体对象</param>
        /// <param name="proNames">要修改的属性名称</param>
        /// <returns></returns>
        public bool ModifyEntity(T entity, params string[] proNames)
        {
            db.Set<T>().Attach(entity);
            DbEntityEntry<T> dbee = db.Entry<T>(entity);
            dbee.State = EntityState.Unchanged;//先将所有属性状态标记为未修改
            proNames.ToList().ForEach(c => dbee.Property(c).IsModified = true);//将要修改的属性状态标记为修改
            return db.SaveChanges() > 0;
        }
        /// <summary>
        /// 根据条件批量修改指定的列********************
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="whereLambds"></param>
        /// <param name="proNames"></param>
        /// <returns></returns>
        public bool ModifyEntity(T entity, Expression<Func<T, bool>> whereLambds, params string[] proNames)
        {
            var entitys = db.Set<T>().Where(whereLambds).ToList();
            PropertyInfo[] proinfos = entity.GetType().GetProperties();
            List<PropertyInfo> list = new List<PropertyInfo>();
            foreach (var p in proinfos)
            {
                if (proNames.Contains(p.Name))
                {
                    list.Add(p);
                }
            }
            entitys.ForEach(c =>
            {
                foreach (var p in list)
                {
                    object value = p.GetValue(entity, null);
                    p.SetValue(c, value, null);
                }
            });
            return db.SaveChanges() > 0;
        }
        #endregion

        #region 3.Delete

        /// <summary>
        /// 删除一个实体对象
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public bool DeleteEntity(T entity)
        {
            db.Set<T>().Attach(entity);
            db.Entry<T>(entity).State = EntityState.Deleted;
            return db.SaveChanges() > 0;
        }
        /// <summary>
        /// 根据条件批量删除实体对象
        /// </summary>
        /// <param name="whereLambds"></param>
        /// <returns></returns>
        public bool DeleteEntityByWhere(Func<T, bool> whereLambds)
        {
            var data = db.Set<T>().Where<T>(whereLambds).ToList();
            return DeleteEntitys(data);
        }
        /// <summary>
        /// 事务批量删除实体对象
        /// </summary>
        /// <param name="entitys"></param>
        /// <returns></returns>
        public bool DeleteEntitys(List<T> entitys)
        {
            foreach (var item in entitys)
            {
                db.Set<T>().Attach(item);
                db.Entry<T>(item).State = EntityState.Deleted;
            }
            return db.SaveChanges() > 0;
        }

        #endregion

        #region 4.Select
        //带条件查询
        public IList<T> GetEntitys(Expression<Func<T, bool>> whereLambds)
        {
            return db.Set<T>().Where<T>(whereLambds).AsNoTracking().ToList<T>();
        }
        //带排序查询
        public IList<T> GetEntitys<S>(Expression<Func<T, bool>> whereLambds, bool isAsc, Expression<Func<T, S>> orderByLambds)
        {
            var temp = db.Set<T>().AsQueryable();
            if (whereLambds != null)
            {
                temp = db.Set<T>().Where<T>(whereLambds);
            }
            if (isAsc)
            {
                return temp.OrderBy<T, S>(orderByLambds).AsNoTracking().ToList<T>();
            }
            else
            {
                return temp.OrderByDescending<T, S>(orderByLambds).AsNoTracking().ToList<T>();
            }
        }
        //带分页查询
        public IList<T> GetPagedEntitys<S>(int pageIndex, int pageSize, out int rows, out int totalPage, Expression<Func<T, bool>> whereLambds, bool isAsc, Expression<Func<T, S>> orderByLambds)
        {
            var temp = db.Set<T>().Where<T>(whereLambds);
            rows = temp.Count();
            if (rows % pageSize == 0)
            {
                totalPage = rows / pageSize;
            }
            else
            {
                totalPage = rows / pageSize + 1;
            }
            if (isAsc)
            {
                temp = temp.OrderBy<T, S>(orderByLambds);
            }
            else
            {
                temp = temp.OrderByDescending<T, S>(orderByLambds);
            }
            temp = temp.Skip<T>(pageSize * (pageIndex - 1)).Take<T>(pageSize);

            return temp.AsNoTracking().ToList<T>();
        }
        //传统sql结合EF分页实现查询
        public IList<T> GetPagedEntitys(int pageIndex, int pageSize, out int rows, out int totalPage, string sql, string where, bool isAsc, string orderKey)
        {

            sql = sql + " where 1=1 " + where;
            sql += " order by " + orderKey;
            if (!isAsc)
            {
                sql += " desc";
            }
            var temp = db.Database.SqlQuery<T>(sql);
            rows = temp.Count();
            if (rows % pageSize == 0)
            {
                totalPage = rows / pageSize;
            }
            else
            {
                totalPage = rows / pageSize + 1;
            }

            var query = temp.Skip(pageSize * (pageIndex - 1)).Take(pageSize).AsQueryable();
            return query.AsNoTracking().ToList<T>(); ;

        }
        //获得单一实体
        public T GetSingleEntity(Expression<Func<T, bool>> whereLambds)
        {
            return db.Set<T>().AsNoTracking().SingleOrDefault<T>(whereLambds);
        }
        #endregion

        #region 5.显式Tran
        /// <summary>
        /// 显式执行事务
        /// </summary>
        /// <param name="action"></param>
        /// <param name="isolationLevel"></param>
        /// <returns></returns>
        public int DBTransactionScope(Func<EFBaseDao<T>, int> action,
            System.Data.IsolationLevel isolationLevel = System.Data.IsolationLevel.ReadCommitted)
        {
            if (action == null)
            {
                return 1;
            }
            int result = 0;
            using (var transaction = this.db.Database.BeginTransaction(isolationLevel))
            {
                try
                {
                    result = action.Invoke(this);
                    if (result < 1)
                    {
                        transaction.Rollback();
                        return result;
                    }
                    transaction.Commit();
                    return result;
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    LogHelper.Error("DBTransactionScope", ex);
                    return -1;

                }
            }
        }

        #endregion

        public int GetDataCount(string sqlText, params DbParameter[] dbParameters)
        {
            int r = 0;
            this.db.Database.Connection.Open();
            var command = this.db.Database.Connection.CreateCommand();
            command.CommandText = sqlText;
            if (dbParameters != null && dbParameters.Length > 0)
            {
                command.Parameters.AddRange(dbParameters);
            }
            r = (int)command.ExecuteScalar();
            command.Parameters.Clear();
            return r;
        }

        #region 获得动态list
        public virtual List<dynamic> QueryDynamicData(string sqlText, params DbParameter[] dbParameter)
        {
            try
            {
                var connection = this.db.Database.Connection;
                if (connection.State != System.Data.ConnectionState.Open)
                {
                    connection.Open();
                }
                var command = connection.CreateCommand();
                command.CommandText = sqlText;
                command.Parameters.AddRange(dbParameter);
                using (var reader = command.ExecuteReader())
                {
                    return ToExpandoList(reader);
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error("QueryData", ex);
                return null;
            }
        }

        protected virtual List<dynamic> ToExpandoList(IDataReader rdr)
        {
            var result = new List<dynamic>();
            while (rdr.Read())
            {
                result.Add(RecordToExpando(rdr));
            }
            return result;
        }

        protected virtual dynamic RecordToExpando(IDataReader rdr)
        {
            dynamic e = new ExpandoObject();
            var d = e as IDictionary<string, object>;
            for (int i = 0; i < rdr.FieldCount; i++)
                d.Add(rdr.GetName(i), rdr[i]);
            return e;
        }
        #endregion

        public void Dispose()
        {
            this.db.Dispose();
        }

        /// <summary>
        /// 获得EF实体操作类上下文
        /// </summary>
        public DbContext GetDbContext
        {
            get { return this.db; }
        }

        /// <summary>
        /// 获得实体集合
        /// </summary>
        /// <typeparam name="Entity"></typeparam>
        /// <returns></returns>
        public DbSet<Entity> Set<Entity>() where Entity : class, IBaseEntity
        {
            return this.db.Set<Entity>();
        }

        /// <summary>
        /// 变更EF跟踪的实体状态，默认变更为修改
        /// </summary>
        /// <typeparam name="Entity"></typeparam>
        /// <param name="entity"></param>
        /// <param name="entityState"></param>
        public void UpdateEntityState<Entity>(Entity entity, EntityState entityState = EntityState.Modified) where Entity : class, IBaseEntity
        {
            this.db.Entry<Entity>(entity).State = EntityState.Modified;
        }

        /// <summary>
        /// 调用EF的SaveChanges方法，保存上下文的修改。返回int类型受影响的行数
        /// </summary>
        /// <returns></returns>
        public int SaveChangesInt()
        {
            return this.db.SaveChanges();
        }

        /// <summary>
        /// 调用EF的SaveChanges方法，保存上下文的修改。返回sbyte类型受影响的行数
        /// </summary>
        /// <returns></returns>
        public sbyte SaveChangesSbyte()
        {
            return (sbyte)this.SaveChangesInt();
        }

        /// <summary>
        /// 上下文更新实体状态，按照给定的列更新
        /// </summary>
        /// <typeparam name="E"></typeparam>
        /// <param name="entity"></param>
        /// <param name="cols"></param>
        public void UpdateEntity<E>(E entity, params string[] cols)
            where E : class, IBaseEntity
        {
            this.db.UpdateEntity<E>(entity, cols);
        }

        /// <summary>
        /// 扩展方法。可以删除和上下文分离状态的实体
        /// </summary>
        /// <typeparam name="E"></typeparam>
        /// <param name="dbContext"></param>
        /// <param name="entity"></param>
        public void DeleteEntity<E>(E entity) where E : class, IBaseEntity
        {
            this.db.DeleteEntity<E>(entity);
        }

        /// <summary>
        /// 扩展方法。保存时确定唯一索引异常
        /// </summary>
        /// <returns></returns>
        public int SaveChangesDbUpdateException()
        {
            return this.db.SaveChangesDbUpdateException();
        }

        /// <summary>
        /// 执行一个sql语句
        /// </summary>
        /// <param name="sqlText"></param>
        /// <param name="paramArray"></param>
        /// <returns></returns>
        public int ExcuteSql(string sqlText, params object[] paramArray)
        {
            return this.db.Database.ExecuteSqlCommand(sqlText, paramArray);
        }
    }

    public interface IBaseEntity
    {
        /// <summary>
        /// 为了主键统一，而手动设置的
        /// </summary>
        System.Guid ID { get; set; }
    }
}
