// The MIT License (MIT)
// 
// Copyright (c) 2016 Jesse Sweetland
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

using System;
#if NET452
using System.Configuration;
#elif NETSTANDARD2_0
using Platibus.Config;
#endif
using System.Data;
using System.Data.Common;

namespace Platibus.SQL
{
    /// <summary>
    /// Extension methods to simplify working with ADO.NET connections, commands, and data records 
    /// </summary>
    public static class DbExtensions
    {
        /// <summary>
        /// Opens a connection using the specified <paramref name="connectionStringSettings"/>
        /// </summary>
        /// <param name="connectionStringSettings">The connection string settings containing the
        /// provider name and connection string</param>
        /// <returns>Returns the newly opened connection, or <c>null</c> if the provider factory
        /// produces a <c>null</c> connection</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="connectionStringSettings"/>
        /// is <c>null</c></exception>
        public static DbConnection OpenConnection(this ConnectionStringSettings connectionStringSettings)
        {
            if (connectionStringSettings == null) throw new ArgumentNullException(nameof(connectionStringSettings));

            var providerFactory = DbProviderFactories.GetFactory(connectionStringSettings.ProviderName);
            var connection = providerFactory.CreateConnection();
            if (connection == null) return null;

            connection.ConnectionString = connectionStringSettings.ConnectionString;
            connection.Open();
            return connection;
        }

        /// <summary>
        /// Sets a parameter on an ADO.NET command
        /// </summary>
        /// <param name="command">The command</param>
        /// <param name="name">The name of the parameter to set</param>
        /// <param name="value">The parameter value</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="command"/> or
        /// <paramref name="name"/> is <c>null</c> or whitespace</exception>
        /// <remarks>
        /// If <paramref name="value"/> is <c>null</c> then the value of the parameter will be
        /// set to <see cref="DBNull.Value"/>.  If <paramref name="value"/> is a <see cref="TimeSpan"/>
        /// then the value will be set to the <see cref="TimeSpan.TotalMilliseconds"/> (cast to
        /// <c>long</c>).  Otherwise the value will be set on the parameter as-is.
        /// </remarks>
        public static void SetParameter(this DbCommand command, string name, object value)
        {
            if (command == null) throw new ArgumentNullException(nameof(command));
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentNullException(nameof(name));

            var parameter = command.CreateParameter();
            parameter.ParameterName = name;

            if (value == null)
            {
                parameter.Value = DBNull.Value;
            }
            else if (value is TimeSpan)
            {
                parameter.Value = (long) ((TimeSpan) value).TotalMilliseconds;
            }
            else
            {
                parameter.Value = value;
            }

            command.Parameters.Add(parameter);
        }

        /// <summary>
        /// Returns the value of the column with the specified <paramref name="name"/> as a string
        /// </summary>
        /// <param name="record">The data record from which the value is to be read</param>
        /// <param name="name">The name of the column from which the value is to be read</param>
        /// <param name="defaultValue">(Optional) The default value to return if the value of
        /// the named column <see cref="IDataRecord.IsDBNull"/></param>
        /// <returns>Returns the value of the named column as a string, or 
        /// <paramref name="defaultValue"/> if the value in the <paramref name="record"/> is
        /// <c>null</c></returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="record"/> or
        /// <paramref name="name"/> is <c>null</c> or whitespace</exception>
        public static string GetString(this IDataRecord record, string name, string defaultValue = null)
        {
            if (record == null) throw new ArgumentNullException(nameof(record));
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentNullException(nameof(name));
            var ordinal = record.GetOrdinal(name);
            return record.IsDBNull(ordinal) ? defaultValue : record.GetString(ordinal);
        }

        /// <summary>
        /// Returns the string value of the column with the specified <paramref name="name"/> as an
        /// enum
        /// </summary>
        /// <typeparam name="TEnum">The type of enum value represented by the </typeparam>
        /// <param name="record">The data record from which the value is to be read</param>
        /// <param name="name">The name of the column from which the value is to be read</param>
        /// <param name="defaultValue">(Optional) The default value to return if the value of
        /// the named column <see cref="IDataRecord.IsDBNull"/></param>
        /// <returns>Returns the value of the named column as a string, or 
        /// <paramref name="defaultValue"/> if the value in the <paramref name="record"/> is
        /// <c>null</c></returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="record"/> or
        /// <paramref name="name"/> is <c>null</c> or whitespace</exception>
        /// <exception cref="FormatException">Thrown if the value in the specified column is not
        /// a string or is not a valid <typeparamref name="TEnum"/> value</exception>
        public static TEnum Get<TEnum>(this IDataRecord record, string name, TEnum defaultValue = default(TEnum))
            where TEnum : struct
        {
            var str = record.GetString(name);
            if (string.IsNullOrWhiteSpace(str)) return defaultValue;
            return (TEnum)Enum.Parse(typeof(TEnum), str, true);
        }

