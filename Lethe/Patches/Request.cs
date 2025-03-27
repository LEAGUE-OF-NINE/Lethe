using System;
using System.Collections;
using System.Reflection;
using System.Text;
using BepInEx.Unity.IL2CPP.Utils;
using HarmonyLib;
using Il2CppInterop.Runtime.Injection;
using Server;
using UnityEngine;
using UnityEngine.Networking;


namespace Lethe.Patches;

public class Request : MonoBehaviour
{
    private static Request _instance;

    public Request(IntPtr ptr) : base(ptr)
    {
    }


    public static void Setup(Harmony harmony)
    {
        ClassInjector.RegisterTypeInIl2Cpp<Request>();

        GameObject obj = new("CustomHttpLoop");
        DontDestroyOnLoad(obj);
        obj.hideFlags |= HideFlags.HideAndDontSave;
        _instance = obj.AddComponent<Request>();
        harmony.PatchAll(typeof(Request));
    }

    [HarmonyPatch(typeof(HttpApiRequester), nameof(HttpApiRequester.AddRequest))]
    [HarmonyPrefix]
    public static bool PreSendRequest(HttpApiRequester __instance, JIMKEFOFIME NEKAOPOFEOI)
    {
        var httpApiSchema = NEKAOPOFEOI;
        _instance.StartCoroutine(PostWebRequest(__instance, httpApiSchema, false));
        return false;
    }

    public static void EnqueueWebRequest(HttpApiRequester requester, JIMKEFOFIME  httpApiSchema, bool isUrgent)
    {
        _instance.StartCoroutine(PostWebRequest(requester, httpApiSchema, true));
    }

    private static IEnumerator PostWebRequest(HttpApiRequester requester, JIMKEFOFIME httpApiSchema, bool isUrgent)
    {
        var url = ReplaceDomain(httpApiSchema.PECNGBJOINB, LetheMain.ConfigServer.Value);
        var www = UnityWebRequest.Post(url, httpApiSchema.MPDJBIGJIDI);
        try
        {
            var bytes = Encoding.UTF8.GetBytes(httpApiSchema.MPDJBIGJIDI);
            www.uploadHandler.Dispose();
            www.uploadHandler = new UploadHandlerRaw(bytes);
            www.SetRequestHeader("Content-Type", "application/json");
            requester.networkingUI.ActiveConnectingText(true);
            yield return www.SendWebRequest();
            while (!www.isDone)
                yield return null;
            requester.networkingUI.ActiveConnectingText(false);
            if (isUrgent) yield break;
            if (www.result == UnityWebRequest.Result.ConnectionError ||
                www.result == UnityWebRequest.Result.ProtocolError)
            {
                if (Application.internetReachability == NetworkReachability.NotReachable)
                    requester.OnResponseWithErrorCode(httpApiSchema, -4, false, true, false);
                else
                    requester.OnResponseWithErrorCode(httpApiSchema, 0, false, true, false);
            }
            else
            {
                // LetheHooks.LOG.LogInfo($"Response: {www.downloadHandler.text}");
                httpApiSchema.PDMIKLHJOAC.Invoke(www.downloadHandler.text);
            }
        }
        finally
        {
            www.Dispose();
        }
    }

    static string ReplaceDomain(string originalUrl, string newHost)
    {
        Uri uri = new Uri(originalUrl);
        Uri target = new Uri(newHost);
        UriBuilder uriBuilder = new UriBuilder(uri)
        {
            Host = target.Host,
            Scheme = target.Scheme
        };
        return uriBuilder.ToString();
    }

    private static void PrintAllMethodValues(JIMKEFOFIME  httpApiSchema)
    {
        Type type = httpApiSchema.GetType();
        MethodInfo[] methods = type.GetMethods(BindingFlags.Public | BindingFlags.Instance);

        foreach (MethodInfo method in methods)
        {
            if (method.GetParameters().Length == 0 && method.ReturnType != typeof(void)) // Only invoke parameterless methods with return values
            {
                try
                {
                    object result = method.Invoke(httpApiSchema, null);
                    LetheHooks.LOG.LogError($"Method: {method.Name}, Value: {result}");
                }
                catch (Exception e)
                {
                    LetheHooks.LOG.LogError($"Method: {method.Name}, Value: {e.Message}");
                }
            }
        }
    }
}