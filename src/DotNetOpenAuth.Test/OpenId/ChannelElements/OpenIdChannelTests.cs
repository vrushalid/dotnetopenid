﻿//-----------------------------------------------------------------------
// <copyright file="OpenIdChannelTests.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Test.OpenId.ChannelElements {
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	using System.Net;
	using System.Text;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.Messaging.Bindings;
	using DotNetOpenAuth.Messaging.Reflection;
	using DotNetOpenAuth.OpenId;
	using DotNetOpenAuth.OpenId.ChannelElements;
	using DotNetOpenAuth.OpenId.RelyingParty;
	using DotNetOpenAuth.Test.Mocks;
	using Microsoft.VisualStudio.TestTools.UnitTesting;

	[TestClass]
	public class OpenIdChannelTests : TestBase {
		private static readonly TimeSpan maximumMessageAge = TimeSpan.FromHours(3); // good for tests, too long for production
		private OpenIdChannel channel;
		private OpenIdChannel_Accessor accessor;
		private Mocks.TestWebRequestHandler webHandler;

		[TestInitialize]
		public void Setup() {
			this.webHandler = new Mocks.TestWebRequestHandler();
			this.channel = new OpenIdChannel(new AssociationMemoryStore<Uri>(), new NonceMemoryStore(maximumMessageAge), new RelyingPartySecuritySettings());
			this.accessor = OpenIdChannel_Accessor.AttachShadow(this.channel);
			this.channel.WebRequestHandler = this.webHandler;
		}

		[TestMethod]
		public void Ctor() {
			// Verify that the channel stack includes the expected types.
			// While other binding elements may be substituted for these, we'd then have
			// to test them.  Since we're not testing them in the OpenID battery of tests,
			// we make sure they are the standard ones so that we trust they are tested
			// elsewhere by the testing library.
			var replayElement = (StandardReplayProtectionBindingElement)this.channel.BindingElements.SingleOrDefault(el => el is StandardReplayProtectionBindingElement);
			Assert.IsTrue(this.channel.BindingElements.Any(el => el is StandardExpirationBindingElement));
			Assert.IsNotNull(replayElement);

			// Verify that empty nonces are allowed, since OpenID 2.0 allows this.
			Assert.IsTrue(replayElement.AllowZeroLengthNonce);
		}

		/// <summary>
		/// Verifies that the channel sends direct message requests as HTTP POST requests.
		/// </summary>
		[TestMethod]
		public void DirectRequestsUsePost() {
			IDirectedProtocolMessage requestMessage = new Mocks.TestDirectedMessage(MessageTransport.Direct) {
				Recipient = new Uri("http://host"),
				Name = "Andrew",
			};
			HttpWebRequest httpRequest = this.accessor.CreateHttpRequest(requestMessage);
			Assert.AreEqual("POST", httpRequest.Method);
			StringAssert.Contains(this.webHandler.RequestEntityAsString, "Name=Andrew");
		}

		/// <summary>
		/// Verifies that direct response messages are encoded using Key Value Form
		/// per OpenID 2.0 section 5.1.2.
		/// </summary>
		/// <remarks>
		/// The validity of the actual KVF encoding is not checked here.  We assume that the KVF encoding
		/// class is verified elsewhere.  We're only checking that the KVF class is being used by the 
		/// <see cref="OpenIdChannel.SendDirectMessageResponse"/> method.
		/// </remarks>
		[TestMethod]
		public void DirectResponsesSentUsingKeyValueForm() {
			IProtocolMessage message = MessagingTestBase.GetStandardTestMessage(MessagingTestBase.FieldFill.AllRequired);
			MessageDictionary messageFields = this.MessageDescriptions.GetAccessor(message);
			byte[] expectedBytes = KeyValueFormEncoding.GetBytes(messageFields);
			string expectedContentType = OpenIdChannel_Accessor.KeyValueFormContentType;

			OutgoingWebResponse directResponse = this.accessor.PrepareDirectResponse(message);
			Assert.AreEqual(expectedContentType, directResponse.Headers[HttpResponseHeader.ContentType]);
			byte[] actualBytes = new byte[directResponse.ResponseStream.Length];
			directResponse.ResponseStream.Read(actualBytes, 0, actualBytes.Length);
			Assert.IsTrue(MessagingUtilities.AreEquivalent(expectedBytes, actualBytes));
		}

		/// <summary>
		/// Verifies that direct message responses are read in using the Key Value Form decoder.
		/// </summary>
		[TestMethod]
		public void DirectResponsesReceivedAsKeyValueForm() {
			var fields = new Dictionary<string, string> {
				{ "var1", "value1" },
				{ "var2", "value2" },
			};
			var response = new CachedDirectWebResponse {
				CachedResponseStream = new MemoryStream(KeyValueFormEncoding.GetBytes(fields)),
			};
			Assert.IsTrue(MessagingUtilities.AreEquivalent(fields, this.accessor.ReadFromResponseCore(response)));
		}

		/// <summary>
		/// Verifies that messages asking for special HTTP status codes get them.
		/// </summary>
		[TestMethod]
		public void SendDirectMessageResponseHonorsHttpStatusCodes() {
			IProtocolMessage message = MessagingTestBase.GetStandardTestMessage(MessagingTestBase.FieldFill.AllRequired);
			OutgoingWebResponse directResponse = this.accessor.PrepareDirectResponse(message);
			Assert.AreEqual(HttpStatusCode.OK, directResponse.Status);

			var httpMessage = new TestDirectResponseMessageWithHttpStatus();
			MessagingTestBase.GetStandardTestMessage(MessagingTestBase.FieldFill.AllRequired, httpMessage);
			httpMessage.HttpStatusCode = HttpStatusCode.NotAcceptable;
			directResponse = this.accessor.PrepareDirectResponse(httpMessage);
			Assert.AreEqual(HttpStatusCode.NotAcceptable, directResponse.Status);
		}
	}
}
