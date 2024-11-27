using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using HarmonyLib;
using UnhollowerRuntimeLib;
using Server;
using System.Net;
using System.IO;
using UnhollowerBaseLib;
using BepInEx.IL2CPP.Utils.Collections;
using MonoMod.Utils;
using SimpleJSON;
using Newtonsoft.Json;
using System.Text.Json;

namespace LimbusSandbox.ps2
{
    internal class ps_cs : MonoBehaviour
    {
        private static string SERVER_URL = "http://127.0.0.1:8080/";
        private static SimpleCrypto crypto => HttpApiRequester.Instance.crypto;
        private static string SECRET_KEY = new SimpleCrypto().SHA512Hash(SimpleCrypto.SECRET_KEY);
        public static void Setup(Harmony harmony)
        {
            ClassInjector.RegisterTypeInIl2Cpp<ps_cs>();
            harmony.PatchAll(typeof(ps_cs));
            var ok = ServerSetup();
            Plugin.Log.LogWarning("SSSSSSSSSSSSSSS");
        }

        private static async Task ServerSetup()
        {
            HttpListener listener = new HttpListener();
            listener.Prefixes.Add(SERVER_URL);
            listener.Start();
            Plugin.Log.LogWarning($"SERVER RUNNING {listener.IsListening}");
            while (listener.IsListening)
            {
                var context = await listener.GetContextAsync();
                var req = context.Request;
                Plugin.Log.LogWarning($"RECEIVED {req.Url} {req.HttpMethod} {req.Headers} {req.RemoteEndPoint}");
                var body = new StreamReader(req.InputStream).ReadToEnd();
                var time = req.Headers.Get("Content-Encrypted");
                var decrypted = decrypt(body, time);                
                Plugin.Log.LogWarning($"decrypted: {decrypted}");
                Plugin.Log.LogWarning($"encypted: {body}");
                var absolutePath = req.Url.ToString().Substring(SERVER_URL.Length);
                Plugin.Log.LogWarning($"absolute path {absolutePath}");
                /*
                    var funky = JsonDocument.Parse(decrypted).RootElement;
                    var bb = new UserAuthData();
                    var ok = JsonUtility.FromJson<UserAuthFormat>(funky[0].ToString());
                    Plugin.Log.LogWarning($"{ok.data_version}");
                */

                if (absolutePath.StartsWith("login"))
                {

                }

                var response = context.Response;

            }
        }

        private static string decrypt(string encrypted, string time)
        {
            var bytes = crypto.HexToBytes(encrypted);
            var decrypted = crypto.Decrypt(bytes, long.Parse(time)).ToArray();
            return Encoding.UTF8.GetString(decrypted);
        }

        private static string encrypt(string decrypted, string time)
        {
            var bro = Encoding.UTF8.GetBytes(decrypted);
            var encrypted = crypto.Encrypt(bro, long.Parse(time)).ToArray();
            var bytes = ByteArrayToHexString(encrypted);
            
            return bytes;
        }

        static string ByteArrayToHexString(byte[] byteArray) { StringBuilder hex = new StringBuilder(byteArray.Length * 2); foreach (byte b in byteArray) { hex.AppendFormat("{0:x2}", b); } return hex.ToString(); }

        private static long get_time()
        {
            var datetime = DateTimeOffset.UtcNow;
            var timestamp = datetime.ToUnixTimeSeconds();
            return timestamp;
        }

        [HarmonyPatch(typeof(HttpApiRequester), nameof(HttpApiRequester.SendRequest))]
        [HarmonyPrefix]
        private static void req(HttpApiSchema httpApiSchema, bool isUrgent)
        {
            if (httpApiSchema._url.Contains("limbuscompanyapi.com") || httpApiSchema._url.Contains("limbuscompanyapi-2.com"))
            {
                httpApiSchema._url = httpApiSchema._url.Replace(@"https://www.limbuscompanyapi.com", SERVER_URL).Replace(@"https://www.limbuscompanyapi-2.com", SERVER_URL);
            }
            Plugin.Log.LogWarning(httpApiSchema._url);
        }
    }
}
