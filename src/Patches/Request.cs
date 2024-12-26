using System;
using System.Collections;
using System.Text;
using BepInEx.IL2CPP.Utils;
using HarmonyLib;
using Server;
using UnhollowerRuntimeLib;
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
    public static bool PreSendRequest(HttpApiRequester __instance, HttpApiSchema httpApiSchema)
    {
        _instance.StartCoroutine(PostWebRequest(__instance, httpApiSchema, false));
        return false;
    }

    public static void EnqueueWebRequest(HttpApiRequester requester, HttpApiSchema httpApiSchema, bool isUrgent)
    {
        _instance.StartCoroutine(PostWebRequest(requester, httpApiSchema, true));
    }

    private static IEnumerator PostWebRequest(HttpApiRequester requester, HttpApiSchema httpApiSchema, bool isUrgent)
    {
        var www = UnityWebRequest.Post(httpApiSchema.URL, httpApiSchema.RequestJson);
        try
        {
            var bytes = Encoding.UTF8.GetBytes(httpApiSchema.RequestJson);
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
                httpApiSchema.ResponseEvent.Invoke(www.downloadHandler.text);
            }
        }
        finally
        {
            www.Dispose();
        }
    }
}