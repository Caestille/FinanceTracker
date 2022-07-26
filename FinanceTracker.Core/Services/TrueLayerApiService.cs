using CoreUtilities.Interfaces;
using FinanceTracker.Core.Interfaces;
using FinanceTracker.Core.Models;
using System.Diagnostics;
using System.Net;

namespace FinanceTracker.Core.Services
{
	public class TrueLayerApiService : IBankApiService
	{
        public event EventHandler<(Guid, BankLinkStatus)> NewBankLinkStatusForGuid;

        private string liveAuthLink = "https://auth.truelayer.com/?response_type=code&client_id=josephward-732872&scope=info%20accounts%20balance%20cards%20transactions%20direct_debits%20standing_orders%20offline_access&redirect_uri=http://localhost:3000/callback&providers=uk-ob-all%20uk-oauth-all";
        private string clientId = "josephward-732872";
        private string clientSecret = "035d9190-3341-4181-99eb-efee840456b4";
        private string callbackUri = @"http://localhost:3000/";

        private bool isRequestExecuting;

        private IRegistryService registryService;
        private IHttpService httpService;
        private Dictionary<Guid, LinkedBankModel> bankLinks = new Dictionary<Guid, LinkedBankModel>();

        private System.Timers.Timer bankLinkValidationTimer = new System.Timers.Timer(1000);

        public TrueLayerApiService(IRegistryService registryService, IHttpService httpService)
		{
            this.registryService = registryService;
            this.httpService = httpService;

            bankLinkValidationTimer.AutoReset = true;
			bankLinkValidationTimer.Elapsed += BankLinkValidationTimer_Elapsed;
            bankLinkValidationTimer.Start();
        }

		public async Task<bool> LinkBank(Guid bankGuid, CancellationToken token)
        {
            isRequestExecuting = true;
            var linkedBank = new LinkedBankModel(registryService, bankGuid);
            linkedBank.NewBankLinkStatus += LinkedBank_NewBankLinkStatus;
            linkedBank.BankLinkStatus = BankLinkStatus.Linking;
            bankLinks[bankGuid] = linkedBank;

            Process.Start(new ProcessStartInfo()
            {
                FileName = liveAuthLink,
                UseShellExecute = true,
            });

            (bool success, string authCode) = await httpService.WaitForAndQueryResponseOverUri(callbackUri, "code", token);

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

            isRequestExecuting = false;
            return success;
        }

		public void ReloadBankLinkDetails(Guid bankGuid)
        {
            isRequestExecuting = true;

            var linkedBank = new LinkedBankModel(registryService, bankGuid);
            linkedBank.NewBankLinkStatus += LinkedBank_NewBankLinkStatus;
            if (linkedBank.LoadFromRegistry())
			{
                bankLinks[bankGuid] = linkedBank;
            }

            isRequestExecuting = false;
        }

        public async Task DeleteLink(Guid bankGuid)
        {
            isRequestExecuting = true;

            var request = httpService.GetHttpRequestBuilder()
                .CreateRequest(IHttpRequestBuilder.HttpCommandType.Delete, "https://auth.truelayer.com/api/delete")
                .WithUnvalidatedHeader("Accept", "application/json")
                .WithUnvalidatedHeader("Authorization", $"Bearer {bankLinks[bankGuid].AccessToken}")
                .Build();

            (HttpStatusCode status, string response) = await httpService.SendAsyncDisposeAndGetResponse(request);

            // TODO: what if deleting fails?

			registryService.DeleteSubTree(@$"\{bankGuid}");
			if (bankLinks.ContainsKey(bankGuid))
			{
				bankLinks[bankGuid].NewBankLinkStatus -= LinkedBank_NewBankLinkStatus;
				bankLinks.Remove(bankGuid);
			}
			NewBankLinkStatusForGuid?.Invoke(this, (bankGuid, BankLinkStatus.NotLinked));

            isRequestExecuting = false;
        }

