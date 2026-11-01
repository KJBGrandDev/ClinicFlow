#nullable enable

using System;
using System.Linq;
using System.Security.Cryptography;

namespace ClinicFlow_Unified.Utilities;

public static class PasswordHasher
{
    private const int SaltSize = 16;
    private const int HashSize = 32;
    private const int Iterations = 100_000;

    /// <summary>
    /// Hashes a password using PBKDF2-SHA256 with 100,000 iterations and embeds the 16-byte salt into a 48-byte Base64 payload.
    /// </summary>
    public static string HashPassword(string password)
    {
        if (string.IsNullOrEmpty(password))
            throw new ArgumentNullException(nameof(password));

        byte[] salt = RandomNumberGenerator.GetBytes(SaltSize);
        byte[] subkey = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, HashAlgorithmName.SHA256, HashSize);

        // Combine salt (16 bytes) + hash (32 bytes) = 48 bytes total
        byte[] outputBytes = new byte[SaltSize + HashSize];
        Buffer.BlockCopy(salt, 0, outputBytes, 0, SaltSize);
        Buffer.BlockCopy(subkey, 0, outputBytes, SaltSize, HashSize);

        return Convert.ToBase64String(outputBytes);
    }

    /// <summary>
    /// Generates a distinct Base64 salt and PBKDF2-SHA256 hash.
    /// </summary>
    public static (string Hash, string Salt) HashPasswordWithSalt(string password)
    {
        if (string.IsNullOrEmpty(password))
            throw new ArgumentNullException(nameof(password));

        byte[] salt = RandomNumberGenerator.GetBytes(SaltSize);
        byte[] subkey = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, HashAlgorithmName.SHA256, HashSize);

        return (Convert.ToBase64String(subkey), Convert.ToBase64String(salt));
    }

    /// <summary>
    /// Verifies a password against the stored password hash and optional salt.
    /// Supports combined 48-byte Base64, delimited salt:hash, and separate column salt.
    /// </summary>
    public static bool VerifyPassword(string password, string storedHash, string? storedSalt = null)
    {
        if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(storedHash))
            return false;

        storedHash = storedHash.Trim();

        // 1. Separate Salt column check (e.g., from dbo.Users.PasswordSalt)
        if (!string.IsNullOrWhiteSpace(storedSalt))
        {
            try
            {
                byte[] saltBytes = Convert.FromBase64String(storedSalt.Trim());
                byte[] expectedHash = Convert.FromBase64String(storedHash);
                byte[] actualHash = Rfc2898DeriveBytes.Pbkdf2(password, saltBytes, Iterations, HashAlgorithmName.SHA256, HashSize);

                if (CryptographicOperations.FixedTimeEquals(actualHash, expectedHash))
                    return true;
            }
            catch { }
        }

        // 3. Delimited format check ("salt:hash" or "salt.hash")
        char[] delimiters = { ':', '.' };
        if (storedHash.IndexOfAny(delimiters) > 0)
        {
            var parts = storedHash.Split(delimiters);
            if (parts.Length == 2)
            {
                try
                {
                    string saltString = parts.First();
                    string hashString = parts.Last();

                    byte[] saltBytes = Convert.FromBase64String(saltString);
                    byte[] expectedHash = Convert.FromBase64String(hashString);
                    byte[] actualHash = Rfc2898DeriveBytes.Pbkdf2(password, saltBytes, Iterations, HashAlgorithmName.SHA256, HashSize);

                    if (CryptographicOperations.FixedTimeEquals(actualHash, expectedHash))
                        return true;
                }
                catch { }
            }
        }

        // 4. Combined 48-byte Base64 payload (16 bytes salt + 32 bytes subkey)
        try
        {
            byte[] decoded = Convert.FromBase64String(storedHash);
            if (decoded.Length == SaltSize + HashSize)
            {
                byte[] salt = new byte[SaltSize];
                byte[] expectedSubkey = new byte[HashSize];

                Buffer.BlockCopy(decoded, 0, salt, 0, SaltSize);
                Buffer.BlockCopy(decoded, SaltSize, expectedSubkey, 0, HashSize);

                byte[] actualSubkey = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, HashAlgorithmName.SHA256, HashSize);
                return CryptographicOperations.FixedTimeEquals(actualSubkey, expectedSubkey);
            }
        }
        catch { }

        return false;
    }
}