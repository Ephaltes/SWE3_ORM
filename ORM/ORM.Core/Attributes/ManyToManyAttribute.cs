using System;

namespace ORM.Core.Attributes
{
    public class ManyToManyAttribute : Attribute
    {
        /// <summary>
        /// Name of the ManyToMany table
        /// </summary>
        public string RemoteTableName { get; set; }
        /// <summary>
        /// Name of the Primary Key in the local Table
        /// </summary>
        public string LocalColumnName { get; set; }
        /// <summary>
        /// Name of the primary key in the remote Table
        /// </summary>
        public string RemoteColumnName { get; set; }
        
        public ManyToManyAttribute(string remoteTableName, string localColumnName, string remoteColumnName)
        {
            RemoteTableName = remoteTableName;
            LocalColumnName = localColumnName;
            RemoteColumnName = remoteColumnName;
        }
    }
}