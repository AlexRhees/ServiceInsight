using Caliburn.PresentationFramework.ApplicationModel;
using NServiceBus.Profiler.Common.Events;

namespace NServiceBus.Profiler.Desktop.Explorer
{
    public interface IExplorerView : IHandle<WorkStarted>, IHandle<WorkFinished>
    {
        void Expand();
        void SelectRow(int rowHandle);
        void ExpandNode(ExplorerItem item);
    }
}