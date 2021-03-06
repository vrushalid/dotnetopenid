﻿//-----------------------------------------------------------------------
// <copyright file="OpenIdTestBase.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Test.OpenId {
	using System;
	using System.IO;
	using System.Reflection;
	using DotNetOpenAuth.Configuration;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.Messaging.Bindings;
	using DotNetOpenAuth.OpenId;
	using DotNetOpenAuth.OpenId.Provider;
	using DotNetOpenAuth.OpenId.RelyingParty;
	using DotNetOpenAuth.Test.Mocks;
	using Microsoft.VisualStudio.TestTools.UnitTesting;

	public class OpenIdTestBase : TestBase {
		internal IDirectWebRequestHandler RequestHandler;

		internal MockHttpRequest MockResponder;

		protected internal const string IdentifierSelect = "http://specs.openid.net/auth/2.0/identifier_select";

		protected internal static readonly Uri BaseMockUri = new Uri("http://localhost/");
		protected internal static readonly Uri BaseMockUriSsl = new Uri("https://localhost/");

		protected internal static readonly Uri OPUri = new Uri(BaseMockUri, "/provider/endpoint");
		protected internal static readonly Uri OPUriSsl = new Uri(BaseMockUriSsl, "/provider/endpoint");
		protected internal static readonly Uri[] OPLocalIdentifiers = new[] { new Uri(OPUri, "/provider/someUser0"), new Uri(OPUri, "/provider/someUser1") };
		protected internal static readonly Uri[] OPLocalIdentifiersSsl = new[] { new Uri(OPUriSsl, "/provider/someUser0"), new Uri(OPUriSsl, "/provider/someUser1") };

		// Vanity URLs are Claimed Identifiers that delegate to some OP and its local identifier.
		protected internal static readonly Uri VanityUri = new Uri(BaseMockUri, "/userControlled/identity");
		protected internal static readonly Uri VanityUriSsl = new Uri(BaseMockUriSsl, "/userControlled/identity");

		protected internal static readonly Uri RPUri = new Uri(BaseMockUri, "/relyingparty/login");
		protected internal static readonly Uri RPUriSsl = new Uri(BaseMockUriSsl, "/relyingparty/login");
		protected internal static readonly Uri RPRealmUri = new Uri(BaseMockUri, "/relyingparty/");
		protected internal static readonly Uri RPRealmUriSsl = new Uri(BaseMockUriSsl, "/relyingparty/");

		/// <summary>
		/// Initializes a new instance of the <see cref="OpenIdTestBase"/> class.
		/// </summary>
		internal OpenIdTestBase() {
			this.AutoProviderScenario = Scenarios.AutoApproval;
		}

		public enum Scenarios {
			AutoApproval,
			AutoApprovalAddFragment,
			ApproveOnSetup,
			AlwaysDeny,
		}

		internal Scenarios AutoProviderScenario { get; set; }

		protected RelyingPartySecuritySettings RelyingPartySecuritySettings { get; private set; }

		protected ProviderSecuritySettings ProviderSecuritySettings { get; private set; }

		[TestInitialize]
		public override void SetUp() {
			base.SetUp();

			this.RelyingPartySecuritySettings = DotNetOpenAuthSection.Configuration.OpenId.RelyingParty.SecuritySettings.CreateSecuritySettings();
			this.ProviderSecuritySettings = DotNetOpenAuthSection.Configuration.OpenId.Provider.SecuritySettings.CreateSecuritySettings();

			this.MockResponder = MockHttpRequest.CreateUntrustedMockHttpHandler();
			this.RequestHandler = this.MockResponder.MockWebRequestHandler;
			this.AutoProviderScenario = Scenarios.AutoApproval;
		}

		/// <summary>
		/// Forces storage of an association in an RP's association store.
		/// </summary>
		/// <param name="relyingParty">The relying party.</param>
		/// <param name="providerEndpoint">The provider endpoint.</param>
		/// <param name="association">The association.</param>
		internal static void StoreAssociation(OpenIdRelyingParty relyingParty, Uri providerEndpoint, Association association) {
			var associationManagerAccessor = AssociationManager_Accessor.AttachShadow(relyingParty.AssociationManager);

			// Only store the association if the RP is not in stateless mode.
			if (associationManagerAccessor.associationStore != null) {
				associationManagerAccessor.associationStore.StoreAssociation(providerEndpoint, association);
			}
		}

		/// <summary>
		/// Returns the content of a given embedded resource.
		/// </summary>
		/// <param name="path">The path of the file as it appears within the project,
		/// where the leading / marks the root directory of the project.</param>
		/// <returns>The content of the requested resource.</returns>
		internal static string LoadEmbeddedFile(string path) {
			if (!path.StartsWith("/")) {
				path = "/" + path;
			}
			path = "DotNetOpenAuth.Test.OpenId" + path.Replace('/', '.');
			Stream resource = Assembly.GetExecutingAssembly().GetManifestResourceStream(path);
			if (resource == null) {
				throw new ArgumentException();
			}
			using (StreamReader sr = new StreamReader(resource)) {
				return sr.ReadToEnd();
			}
		}

		internal static ServiceEndpoint GetServiceEndpoint(int user, ProtocolVersion providerVersion, int servicePriority, bool useSsl) {
			var providerEndpoint = new ProviderEndpointDescription(
				useSsl ? OpenIdTestBase.OPUriSsl : OpenIdTestBase.OPUri,
				new string[] { Protocol.Lookup(providerVersion).ClaimedIdentifierServiceTypeURI });
			return ServiceEndpoint.CreateForClaimedIdentifier(
				useSsl ? OPLocalIdentifiersSsl[user] : OPLocalIdentifiers[user],
				useSsl ? OPLocalIdentifiersSsl[user] : OPLocalIdentifiers[user],
				useSsl ? OPLocalIdentifiersSsl[user] : OPLocalIdentifiers[user],
				providerEndpoint,
				servicePriority,
				10);
		}

		/// <summary>
		/// A default implementation of a simple provider that responds to authentication requests
		/// per the scenario that is being simulated.
		/// </summary>
		/// <param name="provider">The OpenIdProvider on which the process messages.</param>
		/// <remarks>
		/// This is a very useful method to pass to the OpenIdCoordinator constructor for the Provider argument.
		/// </remarks>
		internal void AutoProvider(OpenIdProvider provider) {
			while (!((CoordinatingChannel)provider.Channel).RemoteChannel.IsDisposed) {
				IRequest request = provider.GetRequest();
				if (request == null) {
					continue;
				}

				if (!request.IsResponseReady) {
					var authRequest = (DotNetOpenAuth.OpenId.Provider.IAuthenticationRequest)request;
					switch (this.AutoProviderScenario) {
						case Scenarios.AutoApproval:
							authRequest.IsAuthenticated = true;
							break;
						case Scenarios.AutoApprovalAddFragment:
							authRequest.SetClaimedIdentifierFragment("frag");
							authRequest.IsAuthenticated = true;
							break;
						case Scenarios.ApproveOnSetup:
							authRequest.IsAuthenticated = !authRequest.Immediate;
							break;
						case Scenarios.AlwaysDeny:
							authRequest.IsAuthenticated = false;
							break;
						default:
							// All other scenarios are done programmatically only.
							throw new InvalidOperationException("Unrecognized scenario");
					}
				}

				provider.SendResponse(request);
			}
		}

		protected Identifier GetMockIdentifier(ProtocolVersion providerVersion) {
			return this.GetMockIdentifier(providerVersion, false);
		}

		protected Identifier GetMockIdentifier(ProtocolVersion providerVersion, bool useSsl) {
			ServiceEndpoint se = GetServiceEndpoint(0, providerVersion, 10, useSsl);
			UriIdentifier identityUri = useSsl ? OpenIdTestBase.OPLocalIdentifiersSsl[0] : OpenIdTestBase.OPLocalIdentifiers[0];
			return new MockIdentifier(identityUri, this.MockResponder, new ServiceEndpoint[] { se });
		}

		/// <summary>
		/// Creates a standard <see cref="OpenIdRelyingParty"/> instance for general testing.
		/// </summary>
		/// <returns>The new instance.</returns>
		protected OpenIdRelyingParty CreateRelyingParty() {
			var rp = new OpenIdRelyingParty(new StandardRelyingPartyApplicationStore());
			rp.Channel.WebRequestHandler = this.MockResponder.MockWebRequestHandler;
			return rp;
		}

		/// <summary>
		/// Creates a standard <see cref="OpenIdProvider"/> instance for general testing.
		/// </summary>
		/// <returns>The new instance.</returns>
		protected OpenIdProvider CreateProvider() {
			var op = new OpenIdProvider(new StandardProviderApplicationStore());
			op.Channel.WebRequestHandler = this.MockResponder.MockWebRequestHandler;
			return op;
		}
	}
}
