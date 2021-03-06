﻿using MSCLoader;
using MSCTranslateChs.Script.Common.Procurios.Public;
using System;
using System.Collections;
using System.Net;
using System.Security.Cryptography;
using System.Text;


namespace MSCTranslateChs.Script.Common.Translate
{
    public class TranslateApi
    {
        private static LOGGER logger = new LOGGER(typeof(TranslateApi));

        public string appid = "";
        public string apikey = "";
        public string translationErrorString = "[自动翻译失败]";

        public TranslateApi(string appid, string apikey)
        {
            this.appid = appid;
            this.apikey = apikey;
        }

        public string GetSign(string source, int salt)
        {
            string str = appid + source + salt + apikey;
            string result = StringToMd5(str);
            logger.LOG("StringToMd5 : " + str + "->" + result);
            return result;
        }

        public string StringToMd5(string str)
        {
            string result = "";
            MD5 md5 = MD5.Create();
            byte[] s = md5.ComputeHash(Encoding.UTF8.GetBytes(str));
            for (int i = 0; i < s.Length; i++)
            {
                result = result + s[i].ToString("x2");
            }
            return result;
        }


        public Hashtable TranslationFromBaiduFanyi(string source, string from, string to)
        {
            System.Random rd = new System.Random();
            int salt = rd.Next();

            UTF8Encoding utf8 = new UTF8Encoding();
            Byte[] encodedBytes = utf8.GetBytes(source);
            source = utf8.GetString(encodedBytes);

            string jsonResult = string.Empty;
            string url = string.Format(
                    "http://api.fanyi.baidu.com/api/trans/vip/translate?q={0}&from={1}&to={2}&appid={3}&salt={4}&sign={5}",
                    // HttpUtility.UrlEncode(source, Encoding.UTF8),
                    source.Replace(" ", "+"),
                    from.ToLower(),
                    to.ToLower(),
                    appid,
                    salt,
                    GetSign(source, salt)
                );
            logger.LOG("翻译API请求的URL : " + url);
            WebClient wc = new WebClient();

            jsonResult = wc.DownloadString(url);
            logger.LOG("翻译API请求的结果 : " + jsonResult);

            Byte[] jsonResultEncodedBytes = utf8.GetBytes(jsonResult);
            jsonResult = utf8.GetString(jsonResultEncodedBytes);

            Hashtable resultTable = JSON.JsonDecode(jsonResult) as Hashtable;
            return resultTable;
        }

        public string TranslationEnglishToChineseFromBaiduFanyi(string source)
        {
            try
            {
                Hashtable resultTable = TranslationFromBaiduFanyi(source, "en", "zh");
                if (resultTable != null &&
                    resultTable["error_code"] == null &&
                    resultTable["trans_result"] != null &&
                     (resultTable["trans_result"] as ArrayList).Count > 0 &&
                     ((resultTable["trans_result"] as ArrayList)[0] as Hashtable) != null &&
                     ((resultTable["trans_result"] as ArrayList)[0] as Hashtable)["dst"] != null
                    )
                {
                    return ((resultTable["trans_result"] as ArrayList)[0] as Hashtable)["dst"].ToString();
                } else if (resultTable["error_code"] != null && resultTable["error_msg"] != null)
                {
                    return translationErrorString + resultTable["error_code"] + "->" + resultTable["error_msg"];
                }
                else
                {
                    return translationErrorString;
                }
            }
            catch (Exception e)
            {
                logger.LOG("翻译API英文到中文出现异常 : " + e.Message);
                logger.LOG(e);
                return translationErrorString;
            }
        }

        public static void Main(string[] args)
        {
            Console.WriteLine("test hashtable");
            Console.WriteLine(new TranslateApi("", "").TranslationFromBaiduFanyi("racing flywheel", "en", "zh"));
            Console.WriteLine("test string");
            Console.WriteLine(new TranslateApi("", "").TranslationEnglishToChineseFromBaiduFanyi("racing flywheel")); 
        }
    }
}
