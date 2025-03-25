using System;
using System.Web.UI;
using Newtonsoft.Json;

namespace TokenManagement
{
    public partial class TokenManager : System.Web.UI.UserControl
    {
        private TokenContext _apiTokenContext;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                // Initialize cleanup task if in Global.asax
                // VisitorCleanupTask.StartCleanupTask();

                // Create token context for API access
                _apiTokenContext = AnonymousTokenHelper.CreateTokenContext("api");

                // Update display
                UpdateTokenStatus();
            }
        }

        protected void UseToken_Click(object sender, EventArgs e)
        {
            string token = _apiTokenContext.GetToken();
            if (!string.IsNullOrEmpty(token))
            {
                tokenStatusLabel.Text = $"Using token: {token.Substring(0, 10)}...";

                // Simulate API call with token
                // MakeApiCall(token);
            }
            else
            {
                tokenStatusLabel.Text = "No valid token available";
            }
        }

        protected void RefreshToken_Click(object sender, EventArgs e)
        {
            _apiTokenContext.RefreshToken((success, newToken) =>
            {
                if (success)
                {
                    tokenStatusLabel.Text = "Token refreshed successfully";
                    
                    // Force backup after manual refresh
                    _apiTokenContext.ForceBackup();
                }
                else
                    tokenStatusLabel.Text = "Token refresh failed";
            });

            tokenStatusLabel.Text = "Refreshing token...";
        }

        protected void BackupTokens_Click(object sender, EventArgs e)
        {
            bool success = _apiTokenContext.ForceBackup();
            if (success)
            {
                tokenStatusLabel.Text = "Tokens backed up successfully";
            }
            else
            {
                tokenStatusLabel.Text = "Token backup failed";
            }
        }

        private void UpdateTokenStatus()
        {
            string token = _apiTokenContext.GetToken();
            if (string.IsNullOrEmpty(token))
                tokenStatusLabel.Text = "Initializing token...";
            else if (_apiTokenContext.NeedsRefresh())
                tokenStatusLabel.Text = "Token needs refresh";
            else
                tokenStatusLabel.Text = "Token is valid";
        }

        protected void Page_Unload(object sender, EventArgs e)
        {
            // Clean up token context
            _apiTokenContext?.Dispose();
        }

        // Example of how to register in Global.asax Session_End
        protected void Application_SessionEnd()
        {
            AnonymousTokenHelper.CleanupVisitor();
        }
        
        // Example of how to register in Global.asax Application_End
        protected void Application_End()
        {
            // Ensure all tokens are backed up before application shuts down
            AnonymousTokenHelper.CleanupOnShutdown();
        }
    }
}