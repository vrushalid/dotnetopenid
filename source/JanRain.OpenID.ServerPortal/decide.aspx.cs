using System;
using System.Web.Security;
using System.Web.UI;
using Janrain.OpenId.Server;

/// <summary>
/// Page for giving the user the option to continue or cancel out of authentication with a consumer.
/// </summary>
public partial class decide : Page
{
    protected void Page_Load(object src, EventArgs e)
    {
        State.Session.CheckExpectedSateIsAvailable();
        
        // check that the logged in user is the same as the user requesting authentication to the consumer. If not, then log them out.
        String s = Util.ExtractUserName(State.Session.LastRequest.IdentityUrl);        
        if (s != User.Identity.Name)
        {
            FormsAuthentication.SignOut();
            Response.Redirect(Request.Url.AbsoluteUri);
        }
        else
        {
            // if simple registration fields were used, then prompt the user for them
            if (State.Session.LastRequest.IsAnySimpleRegistrationFieldsRequestedOrRequired)
            {
                this.profileFields.Visible = true;
                this.profileFields.SetRequiredFieldsFromRequest(State.Session.LastRequest);
            }
        }
    }

    ///
    protected void Yes_Click(Object sender, EventArgs e)
    {
        Janrain.OpenId.Server.Response response;
        if ( this.profileFields.DoesAnyFieldHaveAValue)
        {
            // authenticate with simple registration fields
            response = State.Session.LastRequest.Answer(true, State.ServerUri, this.profileFields.OpenIdProfileFields);
        }
        else
        {
            // no fields available
            response = State.Session.LastRequest.Answer(true, State.ServerUri);
        }
        Util.GenerateHttpResponse(response);
    }

    protected void No_Click(Object sender, EventArgs e)
    {
        Janrain.OpenId.Server.Response response = State.Session.LastRequest.Answer(false, State.ServerUri);
        Util.GenerateHttpResponse(response);
    }
}