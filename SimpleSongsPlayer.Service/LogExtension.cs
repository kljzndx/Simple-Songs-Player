using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
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
        
        public static void LogByObject<T>(this T obj, string message, [CallerMemberName] string callerName = "")
        {
            typeof(T).LogByType(message, callerName);
        }

        public static void LogByObject<T>(this T obj, Exception exception, string message = "", [CallerMemberName] string callerName = "")
        {
            typeof(T).LogByType(exception, message, callerName);
        }

        public static void LogByType(this Type type, string message, [CallerMemberName] string callerName = "")
        {
            string assemblyName = GetAssemblyNameFromType(type);
            AllAssembly[assemblyName].Info($"{message} on {callerName} in {type.Name}");
        }

        public static void LogByType(this Type type, Exception exception, string message = "", [CallerMemberName] string callerName = "")
        {
            string assemblyName = GetAssemblyNameFromType(type);
            AllAssembly[assemblyName].Error(exception, $"{message} on {callerName} in {type.Name}");
        }

        private static string GetAssemblyNameFromType(Type type)
        {
            var split = type.AssemblyQualifiedName.Split(',');
            return split[split.Length - 4].Trim();
        }
    }
}