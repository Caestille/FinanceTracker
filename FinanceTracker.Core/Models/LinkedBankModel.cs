using CoreUtilities.Interfaces;
using System.ComponentModel;

namespace FinanceTracker.Core.Models
{
    public enum BankLinkStatus
    {
		[Description("Link bank")]
        NotLinked,

        [Description("Linking")]
        Linking,

        [Description("Cancelled")]
        LinkingCancelled,

        [Description("Linked")]
        LinkVerified,

        [Description("Link broken")]
        LinkBroken
    }

    public class LinkedBankModel
    {
        public event EventHandler<BankLinkStatus> NewBankLinkStatus;

        private IRegistryService registryService;

        private Guid guid;
        public Guid Guid
        {
            get => guid;
            set => guid = value;
        }

        private string authorisationCode;
        public string AuthorisationCode
        {
            get => authorisationCode;
            set
            {
                authorisationCode = value;
                registryService.SetSetting(nameof(AuthorisationCode), value, $@"\{guid}");
            }
        }

        private string accessToken;
        public string AccessToken
        {
            get => accessToken;
            set
            {
                accessToken = value;
                registryService.SetSetting(nameof(AccessToken), value, $@"\{guid}");
            }
        }

        private string refreshToken;
        public string RefreshToken
        {
            get => refreshToken;
            set
            {
                refreshToken = value;
                registryService.SetSetting(nameof(RefreshToken), value, $@"\{guid}");
            }
        }

        private DateTime accessExpires;
        public DateTime AccessExpires
        {
            get => accessExpires;
            set
            {
                accessExpires = value;
                registryService.SetSetting(nameof(AccessExpires), value.ToString(), $@"\{guid}");
            }
        }

        private BankLinkStatus bankLinkStatus = BankLinkStatus.NotLinked;
        public BankLinkStatus BankLinkStatus
        {
            get => bankLinkStatus;
            set
            {
                if (bankLinkStatus != value)
				{
                    NewBankLinkStatus?.Invoke(this, value);
				}
                bankLinkStatus = value;
            }
        }

        public LinkedBankModel(IRegistryService registryService, Guid guid)
        {
            this.registryService = registryService;
            Guid = guid;
        }

        public bool LoadFromRegistry()
        {
            registryService.TryGetSetting(nameof(AuthorisationCode), string.Empty, out var authCode, $@"\{guid}");
            AuthorisationCode = authCode;
            registryService.TryGetSetting(nameof(AccessToken), string.Empty, out var accessToken, $@"\{guid}");
            AccessToken = accessToken;    
            registryService.TryGetSetting(nameof(RefreshToken), string.Empty, out var refreshToken, $@"\{guid}");
            RefreshToken = refreshToken;  
            registryService.TryGetSetting(nameof(AccessExpires), DateTime.Now, out var accessExpires, $@"\{guid}");
            AccessExpires = accessExpires;

            return AuthorisationCode != string.Empty;
        }
    }
}
