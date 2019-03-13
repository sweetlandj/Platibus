using System;
using System.Threading;
using System.Threading.Tasks;

namespace Platibus.SQLite
{
    public interface ICommandExecutor
    {
        Task Execute(Func<Task> command, CancellationToken cancellationToken = default(CancellationToken));
        Task<TResult> ExecuteRead<TResult>(Func<Task<TResult>> command, CancellationToken cancellationToken = default(CancellationToken));
    }
}