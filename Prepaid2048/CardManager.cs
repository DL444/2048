using System;
using System.Text;

namespace Prepaid2048
{
    public static class PrepaidCardManager
    {
        public static string GenerateCardKey(int value)
        {
            if (value == 10 || value == 20 || value == 5 || value == 50 || value == 100)
            {
                return GenerateCardKey((CardValues)value);
            }
            throw new ArgumentOutOfRangeException(nameof(value));
        }
        public static string GenerateCardKey(CardValues value)
        {
            string tickStr = DateTime.UtcNow.Ticks.ToString();
            byte[] tickBytes = Encoding.UTF8.GetBytes(tickStr.Substring(tickStr.Length - 16));

            string displayStr = GetDisplayTick(tickBytes);
            byte[] key = GetKey(tickBytes);
            byte[] vi = GetVi(key);

            string encrypted = FormatEncryptedPayload(EncryptionHelper.Encrypt($"{(int)value}", key, vi));

            return displayStr + encrypted;

        }
        public static CardValues GetValue(string cardKey)
        {
            // Values: 1024 * 5, 1024 * 10, 1024 * 20, 1024 * 50, 1024 * 100

            if (cardKey.Length != 24)
            {
                throw new ArgumentException(nameof(cardKey));
            }
            string formatKey = cardKey.ToUpper();
            byte[] tickBytes = GetByteTicks(formatKey);
            string cipher = formatKey.Substring(formatKey.Length - 8);
            byte[] key = GetKey(tickBytes);
            byte[] vi = GetVi(key);

            if(FormatEncryptedPayload(EncryptionHelper.Encrypt("10", key, vi)) == cipher)
            {
                return CardValues.Ten;
            }
            if (FormatEncryptedPayload(EncryptionHelper.Encrypt("20", key, vi)) == cipher)
            {
                return CardValues.Twenty;
            }
            if (FormatEncryptedPayload(EncryptionHelper.Encrypt("5", key, vi)) == cipher)
            {
                return CardValues.Five;
            }
            if (FormatEncryptedPayload(EncryptionHelper.Encrypt("50", key, vi)) == cipher)
            {
                return CardValues.Fifty;
            }
            if (FormatEncryptedPayload(EncryptionHelper.Encrypt("100", key, vi)) == cipher)
            {
                return CardValues.Hundred;
            }
            throw new ArgumentException(nameof(cardKey));
        }

        static string GetDisplayTick(byte[] tickBt)
        {
            string ticks = Encoding.UTF8.GetString(tickBt);
            char[] s = new char[16];
            for (int i = 0; i < 16; i++)
            {
                byte digit = byte.Parse(ticks[i].ToString());
                if (i % 2 == 0)
                {
                    if (digit % 2 == 0)
                    {
                        s[i] = (char)('A' + i + digit);
                    }
                    else
                    {
                        s[i] = ticks[i];
                    }
                }
                else
                {
                    if (digit % 2 == 1)
                    {
                        s[i] = (char)('A' + i + digit);
                    }
                    else
                    {
                        s[i] = ticks[i];
                    }
                }
            }
            char[] c = new char[16];
            for (int i = 0; i < 16; i++)
            {
                if (i % 2 == 0)
                {
                    c[i] = s[8 + i / 2];
                }
                else
                {
                    c[i] = s[i / 2];
                }
            }
            return new String(c).ToUpper();
        }
        static byte[] GetByteTicks(string display)
        {
            string d = display;
            if(d.Length > 16)
            {
                d = d.Substring(0, 16);
            }
            char[] c = d.ToCharArray();

            char[] s = new char[16];
            for(int i = 0; i < 16; i++)
            {
                if(i % 2 == 0)
                {
                    s[8 + i / 2] = c[i];
                }
                else
                {
                    s[i / 2] = c[i];
                }
            }
            for (int i = 0; i < 16; i++)
            {
                if(char.IsLetter(s[i]))
                {
                    s[i] = (char)(s[i] - i - 'A' + '0');
                }
            }
            return Encoding.UTF8.GetBytes(s);
        }
        static byte[] GetKey(byte[] input)
        {
            byte[] k = new byte[32];
            for (int i = 0; i < 8; i++)
            {
                k[i] = (byte)(input[i] & input[8 + i]);
                k[8 + i] = (byte)(input[i] | input[8 + i]);
                k[16 + i] = input[i] > input[8 + i] ? (byte)1 : (byte)0;
                k[24 + i] = (byte)(input[i] ^ input[8 + i]);
            }
            return k;
        }
        static byte[] GetVi(byte[] k)
        {
            byte[] v = new byte[16];
            int startBit = (k[31] & 1);
            for (int i = startBit; i < 32; i += 2)
            {
                v[i / 2] = k[i];
            }
            return v;
        }
        static string FormatEncryptedPayload(string original)
        {
            string encrypted = original.Replace("=", "").Replace('+', 'P').Replace('/', 'S').ToUpper();
            string displayStr = "";
            displayStr = displayStr + encrypted.Substring(0, 4);
            displayStr = displayStr + encrypted.Substring(encrypted.Length - 4);
            return displayStr;
        }
    }

    public enum CardValues
    {
        Five = 5, Ten = 10, Twenty = 20, Fifty = 50, Hundred = 100
    }
}
