﻿//-----------------------------------------------------------------------
// <copyright file="GoogleConsumer.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.ApplicationBlock {
	using System;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.IO;
	using System.Linq;
	using System.Net;
	using System.Text;
	using System.Text.RegularExpressions;
	using System.Xml;
	using System.Xml.Linq;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OAuth;
	using DotNetOpenAuth.OAuth.ChannelElements;

	/// <summary>
	/// A consumer capable of communicating with Google Data APIs.
	/// </summary>
	public static class GoogleConsumer {
		/// <summary>
		/// The Consumer to use for accessing Google data APIs.
		/// </summary>
		private static readonly ServiceProviderDescription GoogleDescription = new ServiceProviderDescription {
			RequestTokenEndpoint = new MessageReceivingEndpoint("https://www.google.com/accounts/OAuthGetRequestToken", HttpDeliveryMethods.AuthorizationHeaderRequest | HttpDeliveryMethods.GetRequest),
			UserAuthorizationEndpoint = new MessageReceivingEndpoint("https://www.google.com/accounts/OAuthAuthorizeToken", HttpDeliveryMethods.AuthorizationHeaderRequest | HttpDeliveryMethods.GetRequest),
			AccessTokenEndpoint = new MessageReceivingEndpoint("https://www.google.com/accounts/OAuthGetAccessToken", HttpDeliveryMethods.AuthorizationHeaderRequest | HttpDeliveryMethods.GetRequest),
			TamperProtectionElements = new ITamperProtectionChannelBindingElement[] { new HmacSha1SigningBindingElement() },
		};

		/// <summary>
		/// A mapping between Google's applications and their URI scope values.
		/// </summary>
		private static readonly Dictionary<Applications, string> DataScopeUris = new Dictionary<Applications, string> {
			{ Applications.Contacts, "http://www.google.com/m8/feeds/" },
			{ Applications.Calendar, "http://www.google.com/calendar/feeds/" },
			{ Applications.Blogger, "http://www.blogger.com/feeds" },
		};

		/// <summary>
		/// The URI to get contacts once authorization is granted.
		/// </summary>
		private static readonly MessageReceivingEndpoint GetContactsEndpoint = new MessageReceivingEndpoint("http://www.google.com/m8/feeds/contacts/default/full/", HttpDeliveryMethods.GetRequest);

		/// <summary>
		/// The many specific authorization scopes Google offers.
		/// </summary>
		[Flags]
		public enum Applications : long {
			/// <summary>
			/// The Gmail address book.
			/// </summary>
			Contacts = 0x1,

			/// <summary>
			/// Appointments in Google Calendar.
			/// </summary>
			Calendar = 0x2,

			/// <summary>
			/// Blog post authoring.
			/// </summary>
			Blogger = 0x4,
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="WebConsumer"/> class that is prepared to communicate with Google.
		/// </summary>
		/// <param name="tokenManager">The token manager.</param>
		/// <param name="consumerKey">The consumer key.</param>
		/// <returns>The newly instantiated <see cref="WebConsumer"/>.</returns>
		public static WebConsumer CreateWebConsumer(ITokenManager tokenManager, string consumerKey) {
			return new WebConsumer(GoogleDescription, tokenManager) {
				ConsumerKey = consumerKey,
			};
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="DesktopConsumer"/> class that is prepared to communicate with Google.
		/// </summary>
		/// <param name="tokenManager">The token manager.</param>
		/// <param name="consumerKey">The consumer key.</param>
		/// <returns>The newly instantiated <see cref="DesktopConsumer"/>.</returns>
		public static DesktopConsumer CreateDesktopConsumer(ITokenManager tokenManager, string consumerKey) {
			return new DesktopConsumer(GoogleDescription, tokenManager) {
				ConsumerKey = consumerKey,
			};
		}

		/// <summary>
		/// Requests authorization from Google to access data from a set of Google applications.
		/// </summary>
		/// <param name="consumer">The Google consumer previously constructed using <see cref="CreateWebConsumer"/> or <see cref="CreateDesktopConsumer"/>.</param>
		/// <param name="requestedAccessScope">The requested access scope.</param>
		public static void RequestAuthorization(WebConsumer consumer, Applications requestedAccessScope) {
			if (consumer == null) {
				throw new ArgumentNullException("consumer");
			}

			var extraParameters = new Dictionary<string, string> {
				{ "scope", GetScopeUri(requestedAccessScope) },
			};
			Uri callback = Util.GetCallbackUrlFromContext();
			var request = consumer.PrepareRequestUserAuthorization(callback, extraParameters, null);
			consumer.Channel.Send(request);
		}

		/// <summary>
		/// Requests authorization from Google to access data from a set of Google applications.
		/// </summary>
		/// <param name="consumer">The Google consumer previously constructed using <see cref="CreateWebConsumer"/> or <see cref="CreateDesktopConsumer"/>.</param>
		/// <param name="requestedAccessScope">The requested access scope.</param>
		/// <param name="requestToken">The unauthorized request token assigned by Google.</param>
		/// <returns>The request token</returns>
		public static Uri RequestAuthorization(DesktopConsumer consumer, Applications requestedAccessScope, out string requestToken) {
			if (consumer == null) {
				throw new ArgumentNullException("consumer");
			}

			var extraParameters = new Dictionary<string, string> {
				{ "scope", GetScopeUri(requestedAccessScope) },
			};

			return consumer.RequestUserAuthorization(extraParameters, null, out requestToken);
		}

		/// <summary>
		/// Gets the Gmail address book's contents.
		/// </summary>
		/// <param name="consumer">The Google consumer previously constructed using <see cref="CreateWebConsumer"/> or <see cref="CreateDesktopConsumer"/>.</param>
		/// <param name="accessToken">The access token previously retrieved.</param>
		/// <returns>An XML document returned by Google.</returns>
		public static XDocument GetContacts(ConsumerBase consumer, string accessToken) {
			if (consumer == null) {
				throw new ArgumentNullException("consumer");
			}

			var response = consumer.PrepareAuthorizedRequestAndSend(GetContactsEndpoint, accessToken);
			string body = response.GetResponseReader().ReadToEnd();
			XDocument result = XDocument.Parse(body);
			return result;
		}

		public static void PostBlogEntry(ConsumerBase consumer, string accessToken, string blogUrl, string title, XElement body) {
			string feedUrl;
			var getBlogHome = WebRequest.Create(blogUrl);
			using (var blogHomeResponse = getBlogHome.GetResponse()) {
				using (StreamReader sr = new StreamReader(blogHomeResponse.GetResponseStream())) {
					string homePageHtml = sr.ReadToEnd();
					Match m = Regex.Match(homePageHtml, @"http://www.blogger.com/feeds/\d+/posts/default");
					Debug.Assert(m.Success, "Posting operation failed.");
					feedUrl = m.Value;
				}
			}
			const string Atom = "http://www.w3.org/2005/Atom";
			XElement entry = new XElement(
				XName.Get("entry", Atom),
				new XElement(XName.Get("title", Atom), new XAttribute("type", "text"), title),
				new XElement(XName.Get("content", Atom), new XAttribute("type", "xhtml"), body),
				new XElement(XName.Get("category", Atom), new XAttribute("scheme", "http://www.blogger.com/atom/ns#"), new XAttribute("term", "oauthdemo")));

			MemoryStream ms = new MemoryStream();
			XmlWriterSettings xws = new XmlWriterSettings() {
				Encoding = Encoding.UTF8,
			};
			XmlWriter xw = XmlWriter.Create(ms, xws);
			entry.WriteTo(xw);
			xw.Flush();

			WebRequest request = consumer.PrepareAuthorizedRequest(new MessageReceivingEndpoint(feedUrl, HttpDeliveryMethods.PostRequest | HttpDeliveryMethods.AuthorizationHeaderRequest), accessToken);
			request.ContentType = "application/atom+xml";
			request.Method = "POST";
			request.ContentLength = ms.Length;
			ms.Seek(0, SeekOrigin.Begin);
			using (Stream requestStream = request.GetRequestStream()) {
				ms.CopyTo(requestStream);
			}
			using (HttpWebResponse response = (HttpWebResponse)request.GetResponse()) {
				if (response.StatusCode == HttpStatusCode.Created) {
					// Success
				} else {
					// Error!
				}
			}
		}

		/// <summary>
		/// Gets the scope URI in Google's format.
		/// </summary>
		/// <param name="scope">The scope, which may include one or several Google applications.</param>
		/// <returns>A space-delimited list of URIs for the requested Google applications.</returns>
		private static string GetScopeUri(Applications scope) {
			return string.Join(" ", Util.GetIndividualFlags(scope).Select(app => DataScopeUris[(Applications)app]).ToArray());
		}
	}
}