        /// <summary>
        /// Returns the value of the column with the specified <paramref name="name"/> as a 32-bit
        /// integer value (<c>int</c>)
        /// </summary>
        /// <param name="record">The data record from which the value is to be read</param>
        /// <param name="name">The name of the column from which the value is to be read</param>
        /// <returns>Returns the value of the named column as a 32-bit integer, or 
        /// <c>null</c> if the value in the <paramref name="record"/> is <c>null</c></returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="record"/> or
        /// <paramref name="name"/> is <c>null</c> or whitespace</exception>
        public static int? GetInt(this IDataRecord record, string name)
        {
            if (record == null) throw new ArgumentNullException(nameof(record));
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentNullException(nameof(name));
            var ordinal = record.GetOrdinal(name);
            return record.IsDBNull(ordinal) ? null : (int?) record.GetInt32(ordinal);
        }

        /// <summary>
        /// Returns the value of the column with the specified <paramref name="name"/> as a 64-bit
        /// integer value (<c>long</c>)
        /// </summary>
        /// <param name="record">The data record from which the value is to be read</param>
        /// <param name="name">The name of the column from which the value is to be read</param>
        /// <returns>Returns the value of the named column as a 64-bit integer, or 
        /// <c>null</c> if the value in the <paramref name="record"/> is <c>null</c></returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="record"/> or
        /// <paramref name="name"/> is <c>null</c> or whitespace</exception>
        public static long? GetLong(this IDataRecord record, string name)
        {
            if (record == null) throw new ArgumentNullException(nameof(record));
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentNullException(nameof(name));
            var ordinal = record.GetOrdinal(name);
            return record.IsDBNull(ordinal) ? null : (long?) record.GetInt64(ordinal);
        }

        /// <summary>
        /// Returns the value of the column with the specified <paramref name="name"/> as a boolean
        /// </summary>
        /// <param name="record">The data record from which the value is to be read</param>
        /// <param name="name">The name of the column from which the value is to be read</param>
        /// <returns>Returns the value of the named column as a boolean, or 
        /// <c>null</c> if the value in the <paramref name="record"/> is <c>null</c></returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="record"/> or
        /// <paramref name="name"/> is <c>null</c> or whitespace</exception>
        public static bool? GetBoolean(this IDataRecord record, string name)
        {
            if (record == null) throw new ArgumentNullException(nameof(record));
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentNullException(nameof(name));
            var ordinal = record.GetOrdinal(name);
            return record.IsDBNull(ordinal) ? null : (bool?) record.GetBoolean(ordinal);
        }

        /// <summary>
        /// Returns the value of the column with the specified <paramref name="name"/> as a 
        /// <see cref="DateTime"/>
        /// </summary>
        /// <param name="record">The data record from which the value is to be read</param>
        /// <param name="name">The name of the column from which the value is to be read</param>
        /// <returns>Returns the value of the named column as a <see cref="DateTime"/>, or 
        /// <c>null</c> if the value in the <paramref name="record"/> is <c>null</c></returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="record"/> or
        /// <paramref name="name"/> is <c>null</c> or whitespace</exception>
        public static DateTime? GetDateTime(this IDataRecord record, string name)
        {
            if (record == null) throw new ArgumentNullException(nameof(record));
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentNullException(nameof(name));
            var ordinal = record.GetOrdinal(name);
            return record.IsDBNull(ordinal) ? null : (DateTime?) record.GetDateTime(ordinal);
        }
        
        /// <summary>
        /// Returns the value of the column with the specified <paramref name="name"/> as a 
        /// <see cref="TimeSpan"/>
        /// </summary>
        /// <param name="record">The data record from which the value is to be read</param>
        /// <param name="name">The name of the column from which the value is to be read</param>
        /// <returns>Returns the value of the named column as a <see cref="TimeSpan"/>, or 
        /// <c>null</c> if the value in the <paramref name="record"/> is <c>null</c></returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="record"/> or
        /// <paramref name="name"/> is <c>null</c> or whitespace</exception>
        /// <remarks>
        /// This method expects the value in the <paramref name="record"/> to be the total
        /// milliseconds in the time span represented as a 64-bit integer (<c>long</c>) value
        /// </remarks>
        public static TimeSpan? GetTimeSpan(this IDataRecord record, string name)
        {
            if (record == null) throw new ArgumentNullException(nameof(record));
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentNullException(nameof(name));
            var milliseconds = record.GetLong(name);
            return milliseconds == null ? null : (TimeSpan?) TimeSpan.FromMilliseconds(milliseconds.Value);
        }
    }
}