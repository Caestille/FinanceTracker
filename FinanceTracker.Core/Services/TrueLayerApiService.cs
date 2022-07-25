using CoreUtilities.HelperClasses;
using CoreUtilities.Interfaces;
using FinanceTracker.Core.Interfaces;
using FinanceTracker.Core.Models;
using System.Diagnostics;
using System.Net;
using System.Net.Http.Headers;
using System.Text;

namespace FinanceTracker.Core.Services
{
	public class TrueLayerApiService : IBankApiService
	{
        public event EventHandler<(Guid, BankLinkStatus)> NewBankLinkStatusForGuid;

        private string liveAuthLink = "https://auth.truelayer.com/?response_type=code&client_id=josephward-732872&scope=info%20accounts%20balance%20cards%20transactions%20direct_debits%20standing_orders%20offline_access&redirect_uri=http://localhost:3000/callback&providers=uk-ob-all%20uk-oauth-all";
        private string clientId = "josephward-732872";
        private string clientSecret = "035d9190-3341-4181-99eb-efee840456b4";
        private string callbackUri = @"http://localhost:3000/";

        private IRegistryService registryService;
        private Dictionary<Guid, LinkedBankModel> bankLinks = new Dictionary<Guid, LinkedBankModel>();
        HttpClient httpClient = new HttpClient();

        private System.Timers.Timer bankLinkValidationTimer = new System.Timers.Timer(1000);

        public TrueLayerApiService(IRegistryService registryService)
		{
            this.registryService = registryService;

            bankLinkValidationTimer.AutoReset = true;
			bankLinkValidationTimer.Elapsed += BankLinkValidationTimer_Elapsed;
            bankLinkValidationTimer.Start();
        }

		public async Task<bool> LinkBank(Guid bankGuid, CancellationToken token)
		{
            var linkedBank = new LinkedBankModel(registryService, bankGuid);
            linkedBank.NewBankLinkStatus += LinkedBank_NewBankLinkStatus;
            linkedBank.BankLinkStatus = BankLinkStatus.Linking;
            bankLinks[bankGuid] = linkedBank;

            Process.Start(new ProcessStartInfo()
            {
                FileName = liveAuthLink,
                UseShellExecute = true,
            });

            (bool success, string authCode) = await ListenForAuthorisationCode(bankGuid, token);

            if (!success)
            {
                bankLinks[bankGuid].BankLinkStatus = BankLinkStatus.LinkingCancelled;
                await Task.Delay(2000);
                await DeleteLink(bankGuid);
                return success;
            }

            (success, string accessToken, string refreshToken, DateTime accessExpires) = await SwapCodeForAccessTokens(authCode);

            if (success)
            {
                linkedBank.AccessToken = accessToken;
                linkedBank.RefreshToken = refreshToken;
                linkedBank.AccessExpires = accessExpires;
                linkedBank.AuthorisationCode = authCode;
                linkedBank.BankLinkStatus = BankLinkStatus.NotLinked;
            }
            else
			{
                await DeleteLink(bankGuid);
            }

            return success;
        }

		public void ReloadLink(Guid bankGuid)
		{
            var linkedBank = new LinkedBankModel(registryService, bankGuid);
            linkedBank.NewBankLinkStatus += LinkedBank_NewBankLinkStatus;
            if (linkedBank.LoadFromRegistry())
			{
                bankLinks[bankGuid] = linkedBank;
            }
		}

        public async Task DeleteLink(Guid bankGuid)
		{
            var request = new HttpRequestMessage(new HttpMethod("DELETE"), "https://auth.truelayer.com/api/delete");
            request.Headers.TryAddWithoutValidation("Accept", "application/json");
            request.Headers.TryAddWithoutValidation("Authorization", $"Bearer {bankLinks[bankGuid].AccessToken}");

            var response = await httpClient.SendAsync(request);

            var success = response.StatusCode == HttpStatusCode.OK;

            //if (!success)
            //    return;

			registryService.DeleteSubTree(@$"\{bankGuid}");
			if (bankLinks.ContainsKey(bankGuid))
			{
				bankLinks[bankGuid].NewBankLinkStatus -= LinkedBank_NewBankLinkStatus;
				bankLinks.Remove(bankGuid);
			}
			NewBankLinkStatusForGuid?.Invoke(this, (bankGuid, BankLinkStatus.NotLinked));
		}

        private async Task<(bool success, string accessCode)> ListenForAuthorisationCode(Guid bankGuid, CancellationToken token)
		{
            var listener = new HttpListener();
            listener.Prefixes.Add(callbackUri);
            listener.Start();

            HttpListenerContext? context = null;
            try
            {
                context = await listener.GetContextAsync().AsCancellable(token);
            }
            catch (TaskCanceledException)
            {
                listener.Stop();
                return (false, string.Empty);
            }

            string html = string.Format("<html><body></body></html>");
            var buffer = Encoding.UTF8.GetBytes(html);
            context.Response.ContentLength64 = buffer.Length;
            var stream = context.Response.OutputStream;
            var responseTask = stream.WriteAsync(buffer, 0, buffer.Length).ContinueWith((task) =>
            {
                stream.Close();
                listener.Stop();
            });

            string? authCode = context.Request.QueryString["code"];

            return (authCode != null, authCode ?? string.Empty);
        }

