# Pluribus
Pluribus is a decentralized asynchronous messaging framework designed to be platform and language agnostic.  The goal of this project is to make durable asynchronous transmission and processing of messages as simple as possible.

# Overview

Pluribus currently supports an HTTP transport layer in which senders POST message resources directly to their intended destinations.  Replies are simply the same thing in reverse; the recipient POSTs a message resource back to the sender.  Queuing on either the sending or receiving side is optional; durable transport can be requested when sending each message.  (One of the goals of this project is to enable senders to receive immediate feedback on the success or failure of the message delivery.) 

Pub/sub is also implemented via direct HTTP requests.  Subscribers can POST or DELETE subscription requests to subscribe or unsubscribe to a topic.  When a message is published, publishers then POST a copy of the message to all subscribers like any other message.

Queuing and subscription tracking are currently implemented via files on the fileystem.  The file formats are text-based and are designed easy to read and edit, if necessary.

# Configuration

Pluribus can be configured programmatically, declaratively, or with a mixture of both styles.

## Programmatic Configuration

The `Pluribus.Bus` class takes a single argument of type `Pluribus.Config.IPluribusConfiguration` that specifies all of the configurable aspects of the bus.  The `Pluribus.Config.PluribusConfiguration` class is a concrete mutable implementation of this class that can be used to build of the bus configuration programmatically.  

(Note: the `Pluribus.Bus` class defensively copies configuration from the supplied `Pluribus.IPluribusConfiguration` to avoid side effects if the configuration changes at runtime.  In other words, a bus instance cannot be reconfigured once it is initialized.  If reconfiguration is needed, then the old bus instance should be disposed and a new one should be created.)

The `Pluribus.Config.PluribusConfiguration` object exposes the following configurable features:

### Base URI
The base URI for the hosted pluribus instance.  This is the value that will be sent in the `Pluribus-Origination` header for messages sent from this instance.  URIs for messages, topics, subscribers, and other resources will be based on this URI.

### Serialization Service
An `Pluribus.ISerializationService` implementation used to serialize message content.  This service provides `IPluribus.ISerializer` implementations for various content types.  The default implementation provides support for JSON and XML

### Message Naming Service
An `IPluribus.IMessageNamingService` implementation used to map the `System.Type` of the message content object onto a value for the `Pluribus-MessageName` header.  This is used to identify the appropriate `System.Type` when deserializing message content.  It is also one of the primary means by which messages can be routed to handlers.  The default implementation uses the full name of the `System.Type`.  Other implementations might look for special type attributes or use other conventions to name messages.

### Message Queueing Service
An `IPluribus.IMessageQueueingService` implementation used to enqueue messages.  The default implementation writes messaages to the filesystem and deletes them after they have been acknowledged.

### Subscription Tracking Service
As `IPluribus.SubscriptionTrackingService` implementation used to store and retrieve information about remote subscribers.  The default implementation stores subscriber information on the filesystem using a file per topic.

### Topics

Topics are named streams of publications to which consumers may subscribe.  All messages published to a topic will be delivered to subscribers of that topic, regardless of message name, type, etc.  Only topics that have been added to the bus configuration may be subscribed or published to.

### Endpoints

Endpoints are name references the base URI of another Pluribus server.  Send and subscription rules reference these names rather than the base URIs themselves to minimize changes needed i.e. when reconfiguring applications for other environments.  (In declarative style configurations XDT transforms can be used to replace the endpoint URIs without having to modify send and subscription rules.)

### Send Rules

Send rules consist of a `Pluribus.Config.IMessageSpecification` and an endpoint name.  The former is used to indicate the messages to which the rule applies.  The latter is the name of the endpoint to which the matching messages should be sent.  A message specification can be anything, but typically send rules are based on the message name (see: `Pluribus.Config.MessageNamePatternSpecification`).

### Subscription Rules

Subscription rules indicate the remote topics to which the Pluribus bus should subscribe.  Each subscription rule indicates an endpoint, a topic, and an optional TTL (time to live) for the subscription.  When the bus is initialized, it will send subscription requests to the appropriate servers.  If a TTL is specified, the subscription requests will be resent well before the TTL expires.  However, if the application goes offline or the subscription rule is removed, the subscription will eventually expire on the remote end.  (This is to prevent eternal publication of messages to subscribers that are no longer listening.)

### Handling Rules

Handling rules specify which handlers should process certain messages by mapping `Pluribus.Config.IMessageSpecification`s onto `IPluribus.IMessageHandler` implementations.  A message specification can be anything, but typically send rules are based on the message name (see: `Pluribus.Config.MessageNamePatternSpecification`).

## Declarative Configuration

