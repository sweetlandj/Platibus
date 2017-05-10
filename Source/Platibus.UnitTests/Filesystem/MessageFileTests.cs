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
        }

        [Test]
        public async Task LegacyMessageFileWithNoSenderPrincipalCanBeRead()
        {
            await GivenLegacyMessageFileWithNoPrincipal();
            WhenReadingTheMessageFileContent();
            await ThenThePrincipalShouldBeNull();
            await ThenTheMessageShouldBeReadSuccessfully();
        }

        [Test]
        public async Task MessageFileWithSecurityTokenCanBeRead()
        {
            await GivenMessageFileWithClaimsPrincipal();
            WhenReadingTheMessageFileContent();
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

            var securityToken = await new JwtSecurityTokenService()
                .Issue(Principal);

            var messageWithSecurityToken = Message.WithSecurityToken(securityToken);

            var directory = new DirectoryInfo(Path.GetTempPath());
            var messageFile = await MessageFile.Create(directory, messageWithSecurityToken);
            return MessageFileInfo = messageFile.File;
        }

        protected async Task<FileInfo> GivenMessageFileWithNoPrincipal()
        {
            GivenNoPrincipal();
            GivenSampleSentMessage();

            var directory = new DirectoryInfo(Path.GetTempPath());
            var messageFile = await MessageFile.Create(directory, Message);
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
