using System;
using System.Collections.Generic;
using System.Reflection;
using NLog;
using SimpleSongsPlayer.Service.Models;

namespace SimpleSongsPlayer.Service
{
    public static class LogExtension
    {
        private static readonly Dictionary<string, Logger> AllAssembly = new Dictionary<string, Logger>();

        static LogExtension()
        {
            SetUpAssembly(typeof(LogExtension).GetTypeInfo().Assembly, LoggerMembers.Service);
        }

        public static void SetUpAssembly(Assembly assembly, LoggerMembers member)
        {
            AllAssembly[assembly.FullName.Split(',')[0].Trim()] = LoggerService.GetLogger(member);
        }
        
        public static void LogByObject<T>(this T obj, string message)
        {
            typeof(T).LogByType(message);
        }

        public static void LogByObject<T>(this T obj, Exception exception, string message = "")
        {
            typeof(T).LogByType(exception, message);
        }

        public static void LogByType(this Type type, string message)
        {
            string assemblyName = GetAssemblyNameFromType(type);
            AllAssembly[assemblyName].Info($"{message} in {type.Name}");
        }

        public static void LogByType(this Type type, Exception exception, string message = "")
        {
            string assemblyName = GetAssemblyNameFromType(type);
            AllAssembly[assemblyName].Error(exception, $"{message} in {type.Name}");
        }

        private static string GetAssemblyNameFromType(Type type)
        {
            return type.AssemblyQualifiedName.Split(',')[1].Trim();
        }
    }
}