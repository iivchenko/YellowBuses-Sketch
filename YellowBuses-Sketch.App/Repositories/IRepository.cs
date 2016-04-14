// <copyright company="XATA">
//      Copyright (c) 2016, All Right Reserved
// </copyright>
// <author>Ivan Ivchenko</author>
// <email>iivchenko@live.com</email>

using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace YellowBuses_Sketch.App.Repositories
{
    public interface IRepository<TEntity> : IEnumerable<TEntity>
        where TEntity : Entity
    {
        uint Count { get; }

        void Add(TEntity entity);

        void Remove(TEntity entity);

        void Update(TEntity entity);

        IEnumerable<TEntity> Find(Expression<Func<TEntity, bool>> predicate);
    }
}