The `Pluribus.Config.PluribusConfigurationSection` class provided in the main `Pluribus` package can be used to declaratively configure all features except handling rules.  (Handling rules involve strong references to caller code and must be added via Configuration Hooks.  See below.)  This is particularly useful when an application is deployed to multiple environments in which endpoint URIs might vary; configuration transforms can be used to replace portions of the configuration section when building for different configurations. 

Example configuration:

```
<configSections>
  <section name="pluribus" type="Pluribus.Config.PluribusConfigurationSection, Pluribus" />
</configSections>

<pluribus baseUri="http://localhost:52180/pluribus/">
  <timeouts replyTimeout="00:00:30" />
  <queueing provider="Filesystem" path="pluribus\queues" />
  <journaling provider="Filesystem" path="pluribus\journal" />
  <subscriptionTracking provider="Filesystem" path="pluribus\subscriptions" />
  <endpoints>
    <add name="pluribus" address="http://localhost:52180/pluribus/" />
  </endpoints>
  <topics>
    <add name="Topic0" />
  </topics>
  <sendRules>
    <add namePattern=".*" endpoint="pluribus" />
  </sendRules>
  <subscriptions>
    <add endpoint="pluribus" topic="Topic0" />
  </subscriptions>
</pluribus>
```

### `<pluribus>`

This is the top level configuration element.  The `baseUri` attribute is required and specifies the base URI for this instance.  All nested elements are optional.

#### `<timeouts>`

Specifies the various timeouts for the application.  The `replyTimeout` specifies the maximum amount of time a reply observer will be held open waiting for correlated messages to arrive.

#### `<queueing>`

Overrides the default queueing configuration.  The value of the `provider` attribute indicates the `IMessageQueueingServiceProvider` implementation that will provide the `IMessageQueueingService` implementation (by assembly qualified name or the name specified in the `ProviderAttribute` on the concrete type).  Additional provider-specific attributes (such as `path`) can also be specified on this element to influence how the `IMessageQueueingService` is initialized.

#### `<journaling>`

Overrides the default journaling configuration.  The value of the `provider` attribute indicates the `IMessageJournalingServiceProvider` implementation that will provide the `IMessageJournalingService` implementation (by assembly qualified name or the name specified in the `ProviderAttribute` on the concrete type).  Additional provider-specific attributes (such as `path`) can also be specified on this element to influence how the `IMessageJournalingService` is initialized.

#### `<subscriptionTracking>`

Overrides the default subscription tracking configuration.  The value of the `provider` attribute indicates the `ISubscriptionTrackingServiceProvider` implementation that will provide the `ISubscriptionTrackingService` implementation (by assembly qualified name or the name specified in the `ProviderAttribute` on the concrete type).  Additional provider-specific attributes (such as `path`) can also be specified on this element to influence how the `ISubscriptionTrackingService` is initialized.

#### `<endpoints>`

A collection of endpoint configuration elements.  Each endpoint requires a `name` and `address` (URI).

#### `<sendRules>`

A collection of send rule configuration elements.  Each rule requires a `namePattern` and the name of the `endpoint` to which matching messages will be sent.  The `namePattern` is a regular expression used to match message names.

#### `<subscriptionRules>`

A collection of subscription rule configuration elements.  Each rule requires an `endpoint` name and the name of the `topic` at that endpoint being subscribed to.  A `ttl` (time to live) may also be specified to limit how long the subscription should remain on the remote server after the last renewal.

## Configuration Hooks

When using declarative stule configuration, handling rules must be added using an `Pluribus.Config.IConfigurationHook` implementation.  Pluribus will automatically scan assemblies in the app domain base directory for implementations of `Pluribus.Config.IConfigurationHook` and invoke each of them.  The order in which they are invoked is not specified and should not be counted upon.  Best practice is to use a single configuration hook that calls out to other modules as needed.

Each configuration hook is provided with the current `Pluribus.Config.PluribusConfiguration` object built up by the `Pluribus.Config.PluribusConfigurationManager`.  Configuration hooks may modify any aspect of the configuration.  Once all configuration hooks have executed, the resulting configuration object is then used to initialize the bus. 

## Configuration Manager and Bootstrapping

A static `InitBus` method is provided in the `Pluribus.Config.Bootstrapper` class to initialize a bus instance using any declarative configuration elements and configuration hooks that are available in the current app domain.  This method uses the `Pluribus.Config.PluribusConfigurationManager` class to load the configuration section and map it onto a `Pluribus.Config.PluribusConfiguration` object.  These two classes are available for use if more control is needed when configuring and initializing a bus instance.

# Hosting

TODO

## IIS HTTP Handler

TODO

## Standalone HTTP Server

TODO

# Usage

TODO

## Sending Messages

TODO

### Observing Replies

TODO

## Publishing Messages

TODO

## Handling Messages

TODO
