using System;
using System.Collections.Generic;
using System.Text;
using websocketschat.Database.Context;

namespace websocketschat.Database.Repositories
{
    /// <summary>
    /// Shared part of every repository.
    /// </summary>
    public class BaseRepository : IDisposable
    {
        protected readonly NpgSqlContext _dbContext;
        private bool disposedValue;

        public BaseRepository(NpgSqlContext context)
        {
            _dbContext = context;
        }

        /// <summary>
        /// Releases unmanaged resources.
        /// </summary>
        /// <param name="disposing">Handle the process of releasing.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _dbContext.Dispose();
                }

                disposedValue = true;
            }
        }

        /// <summary>
        /// Implements Dispose pattern.
        /// </summary>
        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
