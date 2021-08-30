using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace ORM.Core
{
    public class DbSetOrm<TEntity> : IEnumerable<TEntity> where TEntity : class
    {
        IEnumerator<TEntity> IEnumerable<TEntity>.GetEnumerator()
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }

        public virtual EntityTracker<TEntity> Add([NotNull] TEntity entity)
        {
            throw new NotImplementedException();
        }

        public virtual EntityTracker<TEntity> Update([NotNull] TEntity entity)
        {
            throw new NotImplementedException();
        }

        public virtual EntityTracker<TEntity> Remove([NotNull] TEntity entity)
        {
            throw new NotImplementedException();
        }

        public virtual TEntity Find(params object[] keyValues)
        {
            throw new NotImplementedException();
        }
    }
}