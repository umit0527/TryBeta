using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Web;
using System.Security.Cryptography;
using Konscious.Security.Cryptography;
using System.Text;

namespace TryBeta.Models
{
    public class PasswordHasher
    {
        public static string HashPassword(string password)
        {
            byte[] salt = new byte[16];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }

            var argon2 = new Argon2id(Encoding.UTF8.GetBytes(password))
            {
                Salt = salt,
                DegreeOfParallelism = 8,
                Iterations = 4,
                MemorySize = 1024 * 64
            };

            byte[] hashBytes = argon2.GetBytes(32);

            byte[] result = new byte[salt.Length + hashBytes.Length];
            Buffer.BlockCopy(salt, 0, result, 0, salt.Length);
            Buffer.BlockCopy(hashBytes, 0, result, salt.Length, hashBytes.Length);

            return Convert.ToBase64String(result);
        }

        public static bool VerifyPassword(string hashedPassword, string passwordToCheck)
        {
            byte[] decoded = Convert.FromBase64String(hashedPassword);

            byte[] salt = new byte[16];
            Buffer.BlockCopy(decoded, 0, salt, 0, 16);

            byte[] hash = new byte[decoded.Length - 16];
            Buffer.BlockCopy(decoded, 16, hash, 0, hash.Length);

            var argon2 = new Argon2id(Encoding.UTF8.GetBytes(passwordToCheck))
            {
                Salt = salt,
                DegreeOfParallelism = 8,
                Iterations = 4,
                MemorySize = 1024 * 64
            };

            byte[] newHash = argon2.GetBytes(32);

            return FixedTimeEquals(hash, newHash);
        }
        public static bool FixedTimeEquals(byte[] a, byte[] b)
        {
            if (a.Length != b.Length) return false;

            int diff = 0;
            for (int i = 0; i < a.Length; i++)
            {
                diff |= a[i] ^ b[i];
            }

            return diff == 0;
        }
    }
}