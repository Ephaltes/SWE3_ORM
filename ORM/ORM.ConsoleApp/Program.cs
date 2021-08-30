using System;
using Microsoft.Extensions.DependencyInjection;
using ORM.Configuration;

namespace ORM.ConsoleApp
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            ConsoleConfiguration configuration = new();
            ServiceProvider serviceProvider = configuration.Setup();
        }
    }
}