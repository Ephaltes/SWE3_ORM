using System;

namespace ORM.Core.Attributes
{
    public class PrimaryKeyAttribute : Attribute
    {
        /// <summary>
        /// If the primary key is auto-incrementing.
        /// </summary>
        public bool AutoIncrement { get; }

        public PrimaryKeyAttribute(bool autoIncrement = true)
        {
            AutoIncrement = autoIncrement;
        }

    }
}