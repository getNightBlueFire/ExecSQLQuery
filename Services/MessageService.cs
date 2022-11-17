using RnD.Messages.Client;
using RnD.Messages.Interfaces.Data;
using System;

namespace ExecSQLQueryInfoField.Services
{
    /// <summary>
    /// Сервис отправки сообщений об ошибках.
    /// </summary>
    public static class MessageService
    {
        public static void SendErrorMessage(string message, string username, string db, Guid guid)
        {
            var client = new NotificationClient();
    
            client.SendTo(new Notification
            {
                NotificationType = NotificationType.ErrorMessage,
                Data = new
                {
                    Message = message
                }
            },
            db,
            username,
            guid
            );
        }

        public static void SendInfoMessage(string message, string username, string db, Guid guid)
        {
            var client = new NotificationClient();
            client.SendTo(new Notification
            {
                NotificationType = NotificationType.Message,
                Data = new
                {
                    Message = message
                }
            }, db, username, guid);
        }
    }
}
