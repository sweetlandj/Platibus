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
using Xunit;
using Platibus.Filesystem;
using Platibus.Security;

#pragma warning disable 618
namespace Platibus.UnitTests.Filesystem
{
    [Trait("Category", "UnitTests")]
    public class MessageFileTests
    {
        protected IPrincipal Principal;
        protected Message Message;
        protected FileInfo MessageFileInfo;

        protected MessageFile MessageFile;

       [Fact]
        public async Task LegacyMessageFileWithSenderPrincipalCanBeRead()
        {
            await GivenLegacyMessageFileWithSenderPrincipal();
            WhenReadingTheMessageFileContent();
            await ThenTheSenderPrincipalShouldBeReadSuccessfully();
            await ThenTheMessageShouldBeReadSuccessfully();
        }

       [Fact]
        public async Task LegacyMessageFileWithNoSenderPrincipalCanBeRead()
        {
            await GivenLegacyMessageFileWithNoPrincipal();
            WhenReadingTheMessageFileContent();
            await ThenThePrincipalShouldBeNull();
            await ThenTheMessageShouldBeReadSuccessfully();
        }

       [Fact]
        public async Task MessageFileWithSecurityTokenCanBeRead()
        {
            await GivenMessageFileWithClaimsPrincipal();
            WhenReadingTheMessageFileContent();
            await ThenTheMessageShouldBeReadSuccessfully();
            await ThenTheMessageShouldHaveASecurityTokenHeader();
        }

       [Fact]
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
            Assert.NotNull(MessageFileInfo);
            return MessageFile = new MessageFile(MessageFileInfo);
        }

        protected async Task ThenTheSenderPrincipalShouldBeReadSuccessfully()
        {
            Assert.NotNull(MessageFileInfo);
            var principal = await MessageFile.ReadPrincipal();
            var principalAsSenderPrincipal = SenderPrincipal.From(principal);
            Assert.Equal(Principal, principalAsSenderPrincipal);
        }

        protected async Task ThenTheClaimsPrincipalShouldBeReadSuccessfully()
        {
            Assert.NotNull(MessageFile);
            var principal = await MessageFile.ReadPrincipal();

            Assert.NotNull(principal);

            Assert.True(principal.IsInRole("user"));
            Assert.True(principal.IsInRole("staff"));

            var readIdentity = principal.Identity;
            Assert.NotNull(readIdentity);
            Assert.Equal(Principal.Identity.Name, readIdentity.Name);
        }

        protected async Task ThenThePrincipalShouldBeNull()
        {
            Assert.NotNull(MessageFile);
            var principal = await MessageFile.ReadPrincipal();
            Assert.Null(principal);
        }

        protected async Task ThenTheMessageShouldBeReadSuccessfully()
        {
            Assert.NotNull(MessageFile);
            var message = await MessageFile.ReadMessage();
            Assert.Equal(Message, message, new MessageEqualityComparer(HeaderName.SecurityToken));
        }

        protected async Task ThenTheMessageShouldHaveASecurityTokenHeader()
        {
            Assert.NotNull(MessageFile);
            var message = await MessageFile.ReadMessage();
            Assert.NotNull(message.Headers.SecurityToken);
            Assert.True(message.Headers.SecurityToken.Length > 0);
        }

        protected async Task ThenTheMessageShouldNotHaveASecurityTokenHeader()
        {
            Assert.NotNull(MessageFile);
            var message = await MessageFile.ReadMessage();
            Assert.Null(message.Headers.SecurityToken);
        }
    }
}
