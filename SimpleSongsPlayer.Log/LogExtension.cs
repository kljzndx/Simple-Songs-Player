using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using NLog;
using SimpleSongsPlayer.Log.Models;

namespace SimpleSongsPlayer.Log
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

        public static void LogWhetherByObject<T>(this T type, string info, bool condition, string trueMessage, string falseMessage = null, Action trueAction = null, Action falseAction = null)
        {
            typeof(T).LogWhetherByType(info, condition, trueMessage, falseMessage, trueAction, falseAction);
        }

        public static async Task LogWhetherByObjectAsync<T>(this T type, string info, bool condition, string trueMessage, string falseMessage = null, Func<Task> trueAction = null, Func<Task> falseAction = null)
        {
            await typeof(T).LogWhetherByTypeAsync(info, condition, trueMessage, falseMessage, trueAction, falseAction);
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

        public static void LogWhetherByType(this Type type, string info, bool condition, string trueMessage, string falseMessage = null, Action trueAction = null, Action falseAction = null)
        {
            if (condition)
            {
                type.LogByType(String.Format("{0}：{1}", info, trueMessage));
                trueAction?.Invoke();
            }
            else if (!String.IsNullOrWhiteSpace(falseMessage))
            {
                type.LogByType(String.Format("{0}：{1}", info, falseMessage));
                falseAction?.Invoke();
            }
        }

        public static async Task LogWhetherByTypeAsync(this Type type, string info, bool condition, string trueMessage, string falseMessage = null, Func<Task> trueAction = null, Func<Task> falseAction = null)
        {
            if (condition)
            {
                type.LogByType(String.Format("{0}：{1}", info, trueMessage));
                if (trueAction != null)
                    await trueAction.Invoke();
            }
            else if (!String.IsNullOrWhiteSpace(falseMessage))
            {
                type.LogByType(String.Format("{0}：{1}", info, falseMessage));
                if (falseAction != null)
                    await falseAction.Invoke();
            }
        }

        private static string GetAssemblyNameFromType(Type type)
        {
            var split = type.AssemblyQualifiedName.Split(',');
            return split[split.Length - 4].Trim();
        }
    }
}