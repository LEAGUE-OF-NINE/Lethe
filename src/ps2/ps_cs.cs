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

namespace LimbusSandbox.ps2
{
    internal class ps_cs : MonoBehaviour
    {
        private static string SERVER_URL = "http://127.0.0.1:8080/";
        private static SimpleCrypto crypto => HttpApiRequester.Instance.crypto;
        private static string SECRET_KEY = crypto.SHA512Hash("");
        public static void Setup(Harmony harmony)
        {
            ClassInjector.RegisterTypeInIl2Cpp<ps_cs>();
            harmony.PatchAll(typeof(ps_cs));
            ServerSetup();
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
                Plugin.Log.LogWarning($"RECEIVED {req.Url} {req.HttpMethod} {req.Headers}");
                var body = new StreamReader(req.InputStream).ReadToEnd();
                var time = req.Headers.Get("Content-Encrypted");
                var decrypted = decrypt(body, time);
                //Plugin.Log.LogWarning();
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
            var encrypted = crypto.Encrypt(Encoding.UTF8.GetBytes(decrypted), get_time());
            var bytes = crypto.BytesToHex(encrypted);
            return bytes;
        }

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
