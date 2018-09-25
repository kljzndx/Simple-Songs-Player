using System;
using System.Text;
using Windows.ApplicationModel.Resources;
using HappyStudio.UwpToolsLibrary.Information;

namespace NCEWalkman.Models
{
    public static class ExceptionExtension
    {
        public static readonly ResourceLoader ErrorTable = ResourceLoader.GetForCurrentView("ErrorTable");

        public static string ToShortString(this Exception ex)
        {
            StringBuilder builder = new StringBuilder();

            builder.AppendLine($"{ErrorTable.GetString("Code")} 0x{ex.HResult:X}");
            builder.AppendLine($"{ErrorTable.GetString("Information")} {ex.Message}");

            return builder.ToString().Trim();
        }

        public static string ToLongString(this Exception ex)
        {
            StringBuilder builder = new StringBuilder();

            builder.AppendLine($"{ErrorTable.GetString("Code")} 0x{ex.HResult:X}");
            builder.AppendLine($"{ErrorTable.GetString("Information")} {ex.Message}");
            builder.AppendLine($"{ErrorTable.GetString("Source")} {ex.Source}");
            builder.AppendLine($"{ErrorTable.GetString("HelpLink")}: {ex.HelpLink}");
            builder.AppendLine($"{ErrorTable.GetString("Other")} {ex.Data}");
            builder.AppendLine(ErrorTable.GetString("StackTrace"));
            builder.AppendLine(ex.StackTraceEx());

            return builder.ToString().Trim();
        }
    }
}