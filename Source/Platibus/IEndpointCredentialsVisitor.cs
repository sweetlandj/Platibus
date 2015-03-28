using Platibus.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Platibus
{
    public interface IEndpointCredentialsVisitor
    {
        void Visit(BasicAuthCredentials credentials);
        void Visit(DefaultCredentials credentials);
    }
}
