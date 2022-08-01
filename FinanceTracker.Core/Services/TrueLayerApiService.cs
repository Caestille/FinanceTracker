using CoreUtilities.Interfaces;
using FinanceTracker.Core.DataTypeObjects;
using FinanceTracker.Core.Interfaces;
using FinanceTracker.Core.Models;
using System.Diagnostics;
using System.Net;
using System.Text.Json;
using System.Windows;

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
		private string publicIp;

		private IRegistryService registryService;
		private IHttpService httpService;
		private Dictionary<Guid, LinkedBankModel> bankLinks = new Dictionary<Guid, LinkedBankModel>();

		private CancellationTokenSource tokenSource = new CancellationTokenSource();

		private System.Timers.Timer bankLinkValidationTimer = new System.Timers.Timer(1000);

		public TrueLayerApiService(IRegistryService registryService, IHttpService httpService)
		{
			this.registryService = registryService;
			this.httpService = httpService;

			bankLinkValidationTimer.AutoReset = true;
			bankLinkValidationTimer.Elapsed += BankLinkValidationTimer_Elapsed;
			bankLinkValidationTimer.Start();

			publicIp = GetPublicIP();
		}

		#region Linking

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

			(bool success, string authCode) = await httpService.WaitForAndQueryResponseOverUri(callbackUri, "code", token);

			if (!success)
			{
				bankLinks[bankGuid].BankLinkStatus = BankLinkStatus.LinkingCancelled;
				await Task.Delay(2000);
				await DeleteLink(bankGuid);
				return success;
			}

			(success, string accessToken, string refreshToken, DateTime accessExpires) = await SwapAuthCodeForAccessToken(authCode);

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
				bankLinks[bankGuid].BankLinkStatus = BankLinkStatus.LinkingCancelled;
				await Task.Delay(2000);
				await DeleteLink(bankGuid);
			}

			return success;
		}

		public void TryReloadBankLinkDetails(Guid bankGuid)
		{
			BlockingPolicy(() =>
			{
				var linkedBank = new LinkedBankModel(registryService, bankGuid);
				linkedBank.NewBankLinkStatus += LinkedBank_NewBankLinkStatus;
				if (linkedBank.LoadFromRegistry())
				{
					bankLinks[bankGuid] = linkedBank;
				}
			});
		}

		public async Task DeleteLink(Guid bankGuid)
		{
			await BlockingPolicy(async () =>
			{
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
			});
		}

		private async Task<(bool success, string accessToken, string refreshToken, DateTime expiresIn)> SwapAuthCodeForAccessToken(string authCode)
		{
			return await BlockingPolicy(async () =>
			{
				var request = httpService.GetHttpRequestBuilder()
					.CreateRequest(IHttpRequestBuilder.HttpCommandType.Post, "https://auth.truelayer.com/connect/token")
						.WithUnvalidatedHeader("Accept", "application/json")
						.WithContent("grant_type", "authorization_code")
						.WithContent("client_id", clientId)
						.WithContent("client_secret", clientSecret)
						.WithContent("code", authCode)
						.WithContent("redirect_uri", callbackUri + "callback")
						.WithHeaderContentType("application/json")
							.Build();

				(HttpStatusCode status, string response) = await httpService.SendAsyncDisposeAndGetResponse(request);

				var success = status == HttpStatusCode.OK;

				if (!success)
					return (success, string.Empty, string.Empty, DateTime.Now);

				var tokenDetails = JsonSerializer.Deserialize<ConnectionOAuthTokenDto>(response);

				var accessToken = tokenDetails.AccessToken;
				var expiresIn = DateTime.Now + TimeSpan.FromSeconds(tokenDetails.ExpiresInS);
				var refreshToken = tokenDetails.RefreshToken;
				success = tokenDetails.TokenType == "Bearer";

				return (success, accessToken, refreshToken, expiresIn);
			});
		}

		private async Task<bool> RefreshBankLinkIfNeeded(Guid bankGuid)
		{
			return await BlockingPolicy(async () =>
			{
				if (!bankLinks[bankGuid].IsAboutToExpire())
					return false;

				var request = httpService.GetHttpRequestBuilder()
					.CreateRequest(IHttpRequestBuilder.HttpCommandType.Post, "https://auth.truelayer.com/connect/token")
						.WithUnvalidatedHeader("Accept", "application/json")
						.WithContent("grant_type", "refresh_token")
						.WithContent("client_id", clientId)
						.WithContent("client_secret", clientSecret)
						.WithContent("refresh_token", bankLinks[bankGuid].RefreshToken)
						.WithHeaderContentType("application/json")
							.Build();

				(HttpStatusCode status, string response) = await httpService.SendAsyncDisposeAndGetResponse(request, tokenSource.Token);

				var success = status == HttpStatusCode.OK;

				if (!success)
					return false;

				var tokenDetails = JsonSerializer.Deserialize<ConnectionOAuthTokenDto>(response);

				bankLinks[bankGuid].AccessToken = tokenDetails.AccessToken;
				bankLinks[bankGuid].AccessExpires = DateTime.Now + TimeSpan.FromSeconds(tokenDetails.ExpiresInS);
				bankLinks[bankGuid].RefreshToken = tokenDetails.RefreshToken;

				success = tokenDetails.TokenType == "Bearer";
				return success;
			});
		}

		private async Task<BankLinkStatus> GetBankLinkStatus(Guid bankGuid)
		{
			return await BlockingPolicy(async () =>
			{
				var request = httpService.GetHttpRequestBuilder()
					.CreateRequest(IHttpRequestBuilder.HttpCommandType.Get, "https://api.truelayer.com/data/v1/me")
						.WithUnvalidatedHeader("Accept", "application/json")
						.WithUnvalidatedHeader("Authorization", $"Bearer {bankLinks[bankGuid].AccessToken}")
							.Build();

				(HttpStatusCode status, string response) = await httpService.SendAsyncDisposeAndGetResponse(request, tokenSource.Token);
				var connectionData = JsonSerializer.Deserialize<ResultsHostDto<ConnectionMetadataDto>>(response).Results;

				return status == HttpStatusCode.OK && connectionData.FirstOrDefault()?.ClientId == clientId;
			}) ? BankLinkStatus.LinkVerified : BankLinkStatus.LinkBroken;
		}

		#endregion

		#region Download data

		public async Task<IEnumerable<AccountDto>> GetAccounts(Guid bankGuid)
		{
			return await BlockingPolicy(async () =>
			{
				var request = httpService.GetHttpRequestBuilder()
					.CreateRequest(IHttpRequestBuilder.HttpCommandType.Get, "https://api.truelayer.com/data/v1/accounts")
						.WithUnvalidatedHeader("Accept", "application/json")
						.WithUnvalidatedHeader("Authorization", $"Bearer {bankLinks[bankGuid].AccessToken}")
							.Build();

				(HttpStatusCode status, string response) = await httpService.SendAsyncDisposeAndGetResponse(request, tokenSource.Token);

				var success = status == HttpStatusCode.OK;

				var result = new List<AccountDto>();

				if (success)
				{
					var accounts = JsonSerializer.Deserialize<ResultsHostDto<AccountDto>>(response).Results;
					Dictionary<string, string> accountIdMaps = new Dictionary<string, string>();
					foreach (var account in accounts)
					{
						accountIdMaps[account.DisplayName] = account.AccountId;
					}
					bankLinks[bankGuid].AccountNamesAndIds = accountIdMaps;
					result.AddRange(accounts);
				}

				return result;
			});
		}

		public async Task<IEnumerable<TransactionDto>> GetTransactions(Guid bankGuid, string accountName)
		{
			return await BlockingPolicy(async () =>
			{
				var accountId = bankLinks[bankGuid].AccountNamesAndIds[accountName];

				var result = new List<TransactionDto>();
				var request = httpService.GetHttpRequestBuilder()
				.CreateRequest(IHttpRequestBuilder.HttpCommandType.Get,
					$"https://api.truelayer.com/data/v1/accounts/{accountId}/transactions?to={(DateTime.Now - TimeSpan.FromHours(6)).ToString("yyyy-MM-dd")}&from={(DateTime.Now - TimeSpan.FromDays(2 * 367)).ToString("yyyy-MM-dd")}")
					.WithUnvalidatedHeader("Accept", "application/json")
					.WithUnvalidatedHeader("Authorization", $"Bearer {bankLinks[bankGuid].AccessToken}")
					.WithUnvalidatedHeader("X-PSU-IP", publicIp)
						.Build();

				(HttpStatusCode status, string response) = await httpService.SendAsyncDisposeAndGetResponse(request, CancellationToken.None);

				var success = status == HttpStatusCode.OK;

				if (success)
				{
					var transactions = JsonSerializer.Deserialize<ResultsHostDto<TransactionDto>>(response).Results;
					result.AddRange(transactions.Where(x => !result.Any(y => y.NormalisedProviderId == x.NormalisedProviderId)));
				}

				return result;
			});
		}

		public async Task<IEnumerable<CardDto>> GetCards(Guid bankGuid)
		{
			return await BlockingPolicy(async () =>
			{
				var request = httpService.GetHttpRequestBuilder()
					.CreateRequest(IHttpRequestBuilder.HttpCommandType.Get, "https://api.truelayer.com/data/v1/cards")
						.WithUnvalidatedHeader("Accept", "application/json")
						.WithUnvalidatedHeader("Authorization", $"Bearer {bankLinks[bankGuid].AccessToken}")
							.Build();

				(HttpStatusCode status, string response) = await httpService.SendAsyncDisposeAndGetResponse(request, tokenSource.Token);

				var success = status == HttpStatusCode.OK;

				var result = new List<CardDto>();

				if (success)
				{
					var cards = JsonSerializer.Deserialize<ResultsHostDto<CardDto>>(response).Results;
					Dictionary<string, string> accountIdMaps = new Dictionary<string, string>();
					foreach (var card in cards)
					{
						accountIdMaps[card.DisplayName] = card.AccountId;
					}
					bankLinks[bankGuid].AccountNamesAndIds = accountIdMaps;
					result.AddRange(cards);
				}

				return result;
			});
		}

		public async Task<IEnumerable<TransactionDto>> GetCardTransactions(Guid bankGuid, string accountName)
		{
			return await BlockingPolicy(async () =>
			{
				var accountId = bankLinks[bankGuid].AccountNamesAndIds[accountName];

				var result = new List<TransactionDto>();
				var request = httpService.GetHttpRequestBuilder()
				.CreateRequest(IHttpRequestBuilder.HttpCommandType.Get,
					$"https://api.truelayer.com/data/v1/cards/{accountId}/transactions?to={(DateTime.Now - TimeSpan.FromHours(6)).ToString("yyyy-MM-dd")}&from={(DateTime.Now - TimeSpan.FromDays(2 * 367)).ToString("yyyy-MM-dd")}")
					.WithUnvalidatedHeader("Accept", "application/json")
					.WithUnvalidatedHeader("Authorization", $"Bearer {bankLinks[bankGuid].AccessToken}")
					.WithUnvalidatedHeader("X-PSU-IP", publicIp)
						.Build();

				(HttpStatusCode status, string response) = await httpService.SendAsyncDisposeAndGetResponse(request, CancellationToken.None);

				var success = status == HttpStatusCode.OK;

				if (success)
				{
					var transactions = JsonSerializer.Deserialize<ResultsHostDto<TransactionDto>>(response).Results;
					result.AddRange(transactions.Where(x => !result.Any(y => y.NormalisedProviderId == x.NormalisedProviderId)));
				}

				return result;
			});
		}

		#endregion

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
				catch { /* Guid deleted */ }
			}
		}

		private void LinkedBank_NewBankLinkStatus(object? sender, BankLinkStatus e)
		{
			NewBankLinkStatusForGuid?.Invoke(this, ((sender as LinkedBankModel).Guid, e));
		}

		public void Dispose()
		{
			bankLinkValidationTimer.Stop();
			bankLinkValidationTimer.Dispose();
		}

		private void BlockingPolicy(Action functor)
		{
			isRequestExecuting = true;
			tokenSource.Cancel();
			tokenSource = new CancellationTokenSource();
			functor();
			isRequestExecuting = false;
		}

		private async Task BlockingPolicy(Func<Task> functor)
		{
			isRequestExecuting = true;
			tokenSource.Cancel();
			tokenSource = new CancellationTokenSource();
			await functor();
			isRequestExecuting = false;
		}

		private T BlockingPolicy<T>(Func<T> functor)
		{
			isRequestExecuting = true;
			tokenSource.Cancel();
			tokenSource = new CancellationTokenSource();
			var result = functor();
			isRequestExecuting = false;
			return result;
		}

		private async Task<T> BlockingPolicy<T>(Func<Task<T>> functor)
		{
			isRequestExecuting = true;
			tokenSource.Cancel();
			tokenSource = new CancellationTokenSource();
			var result = await functor();
			isRequestExecuting = false;
			return result;
		}

		public static string GetPublicIP()
		{
			try
			{
				string url = "http://checkip.dyndns.org";
				WebRequest req = WebRequest.Create(url);
				WebResponse resp = req.GetResponse();
				StreamReader sr = new StreamReader(resp.GetResponseStream());
				string response = sr.ReadToEnd().Trim();
				string[] a = response.Split(':');
				string a2 = a[1].Substring(1);
				string[] a3 = a2.Split('<');
				string a4 = a3[0];
				return a4;
			}
			catch
			{
				return "";
			}
		}
	}
}
