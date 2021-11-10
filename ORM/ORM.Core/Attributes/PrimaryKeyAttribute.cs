using System;

namespace ORM.Core.Attributes
{
    public class PrimaryKeyAttribute : Attribute
    {
        public bool AutoIncrement { get; }

        public PrimaryKeyAttribute(bool autoIncrement = true)
        {
            AutoIncrement = autoIncrement;
        }

    }
}