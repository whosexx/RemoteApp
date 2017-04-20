using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LibRDP
{
    class Utils
    {
        [System.Runtime.InteropServices.DllImport("user32.dll ")]
        public static extern int SetWindowLong(IntPtr hWnd, int nIndex, int wndproc);
        [System.Runtime.InteropServices.DllImport("user32.dll ")]
        public static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        public const int GWL_STYLE = -16;
        public const int WS_DISABLED = 0x8000000;

        public static void SetControlEnabled(Control c, bool enabled)
        {
            if (enabled)
            { SetWindowLong(c.Handle, GWL_STYLE, (~WS_DISABLED) & GetWindowLong(c.Handle, GWL_STYLE)); }
            else
            { SetWindowLong(c.Handle, GWL_STYLE, WS_DISABLED | GetWindowLong(c.Handle, GWL_STYLE)); }
        }

        public static string CalcSHA256(string value)
        {
            if (value == null)
                return null;

            byte[] retval = Encoding.UTF8.GetBytes(value);
            //using (MD5 md5 = new MD5CryptoServiceProvider())
            //using (SHA256 sha256 = new SHA256CryptoServiceProvider())
            //{
            //    retval = sha256.ComputeHash(retval);

            //    if (retval == null)
            //        return null;

            //    return Convert.ToBase64String(CalcSHA256(retval));
            //}
            return Convert.ToBase64String(CalcSHA256(retval));
        }

        public static byte[] CalcSHA256(byte[] value)
        {
            if (value == null)
                return null;

            //using (MD5 md5 = new MD5CryptoServiceProvider())
            using (SHA256 sha256 = new SHA256CryptoServiceProvider())
            {
                value = sha256.ComputeHash(value);

                if (value == null)
                    return null;

                return value;
            }
        }

        internal static string EncryptChaCha20(string msg, string nonce, string key)
        {
            if (string.IsNullOrWhiteSpace(key) || nonce.Length != 8)
                return string.Empty;

            var n = Encoding.UTF8.GetBytes(nonce);
            var s = Convert.FromBase64String(key);
            var cliper = Encoding.UTF8.GetBytes(msg);
            var bs = Sodium.StreamEncryption.EncryptChaCha20(cliper, n, s);
            return Convert.ToBase64String(bs);
        }

        internal static string DecryptChaCha20(string msg, string nonce, string key)
        {
            if (string.IsNullOrWhiteSpace(key) || nonce.Length != 8)
                return string.Empty;

            var b = Convert.FromBase64String(msg);
            var n = Encoding.UTF8.GetBytes(nonce);
            var s = Convert.FromBase64String(key);

            var bs = Sodium.StreamEncryption.DecryptChaCha20(b, n, s);
            return Encoding.UTF8.GetString(bs);
        }

        //internal static string AES_Encrypt(string toEncrypt, string salt = "")
        //{
        //    if (string.IsNullOrWhiteSpace(salt))
        //        salt = "88888888";

        //    byte[] keyArray = Encoding.UTF8.GetBytes(salt);
        //    if (keyArray.Length > 24)
        //        throw new ArgumentException("长度不符合要求");

        //    byte[] key = new byte[32];
        //    byte[] rand = new byte[key.Length - keyArray.Length];
        //    new Random().NextBytes(rand);
        //    Buffer.BlockCopy(keyArray, 0, key, 0, keyArray.Length);
        //    Buffer.BlockCopy(rand, 0, key, keyArray.Length, rand.Length);

        //    byte[] toEncryptArray = Encoding.UTF8.GetBytes(toEncrypt);
        //    using (RijndaelManaged rDel = new RijndaelManaged())
        //    {
        //        rDel.Key = key;
        //        rDel.Mode = CipherMode.ECB;
        //        rDel.Padding = PaddingMode.PKCS7;

        //        using (ICryptoTransform cTransform = rDel.CreateEncryptor())
        //        {
        //            byte[] resultArray = cTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);

        //            int total = 0;
        //            Array.Reverse(rand);
        //            rand = Encrypt(rand);
        //            Array.Reverse(rand);

        //            byte l = (byte)rand.Length;
        //            var encpt = resultArray.ToArray();
        //            byte[] seedstr = new byte[rand.Length + 1 + encpt.Length];

        //            Buffer.BlockCopy(encpt, 0, seedstr, 0, encpt.Length);
        //            total += encpt.Length;

        //            Buffer.BlockCopy(rand, 0, seedstr, total, rand.Length);
        //            total += rand.Length;

        //            seedstr[total] = l;

        //            return Convert.ToBase64String(seedstr, 0, seedstr.Length);
        //        }
        //    }
        //}

        //internal static string AES_Decrypt(string pass, string salt = "")
        //{
        //    if (string.IsNullOrWhiteSpace(pass))
        //        return string.Empty;

        //    if (string.IsNullOrWhiteSpace(salt))
        //        salt = "88888888";

        //    byte[] keyArray = Encoding.UTF8.GetBytes(salt);
        //    if (keyArray.Length > 24)
        //        throw new ArgumentException("长度不符合要求");

        //    byte[] input = Convert.FromBase64String(pass);
        //    int total = input.Length;
        //    byte len = input[total - 1];
        //    if (len >= (total - 1))
        //        return null;

        //    byte[] key = new byte[len];
        //    Buffer.BlockCopy(input, total - len - 1, key, 0, len);

        //    Array.Reverse(key);
        //    key = Decrypt(key);
        //    Array.Reverse(key);
        //    if ((key.Length + keyArray.Length) != 32)
        //        return null;

        //    byte[] seed = new byte[32];
        //    Buffer.BlockCopy(keyArray, 0, seed, 0, keyArray.Length);
        //    Buffer.BlockCopy(key, 0, seed, keyArray.Length, key.Length);

        //    using (RijndaelManaged rDel = new RijndaelManaged())
        //    {
        //        rDel.Key = seed;
        //        rDel.Mode = CipherMode.ECB;
        //        rDel.Padding = PaddingMode.PKCS7;

        //        using (ICryptoTransform cTransform = rDel.CreateDecryptor())
        //        {
        //            byte[] resultArray = cTransform.TransformFinalBlock(input, 0, total - len - 1);
        //            return UTF8Encoding.UTF8.GetString(resultArray);
        //        }
        //    }
        //}

        ///// <summary>
        ///// 加密
        ///// </summary>
        ///// <param name="pass"></param>
        ///// <returns></returns>
        //internal static string Encrypt(string pass)
        //{
        //    byte[] rgbkey = { 0x12, 0x34, 0x56, 0x78, 0x90, 0xAB, 0xCD, 0xEF };
        //    byte[] rgbIV = { 0xE2, 0x35, 0xC4, 0x6F, 0x30, 0xAB, 0xB6, 0xA2 };

        //    byte[] inputByteArray = Encoding.UTF8.GetBytes(pass);
        //    inputByteArray = Encrypt(inputByteArray);
        //    return Convert.ToBase64String(inputByteArray);
        //}

        //internal static byte[] Encrypt(byte[] pass)
        //{
        //    byte[] rgbkey = { 0x12, 0x34, 0x56, 0x78, 0x90, 0xAB, 0xCD, 0xEF };
        //    byte[] rgbIV = { 0xE2, 0x35, 0xC4, 0x6F, 0x30, 0xAB, 0xB6, 0xA2 };

        //    using (DESCryptoServiceProvider dCSP = new DESCryptoServiceProvider())
        //    {
        //        using (MemoryStream mStream = new MemoryStream())
        //        {
        //            using (CryptoStream cStream = new CryptoStream(mStream, dCSP.CreateEncryptor(rgbkey, rgbIV), CryptoStreamMode.Write))
        //            {
        //                cStream.Write(pass, 0, pass.Length);
        //                cStream.FlushFinalBlock();

        //                return mStream.ToArray();
        //            }
        //        }
        //    }
        //}

        ///// <summary>
        ///// 解密
        ///// </summary>
        ///// <param name="pass"></param>
        ///// <returns></returns>
        //internal static string Decrypt(string pass)
        //{
        //    if (string.IsNullOrWhiteSpace(pass))
        //        return null;

        //    byte[] rgbkey = { 0x12, 0x34, 0x56, 0x78, 0x90, 0xAB, 0xCD, 0xEF };
        //    byte[] rgbIV = { 0xE2, 0x35, 0xC4, 0x6F, 0x30, 0xAB, 0xB6, 0xA2 };

        //    try
        //    {
        //        byte[] input = Convert.FromBase64String(pass);
        //        input = Decrypt(input);
        //        return Encoding.UTF8.GetString(input.ToArray());
        //    }
        //    catch
        //    {
        //        return null;
        //    }
        //}

        //internal static byte[] Decrypt(byte[] pass)
        //{
        //    if (pass == null || pass.Length <= 0)
        //        return null;

        //    byte[] rgbkey = { 0x12, 0x34, 0x56, 0x78, 0x90, 0xAB, 0xCD, 0xEF };
        //    byte[] rgbIV = { 0xE2, 0x35, 0xC4, 0x6F, 0x30, 0xAB, 0xB6, 0xA2 };

        //    try
        //    {
        //        using (DESCryptoServiceProvider DCSP = new DESCryptoServiceProvider())
        //        {
        //            using (MemoryStream mStream = new MemoryStream())
        //            {
        //                using (CryptoStream cStream = new CryptoStream(mStream, DCSP.CreateDecryptor(rgbkey, rgbIV), CryptoStreamMode.Write))
        //                {
        //                    cStream.Write(pass, 0, pass.Length);
        //                    cStream.FlushFinalBlock();
        //                    return mStream.ToArray();
        //                }
        //            }
        //        }
        //    }
        //    catch
        //    {
        //        return null;
        //    }
        //}
    }
}
