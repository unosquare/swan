﻿namespace Swan.Utilities;

using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

/// <summary>
/// Use this class to compute a hash in MD5, SHA1, SHA256 or SHA512.
/// </summary>
public static class Hasher
{
    /// <summary>
    /// Computes the MD5 hash of the given stream.
    /// Do not use for large streams as this reads ALL bytes at once.
    /// </summary>
    /// <param name="this">The stream.</param>
    /// <returns>
    /// The computed hash code.
    /// </returns>
    /// <exception cref="ArgumentNullException">stream.</exception>
    [Obsolete("Use a better hasher.")]
    public static byte[] ComputeMD5(this Stream @this)
    {
        if (@this == null)
            throw new ArgumentNullException(nameof(@this));

        using var md5 = MD5.Create();
        const int bufferSize = 4096;

        var readAheadBuffer = new byte[bufferSize];
        var readAheadBytesRead = @this.Read(readAheadBuffer, 0, readAheadBuffer.Length);

        do
        {
            var bytesRead = readAheadBytesRead;
            var buffer = readAheadBuffer;

            readAheadBuffer = new byte[bufferSize];
            readAheadBytesRead = @this.Read(readAheadBuffer, 0, readAheadBuffer.Length);

            if (readAheadBytesRead == 0)
                md5.TransformFinalBlock(buffer, 0, bytesRead);
            else
                md5.TransformBlock(buffer, 0, bytesRead, buffer, 0);
        }
        while (readAheadBytesRead != 0);

        return md5.Hash ?? Array.Empty<byte>();
    }

    /// <summary>
    /// Computes the MD5 hash of the given string using UTF8 byte encoding.
    /// </summary>
    /// <param name="value">The input string.</param>
    /// <param name="createHasher">if set to <c>true</c> [create hasher].</param>
    /// <returns>The computed hash code.</returns>
    [Obsolete("Use a better hasher.")]
    public static byte[] ComputeMD5(string value, bool createHasher = false) =>
        ComputeMD5(Encoding.UTF8.GetBytes(value), createHasher);

    /// <summary>
    /// Computes the MD5 hash of the given byte array.
    /// </summary>
    /// <param name="data">The data.</param>
    /// <param name="createHasher">if set to <c>true</c> [create hasher].</param>
    /// <returns>The computed hash code.</returns>
    [Obsolete("Use a better hasher.")]
    public static byte[] ComputeMD5(byte[] data, bool createHasher = false)
    {
        if (createHasher)
        {
            using var hasher = MD5.Create();
            hasher.ComputeHash(data);
        }

        return MD5.HashData(data);
    }


    /// <summary>
    /// Computes the SHA-1 hash of the given string using UTF8 byte encoding.
    /// </summary>
    /// <param name="this">The input string.</param>
    /// <param name="createHasher">if set to <c>true</c> [create hasher].</param>
    /// <returns>
    /// The computes a Hash-based Message Authentication Code (HMAC) 
    /// using the SHA1 hash function.
    /// </returns>
    [Obsolete("Use a better hasher.")]
    public static byte[] ComputeSha1(string @this, bool createHasher = false)
    {
        var inputBytes = Encoding.UTF8.GetBytes(@this);

        if (createHasher)
        {
            using var hasher = SHA1.Create();
            return hasher.ComputeHash(inputBytes);
        }

        return SHA1.HashData(inputBytes);
    }

    /// <summary>
    /// Computes the SHA-256 hash of the given string using UTF8 byte encoding.
    /// </summary>
    /// <param name="value">The input string.</param>
    /// <param name="createHasher">if set to <c>true</c> [create hasher].</param>
    /// <returns>
    /// The computes a Hash-based Message Authentication Code (HMAC) 
    /// by using the SHA256 hash function.
    /// </returns>
    public static byte[] ComputeSha256(string value, bool createHasher = false)
    {
        var inputBytes = Encoding.UTF8.GetBytes(value);
        if (createHasher)
        {
            using var hasher = SHA256.Create();
            return hasher.ComputeHash(inputBytes);
        }

        return SHA256.HashData(inputBytes);
    }

    /// <summary>
    /// Computes the SHA-512 hash of the given string using UTF8 byte encoding.
    /// </summary>
    /// <param name="value">The input string.</param>
    /// <param name="createHasher">if set to <c>true</c> [create hasher].</param>
    /// <returns>
    /// The computes a Hash-based Message Authentication Code (HMAC) 
    /// using the SHA512 hash function.
    /// </returns>
    public static byte[] ComputeSha512(string value, bool createHasher = false)
    {
        var inputBytes = Encoding.UTF8.GetBytes(value);

        if (createHasher)
        {
            using var hasher = SHA512.Create();
            return hasher.ComputeHash(inputBytes);
        }

        return SHA512.HashData(inputBytes);
    }
}
