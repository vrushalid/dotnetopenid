using System.Web.UI.WebControls;

namespace DotNetOpenId.RelyingParty {
	/// <summary>
	/// A TextBox that forces the name and id to be openid_identifier instead of
	/// the complex Container$OpenIdLogin$wrappedTextBox form that ASP.NET typically uses.
	/// </summary>
	/// <remarks>
	/// This is useful because for OpenID fields you want a standard name on the browser
	/// so autocomplete works well.
	/// </remarks>
	class TextBoxNoNameContainer : TextBox {
		public override string UniqueID {
			get {
				return ID;// "openid_identifier";
			}
		}
	}
}
