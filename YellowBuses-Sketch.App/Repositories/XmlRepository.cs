// <copyright company = "XATA" >
//      Copyright (c) 2016, All Right Reserved
// </copyright>
// <author>Ivan Ivchenko</author>
// <email>iivchenko@live.com</email>

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.Serialization;
using Windows.Storage;

namespace YellowBuses_Sketch.App.Repositories
{
    public sealed class XmlRepository<TEntity> : IRepository<TEntity>
        where TEntity : Entity
    {
        private readonly string _path;
        private readonly object _lock;

        private readonly DataContractSerializer _serializer;

        public XmlRepository(string path)
        {
            _path = path;
            _lock = new object();
            _serializer = new DataContractSerializer(typeof(Collection<TEntity>));
        }

        public uint Count
        {
            get
            {
                return (uint)Lock(() => Read().Count);
            }
        }

        public void Add(TEntity entity)
        {
            Lock(() =>
            {
                var entities = Read();

                entity.EntityId =
                    entities.Any()
                        ? entities.Max(x => x.EntityId) + 1
                        : 1;

                entities.Add(entity);

                Write(entities);
            });
        }

        public IEnumerable<TEntity> Find(Expression<Func<TEntity, bool>> predicate)
        {
            return Lock(() => Read().Where(predicate.Compile()));
        }

        public IEnumerator<TEntity> GetEnumerator()
        {
            return Lock(() => Read().GetEnumerator());
        }

        public void Remove(TEntity entity)
        {
            Lock(() =>
            {
                var entities = Read();

                entities.Remove(entities.Single(x => x.EntityId == entity.EntityId));

                Write(entities);
            });
        }

        public void Update(TEntity entity)
        {
            Lock(() =>
            {
                var entities = Read();

                entities.Remove(entities.Single(x => x.EntityId == entity.EntityId));

                entities.Add(entity);

                Write(entities);
            });
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return Lock(GetEnumerator);
        }

        private Collection<TEntity> Read()
        {
            var local = ApplicationData.Current.LocalFolder;

            if (!local.GetFilesAsync().GetAwaiter().GetResult().Any(x => x.Name.Equals(_path)))
            {
                return new Collection<TEntity>();
            }

            var file = local.CreateFileAsync(_path, CreationCollisionOption.OpenIfExists);
            var stream = file.GetAwaiter().GetResult().OpenAsync(FileAccessMode.ReadWrite).GetAwaiter().GetResult().AsStream();
            
            var res = (Collection<TEntity>)_serializer.ReadObject(stream);

            stream.Dispose();
            file.Close();

            return res;
        }

        private void Write(Collection<TEntity> entities)
        {
            var local = ApplicationData.Current.LocalFolder;

            var file = local.CreateFileAsync(_path, CreationCollisionOption.ReplaceExisting);
            var stream = file.GetAwaiter().GetResult().OpenAsync(FileAccessMode.ReadWrite).GetAwaiter().GetResult().AsStream();
            _serializer.WriteObject(stream, entities);
            stream.Dispose();
            file.Close();
        }

        private void Lock(Action action)
        {
            lock (_lock)
            {
                action();
            }
        }

        private T Lock<T>(Func<T> func)
        {
            lock (_lock)
            {
                return func();
            }
        }
    }
}