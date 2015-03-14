# Platibus

Platibus is a decentralized asynchronous messaging framework designed to be platform and language agnostic.  The goal of this project is to make durable asynchronous transmission and processing of messages as simple as possible.

## Package Installation

Platibus is distributed as a set of NuGet packages hosted on nuget.org.  To install the base library, search for and install the package `Pluribus` in the NuGet package manager or run the following PowerShell command from the NuGet Package Manager Console:

```PM> Install-Package Platibus```

This package includes the standalone HTTP server and basic functionality including JSON and XML serialization, HTTP transport, and filesystem based queueing and journaling.  To add support for IIS hosting, install the `Platibus.IIS` package via the NuGet Package Manager or Package Manager Console:

```PM> Install-Package Platibus.IIS```

## Configuration Manager and Bootstrapping

A static `InitBus` method is provided in the `Platibus.Config.Bootstrapper` class to initialize a bus instance using any declarative configuration elements and configuration hooks that are available in the current app domain.  This method uses the `Platibus.Config.PlatibusConfigurationManager` class to load the configuration section and map it onto a `Platibus.Config.PlatibusConfiguration` object.  These two classes are available for use if more control is needed when configuring and initializing a bus instance.

## Hosting

Once an `Platibus.IBus` instance has been initialized, there are some final steps required to route incoming messages to it.  This is accomplished by hosting the bus in a server of some kind. 

### IIS HTTP Handler

A Platibus instance can be hosted in IIS using the `Platibus.IIS.PlatibusHttpHandler` supplied in the `Platibus.IIS` package.  To register the handler, add the following to your web.config:

```
 <system.webServer>
    <handlers>
      <add name="platibus" verb="*" path="platibus" type="Platibus.IIS.PlatibusHttpHandler, Platibus.IIS" />
    </handlers>
  </system.webServer>
```

The path can be anything you like, but it must agree with the `baseUri` specified in the `<platibus>` configuration section.  In the above example, the configured `baseUri` would be `http://<host>:<port>/platibus` where /host/ and /port/ are the hostname and port specified in the `http` protocol binding in the web site configuration.

Due to the inability to specify extra configuration parameters in the `handler` element, only a single Platibus instance can be hosted.  Further, the Platibus configuration must use the default section name `platibus` in order to be recognized and honored by the HTTP handler.

### Standalone HTTP Server

A rudimentary HTTP server is for self-hosting Platibus instances.  The following code is used to initialize and start the HTTP server:

```
// Start a new HTTP server based on the Platibus configuration
// found in the application configuration under the default section
// name "platibus"
var httpServer = new HttpServer();
httpServer.Start();
```

An alternate configuration section name can also be specified:

```
// Custom configuration section, for example nested in another
// configuration section group
var configSectionName = "mySectionGroup/myPlatibusConfig";
var httpServer = new HttpServer(configSectionName);
httpServer.Start();
```

The HTTP server stops listening when the `Dispose` method is invoked on the `HttpServer` instance.  The following is a basic console application that will start the HTTP server and wait for console input before shutting down:

```
public class MyHttpServer
{
    public static void Main(string[] args)
    {
        using (new HttpServer())
        {
            Console.WriteLine("Press any key to stop server...");
            Console.ReadKey();
        }
    }
}
```

## Configuration

Platibus can be configured programmatically, declaratively, or with a mixture of both styles.

### Declarative Configuration (app.config or web.config)

To configure Platibus in your application's configuration file, you must first define a configuration section:

```
<configSections>
  <section name="platibus" type="Platibus.Config.PlatibusConfigurationSection, Platibus" />
</configSections>
```

By convention the default name for the configuration section is `platibus`, but any name can be specified.  Multiple configuration sections can be declared with different names to create multiple configurations either for hosting multiple bus instances or for selecting a specific configuration during application initialization.

Once the configuration section is declared the configuration can be added to the application configuration file.  Here is an example configuration:

