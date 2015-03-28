using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Platibus.Security
{
    public class DefaultCredentials : IEndpointCredentials
    {
        public void Accept(IEndpointCredentialsVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}
