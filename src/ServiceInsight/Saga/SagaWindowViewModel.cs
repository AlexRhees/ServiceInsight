﻿namespace Particular.ServiceInsight.Desktop.Saga
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows.Input;
    using Caliburn.Micro;
    using Models;
    using Particular.ServiceInsight.Desktop.ExtensionMethods;
    using Particular.ServiceInsight.Desktop.Framework;
    using Particular.ServiceInsight.Desktop.Framework.Events;
    using ServiceControl;

    public class SagaWindowViewModel : Screen, IHandle<SelectedMessageChanged>
    {
        SagaData data;
        StoredMessage currentMessage;
        Guid selectedMessageId;
        IEventAggregator eventAggregator;
        IServiceControl serviceControl;

        public SagaWindowViewModel(IEventAggregator eventAggregator, IServiceControl serviceControl, IClipboard clipboard)
        {
            this.eventAggregator = eventAggregator;
            this.serviceControl = serviceControl;
            ShowSagaNotFoundWarning = false;
            CopyCommand = this.CreateCommand(arg => clipboard.CopyTo(arg.ToString()));
        }

        public ICommand CopyCommand { get; private set; }

        public void OnShowMessageDataChanged()
        {
            if (Data == null || Data.Changes == null)
            {
                return;
            }

            RefreshShowData();

            if (ShowMessageData)
            {
                RefreshMessageProperties();
            }
        }

        void RefreshShowData()
        {
            var messages = Data.Changes
                               .Select(c => c.InitiatingMessage)
                               .Union(Data.Changes.SelectMany(c => c.OutgoingMessages));

            foreach (var message in messages)
            {
                message.ShowData = ShowMessageData;
            }

            NotifyOfPropertyChange(() => Data);
        }

        void RefreshMessageProperties()
        {
            foreach (var message in Data.Changes.Select(c => c.InitiatingMessage).Union(Data.Changes.SelectMany(c => c.OutgoingMessages)))
            {
                message.RefreshData(serviceControl);
            }

            NotifyOfPropertyChange(() => Data);
        }

        public void Handle(SelectedMessageChanged @event)
        {
            var message = @event.Message;

            if (message == null)
            {
                return;
            }

            RefreshSaga(message);

            SelectedMessageId = Guid.Parse(message.MessageId);
        }

        void RefreshSaga(StoredMessage message)
        {
            currentMessage = message;
            ShowSagaNotFoundWarning = false;

            if (message == null || message.Sagas == null || !message.Sagas.Any())
            {
                Data = null;
            }
            else
            {
                var originatingSaga = message.Sagas.First();

                if (originatingSaga != null)
                {
                    RefreshSaga(originatingSaga);
                }
            }
        }

        void RefreshSaga(SagaInfo originatingSaga)
        {
            try
            {
                eventAggregator.Publish(new WorkStarted("Loading message body..."));

                var previousSagaId = Guid.Empty;

                if (Data == null || Data.SagaId != originatingSaga.SagaId)
                {
                    if (Data != null)
                    {
                        previousSagaId = Data.SagaId;
                    }

                    Data = serviceControl.GetSagaById(originatingSaga.SagaId);

                    if (Data != null)
                    {
                        if (Data.SagaId == Guid.Empty)
                        {
                            ShowSagaNotFoundWarning = true;
                            Data = null;
                        }
                        else if (Data.Changes != null)
                        {
                            ProcessDataValues(Data.Changes);
                        }
                        else
                        {
                            Data = null;
                        }
                    }
                }

                if (Data == null || Data.Changes == null)
                {
                    return;
                }

                // Skip refreshing if we already displaying the correct saga data
                if (previousSagaId == Data.SagaId)
                {
                    return;
                }

                RefreshShowData();

                if (ShowMessageData)
                {
                    RefreshMessageProperties();
                }
            }
            finally
            {
                eventAggregator.Publish(new WorkFinished());
            }
        }

        static void ProcessDataValues(IEnumerable<SagaUpdate> list)
        {
            var oldValues = new List<SagaUpdatedValue>();
            foreach (var change in list)
            {
                ProcessChange(oldValues, change.Values);
                oldValues = change.Values;
            }
        }

        static void ProcessChange(IList<SagaUpdatedValue> oldValues, IEnumerable<SagaUpdatedValue> newValues)
        {
            foreach (var value in newValues)
            {
                var oldValueViewModel = oldValues.FirstOrDefault(v => v.Name == value.Name);
                value.UpdateOldValue(oldValueViewModel);
            }
        }

        public bool ShowSagaNotFoundWarning { get; set; }

        public bool HasSaga { get { return Data != null; } }

        public SagaData Data
        {
            get { return data; }
            private set
            {
                data = value;
                Header = new SagaHeader(data);
                Footer = new SagaFooter(data);
            }
        }

        public SagaHeader Header { get; private set; }

        public SagaFooter Footer { get; private set; }

        public bool ShowEndpoints { get; set; }

        public bool ShowMessageData { get; set; }

        public void ShowFlow()
        {
            eventAggregator.Publish(SwitchToFlowWindow.Instance);
        }

        public void RefreshSaga()
        {
            RefreshSaga(currentMessage);
        }

        public Guid SelectedMessageId
        {
            get { return selectedMessageId; }
            set
            {
                selectedMessageId = value;
                UpdateSelectedMessages();
            }
        }

        void UpdateSelectedMessages()
        {
            if (Data == null)
            {
                return;
            }

            foreach (var step in Data.Changes)
            {
                SetSelected(step.InitiatingMessage, selectedMessageId);
                SetSelected(step.OutgoingMessages, selectedMessageId);
            }
        }

        void SetSelected(IEnumerable<SagaMessage> messages, Guid id)
        {
            if (messages == null)
                return;

            foreach (var message in messages)
            {
                SetSelected(message, id);
            }
        }

        void SetSelected(SagaMessage message, Guid id)
        {
            message.IsSelected = message.MessageId == id;
        }
    }
}