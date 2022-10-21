using RnD.Messages.Client;
using RnD.Messages.Interfaces.Data;

namespace SinExecSQLQueryInfoField.Services
{
    /// <summary>
    /// Сервис отправки сообщений об ошибках.
    /// </summary>
    public static class MessageService
    {
        public static void SendErrorMessage(string message)
        {
            var client = new NotificationClient();
            client.SendToAll(new Notification
            {
                NotificationType = NotificationType.ErrorMessage,
                Data = new
                {
                    Message = message
                }
            });
        }

        public static void SendInfoMessage(string message)
        {
            var client = new NotificationClient();
            client.SendToAll(new Notification
            {
                NotificationType = NotificationType.Message,
                Data = new
                {
                    Message = message
                }
            });
        }
    }
}
