<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage" %>

<%@ Register Assembly="DotNetOpenAuth" Namespace="DotNetOpenAuth.OpenId.Provider"
	TagPrefix="op" %>
<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
	<%=ViewData["username"]%>
	identity page
</asp:Content>
<asp:Content runat="server" ContentPlaceHolderID="HeadContent">
	<op:IdentityEndpoint ID="IdentityEndpoint11" runat="server" ProviderEndpointUrl="~/OpenId/Provider" ProviderVersion="V11" />
	<op:IdentityEndpoint ID="IdentityEndpoint20" runat="server" ProviderEndpointUrl="~/OpenId/Provider" XrdsUrl="~/User/all/xrds" XrdsAutoAnswer="false" />
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
	<h2>This is
		<%=ViewData["username"]%>'s OpenID identity page </h2>
		
	<% if (string.Equals(User.Identity.Name, ViewData["username"])) { %>
		<p>This is <b>your</b> identity page. </p>
	<% } %>
</asp:Content>
