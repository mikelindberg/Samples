using System;
using System.Linq;
using Microsoft.Azure.KeyVault;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.Azure.KeyVault.Models;

namespace AzureHelpers
{
    public class KeyVaultHelper
    {
        private KeyVaultClient keyVaultClient;
        private string keyVaultUrl;
        private string clientId;
        private string clientSecret;
        private string[] secretNames;

        public KeyVaultHelper(string keyVaultUrl, string clientId, string clientSecret, string[] secretNames)
        {
            this.keyVaultUrl = keyVaultUrl;
            this.clientId = clientId;
            this.clientSecret = clientSecret;
            this.secretNames = secretNames;
        }

        private void configureKeyVaultClient()
        {
            keyVaultClient = new KeyVaultClient(async (authority, resource, scope) =>
            {
                var adCredential = new ClientCredential(clientId, clientSecret);
                var authenticationContext = new AuthenticationContext(authority, null);
                return (await authenticationContext.AcquireTokenAsync(resource, adCredential)).AccessToken;
            });
        }

        public async Task<SecretBundle> GetSecret(string secretName)
        {
            if (keyVaultClient == null)
                configureKeyVaultClient();

            if (secretNames.Contains(secretName))
            {
                var keyvaultSecret = await keyVaultClient.GetSecretAsync($"{keyVaultUrl}", secretName).ConfigureAwait(false);
                return keyvaultSecret;
            }
            else
                return null;
        }
    }
}