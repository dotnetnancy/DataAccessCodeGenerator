using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;

namespace CommonLibrary
{
    public static class ClientBaseExtenstion
    {
        /// <summary>
        /// Transitions proxy to closed state via Close() or Abort() 
        /// dependently on current proxy state
        /// </summary>
        public static void CloseProxy<T>(this ClientBase<T> proxy) where T : class
        {
            try
            {
                if (proxy.State != CommunicationState.Closed &&
            proxy.State != CommunicationState.Faulted)
                {
                    proxy.Close(); // may throw exception while closing
                }
                else
                {
                    proxy.Abort();
                }
            }
            catch (CommunicationException)
            {
                proxy.Abort();
                throw;
            }
        }
    }

    public static class DuplexClientBaseExtenstion
    {
        /// <summary>
        /// Transitions proxy to closed state via Close() or Abort() 
        /// dependently on current proxy state
        /// </summary>
        public static void CloseProxy<T>(this DuplexClientBase<T> proxy) where T : class
        {
            try
            {
                if (proxy.State != CommunicationState.Closed &&
            proxy.State != CommunicationState.Faulted)
                {
                    proxy.Close(); // may throw exception while closing
                }
                else
                {
                    proxy.Abort();
                }
            }
            catch (CommunicationException)
            {
                proxy.Abort();
                throw;
            }
        }
    }

}