```
<configSections>
  <section name="platibus" type="Platibus.Config.PlatibusConfigurationSection, Platibus" />
</configSections>

<platibus baseUri="http://crm-app.example.com/platibus/">
  <timeouts replyTimeout="00:00:30" />
  <queueing provider="Filesystem" path="platibus\queues" />
  <journaling provider="Filesystem" path="platibus\journal" enabled="true" />
  <subscriptionTracking provider="Filesystem" path="platibus\subscriptions" />
  <endpoints>
    <add name="crm-app" address="http://crm-app.example.com/platibus/" />
    <add name="prv-app" address="http://prv-app.example.com/platibus/" />
  </endpoints>
  <topics>
    <add name="customer-events" />
    <add name="order-events" />
  </topics>
  <sendRules>
    <!-- Send a copy of all messages in the CRM customer and order namespace to the local bus instance -->
    <add namePattern="http://company/crm/customers:.*" endpoint="crm-app" />
    <add namePattern="http://company/crm/orders:.*" endpoint="crm-app" />
    
    <!-- Send all messages in the provisioning namespace to the remote provisioning app -->
    <add namePattern="http://company/prv/.*" endpoint="prv-app"/>
  </sendRules>
  <subscriptions>
    <!-- Subscribe to provisioning events published by the remote provisioning app -->
    <add endpoint="prv-app" topic="provisioning-events" ttl="12:00:00"/>
  </subscriptions>
</platibus>
```

In this example configuration a bus instance is being configured with the base URI `http://crm-app.example.com/platibus`.  This is the URL by which this application will be addressed by other applications.  If the standalone HTTP server is used, this is the address that the HTTP listener is configured to listen on.

The `timeouts` element (optional) defines global timeouts for the application.  In this case the application will cache sent messages for up to 30 minutes awaiting a correlated reply message from the recipient.

The `queueing` element determines which `IMessageQueueingService` implementation will be used to ensure durable transport (store-and-forward) for messages that request it.  In the example a filesystem based implementation is initialized with the path `platibus\queues` relative to the app domain base directory.  Rooted (absolute) paths may also be specified.

Likewise, the `journaling` element controls whether and how messages will be journaled.  This example shows a basic filesystem based journal that stores copies of the messages in subfolders beneath the specified relative path `platibus\journal`.

The `subscriptionTracking` element influences how subscription requests are tracked.  When remote applications subscribe to topics in this application, the topic being subscribed to, the subscribers base URI, and the subscription expiration date will be stored in a file within the `platibus\subscriptions` directory relative to the app domain base directory.

In order to send messages and subscription requests, this application must know the base URIs of the other applications with which it will communicate.  The `endpoints` collection defines a set of named endpoints and their corresponding URIs.  The endpoint names are used in other configuration elements to refer to the URIs indirectly.  This minimizes the configuration that must be changed i.e. when URIs are updated or an application is promoted from environment to environment.

Topics must be declared before messages can be published to them or subscribers can subscriber to them.  The `topics` element simply lists all of the defined topics for this application.  This list is used to provide a list of available topics to callers upon request and to validation subscription requests.

The `sendRules` element is a collection of criteria that determines the endpoints to which messages will initially be sent (they do not affect subscriptions or replies).  Each element consists of a `namePattern`, which is regular expression that matches the message name header, and an `endpoint` which is the name of the endpoint to which matching messages should be sent.

Finally, the `subscriptionRules` element specifies the applications and topics to which this application subscribes.  For each of these rules a subscription request will be sent to the specified `endpoint` requesting receipt of messages published to the specified `topic`.  An optional `ttl` (Time To Live) can be specified which caused the subscription to lapse and expire after a certain period of time.  If a TTL is specified, renewal subscription requests will be sent well in advance of the projected expiration date unless and until the subscription rule is removed.  (This prevents permanent publication of events to defunct applications.)

### Handling Incoming Messages

When using declarative stule configuration, handling rules must be added using an `Platibus.Config.IConfigurationHook` implementation.  Platibus will automatically scan assemblies in the app domain base directory for implementations of `Platibus.Config.IConfigurationHook` and invoke each of them.  The order in which they are invoked is not specified and should not be counted upon.  Best practice is to use a single configuration hook that calls out to other modules as needed.

Each configuration hook is provided with the current `Platibus.Config.PlatibusConfiguration` object built up by the `Platibus.Config.PlatibusConfigurationManager`.  Configuration hooks may modify any aspect of the configuration.  Once all configuration hooks have executed, the resulting configuration object is then used to initialize the bus. 

### Programmatic Configuration

The `Platibus.Bus` class takes a single argument of type `Platibus.Config.IPlatibusConfiguration` that specifies all of the configurable aspects of the bus.  The `Platibus.Config.PlatibusConfiguration` class is a concrete mutable implementation of this class that can be used to build of the bus configuration programmatically.  

(Note: the `Platibus.Bus` class defensively copies configuration from the supplied `Platibus.IPlatibusConfiguration` to avoid side effects if the configuration changes at runtime.  In other words, a bus instance cannot be reconfigured once it is initialized.  If reconfiguration is needed, then the old bus instance should be disposed and a new one should be created.)

The `Platibus.Config.PlatibusConfiguration` object exposes the following configurable features:

### Base URI

