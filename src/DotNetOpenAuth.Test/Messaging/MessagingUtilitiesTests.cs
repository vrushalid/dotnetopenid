﻿//-----------------------------------------------------------------------
// <copyright file="MessagingUtilitiesTests.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Test.Messaging
{
	using System;
	using System.Collections.Generic;
	using System.Collections.Specialized;
	using System.IO;
	using System.Net;
	using System.Web;
	using DotNetOpenAuth.Messaging;
	using Microsoft.VisualStudio.TestTools.UnitTesting;

	[TestClass]
	public class MessagingUtilitiesTests : TestBase {
		[TestMethod]
		public void CreateQueryString() {
			var args = new Dictionary<string, string>();
			args.Add("a", "b");
			args.Add("c/d", "e/f");
			Assert.AreEqual("a=b&c%2fd=e%2ff", MessagingUtilities.CreateQueryString(args));
		}

		[TestMethod]
		public void CreateQueryStringEmptyCollection() {
			Assert.AreEqual(0, MessagingUtilities.CreateQueryString(new Dictionary<string, string>()).Length);
		}

		[TestMethod, ExpectedException(typeof(ArgumentNullException))]
		public void CreateQueryStringNullDictionary() {
			MessagingUtilities.CreateQueryString(null);
		}

		[TestMethod]
		public void AppendQueryArgs() {
			UriBuilder uri = new UriBuilder("http://baseline.org/page");
			var args = new Dictionary<string, string>();
			args.Add("a", "b");
			args.Add("c/d", "e/f");
			MessagingUtilities.AppendQueryArgs(uri, args);
			Assert.AreEqual("http://baseline.org/page?a=b&c%2fd=e%2ff", uri.Uri.AbsoluteUri);
			args.Clear();
			args.Add("g", "h");
			MessagingUtilities.AppendQueryArgs(uri, args);
			Assert.AreEqual("http://baseline.org/page?a=b&c%2fd=e%2ff&g=h", uri.Uri.AbsoluteUri);
		}

		[TestMethod, ExpectedException(typeof(ArgumentNullException))]
		public void AppendQueryArgsNullUriBuilder() {
			MessagingUtilities.AppendQueryArgs(null, new Dictionary<string, string>());
		}

		[TestMethod]
		public void AppendQueryArgsNullDictionary() {
			MessagingUtilities.AppendQueryArgs(new UriBuilder(), null);
		}

		[TestMethod]
		public void ToDictionary() {
			NameValueCollection nvc = new NameValueCollection();
			nvc["a"] = "b";
			nvc["c"] = "d";
			nvc[string.Empty] = "emptykey";
			Dictionary<string, string> actual = MessagingUtilities.ToDictionary(nvc);
			Assert.AreEqual(nvc.Count, actual.Count);
			Assert.AreEqual(nvc["a"], actual["a"]);
			Assert.AreEqual(nvc["c"], actual["c"]);
		}

		[TestMethod, ExpectedException(typeof(ArgumentException))]
		public void ToDictionaryWithNullKey() {
			NameValueCollection nvc = new NameValueCollection();
			nvc[null] = "a";
			nvc["b"] = "c";
			nvc.ToDictionary(true);
		}

		[TestMethod]
		public void ToDictionaryWithSkippedNullKey() {
			NameValueCollection nvc = new NameValueCollection();
			nvc[null] = "a";
			nvc["b"] = "c";
			var dictionary = nvc.ToDictionary(false);
			Assert.AreEqual(1, dictionary.Count);
			Assert.AreEqual(nvc["b"], dictionary["b"]);
		}

		[TestMethod]
		public void ToDictionaryNull() {
			Assert.IsNull(MessagingUtilities.ToDictionary(null));
		}

		[TestMethod, ExpectedException(typeof(ArgumentNullException))]
		public void ApplyHeadersToResponseNullAspNetResponse() {
			MessagingUtilities.ApplyHeadersToResponse(new WebHeaderCollection(), (HttpResponse)null);
		}

		[TestMethod, ExpectedException(typeof(ArgumentNullException))]
		public void ApplyHeadersToResponseNullListenerResponse() {
			MessagingUtilities.ApplyHeadersToResponse(new WebHeaderCollection(), (HttpListenerResponse)null);
		}

		[TestMethod, ExpectedException(typeof(ArgumentNullException))]
		public void ApplyHeadersToResponseNullHeaders() {
			MessagingUtilities.ApplyHeadersToResponse(null, new HttpResponse(new StringWriter()));
		}

		[TestMethod]
		public void ApplyHeadersToResponse() {
			var headers = new WebHeaderCollection();
			headers[HttpResponseHeader.ContentType] = "application/binary";

			var response = new HttpResponse(new StringWriter());
			MessagingUtilities.ApplyHeadersToResponse(headers, response);

			Assert.AreEqual(headers[HttpResponseHeader.ContentType], response.ContentType);
		}
	}
}
