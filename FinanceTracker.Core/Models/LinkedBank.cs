using CoreUtilities.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinanceTracker.Core.Models
{
    public enum BankLinkStatus
    {
        NotConnected,
        LinkVerified,
        LinkBroken
    }

    public class LinkedBankModel
    {
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

        private BankLinkStatus bankLinkStatus;
        public BankLinkStatus BankLinkStatus
        {
            get => bankLinkStatus;
            set => bankLinkStatus = value;
        }

        public LinkedBankModel(IRegistryService registryService, Guid guid)
        {
            this.registryService = registryService;
            Guid = guid;
        }

        public LinkedBankModel(IRegistryService registryService, Guid guid, string authCode, string accessToken, string refreshToken, DateTime accessExpires, BankLinkStatus linkStatus)
        {
            this.registryService = registryService;

            Guid = guid;
            AuthorisationCode = authCode;
            AccessToken = accessToken;
            RefreshToken = refreshToken;
            AccessExpires = accessExpires;
            BankLinkStatus = linkStatus;
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
