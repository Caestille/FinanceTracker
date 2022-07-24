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
        private string liveAuthLink = "https://auth.truelayer.com/?response_type=code&client_id=josephward-732872&scope=info%20accounts%20balance%20cards%20transactions%20direct_debits%20standing_orders%20offline_access&redirect_uri=http://localhost:3000/callback&providers=uk-ob-all%20uk-oauth-all";
        private string clientId = "josephward-732872";
        private string clientSecret = "035d9190-3341-4181-99eb-efee840456b4";
        private string callbackUri = @"http://localhost:3000/";

        private IRegistryService registryService;
        private Dictionary<Guid, LinkedBankModel> bankLinks = new Dictionary<Guid, LinkedBankModel>();
        HttpClient httpClient = new HttpClient();

        public TrueLayerApiService(IRegistryService registryService)
		{
            this.registryService = registryService;
        }

		public async Task<bool> LinkBank(Guid bankGuid, CancellationToken token)
		{
            Process.Start(new ProcessStartInfo()
            {
                FileName = liveAuthLink,
                UseShellExecute = true,
            });

            (bool success, string accessCode) = await ListenForAccessCode(token);

            if (!success)
                return success;

            (success, string accessToken, string refreshToken, DateTime expiresIn) = await SwapCodeForAccessTokens(accessCode);

            bankLinks[bankGuid] = new LinkedBankModel(registryService, bankGuid, accessCode, accessToken, refreshToken, expiresIn, BankLinkStatus.NotConnected);
            bankLinks[bankGuid].BankLinkStatus = TestBankLinkStatus(bankGuid);

            return success;
        }

        public void RefreshLink(Guid bankGuid)
		{
            var linkedBank = new LinkedBankModel(registryService, bankGuid);
            if (linkedBank.LoadFromRegistry())
			{
                bankLinks[bankGuid] = linkedBank;
            }
		}

        public void DeleteLink(Guid bankGuid)
		{
            registryService.DeleteSubTree(@$"\{bankGuid}");
		}

        private async Task<(bool success, string accessCode)> ListenForAccessCode(CancellationToken token)
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

            string? accessCode = context.Request.QueryString["code"];

            return (accessCode != null, accessCode ?? string.Empty);
        }

        private async Task<(bool success, string accessToken, string refreshToken, DateTime expiresIn)> SwapCodeForAccessTokens(string accessCode)
		{
            var request = new HttpRequestMessage(new HttpMethod("POST"), "https://auth.truelayer.com/connect/token");
            request.Headers.TryAddWithoutValidation("Accept", "application/json");

            request.Content = new StringContent("{ \"grant_type\": \"authorization_code\", \"client_id\": \"" + clientId + "\", \"client_secret\": \"" + clientSecret + "\", \"code\": \"" + accessCode + "\", \"redirect_uri\": \"" + callbackUri + "callback\" }");
            request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");

            var response = await httpClient.SendAsync(request);

            request.Dispose();

            var success = response.StatusCode == HttpStatusCode.OK;

            if (!success)
                return (success, string.Empty, string.Empty, DateTime.Now);

            var accessToken = response.Headers.FirstOrDefault(x => x.Key == "access_token").Value.First();
            var expiresIn = DateTime.Now + TimeSpan.FromSeconds(int.Parse(response.Headers.FirstOrDefault(x => x.Key == "expires_in").Value.First()));
            var refreshToken = response.Headers.FirstOrDefault(x => x.Key == "refresh_token").Value.First();
            var tokenType = response.Headers.FirstOrDefault(x => x.Key == "token_type").Value.First();

            success = tokenType == "Bearer";

            return (success, accessToken, refreshToken, expiresIn);
        }

        private BankLinkStatus TestBankLinkStatus(Guid bankGuid)
		{
            var linked = true;
            return linked ? BankLinkStatus.LinkVerified : BankLinkStatus.LinkBroken;
		}
    }
}
