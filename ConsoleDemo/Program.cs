﻿using System;
using System.Linq;
using ConsoleDemo.CSharp9;
using ConsoleDemo.PasswordValidation;
using ConsoleDemo.StringComparison;
using EncryptionLibrary;

namespace ConsoleDemo
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            // CheckPasswordIsWeakly();
            // RsaEncrypt();
            NewFeatures.RangeOperation();

            PrintLine("Press any key to continue...");
            Console.ReadKey(true);
        }

        private static void RsaEncrypt()
        {
            Rsa.Encrypt("12345678",
                "-----BEGIN PUBLIC KEY-----\nMIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQCMN3TTQAsruN8RG2j3hVP8yf3X\n2ZdyRvEyjbgeNpoVM/OE9X1zVSrWTmr+rp0+E08XfnFbMyii9CuoqIZbAIiLP760\nZcOSSKAk0YWDtvVgr/IjuPMOwr3yLrtR+9v7K35PQtqsUfQJhNzzZcpNA4W4Tpbp\nHU3zML0PZCrQNWxD3wIDAQAB\n-----END PUBLIC KEY-----",
                out var encryptedText);
            PrintLine(encryptedText);
        }

        /// <summary>
        /// 输入密码获取密码强度等级
        /// </summary>
        private static void CheckPasswordIsWeakly()
        {
            var arr = WeakPwdListReader.Read();
            PrintLine("请输入用于密码校验的用户名:");
            var username = Console.ReadLine();
        Recycle:
            PrintLine("请输入要校验的密码，回车结束，输入 exit 退出过程:");
            var input = Console.ReadLine();
            var password = string.IsNullOrEmpty(input) ? string.Empty : input;
            if (string.Equals("exit", password)) return;
            PrintLine($"用户名:{username}和密码:{password}的最长交集:{StringSamePart.Get(username, password)}");
            PrintLine($"用户名:{username}和密码:{password}的相似度:{StringSimilarity.Get(username, password)}");
            var result = from p in arr where password.ToLower().Contains(p) select p;
            var values = result as string[] ?? result.ToArray();
            PrintLine($"命中的弱密码是: {string.Join(",", values)}");
            var count = values.Length;
            PrintLine(count > 0 ? "密码包含常用字符" : "密码不包含常用字符");
            goto Recycle;
        }

        private static void PrintLine(object o)
        {
            Console.WriteLine(o);
        }
    }
}
