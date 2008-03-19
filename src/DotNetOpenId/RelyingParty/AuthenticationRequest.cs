using System;
using System.Collections.Generic;
using System.Text;
using DotNetOpenId;
using System.Collections.Specialized;
using System.Globalization;
using System.Web;

namespace DotNetOpenId.RelyingParty {
	/// <summary>
	/// Indicates the mode the Provider should use while authenticating the end user.
	/// </summary>
	public enum AuthenticationRequestMode {
		/// <summary>
		/// The Provider should use whatever credentials are immediately available
		/// to determine whether the end user owns the Identifier.  If sufficient
		/// credentials (i.e. cookies) are not immediately available, the Provider
		/// should fail rather than prompt the user.
		/// </summary>
		Immediate,
		/// <summary>
		/// The Provider should determine whether the end user owns the Identifier,
		/// displaying a web page to the user to login etc., if necessary.
		/// </summary>
		Setup
	}

	class AuthenticationRequest : IAuthenticationRequest {
		Association assoc;
		ServiceEndpoint endpoint;
		string token;

		AuthenticationRequest(string token, Association assoc, ServiceEndpoint endpoint,
			Realm realm, Uri returnToUrl) {
			this.token = token;
			this.assoc = assoc;
			this.endpoint = endpoint;
			Realm = realm;
			ReturnToUrl = returnToUrl;

			Mode = AuthenticationRequestMode.Setup;
			ExtraArgs = new Dictionary<string, string>();
			ReturnToArgs = new Dictionary<string, string>();
			AddCallbackArguments(DotNetOpenId.RelyingParty.Token.TokenKey, token);
		}
		internal static AuthenticationRequest Create(Identifier userSuppliedIdentifier,
			Realm realm, Uri returnToUrl, IRelyingPartyApplicationStore store) {
			if (userSuppliedIdentifier == null) throw new ArgumentNullException("userSuppliedIdentifier");
			if (realm == null) throw new ArgumentNullException("realm");
			var endpoint = userSuppliedIdentifier.Discover();
			if (endpoint == null)
				throw new OpenIdException(Strings.OpenIdEndpointNotFound);

			// Throw an exception now if the trustroot and the return_to URLs don't match
			// as required by the provider.  We could wait for the provider to test this and
			// fail, but this will be faster and give us a better error message.
			if (!realm.Contains(returnToUrl))
				throw new OpenIdException(string.Format(CultureInfo.CurrentUICulture,
					Strings.ReturnToNotUnderTrustRoot, returnToUrl, realm));

			return new AuthenticationRequest(
				new Token(endpoint).Serialize(store),
				getAssociation(endpoint, store), endpoint,
				realm, returnToUrl);
		}
		static Association getAssociation(ServiceEndpoint provider, IRelyingPartyApplicationStore store) {
			Association assoc = store.GetAssociation(provider.ProviderEndpoint);

			if (assoc == null || !assoc.HasUsefulLifeRemaining) {
				var req = AssociateRequest.Create(provider);
				if (req.Response != null) {
					assoc = req.Response.Association;
					if (assoc != null) {
						store.StoreAssociation(provider.ProviderEndpoint, assoc);
					}
				}
			}

			return assoc;
		}

		/// <summary>
		/// Arguments to add to the query string to be sent to the provider.
		/// </summary>
		protected IDictionary<string, string> ExtraArgs { get; private set; }
		/// <summary>
		/// Arguments to add to the return_to part of the query string, so that
		/// these values come back to the consumer when the user agent returns.
		/// </summary>
		protected IDictionary<string, string> ReturnToArgs { get; private set; }