        private async Task<(bool success, string accessToken, string refreshToken, DateTime expiresIn)> SwapCodeForAccessTokens(string authCode)
        {
            isRequestExecuting = true;

            var request = httpService.GetHttpRequestBuilder()
                .CreateRequest(IHttpRequestBuilder.HttpCommandType.Post, "https://auth.truelayer.com/connect/token")
                .WithUnvalidatedHeader("Accept", "application/json")
                .WithContent("grant_type", "authorization_code")
                .WithContent("client_id", clientId)
                .WithContent("client_secret", clientSecret)
                .WithContent("code", authCode)
                .WithContent("redirect_uri", callbackUri + "callback")
                .Build();

            (HttpStatusCode status, string response) = await httpService.SendAsyncDisposeAndGetResponse(request);

            var success = status == HttpStatusCode.OK;

            if (!success)
                return (success, string.Empty, string.Empty, DateTime.Now);

            var accessToken = httpService.QueryValueFromResponse("access_token", response);
            var expiresIn = DateTime.Now + TimeSpan.FromSeconds(int.Parse(httpService.QueryValueFromResponse("expires_in", response)));
            var refreshToken = httpService.QueryValueFromResponse("refresh_token", response);
            var tokenType = httpService.QueryValueFromResponse("token_type", response);

            success = tokenType == "Bearer";

            isRequestExecuting = false;
            return (success, accessToken, refreshToken, expiresIn);
        }

        private async Task<bool> RefreshBankLinkIfNeeded(Guid bankGuid)
        {
            if (!bankLinks[bankGuid].IsAboutToExpire())
                return false;

            isRequestExecuting = true;

            var request = httpService.GetHttpRequestBuilder()
                .CreateRequest(IHttpRequestBuilder.HttpCommandType.Post, "https://auth.truelayer.com/connect/token")
                .WithUnvalidatedHeader("Accept", "application/json")
                .WithContent("grant_type", "refresh_token")
                .WithContent("client_id", clientId)
                .WithContent("client_secret", clientSecret)
                .WithContent("refresh_token", bankLinks[bankGuid].RefreshToken)
                .Build();

            (HttpStatusCode status, string response) = await httpService.SendAsyncDisposeAndGetResponse(request);

            var success = status == HttpStatusCode.OK;

            if (!success)
                return false;

            bankLinks[bankGuid].AccessToken = httpService.QueryValueFromResponse("access_token", response);
            bankLinks[bankGuid].AccessExpires = DateTime.Now + TimeSpan.FromSeconds(int.Parse(httpService.QueryValueFromResponse("expires_in", response)));
            bankLinks[bankGuid].RefreshToken = httpService.QueryValueFromResponse("refresh_token", response);
            var tokenType = httpService.QueryValueFromResponse("token_type", response);

            success = tokenType == "Bearer";

            isRequestExecuting = false;
            return success;
        }

        private async Task<BankLinkStatus> GetBankLinkStatus(Guid bankGuid)
        {
            isRequestExecuting = true;

            var request = httpService.GetHttpRequestBuilder()
                .CreateRequest(IHttpRequestBuilder.HttpCommandType.Get, "https://api.truelayer.com/data/v1/me")
                .WithUnvalidatedHeader("Accept", "application/json")
                .WithUnvalidatedHeader("Authorization", $"Bearer {bankLinks[bankGuid].AccessToken}")
                .Build();

            (HttpStatusCode status, string response) = await httpService.SendAsyncDisposeAndGetResponse(request);

            var linked = status == HttpStatusCode.OK && httpService.QueryValueFromResponse("client_id", response) == clientId;

            isRequestExecuting = false;
            return linked ? BankLinkStatus.LinkVerified : BankLinkStatus.LinkBroken;
		}

        private async void BankLinkValidationTimer_Elapsed(object? sender, System.Timers.ElapsedEventArgs e)
        {
            if (isRequestExecuting)
                return;

            foreach (var bankLink in bankLinks.Values.Where(x => x.BankLinkStatus != BankLinkStatus.Linking && x.BankLinkStatus != BankLinkStatus.LinkingCancelled))
            {
                try
                {
                    bankLink.BankLinkStatus = await GetBankLinkStatus(bankLink.Guid);
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
