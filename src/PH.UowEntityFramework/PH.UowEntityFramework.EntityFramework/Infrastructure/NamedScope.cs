using System;
using Microsoft.Extensions.Logging;

namespace PH.UowEntityFramework.EntityFramework.Infrastructure
{
    internal class NamedScope : IDisposable
    {
        private IDisposable _disposableScope;
        
        public NamedScope(ILogger contextLogger, string scopeName)
        {
            _disposableScope = contextLogger?.BeginScope(scopeName);
        }


        /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
        public void Dispose()
        {
            _disposableScope?.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}