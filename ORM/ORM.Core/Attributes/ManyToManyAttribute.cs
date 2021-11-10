using System;

namespace ORM.Core.Attributes
{
    public class ManyToManyAttribute : Attribute
    {
        
        public string RemoteTableName { get; set; }
        public string LocalColumnName { get; set; }
        public string RemoteColumnName { get; set; }
        
        public ManyToManyAttribute(string remoteTableName, string localColumnName, string remoteColumnName)
        {
            RemoteTableName = remoteTableName;
            LocalColumnName = localColumnName;
            RemoteColumnName = remoteColumnName;
        }
    }
}