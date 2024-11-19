using System;
using System.Collections;
using System.IO;
using System.Net;
using System.Linq;
using BepInEx;
using BepInEx.IL2CPP.Utils;
using BepInEx.Logging;
using Il2CppSystem.Collections.Generic;
using SimpleJSON;
using UnhollowerRuntimeLib;
using UnityEngine;
using Object = Il2CppSystem.Object;

namespace CustomEncounter;

public class CustomEncounterHook : MonoBehaviour
{
    public static ManualLogSource LOG;

    public static DirectoryInfo CustomAppearanceDir, CustomSpriteDir, CustomLocaleDir, CustomAssistantDir;
    private static string _tokenPath;
    private static HttpListener _listener;

    private static readonly List<Object> _gcPrevent = new();

    public CustomEncounterHook(IntPtr ptr) : base(ptr)
    {
    }

    internal void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha0))
        {
            EncounterHelper.SaveLocale();
            EncounterHelper.SaveEncounters();
            EncounterHelper.SaveIdentities();
        }
        if (Input.GetKeyDown(KeyCode.Alpha8))
        {
            Singleton<StaticDataManager>.Instance._isDataLoaded = false;
            GlobalGameManager.Instance.LoadUserDataAndSetScene(SCENE_STATE.Main);
            Patches.Data.LoadCustomLocale(Singleton<TextDataManager>.Instance, GlobalGameManager.Instance.Lang);
        }
    }

    public static string AccountJwt()
    {
        return File.Exists(_tokenPath) ? File.ReadAllText(_tokenPath) : "";
    }

    internal static void Setup(ManualLogSource log, int port)
    {
        ClassInjector.RegisterTypeInIl2Cpp<CustomEncounterHook>();

        LOG = log;

        GameObject obj = new("CustomEncounterHook");
        DontDestroyOnLoad(obj);
        obj.hideFlags |= HideFlags.HideAndDontSave;
        var hook = obj.AddComponent<CustomEncounterHook>();
        _gcPrevent.Add(hook);

        CustomAppearanceDir = Directory.CreateDirectory(Path.Combine(Paths.ConfigPath, "custom_appearance"));
        CustomSpriteDir = Directory.CreateDirectory(Path.Combine(Paths.ConfigPath, "custom_sprites"));
        CustomLocaleDir = Directory.CreateDirectory(Path.Combine(Paths.ConfigPath, "custom_limbus_locale"));
        CustomAssistantDir = Directory.CreateDirectory(Path.Combine(Paths.ConfigPath, "custom_assistant"));

        var appdata = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        _tokenPath = Path.Combine(appdata, "LimbusPrivateServer.jwt");

        _listener = new HttpListener();

        hook.StartCoroutine(HttpCoroutine(port));
    }

    public static void StopHttp()
    {
        _listener.Stop();
    }

    private static IEnumerator HttpCoroutine(int port)
    {
        try
        {
            _listener.Prefixes.Add($"http://localhost:{port}/");
            LOG.LogInfo($"Starting HTTP server at {port}...");
            _listener.Start();
            LOG.LogInfo($"Started HTTP server at {port}...");
            ServerLoop();
        }
        catch (Exception ex)
        {
            LOG.LogError($"Failed to start HTTP server at {port}: " + ex.Message);
        }

        yield return null;
    }

    private static void ServerLoop()
    {
        while (_listener.IsListening)
        {
            var ctx = _listener.GetContext();
            LOG.LogInfo($"Request: {ctx.Request.HttpMethod} {ctx.Request.Url.LocalPath}");
            var req = ctx.Request;
            using var resp = ctx.Response;
            resp.StatusCode = (int)HttpStatusCode.OK;

            resp.Headers.Add("Content-Type", "application/json");
            resp.Headers.Add("Access-Control-Allow-Methods", "*");
            resp.Headers.Add("Access-Control-Allow-Headers", "*");
            resp.Headers.Add("Vary", "Origin");
            var origin = req.Headers.Get("Origin");
            if (origin != null && origin.StartsWith("http://localhost:"))
                resp.Headers.Add("Access-Control-Allow-Origin", origin);
            else
                resp.Headers.Add("Access-Control-Allow-Origin", "https://lethelc.site");

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
        System.IO.File.WriteAllText(_tokenPath, token);
    }
}