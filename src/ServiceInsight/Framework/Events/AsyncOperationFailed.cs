﻿namespace Particular.ServiceInsight.Desktop.Framework.Events
{
    using Particular.ServiceInsight.Desktop.ExtensionMethods;

    public class AsyncOperationFailed
    {
        public const string DefaultMessage = "Operation Failed.";

        public AsyncOperationFailed(string message = DefaultMessage)
        {
            Message = message.IsEmpty() ? DefaultMessage : string.Format("Operation Failed: {0}", message);
        }

        public string Message { get; private set; }
    }
}