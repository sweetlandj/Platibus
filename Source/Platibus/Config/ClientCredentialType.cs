using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Platibus.Config
{
    public enum ClientCredentialType
    {
        None,
        Basic,
        Windows,
        NTLM = Windows
    }
}
