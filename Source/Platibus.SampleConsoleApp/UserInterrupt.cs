using System;
using System.Threading;
using System.Threading.Tasks;

namespace Platibus.SampleConsoleApp
{
    public class UserInterrupt
    {
        public static Task Wait(CancellationToken cancellationToken)
        {
            Console.WriteLine("Press any key to exit...");
            var cancellationSource = new TaskCompletionSource<bool>();
            cancellationToken.Register(() => cancellationSource.TrySetResult(true));
            var userInterrupt = Task.Run(() => { Console.ReadLine(); }, cancellationToken);
            return Task.WhenAny(userInterrupt, cancellationSource.Task);
        }
    }
}