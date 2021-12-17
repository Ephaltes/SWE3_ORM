using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using ORM.Core.Interfaces;
using ORM.Core.Models;

namespace ORM.Cache;

public class TrackingCache : Cache
{
    protected readonly Dictionary<Type, Dictionary<int, string>> _hashes =
        new Dictionary<Type, Dictionary<int, string>>();

    protected virtual Dictionary<int, string> GetHash(Type type)
    {
        bool contains = _hashes.ContainsKey(type);

        if (!contains)
            _hashes.Add(type, new Dictionary<int, string>());

        return _hashes[type];
    }

    /// <summary>
    ///     Compute a hash for the given object with its properties and values.
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="id"></param>
    /// <returns></returns>
    protected virtual string GenerateHash(object obj)
    {
        TableModel tableModel = new TableModel(obj.GetType());
        string rval = "";

        foreach (object value in tableModel.Columns.Select(column => column.GetValue(obj)).Where(value => value != null))
        {
            if (value is IList)
            {
                IList list = value as IList;
                foreach (object listProperty in list)
                {
                    TableModel listModel = new TableModel(list.GetType());
                    rval += listModel.PrimaryKey.GetValue(listProperty);
                }
            }
            else
            {
                rval += value.ToString();
            }
        }

        return Encoding.UTF8.GetString(SHA256.Create().ComputeHash(Encoding.UTF8.GetBytes(rval)));
    }

    public override void Add(object entity, int id)
    {
        base.Add(entity, id);

        Dictionary<int, string> typeDictionary = GetHash(entity.GetType());
        typeDictionary[id] = GenerateHash(entity);
    }
    
    public override void Update(object entity, int id)
    {
        Add(entity, id);
    }

    public override void Remove(Type type, int id)
    {
        base.Remove(type, id);
        GetHash(type).Remove(id);
    }

    public override bool HasChanged(object entity)
    {
        Dictionary<int, string> typeDictionary = GetHash(entity.GetType());
        int id = GetId(entity);

        if (typeDictionary.ContainsKey(id))
            return typeDictionary[id] != GenerateHash(entity);

        return true;
    }
}