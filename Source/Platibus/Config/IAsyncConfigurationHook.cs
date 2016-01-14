using System.Threading;
using System.Threading.Tasks;

namespace Platibus.Config
{
	/// <summary>
	/// Interface used by application developers to provide programmatic configuration
	/// during bus initialization.
	/// </summary>
	public interface IAsyncConfigurationHook
	{
		/// <summary>
		/// Called during bus initialization to allow application developers an
		/// opportunity to modify the configuration before the bus is initialized.
		/// </summary>
		/// <param name="configuration">The bus configuration.</param>
		/// <param name="cancellationToken">A cancellation token that can be used
		/// by the caller to cancel initialiation.</param>
		Task Configure(PlatibusConfiguration configuration, CancellationToken cancellationToken = default(CancellationToken));
	}
}