        private async Task<(bool success, string accessToken, string refreshToken, DateTime expiresIn)> SwapCodeForAccessTokens(string authCode)
		{
            var request = new HttpRequestMessage(new HttpMethod("POST"), "https://auth.truelayer.com/connect/token");
            request.Headers.TryAddWithoutValidation("Accept", "application/json");
            request.Content = new StringContent("{ \"grant_type\": \"authorization_code\", \"client_id\": \"" + clientId + "\", \"client_secret\": \"" + clientSecret + "\", \"code\": \"" + authCode + "\", \"redirect_uri\": \"" + callbackUri + "callback\" }");
            request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");

            var response = await httpClient.SendAsync(request);

            request.Dispose();

            var success = response.StatusCode == HttpStatusCode.OK;

            if (!success)
                return (success, string.Empty, string.Empty, DateTime.Now);

            var content = await response.Content.ReadAsStringAsync();

            var accessToken = GetValueFromContent("access_token", content);
            var expiresIn = DateTime.Now + TimeSpan.FromSeconds(int.Parse(GetValueFromContent("expires_in", content)));
            var refreshToken = GetValueFromContent("refresh_token", content);
            var tokenType = GetValueFromContent("token_type", content);

            success = tokenType == "Bearer";

            return (success, accessToken, refreshToken, expiresIn);
        }

        private async Task<bool> RefreshBankLinkIfNeeded(Guid bankGuid)
        {
            if (bankLinks[bankGuid].AccessExpires - DateTime.Now > TimeSpan.FromMinutes(1))
                return false;

            var request = new HttpRequestMessage(new HttpMethod("POST"), "https://auth.truelayer.com/connect/token");
            request.Headers.TryAddWithoutValidation("Accept", "application/json");
            request.Content = new StringContent("{ \"grant_type\": \"refresh_token\", \"client_id\": \"" + clientId + "\", \"client_secret\": \"" + clientSecret + "\", \"refresh_token\": \"" + bankLinks[bankGuid].RefreshToken + "\" }");
            request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");

            var response = await httpClient.SendAsync(request);

            request.Dispose();

            var success = response.StatusCode == HttpStatusCode.OK;

            if (!success)
                return false;

            var content = await response.Content.ReadAsStringAsync();

            bankLinks[bankGuid].AccessToken = GetValueFromContent("access_token", content);
            bankLinks[bankGuid].AccessExpires = DateTime.Now + TimeSpan.FromSeconds(int.Parse(GetValueFromContent("expires_in", content)));
            bankLinks[bankGuid].RefreshToken = GetValueFromContent("refresh_token", content);
            var tokenType = GetValueFromContent("token_type", content);

            success = tokenType == "Bearer";

            return success;
        }

        private async Task<BankLinkStatus> TestBankLinkStatus(Guid bankGuid)
		{
            var request = new HttpRequestMessage(new HttpMethod("GET"), "https://api.truelayer.com/data/v1/me");
            request.Headers.TryAddWithoutValidation("Accept", "application/json");
            request.Headers.TryAddWithoutValidation("Authorization", $"Bearer {bankLinks[bankGuid].AccessToken}");

            var response = await httpClient.SendAsync(request);
            var content = await response.Content.ReadAsStringAsync();

            request.Dispose();

            var linked = response.StatusCode == HttpStatusCode.OK && GetValueFromContent("client_id", content) == clientId;
            return linked ? BankLinkStatus.LinkVerified : BankLinkStatus.LinkBroken;
		}

        private string GetValueFromContent(string valueName, string content)
		{
            var contentArray = content.Split(',');
            return contentArray.First(x => x.Split(':')[0].Contains(valueName)).Split(':')[1].Replace("\"", "").Trim(new char[] { '{', '}' });
        }

        private async void BankLinkValidationTimer_Elapsed(object? sender, System.Timers.ElapsedEventArgs e)
        {
            foreach (var bankLink in bankLinks.Values.Where(x => x.BankLinkStatus != BankLinkStatus.Linking && x.BankLinkStatus != BankLinkStatus.LinkingCancelled))
            {
                try
                {
                    bankLink.BankLinkStatus = await TestBankLinkStatus(bankLink.Guid);
                    await RefreshBankLinkIfNeeded(bankLink.Guid);
                }
                catch { /* Probably deleted */ }
            }
        }

        private void LinkedBank_NewBankLinkStatus(object? sender, BankLinkStatus e)
        {
            NewBankLinkStatusForGuid?.Invoke(this, ((sender as LinkedBankModel).Guid, e));
        }
    }
}
