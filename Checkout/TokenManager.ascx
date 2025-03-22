<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="TokenManager.ascx.cs" Inherits="TokenManagement.TokenManager" %>

<!-- Sample usage in ASCX page -->
<div runat="server" id="anonymousTokenExample" visible="false">
    <asp:Label ID="tokenStatusLabel" runat="server" Text=""></asp:Label>
    <asp:Button ID="useTokenButton" runat="server" Text="Use Token" OnClick="UseToken_Click" />
    <asp:Button ID="refreshTokenButton" runat="server" Text="Refresh Token" OnClick="RefreshToken_Click" />
</div>