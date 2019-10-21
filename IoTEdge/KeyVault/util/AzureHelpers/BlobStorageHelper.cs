using System;
using System.Threading.Tasks;

namespace AzureHelpers
{
    public class BlobStorageHelper
    {
        private string accountName;
        private string key;

        public BlobStorageHelper(string accountName, string key)
        {
            this.accountName = accountName;
            this.key = key;
        }

        public async Task<string> GetToken(string containerName, string blobName)
        {
            string token = "";

            try
            {

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return token;
        }
    }
}