		public AuthenticationRequestMode Mode { get; set; }
		public Realm Realm { get; private set; }
		public Uri ReturnToUrl { get; private set; }
		/// <summary>
		/// Gets the URL the user agent should be redirected to to begin the 
		/// OpenID authentication process.
		/// </summary>
		public Uri RedirectToProviderUrl {
			get {
				UriBuilder returnToBuilder = new UriBuilder(ReturnToUrl);
				UriUtil.AppendQueryArgs(returnToBuilder, this.ReturnToArgs);

				var qsArgs = new Dictionary<string, string>();

				qsArgs.Add(QueryStringArgs.openid.mode, (Mode == AuthenticationRequestMode.Immediate) ?
					QueryStringArgs.Modes.checkid_immediate : QueryStringArgs.Modes.checkid_setup);
				qsArgs.Add(QueryStringArgs.openid.identity, endpoint.ProviderLocalIdentifier);
				if (endpoint.ProviderVersion.Major >= 2) {
					qsArgs.Add(QueryStringArgs.openid.ns, QueryStringArgs.OpenIdNs.v20);
					qsArgs.Add(QueryStringArgs.openid.claimed_id, endpoint.ClaimedIdentifier);
					qsArgs.Add(QueryStringArgs.openid.realm, Realm.ToString());
				} else {
					qsArgs.Add(QueryStringArgs.openid.trust_root, Realm.ToString());
				}
				qsArgs.Add(QueryStringArgs.openid.return_to, returnToBuilder.ToString());

				if (this.assoc != null)
					qsArgs.Add(QueryStringArgs.openid.assoc_handle, this.assoc.Handle); // !!!!

				UriBuilder redir = new UriBuilder(this.endpoint.ProviderEndpoint);

				UriUtil.AppendQueryArgs(redir, qsArgs);
				UriUtil.AppendQueryArgs(redir, ExtraArgs);

				return redir.Uri;
			}
		}

		/// <summary>
		/// Adds extra query parameters to the request directed at the OpenID provider.
		/// </summary>
		/// <param name="extensionPrefix">
		/// The extension-specific prefix associated with these arguments.
		/// This should not include the 'openid.' part of the prefix.
		/// For example, the extension field openid.sreg.fullname would receive
		/// 'sreg' for this value.
		/// </param>
		/// <param name="arguments">
		/// The key/value pairs of parameters and values to pass to the provider.
		/// The keys should NOT have the 'openid.ext.' prefix.
		/// </param>
		public void AddExtensionArguments(string extensionPrefix, IDictionary<string, string> arguments) {
			if (string.IsNullOrEmpty(extensionPrefix)) throw new ArgumentNullException("extensionPrefix");
			if (arguments == null) throw new ArgumentNullException("arguments");
			if (extensionPrefix.StartsWith(".", StringComparison.Ordinal) ||
				extensionPrefix.EndsWith(".", StringComparison.Ordinal)) 
				throw new ArgumentException(Strings.PrefixWithoutPeriodsExpected, "extensionPrefix");

			foreach (var pair in arguments) {
				if (pair.Key.StartsWith(QueryStringArgs.openid.Prefix) ||
					pair.Key.StartsWith(extensionPrefix))
					throw new ArgumentException(string.Format(CultureInfo.CurrentUICulture,
						Strings.ExtensionParameterKeysWithoutPrefixExpected, pair.Key), "arguments");
				ExtraArgs.Add(QueryStringArgs.openid.Prefix + extensionPrefix + "." + pair.Key, pair.Value);
			}
		}
		/// <summary>
		/// Adds given key/value pairs to the query that the provider will use in
		/// the request to return to the consumer web site.
		/// </summary>
		public void AddCallbackArguments(IDictionary<string, string> arguments) {
			if (arguments == null) throw new ArgumentNullException("arguments");
			foreach (var pair in arguments) {
				AddCallbackArguments(pair.Key, pair.Value);
			}
		}
		/// <summary>
		/// Adds a given key/value pair to the query that the provider will use in
		/// the request to return to the consumer web site.
		/// </summary>
		public void AddCallbackArguments(string key, string value) {
			if (string.IsNullOrEmpty(key)) throw new ArgumentNullException("key");
			if (ReturnToArgs.ContainsKey(key)) throw new ArgumentException(string.Format(CultureInfo.CurrentUICulture,
				Strings.KeyAlreadyExists, key));
			ReturnToArgs.Add(key, value ?? "");
		}

		/// <summary>
		/// Redirects the user agent to the provider for authentication.
		/// </summary>
		/// <remarks>
		/// This method requires an ASP.NET HttpContext.
		/// </remarks>
		public void RedirectToProvider() {
			RedirectToProvider(false);
		}
		/// <summary>
		/// Redirects the user agent to the provider for authentication.
		/// </summary>
		/// <param name="endResponse">
		/// Whether execution of this response should cease after this call.
		/// </param>
		/// <remarks>
		/// This method requires an ASP.NET HttpContext.
		/// </remarks>
		public void RedirectToProvider(bool endResponse) {
			if (HttpContext.Current == null || HttpContext.Current.Response == null) 
				throw new InvalidOperationException(Strings.CurrentHttpContextRequired);
			HttpContext.Current.Response.Redirect(RedirectToProviderUrl.AbsoluteUri, endResponse);
		}
	}
}