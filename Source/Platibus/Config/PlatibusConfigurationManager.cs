
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
using System.Threading.Tasks;
#if NET452 || NET461
using Platibus.Config.Extensibility;
using Platibus.Journaling;
using Platibus.Security;
#endif

namespace Platibus.Config
{
    /// <inheritdoc />
    /// <summary>
    /// Factory class used to initialize <see cref="T:Platibus.Config.PlatibusConfiguration" /> objects from
    /// declarative configuration elements in application configuration files.
    /// </summary>
    public class PlatibusConfigurationManager : PlatibusConfigurationManager<PlatibusConfiguration>
    {
#if NET452 || NET461
        /// <summary>
        /// Helper method to locate, initialize, and invoke all types inheriting from
        /// <see cref="IConfigurationHook"/> found in the application domain
        /// </summary>
        /// <param name="configuration">The configuration that will be passed to the
        ///     configuration hooks</param>
        [Obsolete("Use instance method FindAndProcessConfigurationHooks")]
        public static Task ProcessConfigurationHooks(PlatibusConfiguration configuration)
        {
            if (configuration == null) Task.FromResult(0);
            var configManager = new PlatibusConfigurationManager();
            return configManager.FindAndProcessConfigurationHooks(configuration);
        }

        /// <summary>
        /// Initializes and returns a <see cref="PlatibusConfiguration"/> instance based on
        /// the <see cref="PlatibusConfigurationSection"/> with the specified 
        /// <paramref name="sectionName"/>
        /// </summary>
        /// <param name="sectionName">(Optional) The name of the configuration section 
        /// (default is "platibus")</param>
        /// <param name="processConfigurationHooks">(Optional) Whether to initialize and
        /// process implementations of <see cref="IConfigurationHook"/> found in the
        /// application domain (default is true)</param>
        /// <returns>Returns a task whose result will be an initialized 
        /// <see cref="PlatibusConfiguration"/> object</returns>
        /// <seealso cref="PlatibusConfigurationSection"/>
        /// <seealso cref="IConfigurationHook"/>
        /// <seealso cref="NetFrameworkConfigurationManager{TConfiguration}.Initialize(TConfiguration,string)"/>
        [Obsolete("Use instance method Initialize")]
        public static async Task<PlatibusConfiguration> LoadConfiguration(string sectionName = null,
            bool processConfigurationHooks = true)
        {
            var configurationManager = new PlatibusConfigurationManager();
            var configuration = new PlatibusConfiguration();
            await configurationManager.Initialize(configuration, sectionName);
            await configurationManager.FindAndProcessConfigurationHooks(configuration);
            if (processConfigurationHooks)
            {
                await configurationManager.FindAndProcessConfigurationHooks(configuration);
            }
            return configuration;
        }
        
        /// <summary>
        /// Initializes and returns a <see cref="PlatibusConfiguration"/> instance based on
        /// the <see cref="LoopbackConfigurationSection"/> with the specified 
        /// <paramref name="sectionName"/>
        /// </summary>
        /// <param name="sectionName">(Optional) The name of the configuration section 
        /// (default is "platibus.loopback")</param>
        /// <param name="processConfigurationHooks">(Optional) Whether to initialize and
        /// process implementations of <see cref="IConfigurationHook"/> found in the
        /// application domain (default is true)</param>
        /// <returns>Returns a task whose result will be an initialized 
        /// <see cref="PlatibusConfiguration"/> object</returns>
        /// <seealso cref="PlatibusConfigurationSection"/>
        /// <seealso cref="IConfigurationHook"/>
        /// <seealso cref="LoopbackConfigurationManager.Initialize(LoopbackConfiguration,string)"/>
        [Obsolete("Use instance method LoopbackConfigurationManager.Initialize")]
        public static async Task<LoopbackConfiguration> LoadLoopbackConfiguration(string sectionName = null,
            bool processConfigurationHooks = true)
        {
            var configurationManager = new LoopbackConfigurationManager();
            var configuration = new LoopbackConfiguration();
            await configurationManager.Initialize(configuration, sectionName);
            if (processConfigurationHooks)
            {
                await configurationManager.FindAndProcessConfigurationHooks(configuration);
            }
            return configuration;
        }

