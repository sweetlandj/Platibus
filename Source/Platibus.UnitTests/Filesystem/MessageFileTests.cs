﻿using System;
using System.IO;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;
using NUnit.Framework;
using Platibus.Filesystem;
using Platibus.Security;

#pragma warning disable 618
namespace Platibus.UnitTests.Filesystem
{
    public class MessageFileTests
    {
        protected IPrincipal Principal;
        protected Message Message;
        protected FileInfo MessageFileInfo;

        protected MessageFile MessageFile;

        [Test]
        public async Task LegacyMessageFileWithSenderPrincipalCanBeRead()
        {
            await GivenLegacyMessageFileWithSenderPrincipal();
            WhenReadingTheMessageFileContent();
            await ThenTheSenderPrincipalShouldBeReadSuccessfully();
            await ThenTheMessageShouldBeReadSuccessfully();
            await ThenTheMessageShouldHaveASecurityTokenHeader();
        }

        [Test]
        public async Task LegacyMessageFileWithNoSenderPrincipalCanBeRead()
        {
            await GivenLegacyMessageFileWithNoPrincipal();
            WhenReadingTheMessageFileContent();
            await ThenThePrincipalShouldBeNull();
            await ThenTheMessageShouldBeReadSuccessfully();
            await ThenTheMessageShouldNotHaveASecurityTokenHeader();
        }

        [Test]
        public async Task MessageFileWithSecurityTokenCanBeRead()
        {
            await GivenMessageFileWithClaimsPrincipal();
            WhenReadingTheMessageFileContent();
            await ThenTheClaimsPrincipalShouldBeReadSuccessfully();
            await ThenTheMessageShouldBeReadSuccessfully();
            await ThenTheMessageShouldHaveASecurityTokenHeader();
        }

        [Test]
        public async Task MessageFileWithNoPrincipalCanBeRead()
        {
            await GivenMessageFileWithNoPrincipal();
            WhenReadingTheMessageFileContent();
            await ThenThePrincipalShouldBeNull();
            await ThenTheMessageShouldBeReadSuccessfully();
            await ThenTheMessageShouldNotHaveASecurityTokenHeader();
        }

        protected IPrincipal GivenLegacySenderPrincipal()
        {
            var identity = new GenericIdentity("test@example.com", "basic");
            var principal = new GenericPrincipal(identity, new [] {"user", "staff"});
            return Principal = new SenderPrincipal(principal);
        }

        protected IPrincipal GivenClaimsPrincipal()
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.Name, "test@example.com"),
                new Claim(ClaimTypes.Role, "user"),
                new Claim(ClaimTypes.Role, "staff"),
            };
            return Principal = new ClaimsPrincipal(new ClaimsIdentity(claims));
        }

        protected IPrincipal GivenNoPrincipal()
        {
            return Principal = null;
        }

        protected Message GivenSampleSentMessage()
        {
            var headers = new MessageHeaders
            {
                MessageId = MessageId.Generate(),
                Origination = new Uri("http://localhost/platibus0"),
                Destination = new Uri("http://localhost/platibus1"),
                Importance = MessageImportance.High,
                ContentType = "text/plain",
                Expires = DateTime.UtcNow.AddMinutes(5),
                MessageName = "http://example.com/ns/test",
                Sent = DateTime.UtcNow
            };

            return Message = new Message(headers, "Hello, world!");
        }

        protected async Task<FileInfo> GivenLegacyMessageFileWithSenderPrincipal()
        {
            GivenLegacySenderPrincipal();
            GivenSampleSentMessage();

            var tempFile = Path.GetTempFileName();
            using (var writer = new LegacyMessageFileWriter(File.OpenWrite(tempFile)))
            {
                await writer.WritePrincipal(Principal);
                await writer.WriteMessage(Message);
            }
            return MessageFileInfo = new FileInfo(tempFile);
        }

        protected async Task<FileInfo> GivenLegacyMessageFileWithNoPrincipal()
        {
            GivenNoPrincipal();
            GivenSampleSentMessage();

            var tempFile = Path.GetTempFileName();
            using (var writer = new LegacyMessageFileWriter(File.OpenWrite(tempFile)))
            {
                await writer.WritePrincipal(Principal);
                await writer.WriteMessage(Message);
            }
            return MessageFileInfo = new FileInfo(tempFile);
        }

        protected async Task<FileInfo> GivenMessageFileWithClaimsPrincipal()
        {
            GivenClaimsPrincipal();
            GivenSampleSentMessage();

            var directory = new DirectoryInfo(Path.GetTempPath());
            var messageFile = await MessageFile.Create(directory, Message, Principal);
            return MessageFileInfo = messageFile.File;
        }

        protected async Task<FileInfo> GivenMessageFileWithNoPrincipal()
        {
            GivenNoPrincipal();
            GivenSampleSentMessage();

            var directory = new DirectoryInfo(Path.GetTempPath());
            var messageFile = await MessageFile.Create(directory, Message, Principal);
            return MessageFileInfo = messageFile.File;
        }
        
        protected MessageFile WhenReadingTheMessageFileContent()
        {
            if (MessageFileInfo == null) Assert.Inconclusive();
            return MessageFile = new MessageFile(MessageFileInfo);
        }

        protected async Task ThenTheSenderPrincipalShouldBeReadSuccessfully()
        {
            if (MessageFile == null) Assert.Inconclusive();
            var principal = await MessageFile.ReadPrincipal();
            var principalAsSenderPrincipal = SenderPrincipal.From(principal);
            Assert.That(principalAsSenderPrincipal, Is.EqualTo(Principal));
        }

        protected async Task ThenTheClaimsPrincipalShouldBeReadSuccessfully()
        {
            if (MessageFile == null) Assert.Inconclusive();
            var principal = await MessageFile.ReadPrincipal();

            Assert.NotNull(principal);

            Assert.That(principal.IsInRole("user"));
            Assert.That(principal.IsInRole("staff"));

            var readIdentity = principal.Identity;
            Assert.NotNull(readIdentity);
            Assert.AreEqual(readIdentity.Name, Principal.Identity.Name);
        }

        protected async Task ThenThePrincipalShouldBeNull()
        {
            if (MessageFile == null) Assert.Inconclusive();
            var principal = await MessageFile.ReadPrincipal();
            Assert.That(principal, Is.Null);
        }

        protected async Task ThenTheMessageShouldBeReadSuccessfully()
        {
            if (MessageFile == null) Assert.Inconclusive();
            var message = await MessageFile.ReadMessage();
            Assert.That(message, Is.EqualTo(Message).Using(new MessageEqualityComparer(HeaderName.SecurityToken)));
        }

        protected async Task ThenTheMessageShouldHaveASecurityTokenHeader()
        {
            if (MessageFile == null) Assert.Inconclusive();
            var message = await MessageFile.ReadMessage();
            Assert.That(message.Headers.SecurityToken, Is.Not.Null);
            Assert.That(message.Headers.SecurityToken, Has.Length.GreaterThan(0));
        }

        protected async Task ThenTheMessageShouldNotHaveASecurityTokenHeader()
        {
            if (MessageFile == null) Assert.Inconclusive();
            var message = await MessageFile.ReadMessage();
            Assert.That(message.Headers.SecurityToken, Is.Null);
        }
    }
}