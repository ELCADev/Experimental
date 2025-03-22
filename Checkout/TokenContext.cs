using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace TokenManagement
{
	/// <summary>
	/// Single principal token manager that handles token access for anonymous visitors
	/// </summary>
	public class AnonymousPrincipal
	{
		private static readonly Lazy<AnonymousPrincipal> _instance = new Lazy<AnonymousPrincipal>(() => new AnonymousPrincipal());
		public static AnonymousPrincipal Instance => _instance.Value;

		// Dictionary of token types (e.g., "api", "analytics", etc.)
		private readonly ConcurrentDictionary<string, SharedToken> _tokenTypes = new ConcurrentDictionary<string, SharedToken>();

		// Dictionary mapping visitor ID to their token allocation
		private readonly ConcurrentDictionary<string, VisitorTokens> _visitorTokens = new ConcurrentDictionary<string, VisitorTokens>();

		private AnonymousPrincipal() { }

		/// <summary>
		/// Represents a shared token used by multiple anonymous visitors
		/// </summary>
		public class SharedToken
		{
			// The actual token value
			public string Value { get; set; }

			// When the token expires
			public DateTime ExpiresAt { get; set; }

			// Flag indicating if a refresh is in progress
			public bool IsRefreshing { get; set; }

			// Flag indicating if token is invalid
			public bool IsInvalid { get; set; }

			// Lock object for this token
			public object Lock { get; } = new object();

			// List of callbacks waiting for refresh completion
			public List<Action<bool, string>> Callbacks { get; } = new List<Action<bool, string>>();

			// Current usage count
			public int UsageCount { get; set; }

			// Token type (e.g., "api", "analytics")
			public string TokenType { get; set; }

			// Optional refresh parameters
			public Dictionary<string, object> RefreshParameters { get; set; } = new Dictionary<string, object>();
		}

		/// <summary>
		/// Tracks which tokens a visitor is using
		/// </summary>
		public class VisitorTokens
		{
			// Set of token types this visitor is using
			public HashSet<string> ActiveTokenTypes { get; } = new HashSet<string>();

			// Last activity timestamp
			public DateTime LastActivity { get; set; } = DateTime.UtcNow;

			// Lock for this visitor's token set
			public object Lock { get; } = new object();
		}

		/// <summary>
		/// Get an anonymous token of a specific type for a visitor
		/// </summary>
		public string GetToken(string visitorId, string tokenType)
		{
			if (string.IsNullOrEmpty(visitorId) || string.IsNullOrEmpty(tokenType))
				return null;

			// Register this visitor as using this token type
			RegisterVisitorForToken(visitorId, tokenType);

			// Get or create the shared token
			var token = _tokenTypes.GetOrAdd(tokenType, CreateNewToken);

			// Check if token needs refresh
			if (NeedsRefresh(tokenType))
			{
				// Start a refresh if not already in progress
				RefreshToken(tokenType);
			}

			// Return the current token value
			lock (token.Lock)
			{
				if (token.IsInvalid)
					return null;

				return token.Value;
			}
		}

		/// <summary>
		/// Register a visitor as using a token type
		/// </summary>
		private void RegisterVisitorForToken(string visitorId, string tokenType)
		{
			var visitorTokenSet = _visitorTokens.GetOrAdd(visitorId, _ => new VisitorTokens());

			lock (visitorTokenSet.Lock)
			{
				visitorTokenSet.ActiveTokenTypes.Add(tokenType);
				visitorTokenSet.LastActivity = DateTime.UtcNow;
			}

			// Update usage count for this token type
			var token = _tokenTypes.GetOrAdd(tokenType, CreateNewToken);

			lock (token.Lock)
			{
				token.UsageCount++;
			}
		}

		/// <summary>
		/// Create a new token for a specific type
		/// </summary>
		private SharedToken CreateNewToken(string tokenType)
		{
			var token = new SharedToken
			{
				TokenType = tokenType,
				Value = null,
				ExpiresAt = DateTime.MinValue,
				IsRefreshing = false,
				IsInvalid = false,
				UsageCount = 0
			};

			// Start initial token acquisition
			Task.Run(() => InitializeToken(tokenType));

			return token;
		}

		/// <summary>
		/// Initialize a token by fetching it from the server
		/// </summary>
		private async Task InitializeToken(string tokenType)
		{
			if (!_tokenTypes.TryGetValue(tokenType, out var token))
				return;

			bool isRefreshing = false;

			lock (token.Lock)
			{
				if (token.IsRefreshing)
					return;

				token.IsRefreshing = true;
				isRefreshing = true;
			}

			if (isRefreshing)
			{
				try
				{
					// Get initial token from server
					var result = await GetTokenFromServer(tokenType);

					lock (token.Lock)
					{
						token.Value = result.TokenValue;
						token.ExpiresAt = result.ExpiresAt;
						token.IsRefreshing = false;
						token.IsInvalid = false;
					}
				}
				catch (Exception ex)
				{
					System.Diagnostics.Debug.WriteLine($"Failed to initialize token: {ex.Message}");

					lock (token.Lock)
					{
						token.IsRefreshing = false;
					}

					// Retry after delay
					await Task.Delay(5000);
					await InitializeToken(tokenType);
				}
			}
		}

		/// <summary>
		/// Check if a token needs refresh
		/// </summary>
		public bool NeedsRefresh(string tokenType)
		{
			if (string.IsNullOrEmpty(tokenType))
				return false;

			if (!_tokenTypes.TryGetValue(tokenType, out var token))
				return false;

			lock (token.Lock)
			{
				if (token.IsInvalid)
					return false;

				if (token.IsRefreshing)
					return false;

				// Add 30-second buffer to ensure smooth refresh
				return token.ExpiresAt <= DateTime.UtcNow.AddSeconds(30);
			}
		}

		/// <summary>
		/// Refresh a token with optional callback notification
		/// </summary>
		public bool RefreshToken(string tokenType, Action<bool, string> callback = null)
		{
			if (string.IsNullOrEmpty(tokenType))
				return false;

			if (!_tokenTypes.TryGetValue(tokenType, out var token))
				return false;

			bool shouldStartRefresh = false;

			lock (token.Lock)
			{
				// Can't refresh invalid tokens
				if (token.IsInvalid)
				{
					callback?.Invoke(false, null);
					return false;
				}

				// If already refreshing, just add callback to notification list
				if (token.IsRefreshing)
				{
					if (callback != null)
						token.Callbacks.Add(callback);
					return true;
				}

				// Mark as refreshing and start refresh
				token.IsRefreshing = true;
				shouldStartRefresh = true;

				if (callback != null)
					token.Callbacks.Add(callback);
			}

			// Start refresh outside the lock
			if (shouldStartRefresh)
			{
				Task.Run(() => PerformTokenRefresh(tokenType));
				return true;
			}

			return false;
		}

		/// <summary>
		/// Actually perform the token refresh
		/// </summary>
		private async Task PerformTokenRefresh(string tokenType)
		{
			if (!_tokenTypes.TryGetValue(tokenType, out var token))
				return;

			bool success = false;
			string newTokenValue = null;

			try
			{
				// Get refresh parameters if needed
				Dictionary<string, object> refreshParams;
				lock (token.Lock)
				{
					refreshParams = new Dictionary<string, object>(token.RefreshParameters);
				}

				// Call server to refresh token
				var result = await RefreshTokenFromServer(tokenType, refreshParams);
				success = result.Success;
				newTokenValue = result.TokenValue;

				if (success)
				{
					// Update token with new values
					lock (token.Lock)
					{
						token.Value = newTokenValue;
						token.ExpiresAt = result.ExpiresAt;
					}
				}
				else
				{
					// Mark token as invalid
					lock (token.Lock)
					{
						token.IsInvalid = true;
					}
				}
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine($"Token refresh failed: {ex.Message}");
				success = false;
			}
			finally
			{
				// Prepare callbacks to notify outside lock
				List<Action<bool, string>> callbacks;
				lock (token.Lock)
				{
					callbacks = new List<Action<bool, string>>(token.Callbacks);
					token.Callbacks.Clear();
					token.IsRefreshing = false;
				}

				// Notify all waiting callbacks
				foreach (var callback in callbacks)
				{
					try
					{
						callback(success, newTokenValue);
					}
					catch (Exception ex)
					{
						System.Diagnostics.Debug.WriteLine($"Callback exception: {ex.Message}");
					}
				}
			}
		}

		/// <summary>
		/// Simulated method to get token from server
		/// </summary>
		private async Task<(string TokenValue, DateTime ExpiresAt)> GetTokenFromServer(string tokenType)
		{
			// Simulate network delay
			await Task.Delay(1000);

			// Create a token that expires in 50 minutes
			string tokenValue = $"anonymous_{tokenType}_{Guid.NewGuid()}";
			DateTime expiresAt = DateTime.UtcNow.AddMinutes(50);

			return (tokenValue, expiresAt);
		}

		/// <summary>
		/// Simulated method to refresh token from server
		/// </summary>
		private async Task<(bool Success, string TokenValue, DateTime ExpiresAt)> RefreshTokenFromServer(
			string tokenType,
			Dictionary<string, object> parameters)
		{
			// Simulate network delay
			await Task.Delay(1000);

			// Simulate 95% success rate
			bool success = new Random().NextDouble() <= 0.95;

			if (success)
			{
				string tokenValue = $"refreshed_{tokenType}_{Guid.NewGuid()}";
				DateTime expiresAt = DateTime.UtcNow.AddMinutes(50);

				return (true, tokenValue, expiresAt);
			}

			return (false, null, DateTime.MinValue);
		}

		/// <summary>
		/// Unregister a visitor from using a token
		/// </summary>
		public void UnregisterVisitor(string visitorId)
		{
			if (string.IsNullOrEmpty(visitorId))
				return;

			if (_visitorTokens.TryRemove(visitorId, out var visitorTokenSet))
			{
				// Get the set of token types this visitor was using
				HashSet<string> tokenTypes;
				lock (visitorTokenSet.Lock)
				{
					tokenTypes = new HashSet<string>(visitorTokenSet.ActiveTokenTypes);
				}

				// Decrement usage count for each token type
				foreach (var tokenType in tokenTypes)
				{
					if (_tokenTypes.TryGetValue(tokenType, out var token))
					{
						lock (token.Lock)
						{
							token.UsageCount--;

							// If no one is using this token anymore, we could clean it up
							if (token.UsageCount <= 0)
							{
								// Optional: remove unused tokens
								// _tokenTypes.TryRemove(tokenType, out _);
							}
						}
					}
				}
			}
		}

		/// <summary>
		/// Set refresh parameters for a token type
		/// </summary>
		public void SetTokenRefreshParams(string tokenType, Dictionary<string, object> parameters)
		{
			if (string.IsNullOrEmpty(tokenType) || parameters == null)
				return;

			var token = _tokenTypes.GetOrAdd(tokenType, CreateNewToken);

			lock (token.Lock)
			{
				token.RefreshParameters = new Dictionary<string, object>(parameters);
			}
		}

		/// <summary>
		/// Clean up inactive visitors (call periodically)
		/// </summary>
		public void CleanupInactiveVisitors(TimeSpan inactiveThreshold)
		{
			var now = DateTime.UtcNow;
			var visitorsToRemove = new List<string>();

			// Find inactive visitors
			foreach (var visitorKvp in _visitorTokens)
			{
				bool isInactive;
				lock (visitorKvp.Value.Lock)
				{
					isInactive = (now - visitorKvp.Value.LastActivity) > inactiveThreshold;
				}

				if (isInactive)
				{
					visitorsToRemove.Add(visitorKvp.Key);
				}
			}

			// Remove inactive visitors
			foreach (var visitorId in visitorsToRemove)
			{
				UnregisterVisitor(visitorId);
			}
		}
	}

	/// <summary>
	/// Context class for using anonymous tokens in different parts of the application
	/// </summary>
	public class TokenContext : IDisposable
	{
		private readonly string _visitorId;
		private readonly string _tokenType;
		private bool _disposed;

		public TokenContext(string visitorId, string tokenType)
		{
			_visitorId = visitorId ?? throw new ArgumentNullException(nameof(visitorId));
			_tokenType = tokenType ?? throw new ArgumentNullException(nameof(tokenType));
		}

		/// <summary>
		/// Get the token for API calls
		/// </summary>
		public string GetToken()
		{
			return AnonymousPrincipal.Instance.GetToken(_visitorId, _tokenType);
		}

		/// <summary>
		/// Check if token needs refresh
		/// </summary>
		public bool NeedsRefresh()
		{
			return AnonymousPrincipal.Instance.NeedsRefresh(_tokenType);
		}

		/// <summary>
		/// Request token refresh with notification
		/// </summary>
		public void RefreshToken(Action<bool, string> callback = null)
		{
			AnonymousPrincipal.Instance.RefreshToken(_tokenType, callback);
		}

		public void Dispose()
		{
			if (!_disposed)
			{
				// Nothing to do here - visitor cleanup is handled centrally
				_disposed = true;
			}
		}
	}

	/// <summary>
	/// Helper class for anonymous token management
	/// </summary>
	public static class AnonymousTokenHelper
	{
		// Key for storing visitor ID in session
		private const string VisitorIdSessionKey = "AnonymousVisitorId";

		/// <summary>
		/// Get visitor ID from session or create new one
		/// </summary>
		public static string GetOrCreateVisitorId()
		{
			if (HttpContext.Current?.Session != null)
			{
				string visitorId = HttpContext.Current.Session[VisitorIdSessionKey] as string;

				if (string.IsNullOrEmpty(visitorId))
				{
					visitorId = Guid.NewGuid().ToString();
					HttpContext.Current.Session[VisitorIdSessionKey] = visitorId;
				}

				return visitorId;
			}

			return Guid.NewGuid().ToString();
		}

		/// <summary>
		/// Create a token context for a specific token type
		/// </summary>
		public static TokenContext CreateTokenContext(string tokenType)
		{
			string visitorId = GetOrCreateVisitorId();
			return new TokenContext(visitorId, tokenType);
		}

		/// <summary>
		/// Clean up on session end
		/// </summary>
		public static void CleanupVisitor()
		{
			if (HttpContext.Current?.Session != null)
			{
				string visitorId = HttpContext.Current.Session[VisitorIdSessionKey] as string;

				if (!string.IsNullOrEmpty(visitorId))
				{
					AnonymousPrincipal.Instance.UnregisterVisitor(visitorId);
				}
			}
		}
	}

	/// <summary>
	/// Background cleanup task for inactive visitors
	/// </summary>
	public class VisitorCleanupTask
	{
		private static Timer _cleanupTimer;

		public static void StartCleanupTask()
		{
			// Run cleanup every 10 minutes
			_cleanupTimer = new Timer(CleanupCallback, null, TimeSpan.Zero, TimeSpan.FromMinutes(10));
		}

		private static void CleanupCallback(object state)
		{
			try
			{
				// Consider visitors inactive after 30 minutes
				AnonymousPrincipal.Instance.CleanupInactiveVisitors(TimeSpan.FromMinutes(30));
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine($"Cleanup task error: {ex.Message}");
			}
		}

		public static void StopCleanupTask()
		{
			_cleanupTimer?.Dispose();
		}
	}
}