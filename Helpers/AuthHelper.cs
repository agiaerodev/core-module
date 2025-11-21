using Core.Factory;
using Core.Interfaces;
using Core.Repositories;
using Idata.Data;
using Idata.Data.Entities.Iprofile;
using Ihelpers.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace Core.Helpers
{
    public class AuthHelper
    {
        IHttpContextAccessor _httpContextAccesor;
        public AuthHelper(IHttpContextAccessor httpContextAccesor)
        {
            _httpContextAccesor = httpContextAccesor;
        }

        public async Task<User?> AuthUser(IHttpContextAccessor _httpContextAccessor)
        {
            string token = _httpContextAccessor.HttpContext.Request.Headers.Authorization;

            return AuthUser(token);

        }

        public static User AuthUser(string? token)
        {
            UrlRequestBase urlRequestBase = new UrlRequestBase();

            long? userIdstr = Convert.ToInt64(JWTHelper.getJWTTokenClaim(token, "UserId"));

            IdataContext dbContext = new IdataContext();

            if (userIdstr != null)
            {
               
                User? user = dbContext.Users.Include("roles").Include("departments").Where(us => us.id == userIdstr).FirstOrDefault();

                user?.Initialize();

                if (user != null)
                {
                    user.timezone = !string.IsNullOrEmpty(user?.timezone) ? getTimezoneOffset(user.timezone) : "00:00";
                }


                return (user);


            }
            else
            {
                return null;
            }

        }
        public static async Task<User?> AuthUserAsync(string? token)
        {
            UrlRequestBase urlRequestBase = new UrlRequestBase();

            long? userIdstr = Convert.ToInt64(JWTHelper.getJWTTokenClaim(token, "UserId"));

            IdataContext dbContext = new IdataContext();

            if (userIdstr != null)
            {

                User? user = await dbContext.Users.Include("roles").Include("departments").Where(us => us.id == userIdstr).FirstOrDefaultAsync();

                user?.Initialize();

                if(user != null)
                {
                    user.timezone = !string.IsNullOrEmpty(user?.timezone) ? getTimezoneOffset(user.timezone) : "00:00";
                }

              
                return (user);


            }
            else
            {
                return null;
            }

        }
        public static string getTimezoneOffset(string? timezoneSufix)
        {
            return Ihelpers.Helpers.TimezoneHelper.getTimezoneOffset(timezoneSufix);

        }
        public static async Task<string?> CreateClientHash(string token)
        {
            var user = await AuthHelper.AuthUserAsync(token);

            if (user != null)
            {
                var clientHash = await EncryptionHelper.Encrypt($"{token.Substring(token.Length - 10)}:{user.email}||{user.id}", ConfigurationHelper.GetConfig("EncryptKey:ClientHashesKey"));
                return clientHash;
            }
            else
            {
                return null;
            }
        }
        public static SymmetricSecurityKey ExtendKeyLengthIfNeeded(SymmetricSecurityKey key, int minLenInBytes)
        {
            if (key != null && key.KeySize < (minLenInBytes * 8))
            {
                var newKey = new byte[minLenInBytes]; // zeros by default
                key.Key.CopyTo(newKey, 0);
                return new SymmetricSecurityKey(newKey);
            }
            return key;
        }
    }
}
