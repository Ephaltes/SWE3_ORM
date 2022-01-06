using System;

namespace ORM.Core.Attributes
{
    public class ForeignKeyAttribute : Attribute
    {
        /// <summary>
        /// Name of the primary key in the foreign Table
        /// </summary>
        public string RemoteColumnName { get; set; }

        public ForeignKeyAttribute(string remoteColumnName)
        {
            RemoteColumnName = remoteColumnName;
        }
    }
}