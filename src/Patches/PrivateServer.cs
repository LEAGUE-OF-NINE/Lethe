using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using System.IO;
using HarmonyLib;
using System.Net;
using UnityEngine.ProBuilder;
using System.Threading;
using BepInEx.Logging;
using SimpleJSON;
using BepInEx.Configuration;
using Steamworks;
using Server;
using ServerConfig;
using System.Collections;
using UnhollowerRuntimeLib;
using BepInEx.IL2CPP.Utils;


namespace LimbusSandbox.Patches
{
    internal class PrivateServer : MonoBehaviour
    {
        private static string _tokenPath;
        public static ConfigEntry<string> ConfigServer;
        public static ManualLogSource Log;
        public static HttpListener _listener;
        private static readonly List<Il2CppSystem.Object> _gcPrevent = new();

        public PrivateServer(IntPtr ptr) : base(ptr) { }
        public static string AccountJwt()
        {
            return File.Exists(_tokenPath) ? File.ReadAllText(_tokenPath) : "";
        }

        public static void Setup(Harmony harmony)
        {
            var hook = SandboxLoader.Instance;
            Log = Plugin.Log;
            var rng = new System.Random();
            var port = rng.Next(30000, 65500);

            ClassInjector.RegisterTypeInIl2Cpp<PrivateServer>();
            var appdata = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            harmony.PatchAll(typeof(PrivateServer));
            _tokenPath = Path.Combine(appdata, "LimbusPrivateServer.jwt");

            Log.LogWarning("when the weather outside is rizzy, and the gyatt is so skibidi");
            Log.LogWarning("since i gyatt to go, ohio, ohio, ohio!");
            _gcPrevent.Add(hook);

            Plugin.Configuration.TryGetEntry<string>(new ConfigDefinition("LaunchSettings", "ServerURL"), out var config);
            ConfigServer = config;
            if (string.IsNullOrEmpty(ConfigServer.Value))
                Log.LogWarning("ServerURL is empty, using the default server!!!");
            else
                Log.LogInfo("Using private server: " + ConfigServer.Value);
            Application.OpenURL(ConfigServer.Value + $"/auth/login?port={port}");
            _listener = new HttpListener();
            hook.StartCoroutine(HttpCoroutine(port));


        }

        private static IEnumerator HttpCoroutine(int port)
        {
            try
            {
                _listener.Prefixes.Add($"http://localhost:{port}/");
                Log.LogInfo($"Starting HTTP server at {port}...");
                _listener.Start();
                Log.LogInfo($"Started HTTP server at {port}...");
                ServerLoop();
            }
            catch (Exception ex)
            {
                Log.LogError($"Failed to start HTTP server at {port}: " + ex.Message);
            }

            yield return null;
        }

        private static void ServerLoop()
        {
            while (_listener.IsListening)
            {
                var ctx = _listener.GetContext();
                Log.LogInfo($"Request: {ctx.Request.HttpMethod} {ctx.Request.Url.LocalPath}");
                var req = ctx.Request;
                using var resp = ctx.Response;
                resp.StatusCode = (int)HttpStatusCode.OK;

                resp.Headers.Add("Content-Type", "application/json");
                resp.Headers.Add("Access-Control-Allow-Methods", "*");
                resp.Headers.Add("Access-Control-Allow-Headers", "*");
                resp.Headers.Add("Vary", "Origin");
                var origin = req.Headers.Get("Origin");
                if (origin != null)
                    resp.Headers.Add("Access-Control-Allow-Origin", origin);

                if (req.HttpMethod == "OPTIONS") continue;

                try
                {
                    switch (req.Url.LocalPath)
                    {
                        case "/auth/login":
                            AuthLogin(req, resp);
                            return;
                        default:
                            resp.StatusCode = 404;
                            break;
                    }
                }
                catch
                {
                    resp.StatusCode = 500;
                }
            }
        }

        private static void AuthLogin(HttpListenerRequest req, HttpListenerResponse resp)
        {
            if (req.HttpMethod != "POST")
            {
                resp.StatusCode = 405;
                return;
            }

            using var reader = new StreamReader(req.InputStream);
            var json = reader.ReadToEnd();
            var token = JSON.Parse(json)["token"].Value;
            File.WriteAllText(_tokenPath, token);
        }

        [HarmonyPatch(typeof(SteamUser), nameof(SteamUser.GetAuthSessionTicket))]
        [HarmonyPrefix]
        private static bool SteamUser_GetAuthSessionTicket(ref AuthTicket __result)
        {
            __result = new AuthTicket
            {
                Data = Encoding.ASCII.GetBytes(AccountJwt())
            };

            return false;
        }

        [HarmonyPatch(typeof(HttpApiRequester), nameof(HttpApiRequester.OnResponseWithErrorCode))]
        [HarmonyPrefix]
        private static void OnResponseWithErrorCode(HttpApiRequester __instance)
        {
            __instance._errorCount = 0;
        }

        [HarmonyPatch(typeof(ServerSelector), nameof(ServerSelector.GetServerURL))]
        [HarmonyPostfix]
        private static void ServerSelector_GetServerURL(ServerSelector __instance, ref string __result)
        {
            var serverURL = ConfigServer.Value;
            if (!string.IsNullOrEmpty(serverURL)) __result = serverURL;
        }
    }
}
