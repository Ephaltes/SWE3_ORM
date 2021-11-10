using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;
using ORM.PostgresSQL;

namespace ORM.Core
{
    public class DbContext
    {
        private readonly PostgresSql db;

        public void Add<T>(T item) where T : class
        {
            string tableName = GetTableName<T>();
            Dictionary<string, object> values = GetValues(item);
            db.Insert(tableName, values);
        }
        
        public void Update<T>(T item) where T : class
        {
            string tableName = GetTableName<T>();
            Dictionary<string, object> values = GetValues(item);
            db.Update(tableName, values, $"{GetKeyName<T>()} == {GetId(item)}");
        }
        
        public void Delete<T>(T item) where T : class
        {
            string tableName = GetTableName<T>();
            db.Delete(tableName, GetId(item));
        }
        
        public List<T> GetAll<T>() where T : class
        {
            string tableName = GetTableName<T>();
            return db.Select(tableName,"")
                .Select(x => (T)Activator.CreateInstance(typeof(T), x)).ToList();
        }
        
        public T GetById<T>(int id) where T : class
        {
            string tableName = GetTableName<T>();
            return db.Select(tableName, $"{GetKeyName<T>()} == {id}").FirstOrDefault()
                .Select(x => (T)Activator.CreateInstance(typeof(T), x)).FirstOrDefault();
            
        }
        
        protected string GetId<T>(T item) where T : class
        {
            PropertyInfo property = typeof(T).GetType().GetProperties().FirstOrDefault(p => p.GetCustomAttribute<KeyAttribute>() != null);

            return property.GetValue(item).ToString();
        }
        
        protected string GetKeyName<T>() where T : class
        {
            PropertyInfo property = typeof(T).GetProperties().FirstOrDefault(p => p.GetCustomAttribute<KeyAttribute>() != null);
            return property.Name;
        }

        protected Dictionary<string, object> GetValues<T>(T item)
        {
            return typeof(T).GetProperties()
                .Where(property => property.GetCustomAttribute<NotMappedAttribute>() == null)
                .ToDictionary(property => property.Name, property => property.GetValue(item));
        }

        protected string GetTableName<T>() where T : class
        {
            Type type = typeof(T);
            string tableName = type.Name;

            IEnumerable<Attribute> customAttributes = type.GetCustomAttributes();

            foreach (Attribute customAttribute in customAttributes)
                if (customAttribute is TableAttribute attribute)
                    tableName = attribute.Name;

            return tableName;
        }
    }
}