using System;

namespace Platibus.Diagnostics
{
    /// <summary>
    /// A specialized <see cref="TextLoggingSink"/> that targets <see cref="Console.Out"/>
    /// </summary>
    public class ConsoleLoggingSink : TextLoggingSink
    {
        /// <summary>
        /// Initializes a new <see cref="ConsoleLoggingSink"/>
        /// </summary>
        public ConsoleLoggingSink() : base(Console.Out)
        {
        }
    }
}
