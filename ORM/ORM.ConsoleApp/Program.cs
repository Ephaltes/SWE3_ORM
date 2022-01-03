﻿using ORM.Cache;
using ORM.Core;
using ORM.PostgresSQL;
using ORM.PostgresSQL.Interface;

namespace ORM.ConsoleApp
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Examples example = new Examples();

            Console.WriteLine("Hello World!");
            example.DisplayTables();
            example.InsertObject();
            example.UpdateObject();
            example.ShowEntityWithFk();
            example.ShowEntityWithFkList();
            example.ShowEntityWithManyToManyRelation();
            example.ShowCaching();
            example.ShowQuery();
        }
    }
}