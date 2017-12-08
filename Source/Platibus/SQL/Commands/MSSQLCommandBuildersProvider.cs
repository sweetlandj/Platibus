// The MIT License (MIT)
// 
// Copyright (c) 2017 Jesse Sweetland
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

#if NET452
using System.Configuration;
#elif NETSTANDARD2_0
using Platibus.Config;
#endif
using Platibus.Config.Extensibility;

namespace Platibus.SQL.Commands
{
    /// <summary>
    /// Provides command builders for message journaling, message queueing, and subscription
    /// tracking based on Microsoft SQL Server databases.
    /// </summary>
    [Provider("System.Data.SQLClient")]
    public class MSSQLCommandBuildersProvider : 
        IMessageJournalingCommandBuildersProvider, 
        IMessageQueueingCommandBuildersProvider, 
        ISubscriptionTrackingCommandBuildersProvider
    {
        /// <inheritdoc />
        public IMessageJournalingCommandBuilders GetMessageJournalingCommandBuilders(ConnectionStringSettings connectionStringSettings)
        {
            return new MSSQLMessageJournalingCommandBuilders();
        }

        /// <inheritdoc />
        public IMessageQueueingCommandBuilders GetMessageQueueingCommandBuilders(ConnectionStringSettings connectionStringSettings)
        {
            return new MSSQLMessageQueueingCommandBuilders();
        }

        /// <inheritdoc />
        public ISubscriptionTrackingCommandBuilders GetSubscriptionTrackingCommandBuilders(ConnectionStringSettings connectionStringSettings)
        {
            return new MSSQLSubscriptionTrackingCommandBuilders();
        }
    }
}
