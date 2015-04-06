using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Platibus.SQL
{
	public class MSSQLDialect : CommonSQLDialect
	{
		public override string CreateObjectsCommand
		{
			get { return MSSQLCommands.CreateObjects; }
		}
	}
}
