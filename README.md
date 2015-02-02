# Pluribus
Pluribus is a decentralized asynchronous messaging framework designed to be platform and language agnostic.  The goal of this project is to make durable asynchronous transmission and processing of messages as simple as possible.

# Overview

Pluribus currently supports an HTTP transport layer in which message resources are POSTed to remote servers.  The sender is responsible for communicating directly with the remote server.  Sender-side queuing is optional; the sender can choose to send the message directly, in which case a failure is immediately reported.

Publications and subscriptions are also implemented with HTTP.  Subscribers can POST or DELETE subscription requests to subscribe or unsubscribe to a topic.  When a message is published, publishers then POST a copy of the message to all subscribers like any other message.

Queuing and subscription tracking are currently implemented via files on the fileystem.  The file formats are text-based and are designed easy to read and edit, if necessary.

Pluribus applications can currently be hosted either in IIS (using the PluribusHttpHandler in Pluribus.IIS package) or with the standalone HTTP server provided in the main package.
