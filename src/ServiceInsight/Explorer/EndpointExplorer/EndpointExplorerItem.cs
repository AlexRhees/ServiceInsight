﻿namespace Particular.ServiceInsight.Desktop.Explorer.EndpointExplorer
{
    using Models;

    public abstract class EndpointExplorerItem : ExplorerItem
    {
        protected EndpointExplorerItem(Endpoint endpoint) : base(endpoint.Name)
        {
            Endpoint = endpoint;
        }

        public Endpoint Endpoint { get; private set; }
    }
}