        /// <summary>
        /// Initializes and returns a <typeparamref name="TConfig"/> instance based on
        /// the <see cref="PlatibusConfigurationSection"/> with the specified 
        /// <paramref name="sectionName"/>
        /// </summary>
        /// <typeparam name="TConfig">A type that inherits <see cref="PlatibusConfiguration"/>
        /// and has a default constructor</typeparam>
        /// <param name="sectionName">(Optional) The name of the configuration section 
        /// (default is "platibus")</param>
        /// <param name="processConfigurationHooks">(Optional) Whether to initialize and
        /// process implementations of <see cref="IConfigurationHook"/> found in the
        /// application domain (default is true)</param>
        /// <returns>Returns a task whose result will be an initialized 
        /// <typeparamref name="TConfig"/> object</returns>
        /// <seealso cref="PlatibusConfigurationSection"/>
        /// <seealso cref="IConfigurationHook"/>
        /// <seealso cref="NetFrameworkConfigurationManager{TConfiguration}.Initialize(TConfiguration,string)"/>
        [Obsolete("Use instance method Initialize")]
        public static async Task<TConfig> LoadConfiguration<TConfig>(string sectionName, bool processConfigurationHooks = true)
            where TConfig : PlatibusConfiguration, new()
        {
            var configurationManager = new NetFrameworkConfigurationManager<TConfig>();
            var configuration = new TConfig();
            await configurationManager.Initialize(configuration, sectionName);
            if (processConfigurationHooks)
            {
                await configurationManager.FindAndProcessConfigurationHooks(configuration);
            }
            return configuration;
        }

        /// <summary>
        /// Initializes and returns a <typeparamref name="TConfig"/> instance based on
        /// the supplied <see cref="PlatibusConfigurationSection"/>
        /// </summary>
        /// <typeparam name="TConfig">A type that inherits <see cref="PlatibusConfiguration"/>
        /// and has a default constructor</typeparam>
        /// <param name="configSection">The configuration section</param>
        /// <param name="processConfigurationHooks">(Optional) Whether to initialize and
        /// process implementations of <see cref="IConfigurationHook"/> found in the
        /// application domain (default is true)</param>
        /// <returns>Returns a task whose result will be an initialized 
        /// <typeparamref name="TConfig"/> object</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="configSection"/>
        /// is <c>null</c></exception>
        /// <seealso cref="PlatibusConfigurationSection"/>
        /// <seealso cref="IConfigurationHook"/>
        /// <seealso cref="NetFrameworkConfigurationManager{TConfiguration}.Initialize(TConfiguration,string)"/>
        [Obsolete("Use instance method Initialize")]
        public static async Task<TConfig> LoadConfiguration<TConfig>(PlatibusConfigurationSection configSection,
            bool processConfigurationHooks = true) where TConfig : PlatibusConfiguration, new()
        {
            var configurationManager = new NetFrameworkConfigurationManager<TConfig>();
            var configuration = new TConfig();
            await configurationManager.Initialize(configuration, configSection);
            if (processConfigurationHooks)
            {
                await configurationManager.FindAndProcessConfigurationHooks(configuration);
            }
            return configuration;
        }

        /// <summary>
        /// Helper method to initialize message queueing services based on the
        /// supplied configuration element
        /// </summary>
        /// <param name="config">The queueing configuration element</param>
        /// <returns>Returns a task whose result is an initialized message queueing service</returns>
        /// <seealso cref="MessageQueueingServiceFactory.InitMessageQueueingService"/>
        [Obsolete("Use MessageQueueingServiceFactory.InitMessageQueueingService")]
        public static Task<IMessageQueueingService> InitMessageQueueingService(QueueingElement config)
        {
            var factory = new MessageQueueingServiceFactory();
            return factory.InitMessageQueueingService(config);
        }

        /// <summary>
        /// Helper method to initialize security token services based on the
        /// supplied configuration element
        /// </summary>
        /// <param name="config">The security tokens configuration element</param>
        /// <returns>Returns a task whose result is an initialized security token service</returns>
        [Obsolete("Use SecurityTokenServiceFactory.InitSecurityTokenService")]
        public static Task<ISecurityTokenService> InitSecurityTokenService(SecurityTokensElement config)
        {
            var factory = new SecurityTokenServiceFactory();
            return factory.InitSecurityTokenService(config);
        }

        /// <summary>
        /// Helper method to initialize the message journaling service based on the
        /// supplied configuration element
        /// </summary>
        /// <param name="config">The journaling configuration element</param>
        /// <returns>Returns a task whose result is an initialized message journaling service</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="config"/> is
        /// <c>null</c></exception>
        [Obsolete("Use MessageJournalFactory.InitMessageJournal")]
        public static Task<IMessageJournal> InitMessageJournal(JournalingElement config)
        {
            var factory = new MessageJournalFactory();
            return factory.InitMessageJournal(config);
        }
#endif
    }

    /// <inheritdoc />
    /// <summary>
    /// Factory class used to initialize <see cref="T:Platibus.Config.PlatibusConfiguration" /> objects from
    /// declarative configuration elements in application configuration files.
    /// </summary>
    public class PlatibusConfigurationManager<TConfiguration>
#if NET452 || NET461
        : NetFrameworkConfigurationManager<TConfiguration> where TConfiguration : PlatibusConfiguration
#endif
#if NETSTANDARD2_0
        : NetStandardConfigurationManager<TConfiguration> where TConfiguration : PlatibusConfiguration
#endif
    {
    }
}
