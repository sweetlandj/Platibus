using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Platibus
{
    public interface IEndpointCredentials
    {
        void Accept(IEndpointCredentialsVisitor visitor);
    }
}
