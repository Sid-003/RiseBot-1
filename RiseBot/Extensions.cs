using Microsoft.Extensions.DependencyInjection;
using Qmmands;
using RiseBot.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace RiseBot
{
    public static class Extensions
    {
        //locates and adds all services
        public static IServiceCollection AddServices(this IServiceCollection collection)
        {
            var services = FindTypesWithAttribute<ServiceAttribute>(Assembly.GetEntryAssembly());

            foreach (var service in services)
                collection.AddSingleton(service);

            return collection;
        }

        private static IEnumerable<Type> FindTypesWithAttribute<T>(this Assembly assembly)
        {
            return assembly.GetTypes().Where(x => x.GetCustomAttributes(typeof(T), true).Length > 0);
        }

        public static CommandService AddTypeParsers(this CommandService commands)
        {
            var typeParserInterface = commands.GetType().Assembly.GetTypes()
                .FirstOrDefault(x => x.Name == "ITypeParser").GetTypeInfo();

            if (typeParserInterface is null)
                throw new Exception("Quahu renamed the interface reeeeeeeeee");

            var parsers = Assembly.GetEntryAssembly().GetTypes()
                .Where(x => typeParserInterface.IsAssignableFrom(x));

            var internalAddParser = commands.GetType().GetMethod("AddParserInternal", 
                BindingFlags.NonPublic | BindingFlags.Instance);

            if (internalAddParser is null)
                throw new Exception("Quahu renamed the method reeeeeeeeee");

            foreach (var parser in parsers)
            {
                var targetType = parser.BaseType.GetGenericArguments().First();

                internalAddParser.Invoke(commands, new[] { targetType,
                    Activator.CreateInstance(parser), true });
            }

            return commands;
        }
    }
}
