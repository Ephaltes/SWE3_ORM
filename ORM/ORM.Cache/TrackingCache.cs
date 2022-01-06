using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using ORM.Core.Interfaces;
using ORM.Core.Models;
using Serilog;

namespace ORM.Cache;

/// <summary>
/// Tracking cache, used to track changes in the cache
/// </summary>
public class TrackingCache : Cache
{
    protected readonly Dictionary<Type, Dictionary<int, string>> _hashes =
        new Dictionary<Type, Dictionary<int, string>>();

    /// <summary>
    /// Get the hash of a specific type
    /// </summary>
    /// <param name="type">type to get hash</param>
    /// <returns>returns a list of objects ids with their hashes</returns>
    protected virtual Dictionary<int, string> GetHash(Type type)
    {
        bool contains = _hashes.ContainsKey(type);

        if (!contains)
            _hashes.Add(type, new Dictionary<int, string>());

        return _hashes[type];
    }

    /// <summary>
    ///  Compute a hash for the given object with its properties and values.
    /// </summary>
    /// <param name="obj">object to generate hash from</param>
    /// <returns></returns>
    protected virtual string GenerateHash(object obj)
    {
        _logger.Debug($"Generating hash for object");

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

        StringBuilder sb = new StringBuilder();
        byte[] hashBytes = SHA256.Create().ComputeHash(Encoding.UTF8.GetBytes(rval));

        foreach (byte hashByte in hashBytes)
        {
            sb.Append(hashByte.ToString("x2"));
        }

        string hash = sb.ToString();

        _logger.Debug($"Hash is {hash}");
        return hash;
    }

    /// <inheritdoc/>
    public override void Add(object entity, int id)
    {
        base.Add(entity, id);

        Dictionary<int, string> typeDictionary = GetHash(entity.GetType());
        typeDictionary[id] = GenerateHash(entity);
    }
    /// <inheritdoc/>
    public override void Update(object entity, int id)
    {
        Add(entity, id);
    }
    /// <inheritdoc/>
    public override void Remove(Type type, int id)
    {
        base.Remove(type, id);
        GetHash(type).Remove(id);
    }
    /// <inheritdoc/>
    public override bool HasChanged(object entity)
    {
        Dictionary<int, string> typeDictionary = GetHash(entity.GetType());
        int id = GetId(entity);

        if (typeDictionary.ContainsKey(id))
            return typeDictionary[id] != GenerateHash(entity);

        return true;
    }

    public TrackingCache(ILogger logger) : base(logger)
    {
    }
}