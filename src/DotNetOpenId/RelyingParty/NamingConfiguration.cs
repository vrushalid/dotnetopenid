using System;
using System.Collections.Generic;
using System.Text;

namespace DotNetOpenId.RelyingParty {
	static class NamingConfiguration {
		internal static NamingProvider Provider = new ShortIDsProvider();
	}
}
