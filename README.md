# Pluribus
Pluribus is a decentralized asynchronous messaging framework designed to be platform and language agnostic.  The goal of this project is to make durable asynchronous transmission and processing of messages as simple as possible.

# Overview

Pluribus currently supports an HTTP transport layer in which message resources are POSTed to remote servers.  The sender is responsible for communicating directly with the remote server.  Sender-side queuing is optional; the sender can choose to send the message directly, in which case a failure is immediately reported.

Publications and subscriptions are also implemented with HTTP.  Subscribers can POST or DELETE subscription requests to subscribe or unsubscribe to a topic.  When a message is published, publishers then POST a copy of the message to all subscribers like any other message.

Queuing and subscription tracking are currently implemented via files on the fileystem.  The file formats are text-based and are designed easy to read and edit, if necessary.

# Configuration

Pluribus can be configured programmatically, declaratively, or with a mixture of both styles.

## Programmatic Configuration

The ```Pluribus.Bus``` class takes a single argument of type ```Pluribus.Config.IPluribusConfiguration``` that specifies all of the configurable aspects of the bus.  The ```Pluribus.Config.PluribusConfiguration``` class is a concrete mutable implementation of this class that can be used to build of the bus configuration programmatically.  

(Note: the ```Pluribus.Bus``` class defensively copies configuration from the supplied ```Pluribus.IPluribusConfiguration``` to avoid side effects if the configuration changes at runtime.  In other words, a bus instance cannot be reconfigured once it is initialized.  If reconfiguration is needed, then the old bus instance should be disposed and a new one should be created.)

The ```Pluribus.Config.PluribusConfiguration``` object exposes the following configurable features:

### Base URI
The base URI for the hosted pluribus instance.  This is the value that will be sent in the ```Pluribus-Origination``` header for messages sent from this instance.  URIs for messages, topics, subscribers, and other resources will be based on this URI.

### Serialization Service
An ```Pluribus.ISerializationService``` implementation used to serialize message content.  This service provides ```IPluribus.ISerializer``` implementations for various content types.  The default implementation provides support for JSON and XML

### Message Naming Service
An ```IPluribus.IMessageNamingService``` implementation used to map the ```System.Type``` of the message content object onto a value for the ```Pluribus-MessageName``` header.  This is used to identify the appropriate ```System.Type``` when deserializing message content.  It is also one of the primary means by which messages can be routed to handlers.  The default implementation uses the full name of the ```System.Type```.  Other implementations might look for special type attributes or use other conventions to name messages.

### Message Queueing Service
An ```IPluribus.IMessageQueueingService``` implementation used to enqueue messages.  The default implementation writes messaages to the filesystem and deletes them after they have been acknowledged.

### Subscription Tracking Service
As ```IPluribus.SubscriptionTrackingService``` implementation used to store and retrieve information about remote subscribers.  The default implementation stores subscriber information on the filesystem using a file per topic.

### Topics

Topics are named streams of publications to which consumers may subscribe.  All messages published to a topic will be delivered to subscribers of that topic, regardless of message name, type, etc.  Only topics that have been added to the bus configuration may be subscribed or published to.

### Endpoints

Endpoints are name references the base URI of another Pluribus server.  Send and subscription rules reference these names rather than the base URIs themselves to minimize changes needed i.e. when reconfiguring applications for other environments.  (In declarative style configurations XDT transforms can be used to replace the endpoint URIs without having to modify send and subscription rules.)

### Send Rules

Send rules consist of a ```Pluribus.Config.IMessageSpecification``` and an endpoint name.  The former is used to indicate the messages to which the rule applies.  The latter is the name of the endpoint to which the matching messages should be sent.  A message specification can be anything, but typically send rules are based on the message name (see: ```Pluribus.Config.MessageNamePatternSpecification```).

### Subscription Rules

Subscription rules indicate the remote topics to which the Pluribus bus should subscribe.  Each subscription rule indicates an endpoint, a topic, and an optional TTL (time to live) for the subscription.  When the bus is initialized, it will send subscription requests to the appropriate servers.  If a TTL is specified, the subscription requests will be resent well before the TTL expires.  However, if the application goes offline or the subscription rule is removed, the subscription will eventually expire on the remote end.  (This is to prevent eternal publication of messages to subscribers that are no longer listening.)

### Handling Rules

Handling rules specify which handlers should process certain messages by mapping ```Pluribus.Config.IMessageSpecification```s onto ```IPluribus.IMessageHandler``` implementations.  A message specification can be anything, but typically send rules are based on the message name (see: ```Pluribus.Config.MessageNamePatternSpecification```).

## Declarative Configuration

TODO

### Configuration Hooks

TODO

# Hosting

TODO

## IIS HTTP Handler

TODO

## Standalone HTTP Server

TODO
