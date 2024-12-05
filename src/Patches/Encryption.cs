using System.Text;
using HarmonyLib;
using Server;
using UnhollowerBaseLib;
using UnhollowerRuntimeLib;
using UnityEngine;

namespace Lethe.Patches;

public class Encryption : Il2CppSystem.Object
{
   
    public static void Setup(Harmony harmony)
    {
        ClassInjector.RegisterTypeInIl2Cpp<Encryption>();
        harmony.PatchAll(typeof(Encryption));
    }

    [HarmonyPatch(typeof(HttpApiRequester), nameof(HttpApiRequester.AddRequest))]
    [HarmonyPrefix]
    public static void PreSendRequest(HttpApiSchema httpApiSchema)
    {
        var og = httpApiSchema._responseEvent;
        var del = delegate(string x)
        {
            LetheHooks.LOG.LogInfo($"RESPONSE {x}");
            og.Invoke(x);
        };
        httpApiSchema._responseEvent = del;
    }


    // public byte[] RgsmRRiuGV3B9ztEZ4Iz(
    //     byte[] PCs8wvRcg7CjVEX9A4Lu,
    //     long Ht2UHBHlM01tpwRTBbc2 = 0,
    //     long NvBA4DbLpBs4rsnWoI64 = 0)

    [HarmonyPatch(typeof(N0oFJaUwpdy3krATpCMh), nameof(N0oFJaUwpdy3krATpCMh.RgsmRRiuGV3B9ztEZ4Iz))]
    [HarmonyPrefix]
    public static bool HookPreRequestEncrypt(ref Il2CppStructArray<byte> __result, Il2CppStructArray<byte> PCs8wvRcg7CjVEX9A4Lu)
    {
        var body = Encoding.UTF8.GetString(PCs8wvRcg7CjVEX9A4Lu);
        LetheHooks.LOG.LogInfo($"PRE ENCRYPT: {body}");
        __result = PCs8wvRcg7CjVEX9A4Lu;
        return false;
    }
   
    [HarmonyPatch(typeof(N0oFJaUwpdy3krATpCMh), nameof(N0oFJaUwpdy3krATpCMh.RQbipqUWzlDQbGHj8aRo))]
    [HarmonyPrefix]
    public static bool HookPostRequestEncrypt(ref string __result, Il2CppStructArray<byte> LGq6JRDEtaXezkSuwqnv)
    {
        var body = Encoding.UTF8.GetString(LGq6JRDEtaXezkSuwqnv);
        LetheHooks.LOG.LogInfo($"POST ENCRYPT: {body}");
        __result = body;
        return false;
    }
   
    [HarmonyPatch(typeof(N0oFJaUwpdy3krATpCMh), nameof(N0oFJaUwpdy3krATpCMh.YzUlOJtzH9J6sgvzNer5))]
    [HarmonyPrefix]
    public static bool HookIdk(ref Il2CppStructArray<byte> __result, Il2CppStructArray<byte> O3KIh9bgqKLXxSoOTlvw)
    {
        var body = Encoding.UTF8.GetString(O3KIh9bgqKLXxSoOTlvw);
        LetheHooks.LOG.LogInfo($"IDK: {body}");
        __result = O3KIh9bgqKLXxSoOTlvw;
        return false;
    }
   
    [HarmonyPatch(typeof(N0oFJaUwpdy3krATpCMh), nameof(N0oFJaUwpdy3krATpCMh.StgGkXNbXb2DLUXzt93H))]
    [HarmonyPrefix]
    public static bool HookIdk(ref Il2CppStructArray<byte> __result, string Kl21xpaajl6eqW8Ix3Vx)
    {
        LetheHooks.LOG.LogInfo($"IDK TOO: {Kl21xpaajl6eqW8Ix3Vx}");
        __result = Encoding.UTF8.GetBytes(Kl21xpaajl6eqW8Ix3Vx);
        return false;
    }
   
    [HarmonyPatch(typeof(HttpRequestCommand<ReqPacket_NULL,ResPacket_CheckClientVersion>), nameof(HttpRequestCommand<ReqPacket_NULL,ResPacket_CheckClientVersion>.OnResponse), new []{ typeof(string) })]
    [HarmonyPrefix]
    public static void HookJson(string responseJson)
    {
        LetheHooks.LOG.LogInfo($"JSON: {responseJson}");
    }


    [HarmonyPatch(typeof(ZWnJkVdXZEzStXzNcdMf), nameof(ZWnJkVdXZEzStXzNcdMf.RQbipqUWzlDQbGHj8aRo))]
    [HarmonyPrefix]
    public static bool HookPreResponseDecrypt(ref Il2CppStructArray<byte> __result, Il2CppStructArray<byte> mfSPgKA8Kz6UshoPEdyT)
    {
        var body = Encoding.UTF8.GetString(mfSPgKA8Kz6UshoPEdyT);
        LetheHooks.LOG.LogInfo($"POST DECRYPT: {body}");
        __result = mfSPgKA8Kz6UshoPEdyT;
        return false;
    }

}