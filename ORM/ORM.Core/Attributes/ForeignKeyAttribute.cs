using System;

namespace ORM.Core.Attributes
{
    public class ForeignKeyAttribute : Attribute
    {
        public string RemoteColumnName { get; set; }

        public ForeignKeyAttribute(string remoteColumnName)
        {
            RemoteColumnName = remoteColumnName;
        }
    }
}