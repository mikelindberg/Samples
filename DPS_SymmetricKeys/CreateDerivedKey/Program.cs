using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;

namespace CreateDerivedKey
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length != 5)
                Console.WriteLine("You have not specified all required values service-endpoint idScope registrationid enrollment-group_primarykey enrollment-group_secondarykey");
            else
            {
                Console.WriteLine("Derived Key creation starting!");
                String serviceEndpoint = args[0];
                String idScope = args[1];
                String registrationId = args[2];
                String primaryDPSkey = args[3];
                String secondaryDPSKey = args[4];

                String primaryDerivedKey = ComputeDerivedSymmetricKey(Convert.FromBase64String(primaryDPSkey), registrationId);
                String secondaryDerivedKey = ComputeDerivedSymmetricKey(Convert.FromBase64String(secondaryDPSKey), registrationId);

                WriteSecret(serviceEndpoint,idScope,registrationId, primaryDerivedKey, secondaryDerivedKey);
                Console.WriteLine("Finished creating secrets and writing to device");
            }
        }

        public static string ComputeDerivedSymmetricKey(byte[] masterKey, string registrationId)
        {
            using (var hmac = new HMACSHA256(masterKey))
            {
                return Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes(registrationId)));
            }
        }

        //Instead of the method below you should call some logic - 
        //to store your derived keys and registration id on your device
        public static void WriteSecret(string serviceEndpoint, string idScope,string registrationId, string primaryDerivedKey, string secondaryDerivedKey)
        {
            var secretObject = new
            {
                serviceEndpoint,
                idScope,
                registrationId,
                primaryDerivedKey,
                secondaryDerivedKey
            };

            File.WriteAllText(@".\secrets.json", JsonConvert.SerializeObject(secretObject));
        }
    }
}