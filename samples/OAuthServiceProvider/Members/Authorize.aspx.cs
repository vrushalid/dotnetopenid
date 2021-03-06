﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using DotNetOpenAuth;
using DotNetOpenAuth.OAuth;
using DotNetOpenAuth.OAuth.Messages;

/// <summary>
/// Conducts the user through a Consumer authorization process.
/// </summary>
public partial class Authorize : System.Web.UI.Page {
	protected void Page_Load(object sender, EventArgs e) {
		if (!IsPostBack) {
			if (Global.PendingOAuthAuthorization == null) {
				Response.Redirect("~/Members/AuthorizedConsumers.aspx");
			} else {
				ITokenContainingMessage pendingToken = Global.PendingOAuthAuthorization;
				var token = Global.DataContext.OAuthTokens.Single(t => t.Token == pendingToken.Token);
				desiredAccessLabel.Text = token.Scope;
			}
		}
	}

	protected void allowAccessButton_Click(object sender, EventArgs e) {
		var pending = Global.PendingOAuthAuthorization;
		Global.AuthorizePendingRequestToken();
		multiView.ActiveViewIndex = 1;

		ServiceProvider sp = new ServiceProvider(Constants.SelfDescription, Global.TokenManager);
		var response = sp.PrepareAuthorizationResponse(pending);
		if (response != null) {
			sp.Channel.Send(response);
		}
	}

	protected void denyAccessButton_Click(object sender, EventArgs e) {
		// erase the request token.
		multiView.ActiveViewIndex = 2;
	}
}
