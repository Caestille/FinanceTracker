using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinanceTracker.Core.Interfaces
{
	public interface IBankApiService
	{
		Task<bool> LinkBank(Guid bankGuid, CancellationToken token);

		void RefreshLink(Guid bankGuid);

		void DeleteLink(Guid bankGuid);
	}
}
