using Microsoft.Identity.Client;
using Newtonsoft.Json.Linq;
using System.Security;

namespace Core.Entities
{
    public class MyInformation
    {
        public MyInformation(IPublicClientApplication app, HttpClient client, string microsoftGraphBaseEndpoint)
        {
            tokenAcquisitionHelper = new PublicAppUsingUsernamePassword(app);
            protectedApiCallHelper = new ProtectedApiCallHelper(client);
            MicrosoftGraphBaseEndpoint = microsoftGraphBaseEndpoint;
        }

        public PublicAppUsingUsernamePassword tokenAcquisitionHelper;

        public ProtectedApiCallHelper protectedApiCallHelper;

        /// <summary>
        /// Scopes to request access to the protected Web API (here Microsoft Graph)
        /// </summary>
        private static string[] Scopes { get; set; } = new string[] { "User.Read", "User.ReadBasic.All" };

        /// <summary>
        /// Base endpoint for Microsoft Graph
        /// </summary>
        private string MicrosoftGraphBaseEndpoint { get; set; }

        /// <summary>
        /// URLs of the protected Web APIs to call (here Microsoft Graph endpoints)
        /// </summary>
        public string WebApiUrlMe { get { return $"{MicrosoftGraphBaseEndpoint}/v1.0/me"; } } 
        public string WebApiUrlMyManager { get { return $"{MicrosoftGraphBaseEndpoint}/v1.0/me/manager"; } }

        public async Task<AuthenticationResult> AdquireTokenFromUserNameAndPasswordAsync(string requestUsername, string requestPassword)
        {

            string username = requestUsername;

            SecureString password = buildPassword(requestPassword);

            return await tokenAcquisitionHelper.AcquireATokenFromCacheOrUsernamePasswordAsync(Scopes, username, password);
        }






        //public async Task<JObject> DisplayMeAndMyManagerAsync(string requestUsername, string requestPassword)
        //{
        //    JObject result = null;

        //    string username = requestUsername;

        //    SecureString password = buildPassword(requestPassword);

        //    AuthenticationResult authenticationResult = await tokenAcquisitionHelper.AcquireATokenFromCacheOrUsernamePasswordAsync(Scopes, username, password);
        //    try
        //    {
        //        if (authenticationResult != null)
        //        {
        //            DisplaySignedInAccount(authenticationResult.Account);

        //            Console.WriteLine(authenticationResult.ClaimsPrincipal.ToString());


        //            string accessToken = authenticationResult.AccessToken;
        //            result = await CallWebApiAndDisplayResultAsync(WebApiUrlMe, accessToken, "Me");
        //            await CallWebApiAndDisplayResultAsync(WebApiUrlMyManager, accessToken, "My manager");

        //        }
        //        else
        //        {
        //            succes = false;
        //        }

        //    }
        //    catch (Exception ex)
        //    {
        //        ExceptionBase.HandleException(ex, $"Error trying to get AZURE ID");

        //    }

        //    return result;
        //}

        private static void WriteTryAgainMessage()
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Wrong user or password. Try again!");
            Console.ResetColor();
        }
        private static SecureString buildPassword(string? requestPassword)
        {

            SecureString password = new SecureString();


            foreach (var character in requestPassword)
            {
                password.AppendChar(character);
            }

            return password;
        }




        public static void DisplaySignedInAccount(IAccount account)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            foreach (var charset in account.GetTenantProfiles())
            {
                foreach (var clain in charset.ClaimsPrincipal.Claims)
                {
                    var xxxxx = clain;
                }

            }
            Console.WriteLine($"{account.Username} successfully signed-in");
        }

        public async Task<JObject> CallWebApiAndDisplayResultAsync(string url, string accessToken, string title)
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(title);
            Console.ResetColor();
            return await protectedApiCallHelper.CallWebApiAndProcessResultAsync(url, accessToken, Display);
            Console.WriteLine();
        }

        /// <summary>
        /// Display the result of the Web API call
        /// </summary>
        /// <param name="result">Object to display</param>
        private static void Display(JObject result)
        {
            foreach (JProperty child in result.Properties().Where(p => !p.Name.StartsWith("@")))
            {
                Console.WriteLine($"{child.Name} = {child.Value}");
            }
        }
    }
}
