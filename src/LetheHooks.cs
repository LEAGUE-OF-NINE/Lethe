using System;
using System.Collections;
using System.IO;
using System.Net;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using BepInEx;
using BepInEx.IL2CPP.Utils;
using BepInEx.Logging;
using Il2CppSystem.Collections.Generic;
using SimpleJSON;
using UnhollowerRuntimeLib;
using UnityEngine;
using Object = Il2CppSystem.Object;

namespace Lethe;

public class LetheHooks : MonoBehaviour
{
    public static ManualLogSource LOG;

    public static DirectoryInfo CustomAppearanceDir, CustomSpriteDir, CustomLocaleDir, CustomAssistantDir;
    private static string _token;
    private static HttpListener _listener;

    private static readonly List<Object> _gcPrevent = new();

    public LetheHooks(IntPtr ptr) : base(ptr)
    {
    }

    internal void Update()
    {
        if (Input.GetKeyDown(LetheMain.dumpDataKey.Value))
        {
            EncounterHelper.SaveLocale();
            EncounterHelper.SaveEncounters();
            EncounterHelper.SaveIdentities();
        }
        if (Input.GetKeyDown(LetheMain.reloadDataKey.Value))
        {
            Singleton<StaticDataManager>.Instance._isDataLoaded = false;
            GlobalGameManager.Instance.LoadUserDataAndSetScene(SCENE_STATE.Main);
            Patches.Data.LoadCustomLocale(GlobalGameManager.Instance.Lang);
        }
    }

    public void OnApplicationQuit()
    {
        var synchronousData = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "..", "LocalLow", "ProjectMoon", "LimbusCompany", "synchronous-data_product.json");
        LOG.LogWarning(synchronousData);
        if (File.Exists(synchronousData)) File.Delete(synchronousData);
        VisualMods.Carra.CleanUpAtClose();
        VisualMods.Sound.RestoreSound();
    }

    public static string AccountJwt()
    {
        return _token;
    }

    public static bool IsSignedIn()
    {
        return !(_token == null || _token.IsNullOrWhiteSpace());
    }

    internal static void Setup(ManualLogSource log, int port)
    {
        ClassInjector.RegisterTypeInIl2Cpp<LetheHooks>();

        LOG = log;

        GameObject obj = new("CustomEncounterHook");
        DontDestroyOnLoad(obj);
        obj.hideFlags |= HideFlags.HideAndDontSave;
        var hook = obj.AddComponent<LetheHooks>();
        _gcPrevent.Add(hook);

        CustomAppearanceDir = Directory.CreateDirectory(Path.Combine(Paths.ConfigPath, "custom_appearance"));
        CustomSpriteDir = Directory.CreateDirectory(Path.Combine(Paths.ConfigPath, "custom_sprites"));
        CustomLocaleDir = Directory.CreateDirectory(Path.Combine(Paths.ConfigPath, "custom_limbus_locale"));
        CustomAssistantDir = Directory.CreateDirectory(Path.Combine(Paths.ConfigPath, "custom_assistant"));

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
            Application.OpenURL(LetheMain.ConfigServer.Value + $"/auth/login?port={port}");
            hook.StartCoroutine(HttpCoroutine(port));
        }
        else
        {
            _token = token;
        }
    }

    private static IEnumerator HttpCoroutine(int port)
    {
        _listener.Prefixes.Add($"http://localhost:{port}/");
        LOG.LogInfo($"Starting HTTP server at {port}...");
        _listener.Start();
        LOG.LogInfo($"Started HTTP server at {port}...");
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
        LOG.LogInfo($"Request: {ctx.Request.HttpMethod} {ctx.Request.Url.LocalPath}");
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
        LOG.LogInfo($"Request: {path}");
        try
        {
            if ("/auth/login" == path)
            {
                AuthLogin(req, resp);
            }
            else if (path.StartsWith("/static-data/"))
            {
                StaticData(req, resp);
            }
            else if (path.StartsWith("/texture/"))
            {
                Texture(req, resp);
            }
            else
            {
                resp.StatusCode = 404;
            }
        }
        catch (Exception ex)
        {
            LOG.LogInfo($"Exception in {path}: {ex}");
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
    }
    
    private static void StaticData(HttpListenerRequest req, HttpListenerResponse resp)
    {
        if (req.HttpMethod != "GET")
        {
            resp.StatusCode = 405;
            return;
        }

        var dataClass = req.Url.LocalPath.Substring("/static-data/".Length);
        LOG.LogInfo($"Fetching {dataClass}");
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
    }
    
}