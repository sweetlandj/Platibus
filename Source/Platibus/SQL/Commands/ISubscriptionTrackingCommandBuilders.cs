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

namespace Platibus.SQL.Commands
{
    /// <summary>
    /// An interface describing objects that specify a suite of command builders capable of
    /// producing commands with syntax and formatting that is suitable for a specific database.
    /// </summary>
    public interface ISubscriptionTrackingCommandBuilders
    {
        /// <summary>
        /// Returns a new builder for constructing commands needed to initialize database objects
        /// for storing topic subscription information
        /// </summary>
        /// <returns>
        /// Returns a new builder for constructing commands needed to initialize database objects
        /// for storing topic subscription information
        /// </returns>
        CreateSubscriptionTrackingObjectsCommandBuilder NewCreateObjectsCommandBuilder();

        /// <summary>
        /// Returns a new builder for constructing commands needed to insert new topic subscription
        /// information into the database
        /// </summary>
        /// <returns>
        /// Returns a new builder for constructing commands needed to insert new topic subscription
        /// information into the database
        /// </returns>
        InsertSubscriptionCommandBuilder NewInsertSubscriptionCommandBuilder();

        /// <summary>
        /// Returns a new builder for constructing commands needed to update existing topic
        /// subscription information in the database
        /// </summary>
        /// <returns>
        /// Returns a new builder for constructing commands needed to update existing topic
        /// subscription information in the database
        /// </returns>
        UpdateSubscriptionCommandBuilder NewUpdateSubscriptionCommandBuilder();

        /// <summary>
        /// Returns a new builder for constructing commands needed to deleting existing topic
        /// subscription information from the database
        /// </summary>
        /// <returns>
        /// Returns a new builder for constructing commands needed to deleting existing topic
        /// subscription information from the database
        /// </returns>
        DeleteSubscriptionCommandBuilder NewDeleteSubscriptionCommandBuilder();
    }
}
