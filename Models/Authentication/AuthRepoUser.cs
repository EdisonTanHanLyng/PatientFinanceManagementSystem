using Org.BouncyCastle.Asn1.Ocsp;
using System;
using System.Collections.Concurrent;
using System.Linq;
using static Microsoft.ApplicationInsights.MetricDimensionNames.TelemetryContext;

namespace PFMS_MI04.Models.Authentication
{
    public class AuthRepoUser
    {
        public static readonly ConcurrentDictionary<string, UserInfo> _userMap = new ConcurrentDictionary<string, UserInfo>();
        private static ConcurrentQueue<string> _tokensToRevoke = new ConcurrentQueue<string>();
        private static HashSet<string> _revokedTokens = new HashSet<string>();

        public static void SetJwt(string jwt, string userId, string accRole)
        {
            _userMap[jwt] = new UserInfo { AccId = userId, AccRole = accRole };
        }

        public static UserInfo GetUserInfo(string jwt)
        {
            _userMap.TryGetValue(jwt, out var userInfo);
            return userInfo;
        }

        public static void RemoveUser(string jwt)
        {
            if (_userMap.TryRemove(jwt, out var removedUser))
            {
                Console.WriteLine($"User removed: JWT: {jwt}, UserId: {removedUser.AccId}");
            }
            else
            {
                Console.WriteLine($"Failed to remove user with JWT: {jwt}");
            }
        }

        public static void RemoveUserById(string userId)
        {
            var pairToRemove = _userMap.FirstOrDefault(p => p.Value.AccId == userId);
            if (pairToRemove.Key != null)
            {
                _userMap.TryRemove(pairToRemove.Key, out _);
                Console.WriteLine($"User {userId} has been removed from the system.");
            }
            else
            {
                Console.WriteLine($"User {userId} not found in the system.");
            }
        }


        public static bool IsUserLoggedIn(string userId)
        {
            return _userMap.Values.Any(u => u.AccId == userId);
        }


        public static string GetUserId(string jwt)
        {
            if (_userMap.TryGetValue(jwt, out var userInfo))
            {
                return userInfo.AccId;
            }
            return null;
        }

        public static bool IsTokenInvoke(string jwt)
        {
            return _userMap.Keys.Any(u => u == jwt);
        }

        public static bool IsTokenRevoked(string token)
        {
            return _revokedTokens.Contains(token);
        }

        public static void SignOffTokenInvoke(string userId)
        {
            var pairs = _userMap.Where(p => p.Value.AccId == userId).ToList();
            foreach (var pair in pairs)
            {
                _tokensToRevoke.Enqueue(pair.Key);
            }
        }

        public static string getTokenToRevoke()
        {
            if (_tokensToRevoke.TryDequeue(out string token))
            {
                _revokedTokens.Add(token);
                return token;
            }
            return null;
        }


        public static void PrintUserMap()
        {
            Console.WriteLine("Current User Map:");
            if (_userMap.Any())
            {
                foreach (var pair in _userMap)
                {
                    Console.WriteLine($"JWT: {pair.Key}");
                    Console.WriteLine($"  UserId: {pair.Value.AccId}");
                    Console.WriteLine($"  Role: {pair.Value.AccRole}");
                    Console.WriteLine("--------------------");
                }
            }
            else
            {
                Console.WriteLine("The user map is empty.");
            }
        }
    }

    public class UserInfo
    {
        public string AccId { get; set; }
        public string AccRole { get; set; }
    }
}