using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using Windows.UI.Popups;
using HappyStudio.UwpToolsLibrary.Auxiliarys;
using HappyStudio.UwpToolsLibrary.Information;
using NLog;
using SimpleSongsPlayer.Service;
using SimpleSongsPlayer.Service.Models;

namespace SimpleSongsPlayer.ViewModels.Extensions
{
    public static class ExceptionExtensions
    {
        private const string Email = "kljzndx@outlook.com";

        private static readonly string DialogTitle;
        private static readonly string SendErrorReport;
        private static readonly string DialogClose;
        private static readonly string EmailTitle;
        private static readonly string ErrorCode;
        private static readonly string ErrorMessage;
        private static readonly string LoggingInfo;
        private static readonly string SystemVersion;

        static ExceptionExtensions()
        {
            ResourceLoader dialog_StringResource = ResourceLoader.GetForCurrentView("Dialog");
            ResourceLoader errorDialog_StringResource = ResourceLoader.GetForCurrentView("ErrorDialog");
            ResourceLoader errorTable_StringResource = ResourceLoader.GetForCurrentView("ErrorTable");

            DialogTitle = errorDialog_StringResource.GetString("Title");
            SendErrorReport = errorDialog_StringResource.GetString("SendErrorReport");
            DialogClose = dialog_StringResource.GetString("Close");
            EmailTitle = errorTable_StringResource.GetString("EmailTitle");

            ErrorCode = errorDialog_StringResource.GetString("ErrorCode");
            ErrorMessage = errorDialog_StringResource.GetString("ErrorMessage");

            LoggingInfo = errorTable_StringResource.GetString("LoggingInfo");
            SystemVersion = errorTable_StringResource.GetString("SystemVersion");
        }

        public static async Task ShowErrorDialog(this Exception exception, Logger logger = null)
        {
            if (logger != null)
                logger.Error(exception);
            else
                typeof(ExceptionExtensions).LogByType(exception);

            bool needSendEmail = false;
            await MessageBox.ShowAsync(DialogTitle,
                $"{ErrorCode} 0x{exception.HResult:x8}\r\n{ErrorMessage} {exception.Message}",
                new Dictionary<string, UICommandInvokedHandler> {{SendErrorReport, u => needSendEmail = true}},
                DialogClose);

            if (needSendEmail)
            {
                string title = $"{AppInfo.Name} {AppInfo.Version} {EmailTitle}";
                string body = $"{SystemVersion} {SystemInfo.BuildVersion}\r\n{LoggingInfo}\r\n{await LoggerService.ReadLogs(LoggerMembers.App, 20)}";

                await EmailEx.SendAsync(Email, title, body);
            }
        }
    }
}