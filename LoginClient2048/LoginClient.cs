//#define LOCALDEBUG
using System;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace LoginClient2048
{
    public static class LoginClient
    {
        static HttpClient client;
        static LoginClient()
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
            client = new HttpClient();
#if !LOCALDEBUG
            client.BaseAddress = new Uri("https://login2048.azurewebsites.net/");
#endif
#if LOCALDEBUG
            client.BaseAddress = new Uri("http://localhost:60038/");
#warning Remove Flag!
#endif
        }

        public static async Task<string> Login(string username, string passwordHash)
        {
            var response = await client.PostAsJsonAsync("users/auth", new { Username = username, PasswordHash = passwordHash });
            if (response.StatusCode == HttpStatusCode.BadRequest)
            {
                throw new ArgumentException();
            }
            else if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                throw new InvalidCredentialException();
            }
            else if (response.StatusCode == HttpStatusCode.NotFound)
            {
                throw new UserNotFoundException();
            }
            else
            {
                return await response.Content.ReadAsStringAsync();
            }
        }
        public static async Task<string> Register(string username, string passwordHash)
        {
            var response = await client.PostAsJsonAsync("users/signup", new { Username = username, PasswordHash = passwordHash });
            if (response.StatusCode == HttpStatusCode.BadRequest)
            {
                throw new ArgumentException();
            }
            else if (response.StatusCode == HttpStatusCode.Conflict)
            {
                throw new DuplicateRegistrationException();
            }
            else
            {
                return await response.Content.ReadAsStringAsync();
            }
        }
        public static async Task<string> ChangeCredential(string username, string sid, string passwordHash)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, $"users/cm/{username}");
            request.Headers.Add("usersid", sid);
            request.Content = new StringContent($"\"{passwordHash}\"");
            request.Content.Headers.ContentType.MediaType = "application/json";
            request.Content.Headers.ContentType.CharSet = "utf-8";
            HttpResponseMessage response = await client.SendAsync(request);
            if (response.StatusCode == HttpStatusCode.BadRequest)
            {
                throw new ArgumentException();
            }
            else if (response.StatusCode == HttpStatusCode.NotFound)
            {
                throw new UserNotFoundException();
            }
            else if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                throw new InvalidCredentialException();
            }
            else
            {
                return await response.Content.ReadAsStringAsync();
            }
        }

        public static async Task<int> GetCoins(string username, string sid)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"transactions/coins/{username}");
            request.Headers.Add("usersid", sid);
            HttpResponseMessage response = await client.SendAsync(request);
            if (response.StatusCode == HttpStatusCode.BadRequest)
            {
                throw new ArgumentException();
            }
            else if (response.StatusCode == HttpStatusCode.NotFound)
            {
                throw new UserNotFoundException();
            }
            else if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                throw new InvalidCredentialException();
            }
            else
            {
                return int.Parse(await response.Content.ReadAsStringAsync());
            }
        }
        public static async Task<int> InitiateTransaction(string username, string sid, int value)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, $"transactions/transact/{username}");
            request.Headers.Add("usersid", sid);
            request.Content = new StringContent($"\"{value}\"");
            request.Content.Headers.ContentType.MediaType = "application/json";
            request.Content.Headers.ContentType.CharSet = "utf-8";
            HttpResponseMessage response = await client.SendAsync(request);
            if (response.StatusCode == HttpStatusCode.BadRequest)
            {
                throw new ArgumentException();
            }
            else if (response.StatusCode == HttpStatusCode.NotFound)
            {
                throw new UserNotFoundException();
            }
            else if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                throw new InvalidCredentialException();
            }
            else if (response.StatusCode == HttpStatusCode.PaymentRequired)
            {
                throw new InsufficientFundException();
            }
            else
            {
                return int.Parse(await response.Content.ReadAsStringAsync());
            }
        }
        public static async Task<int> RedeemCard(string username, string sid, string cardKey)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, $"transactions/redeem/{username}");
            request.Headers.Add("usersid", sid);
            request.Content = new StringContent($"\"{cardKey}\"");
            request.Content.Headers.ContentType.MediaType = "application/json";
            request.Content.Headers.ContentType.CharSet = "utf-8";
            HttpResponseMessage response = await client.SendAsync(request);
            if (response.StatusCode == HttpStatusCode.BadRequest)
            {
                throw new ArgumentException();
            }
            else if (response.StatusCode == HttpStatusCode.NotFound)
            {
                throw new UserNotFoundException();
            }
            else if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                throw new InvalidCredentialException();
            }
            else if (response.StatusCode == HttpStatusCode.PaymentRequired)
            {
                string details = await response.Content.ReadAsStringAsync();
                if (details == "CardInvalid") { throw new CardInvalidException(); }
                else { throw new CardRedeemedException(); }
            }
            else
            {
                return int.Parse(await response.Content.ReadAsStringAsync());
            }
        }

        public static async Task<string> GetThemes()
        {
            return await GetThemes("");
        }
        public static async Task<string> GetThemes(string sid)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"themes");
            request.Headers.Add("usersid", sid);
            HttpResponseMessage response = await client.SendAsync(request);
            if (response.StatusCode == HttpStatusCode.BadRequest)
            {
                throw new ArgumentException();
            }
            else if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                throw new InvalidCredentialException();
            }
            else
            {
                return await response.Content.ReadAsStringAsync();
            }
        }
        public static async Task<string> GetThemeContent(long id)
        {
            HttpResponseMessage response = await client.GetAsync($"themes/{id}");
            if (response.StatusCode == HttpStatusCode.BadRequest)
            {
                throw new ArgumentException();
            }
            else if (response.StatusCode == HttpStatusCode.NotFound)
            {
                throw new ThemeNotFoundException();
            }
            else
            {
                return await response.Content.ReadAsStringAsync();
            }
        }
        public static async Task EditTheme(string themeStr, string sid)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, $"themes/edit");
            request.Headers.Add("usersid", sid);
            request.Content = new StringContent(themeStr);
            request.Content.Headers.ContentType.MediaType = "application/json";
            request.Content.Headers.ContentType.CharSet = "utf-8";
            HttpResponseMessage response = await client.SendAsync(request);
            if (response.StatusCode == HttpStatusCode.BadRequest)
            {
                throw new ArgumentException();
            }
            else if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                throw new InvalidCredentialException();
            }
            else if (response.StatusCode == HttpStatusCode.NotFound)
            {
                throw new ThemeNotFoundException();
            }
        }

        public static async Task AddHighscore(string username, string sid, int mode, int size, long score)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, $"highscores/add");
            request.Headers.Add("usersid", sid);
            request.Content = new StringContent($"{{ \"Username\" : \"{username}\", \"Mode\" : \"{mode}\", \"Size\" : \"{size}\", \"Score\" : \"{score}\" }}");
            request.Content.Headers.ContentType.MediaType = "application/json";
            request.Content.Headers.ContentType.CharSet = "utf-8";
            HttpResponseMessage response = await client.SendAsync(request);
            if (response.StatusCode == HttpStatusCode.BadRequest)
            {
                throw new ArgumentException();
            }
            else if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                throw new InvalidCredentialException();
            }
        }
        public static async Task<string> GetHighscore(int mode, int size)
        {
            HttpResponseMessage response = await client.GetAsync($"highscores/{mode}/{size}");
            if (response.StatusCode == HttpStatusCode.BadRequest)
            {
                throw new ArgumentException();
            }
            else
            {
                return await response.Content.ReadAsStringAsync();
            }
        }

        public static string GetPasswordHash(string password)
        {
            SHA256 sha256 = SHA256.Create();
            byte[] hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Encoding.UTF8.GetString(hashBytes);
        }

        public class DuplicateRegistrationException : LoginClientException { }
        public class InvalidCredentialException : LoginClientException { }
        public class UserNotFoundException : LoginClientException { }
        public class InsufficientFundException : LoginClientException { }
        public class CardRedeemedException : LoginClientException { }
        public class CardInvalidException : LoginClientException { }
        public class ThemeNotFoundException : LoginClientException { }

        public class LoginClientException : Exception { }
    }
}
