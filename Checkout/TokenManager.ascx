<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="TokenManager.ascx.cs" Inherits="TokenManagement.TokenManager" %>

<!-- Sample usage in ASCX page -->
<div runat="server" id="anonymousTokenExample" visible="false">
    <asp:Label ID="tokenStatusLabel" runat="server" Text=""></asp:Label>
    <asp:Button ID="useTokenButton" runat="server" Text="Use Token" OnClick="UseToken_Click" />
    <asp:Button ID="refreshTokenButton" runat="server" Text="Refresh Token" OnClick="RefreshToken_Click" />
    <asp:Button ID="backupTokenButton" runat="server" Text="Backup Tokens" OnClick="BackupTokens_Click" />
</div>

<!-- Token Backup Status -->
<div runat="server" id="backupStatusPanel" visible="false">
    <h3>Token Backup Status</h3>
    <asp:Label ID="backupStatusLabel" runat="server" Text=""></asp:Label>
    <asp:Label ID="lastBackupTimeLabel" runat="server" Text=""></asp:Label>
</div>