using Addressable;
using HarmonyLib;
using Il2CppSystem.IO;
using SD;
using System.Linq;
using UnhollowerRuntimeLib;
using UnityEngine;

namespace CustomEncounter.Patches;

public class Skin : MonoBehaviour
{
    
    public static void Setup(Harmony harmony)
    {
        ClassInjector.RegisterTypeInIl2Cpp<Skin>();
        foreach (var declaredMethod in AccessTools.GetDeclaredMethods(typeof(SDCharacterSkinUtil)))
            if (declaredMethod.Name == nameof(SDCharacterSkinUtil.CreateSkin))
            {
                CustomEncounterHook.LOG.LogInfo($"Patching {declaredMethod.Name}");
                var isDirect = declaredMethod.GetParameters().ToArray().Any(x => x.Name == "appearanceID");
                var hookName = isDirect ? nameof(CreateSkin) : nameof(CreateSkinForModel);
                harmony.Patch(declaredMethod, new HarmonyMethod(AccessTools.Method(typeof(Skin), hookName)));
            }
    }
    
    public static bool CreateSkinForModel(BattleUnitView view, BattleUnitModel unit, Transform parent,
        ref CharacterAppearance __result)
    {
        return CreateSkin(view, unit.GetAppearanceID(), parent, ref __result);
    }

    public static bool CreateSkin(BattleUnitView view, string appearanceID, Transform parent,
        ref CharacterAppearance __result)
    {
        var label = "";
        CustomEncounterHook.LOG.LogInfo($"Loading asset {label}: {appearanceID}");

        var prefix = "!custom_";
        if (appearanceID.StartsWith(prefix))
        {
            appearanceID = appearanceID.Substring(prefix.Length);
            var prefabPath = "";
            if (appearanceID.Contains("/"))
            {
                var appearancePath = appearanceID.Split("/".ToCharArray(), 2);
                appearanceID = appearancePath[0];
                prefabPath = appearancePath[1];
            }

            var path = Path.Combine(CustomEncounterHook.CustomAppearanceDir.FullName, appearanceID + ".bundle");
            if (!File.Exists(path))
            {
                CustomEncounterHook.LOG.LogError("Cannot find asset bundle at: " + path);
                return true;
            }

            var bundle = AssetBundle.LoadFromFile(path);
            try
            {
                if (prefabPath == "")
                {
                    foreach (var assetName in bundle.AllAssetNames()) CustomEncounterHook.LOG.LogInfo("Found asset: " + assetName);

                    prefabPath = bundle.AllAssetNames()[0];
                }

                CustomEncounterHook.LOG.LogInfo("Using prefab: " + prefabPath);
                var asset = bundle.LoadAsset(prefabPath);
                var gameObject = Instantiate(asset, parent.position, parent.rotation, parent).Cast<GameObject>();
                var appearance = gameObject.GetComponent<CharacterAppearance>();
                if (appearance != null)
                {
                    appearance.charInfo.appearanceID = appearanceID;
                    appearance.Initialize(view);
                    __result = appearance;
                    return false;
                }

                CustomEncounterHook.LOG.LogError(appearanceID + ": appearance is null!?");
            }
            finally
            {
                bundle.Unload(false);
            }
        }

        prefix = "!abno_";
        if (appearanceID.StartsWith(prefix))
        {
            label = SDCharacterSkinUtil._LABEL_ABNORMALITY;
            appearanceID = appearanceID.Substring(prefix.Length);
        }

        prefix = "!enemy_";
        if (appearanceID.StartsWith(prefix))
        {
            label = SDCharacterSkinUtil._LABEL_ENEMY;
            appearanceID = appearanceID.Substring(prefix.Length);
        }

        prefix = "!identity_";
        if (appearanceID.StartsWith(prefix))
        {
            label = SDCharacterSkinUtil._LABEL_PERSONALITY;
            appearanceID = appearanceID.Substring(prefix.Length);
        }

        if (label == "") return true;

        var res = AddressableManager.Instance.LoadAssetSync<GameObject>(label, appearanceID, parent);
        if (res == null) return true;

        var skin = res.Item1.GetComponent<CharacterAppearance>();
        if (skin == null) return true;
        skin.Initialize(view);
        skin.charInfo.appearanceID = appearanceID;
        __result = skin;
        return false;
    }

}