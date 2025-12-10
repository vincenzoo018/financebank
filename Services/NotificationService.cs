using System;

namespace FinanceBank.Services
{
    public class NotificationService
    {
        public event Action<string, string, NotificationType>? OnNotify;
        public event Action<string, string, Func<Task>, Func<Task>?>? OnConfirm;

        public enum NotificationType
        {
            Success,
            Error,
            Warning,
            Info
        }

        public void ShowSuccess(string title, string message)
        {
            OnNotify?.Invoke(title, message, NotificationType.Success);
        }

        public void ShowError(string title, string message)
        {
            OnNotify?.Invoke(title, message, NotificationType.Error);
        }

        public void ShowWarning(string title, string message)
        {
            OnNotify?.Invoke(title, message, NotificationType.Warning);
        }

        public void ShowInfo(string title, string message)
        {
            OnNotify?.Invoke(title, message, NotificationType.Info);
        }

        public void Confirm(string title, string message, Func<Task> onConfirm, Func<Task>? onCancel = null)
        {
            OnConfirm?.Invoke(title, message, onConfirm, onCancel);
        }
    }
}
