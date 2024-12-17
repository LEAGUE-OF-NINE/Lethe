using BepInEx.Configuration;
using DG.Tweening.Plugins.Core;
using HarmonyLib;
using System;
using Random = System.Random;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using System.IO;
using BepInEx;
using UnityEngine.ProBuilder;
using System.Net;
using MonoMod.RuntimeDetour;
using SimpleJSON;
using System.Text.RegularExpressions;
using BepInEx.IL2CPP.Utils;

namespace LetheV2.Patches
{
    internal class PrivateServer : MonoBehaviour
    {
        public static ConfigEntry<string> ConfigServer;
        private static string _token;
        private static HttpListener _listener;

        public static string AccountJwt()
        {
            return _token;
        }

        public static bool IsSignedIn()
        {
            return !(_token == null || _token.IsNullOrWhiteSpace());
        }

        public void OnApplicationQuit()
        {
            var synchronousData = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "..", "LocalLow", "ProjectMoon", "LimbusCompany", "synchronous-data_product.json");
            LetheV2Main.Log.LogWarning(synchronousData);
            if (File.Exists(synchronousData)) File.Delete(synchronousData);
            //VisualMods.Carra.CleanUpAtClose();
            //VisualMods.Sound.RestoreSound();
        }

        public static void Setup(Harmony harmony)
        {
            LetheV2Main.Configuration.TryGetEntry<string>(new ConfigDefinition("LaunchSettings", "ServerURL"), out var config);
            ConfigServer = config;
            LetheV2Main.Log.LogInfo("PRIVATE SERVER URL: " + config.Value);

            var rng = new Random();
            int port = rng.Next(30000, 65000);

            // Delete legacy jwt dump file
            try
            {
                var appdata = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                var tokenPath = Path.Combine(appdata, "LimbusPrivateServer.jwt");
                File.Delete(tokenPath);
            }
            catch
            {
            }

            _listener = new HttpListener();

            var token = Environment.GetEnvironmentVariable("LETHE_TOKEN");
            Environment.SetEnvironmentVariable("LETHE_TOKEN", null);
            if (token == null)
            {
                Application.OpenURL(ConfigServer.Value + $"/auth/login?port={port}");
                LetheBootstrap.Instance.StartCoroutine(HttpCoroutine(port));
            }
            else
            {
                _token = token;
            }
        }
        private static System.Collections.IEnumerator HttpCoroutine(int port)
        {
            _listener.Prefixes.Add($"http://localhost:{port}/");
            LetheV2Main.Log.LogInfo($"Starting HTTP server at {port}...");
            _listener.Start();
            LetheV2Main.Log.LogInfo($"Started HTTP server at {port}...");
            while (_listener.IsListening)
            {
                var task = _listener.GetContextAsync();
                while (!task.IsCompleted)
                {
                    yield return null;
                }

                ServerLoop(task.Result);
            }
        }

        private static void ServerLoop(HttpListenerContext ctx)
        {
            LetheV2Main.Log.LogInfo($"Request: {ctx.Request.HttpMethod} {ctx.Request.Url.LocalPath}");
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

            if (req.HttpMethod == "OPTIONS") return;

            var path = Regex.Replace(req.Url.LocalPath, "/+", "/");
            LetheV2Main.Log.LogInfo($"Request: {path}");
            try
            {
                if ("/auth/login" == path)
                {
                    AuthLogin(req, resp);
                }
                /*else if (path.StartsWith("/static-data/"))
                {
                    StaticData(req, resp);
                }
                else if (path.StartsWith("/texture/"))
                {
                    Texture(req, resp);
                }*/
                else
                {
                    resp.StatusCode = 404;
                }
            }
            catch (Exception ex)
            {
                LetheV2Main.Log.LogInfo($"Exception in {path}: {ex}");
                resp.StatusCode = 500;
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
            _token = JSON.Parse(json)["token"].Value;
            LetheV2Main.Log.LogInfo("GET");
        }

        /*private static void StaticData(HttpListenerRequest req, HttpListenerResponse resp)
        {
            if (req.HttpMethod != "GET")
            {
                resp.StatusCode = 405;
                return;
            }

            var dataClass = req.Url.LocalPath.Substring("/static-data/".Length);
            LetheV2Main.Log.LogInfo($"Fetching {dataClass}");
            if (!Patches.Login.StaticData.TryGetValue(dataClass, out var staticData))
            {
                resp.StatusCode = 404;
                return;
            }

            var json = new JSONArray();
            foreach (var se in staticData)
            {
                json.Add(JSONNode.Parse(se));
            }

            var body = Encoding.UTF8.GetBytes(json.ToString(2));
            resp.OutputStream.Write(body, 0, body.Length);
            resp.OutputStream.Flush();
        }

        private static void Texture(HttpListenerRequest req, HttpListenerResponse resp)
        {
            if (req.HttpMethod != "GET")
            {
                resp.StatusCode = 405;
                return;
            }

            var path = req.Url.LocalPath.Substring("/texture/".Length).Split("/".ToCharArray(), 2);
            if (path.Length != 2 || !int.TryParse(path[1], out var id))
            {
                resp.StatusCode = 400;
                return;
            }

            Sprite sprite = null;
            switch (path[0])
            {
                case "profile":
                    sprite = Singleton<PlayerUnitSpriteList>.Instance.GetNormalProfileSprite(id);
                    break;
                case "cg":
                    sprite = Singleton<PlayerUnitSpriteList>.Instance.GetCGData(id);
                    break;
                case "skill":
                    var skill = Singleton<StaticDataManager>.Instance.SkillList.GetData(id);
                    if (skill != null) new SkillModel(skill, 1, 1).GetSkillSprite();
                    break;
            }

            if (sprite == null)
            {
                resp.StatusCode = 404;
                return;
            }

            var texture = sprite.texture;
            // Create a temporary RenderTexture of the same size as the texture
            var tmp = RenderTexture.GetTemporary(
                texture.width,
                texture.height,
                0,
                RenderTextureFormat.Default,
                RenderTextureReadWrite.Linear);

            // Blit the pixels on texture to the RenderTexture
            Graphics.Blit(texture, tmp);

            // Backup the currently set RenderTexture
            RenderTexture previous = RenderTexture.active;

            // Set the current RenderTexture to the temporary one we created
            RenderTexture.active = tmp;

            // Create a new readable Texture2D to copy the pixels to it
            Texture2D myTexture2D = new Texture2D(texture.width, texture.height);

            // Copy the pixels from the RenderTexture to the new Texture
            myTexture2D.ReadPixels(new Rect(0, 0, tmp.width, tmp.height), 0, 0);
            myTexture2D.Apply();
            byte[] body = myTexture2D.EncodeToPNG();

            // Reset the active RenderTexture
            RenderTexture.active = previous;

            // Release the temporary RenderTexture
            RenderTexture.ReleaseTemporary(tmp);

            resp.Headers.Set("Content-Type", "image/png");
            resp.OutputStream.Write(body, 0, body.Length);
            resp.OutputStream.Flush();
        }*/
        //
    }
}
