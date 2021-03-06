namespace OpenIdProviderMvc.Controllers {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Web;
	using System.Web.Mvc;
	using System.Web.Mvc.Ajax;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OpenId.Provider;

	public class OpenIdController : Controller {
		internal static OpenIdProvider OpenIdProvider = new OpenIdProvider();

		internal static IAuthenticationRequest PendingAuthenticationRequest {
			get { return ProviderEndpoint.PendingAuthenticationRequest; }
			set { ProviderEndpoint.PendingAuthenticationRequest = value; }
		}

		public ActionResult Provider() {
			IRequest request = OpenIdProvider.GetRequest();
			if (request != null) {
				var authRequest = request as IAuthenticationRequest;
				if (authRequest != null) {
					PendingAuthenticationRequest = authRequest;
					if (User.Identity.IsAuthenticated && (authRequest.IsDirectedIdentity || Models.User.GetClaimedIdentifierForUser(User.Identity.Name) == authRequest.LocalIdentifier)) {
						return this.SendAssertion();
					} else {
						return RedirectToAction("LogOn", "Account", new { returnUrl = Url.Action("SendAssertion") });
					}
				}

				if (request.IsResponseReady) {
					return OpenIdProvider.PrepareResponse(request).AsActionResult();
				} else {
					return RedirectToAction("LogOn", "Account");
				}
			} else {
				return View();
			}
		}

		[Authorize]
		public ActionResult SendAssertion() {
			IAuthenticationRequest authReq = PendingAuthenticationRequest;
			PendingAuthenticationRequest = null;
			if (authReq == null) {
				throw new InvalidOperationException();
			}

			if (authReq.IsDirectedIdentity) {
				authReq.LocalIdentifier = Models.User.GetClaimedIdentifierForUser(User.Identity.Name);
				authReq.ClaimedIdentifier = authReq.LocalIdentifier;
				authReq.IsAuthenticated = true;
			} else {
				if (authReq.LocalIdentifier == Models.User.GetClaimedIdentifierForUser(User.Identity.Name)) {
					authReq.IsAuthenticated = true;
					if (!authReq.IsDelegatedIdentifier) {
						authReq.ClaimedIdentifier = authReq.LocalIdentifier;
					}
				} else {
					authReq.IsAuthenticated = false;
				}
			}
			return OpenIdProvider.PrepareResponse(authReq).AsActionResult();
		}
	}
}
