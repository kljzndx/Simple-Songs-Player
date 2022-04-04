using HappyStudio.UwpToolsLibrary.Auxiliarys;
using HappyStudio.UwpToolsLibrary.Information;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Windows.ApplicationModel.Resources;
using Windows.UI.Popups;

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
        private static readonly string StackInfo;
        private static readonly string SystemVersion;

        static ExceptionExtensions()
        {
            ResourceLoader stringResource = ResourceLoader.GetForCurrentView("ErrorDialog");

            DialogTitle = stringResource.GetString("Title");
            SendErrorReport = stringResource.GetString("SendErrorReport");
            DialogClose = stringResource.GetString("Close");

            EmailTitle = stringResource.GetString("EmailTitle");
            ErrorCode = stringResource.GetString("ErrorCode");
            ErrorMessage = stringResource.GetString("ErrorMessage");
            StackInfo = stringResource.GetString("StackInfo");
            SystemVersion = stringResource.GetString("SystemVersion");
        }

        public static async Task ShowErrorDialog(this Exception exception)
        {
            bool needSendEmail = false;
            await MessageBox.ShowAsync(DialogTitle,
                $"{ErrorCode} 0x{exception.HResult:x8}\r\n{ErrorMessage} {exception.Message}",
                new Dictionary<string, UICommandInvokedHandler> { { SendErrorReport, u => needSendEmail = true } },
                DialogClose);

            if (needSendEmail)
            {
                string title = $"{AppInfo.Name} {AppInfo.Version} {EmailTitle}";
                string body = $"{SystemVersion} {SystemInfo.BuildVersion}\r\n{ErrorCode} {exception.HResult:x8}\r\n{ErrorMessage} {exception.Message}\r\n{StackInfo}\r\n{exception.StackTrace}";

                await EmailEx.SendAsync(Email, title, body);
            }
        }
    }
}