The base URI for the hosted platibus instance.  This is the value that will be sent in the `Platibus-Origination` header for messages sent from this instance.  URIs for messages, topics, subscribers, and other resources will be based on this URI.

### Serialization Service

An `Platibus.ISerializationService` implementation used to serialize message content.  This service provides `IPlatibus.ISerializer` implementations for various content types.  The default implementation provides support for JSON and XML

### Message Naming Service

An `IPlatibus.IMessageNamingService` implementation used to map the `System.Type` of the message content object onto a value for the `Platibus-MessageName` header.  This is used to identify the appropriate `System.Type` when deserializing message content.  It is also one of the primary means by which messages can be routed to handlers.  The default implementation uses the full name of the `System.Type`.  Other implementations might look for special type attributes or use other conventions to name messages.

### Message Queueing Service

An `IPlatibus.IMessageQueueingService` implementation used to enqueue messages.  The default implementation writes messaages to the filesystem and deletes them after they have been acknowledged.

### Message Journaling Service

An `IPlatibus.IMessageJournalingService` implementation used to record copies of messages that are received, sent, and published.  The default implementation writes messaages to the filesystem in a date-based directory hierarchy.

### Subscription Tracking Service

As `IPlatibus.SubscriptionTrackingService` implementation used to store and retrieve information about remote subscribers.  The default implementation stores subscriber information on the filesystem using a file per topic.

### Topics

Topics are named streams of publications to which consumers may subscribe.  All messages published to a topic will be delivered to subscribers of that topic, regardless of message name, type, etc.  Only topics that have been added to the bus configuration may be subscribed or published to.

### Endpoints

Endpoints are named references to the base URIs of other Platibus servers.  Send and subscription rules reference these names rather than the base URIs themselves to minimize changes needed i.e. when reconfiguring applications for other environments.  (In declarative style configurations XDT transforms can be used to replace the endpoint URIs without having to modify send and subscription rules.)

### Send Rules

Send rules consist of a `Platibus.Config.IMessageSpecification` and an endpoint name.  The former is used to indicate the messages to which the rule applies.  The latter is the name of the endpoint to which the matching messages should be sent.  A message specification can be anything, but typically send rules are based on the message name (see: `Platibus.Config.MessageNamePatternSpecification`).

### Subscription Rules

Subscription rules indicate the remote topics to which the Platibus bus should subscribe.  Each subscription rule indicates an endpoint, a topic, and an optional TTL (time to live) for the subscription.  When the bus is initialized, it will send subscription requests to the appropriate servers.  If a TTL is specified, the subscription requests will be resent well before the TTL expires.  However, if the application goes offline or the subscription rule is removed, the subscription will eventually expire on the remote end.  (This is to prevent eternal publication of messages to subscribers that are no longer listening.)

### Handling Rules

Handling rules specify which handlers should process certain messages by mapping `Platibus.Config.IMessageSpecification`s onto `IPlatibus.IMessageHandler` implementations.  A message specification can be anything, but typically send rules are based on the message name (see: `Platibus.Config.MessageNamePatternSpecification`).

# Usage

## Sending Messages

To send a message to endpoints based on the configured subscription rules, simpy invoke the `IBus.Send` method:

```
var message = new MyMessage 
{
    Text = "Hello, world!"
};
await bus.Send(message);
```

The durability of the message and how the message is serialized can be influenced on each call to ```IBus.Send``` by overriding the default `SendOptions`:

```
var sendOptions = new SendOptions
{
    ContentType = "application/xml", // Use XML serialization instead of JSON
    UseDurableTransport = true       // Queue outbound message to ensure eventual delivery
};
await bus.Send(message, sendOptions);
```

In some scenarios it may be necessary to bypass the configured send rules.  In these cases the desired endpoint or endpoint URI can be overridden:

```
var endpointName = "crm-app";
await bus.Send(message, endpointName);

var endpointUri = new Uri("http://crm-app.example.com/platibus");
await bus.Send(message, endpointUri);
```

### Replying to a message

TODO

### Observing Replies

If a reply is expected, then then the `ISentMessage` that is returned can be used to observe replies:

```
var sentMessage = bus.Send(message);
var observableReplies = sentMessage.ObserveReplies();
observable.Subscribe(
    reply => Console.WriteLine("Reply received: " + reply), 
    exception => Console.WriteLine("Uh oh!" + exception));
```

Extension method are supplied to provided to block execution until a reply is received:

```
var sentMessage = bus.Send(message);
var timeout = TimeSpan.FromSeconds(30);
var reply = await sentMessage.GetReply(timeout);
```

## Publishing Messages

TODO

## Handling Messages

TODO
