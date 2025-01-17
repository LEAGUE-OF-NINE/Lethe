using HarmonyLib;
using Server;
using UnhollowerRuntimeLib;

namespace Lethe.Patches;

public class BattleLog : Il2CppSystem.Object
{

    public static void Setup(Harmony harmony)
    {
        ClassInjector.RegisterTypeInIl2Cpp<BattleLog>();
        harmony.PatchAll(typeof(BattleLog));
    }

    [HarmonyPatch(typeof(HttpBattleLogRequester), nameof(HttpBattleLogRequester.EnqueueRequest))]
    [HarmonyPrefix]
    public static bool PreBattleLogEnqueueRequest()
    {
        LetheHooks.LOG.LogInfo($"WARNING: LIMBUS TRIED TO REPORT TO PROJECT MOON");
        return false;
    }

    [HarmonyPatch(typeof(HttpSubAccountLogRequester), nameof(HttpSubAccountLogRequester.EnqueueRequest))]
    [HarmonyPrefix]
    public static bool PreSubBattleLogEnqueueRequest()
    {
        LetheHooks.LOG.LogInfo($"WARNING: SubBattleLog request intercepted");
        return false;
    }

    [HarmonyPatch(typeof(HttpSubAccountLogRequester), nameof(HttpSubAccountLogRequester.SendRequest))]
    [HarmonyPrefix]
    public static bool PreSubBattleLogSendRequest()
    {
        LetheHooks.LOG.LogInfo($"WARNING: SubBattleLog request intercepted");
        return false;
    }

    [HarmonyPatch(typeof(HttpSubAccountLogRequester), nameof(HttpSubAccountLogRequester.ABEMNHKDCKN))]
    [HarmonyPrefix]
    public static bool interceptABEMNHKDCKN()
    {
        LetheHooks.LOG.LogInfo($"WARNING: SubBattleLog ABEMNHKDCKN request intercepted");
        return false;
    }

    [HarmonyPatch(typeof(HttpSubAccountLogRequester), nameof(HttpSubAccountLogRequester.ABGNBKMIBHC))]
    [HarmonyPrefix]
    public static bool ABGNBKMIBHC()
    {
        LetheHooks.LOG.LogInfo($"WARNING: SubBattleLog ABGNBKMIBHC request intercepted");
        return false;
    }

    [HarmonyPatch(typeof(HttpSubAccountLogRequester), nameof(HttpSubAccountLogRequester.AGFKHFPHLMO))]
    [HarmonyPrefix]
    public static bool AGFKHFPHLMO()
    {
        LetheHooks.LOG.LogInfo($"WARNING: SubBattleLog AGFKHFPHLMO request intercepted");
        return false;
    }

    [HarmonyPatch(typeof(HttpSubAccountLogRequester), nameof(HttpSubAccountLogRequester.AGLCCKJHPHO))]
    [HarmonyPrefix]
    public static bool AGLCCKJHPHO()
    {
        LetheHooks.LOG.LogInfo($"WARNING: SubBattleLog AGLCCKJHPHO request intercepted");
        return false;
    }

    [HarmonyPatch(typeof(HttpSubAccountLogRequester), nameof(HttpSubAccountLogRequester.AHEMCFBJCJJ))]
    [HarmonyPrefix]
    public static bool AHEMCFBJCJJ()
    {
        LetheHooks.LOG.LogInfo($"WARNING: SubBattleLog AHEMCFBJCJJ request intercepted");
        return false;
    }

    [HarmonyPatch(typeof(HttpSubAccountLogRequester), nameof(HttpSubAccountLogRequester.AHJKDNEKDIP))]
    [HarmonyPrefix]
    public static bool AHJKDNEKDIP()
    {
        LetheHooks.LOG.LogInfo($"WARNING: SubBattleLog AHJKDNEKDIP request intercepted");
        return false;
    }

    [HarmonyPatch(typeof(HttpSubAccountLogRequester), nameof(HttpSubAccountLogRequester.AIJIGMEGNGL))]
    [HarmonyPrefix]
    public static bool AIJIGMEGNGL()
    {
        LetheHooks.LOG.LogInfo($"WARNING: SubBattleLog AIJIGMEGNGL request intercepted");
        return false;
    }

    [HarmonyPatch(typeof(HttpSubAccountLogRequester), nameof(HttpSubAccountLogRequester.APOFHIEMANL))]
    [HarmonyPrefix]
    public static bool APOFHIEMANL()
    {
        LetheHooks.LOG.LogInfo($"WARNING: SubBattleLog APOFHIEMANL request intercepted");
        return false;
    }

    [HarmonyPatch(typeof(HttpSubAccountLogRequester), nameof(HttpSubAccountLogRequester.BAGDCAHMHGP))]
    [HarmonyPrefix]
    public static bool BAGDCAHMHGP()
    {
        LetheHooks.LOG.LogInfo($"WARNING: SubBattleLog BAGDCAHMHGP request intercepted");
        return false;
    }

    [HarmonyPatch(typeof(HttpSubAccountLogRequester), nameof(HttpSubAccountLogRequester.BBBMNNHCMKN))]
    [HarmonyPrefix]
    public static bool BBBMNNHCMKN()
    {
        LetheHooks.LOG.LogInfo($"WARNING: SubBattleLog BBBMNNHCMKN request intercepted");
        return false;
    }

    [HarmonyPatch(typeof(HttpSubAccountLogRequester), nameof(HttpSubAccountLogRequester.BBJBAMGOKPM))]
    [HarmonyPrefix]
    public static bool BBJBAMGOKPM()
    {
        LetheHooks.LOG.LogInfo($"WARNING: SubBattleLog BBJBAMGOKPM request intercepted");
        return false;
    }

    [HarmonyPatch(typeof(HttpSubAccountLogRequester), nameof(HttpSubAccountLogRequester.BDEBAMOAAOK))]
    [HarmonyPrefix]
    public static bool BDEBAMOAAOK()
    {
        LetheHooks.LOG.LogInfo($"WARNING: SubBattleLog BDEBAMOAAOK request intercepted");
        return false;
    }

    [HarmonyPatch(typeof(HttpSubAccountLogRequester), nameof(HttpSubAccountLogRequester.BHJCCEHFKKC))]
    [HarmonyPrefix]
    public static bool BHJCCEHFKKC()
    {
        LetheHooks.LOG.LogInfo($"WARNING: SubBattleLog BHJCCEHFKKC request intercepted");
        return false;
    }

    [HarmonyPatch(typeof(HttpSubAccountLogRequester), nameof(HttpSubAccountLogRequester.BIJEBIHACEP))]
    [HarmonyPrefix]
    public static bool BIJEBIHACEP()
    {
        LetheHooks.LOG.LogInfo($"WARNING: SubBattleLog BHJCCEHFKKC request intercepted");
        return false;
    }

    [HarmonyPatch(typeof(HttpSubAccountLogRequester), nameof(HttpSubAccountLogRequester.BMGLOEMNGND))]
    [HarmonyPrefix]
    public static bool BMGLOEMNGND()
    {
        LetheHooks.LOG.LogInfo($"WARNING: SubBattleLog BMGLOEMNGND request intercepted");
        return false;
    }

    [HarmonyPatch(typeof(HttpSubAccountLogRequester), nameof(HttpSubAccountLogRequester.CAMGFLIKCBO))]
    [HarmonyPrefix]
    public static bool CAMGFLIKCBO()
    {
        LetheHooks.LOG.LogInfo($"WARNING: SubBattleLog CAMGFLIKCBO request intercepted");
        return false;
    }

    [HarmonyPatch(typeof(HttpSubAccountLogRequester), nameof(HttpSubAccountLogRequester.CCIPFECOPPO))]
    [HarmonyPrefix]
    public static bool CCIPFECOPPO()
    {
        LetheHooks.LOG.LogInfo($"WARNING: SubBattleLog CCIPFECOPPO request intercepted");
        return false;
    }

    [HarmonyPatch(typeof(HttpSubAccountLogRequester), nameof(HttpSubAccountLogRequester.CKBEACOEBNL))]
    [HarmonyPrefix]
    public static bool CKBEACOEBNL()
    {
        LetheHooks.LOG.LogInfo($"WARNING: SubBattleLog CKBEACOEBNL request intercepted");
        return false;
    }

    [HarmonyPatch(typeof(HttpSubAccountLogRequester), nameof(HttpSubAccountLogRequester.CKBKEFBMAAE))]
    [HarmonyPrefix]
    public static bool CKBKEFBMAAE()
    {
        LetheHooks.LOG.LogInfo($"WARNING: SubBattleLog CKBKEFBMAAE request intercepted");
        return false;
    }

    [HarmonyPatch(typeof(HttpSubAccountLogRequester), nameof(HttpSubAccountLogRequester.CNLLFHCODBF))]
    [HarmonyPrefix]
    public static bool CNLLFHCODBF()
    {
        LetheHooks.LOG.LogInfo($"WARNING: SubBattleLog CNLLFHCODBF request intercepted");
        return false;
    }

    [HarmonyPatch(typeof(HttpSubAccountLogRequester), nameof(HttpSubAccountLogRequester.DCCGJLEKJKB))]
    [HarmonyPrefix]
    public static bool DCCGJLEKJKB()
    {
        LetheHooks.LOG.LogInfo($"WARNING: SubBattleLog DCCGJLEKJKB request intercepted");
        return false;
    }

    [HarmonyPatch(typeof(HttpSubAccountLogRequester), nameof(HttpSubAccountLogRequester.DINEOBBFNHK))]
    [HarmonyPrefix]
    public static bool DINEOBBFNHK()
    {
        LetheHooks.LOG.LogInfo($"WARNING: SubBattleLog DINEOBBFNHK request intercepted");
        return false;
    }

    [HarmonyPatch(typeof(HttpSubAccountLogRequester), nameof(HttpSubAccountLogRequester.DJHJFDEFCMP))]
    [HarmonyPrefix]
    public static bool DJHJFDEFCMP()
    {
        LetheHooks.LOG.LogInfo($"WARNING: SubBattleLog DJHJFDEFCMP request intercepted");
        return false;
    }

    [HarmonyPatch(typeof(HttpSubAccountLogRequester), nameof(HttpSubAccountLogRequester.DLKOJJPAHFG))]
    [HarmonyPrefix]
    public static bool DLKOJJPAHFG()
    {
        LetheHooks.LOG.LogInfo($"WARNING: SubBattleLog DLKOJJPAHFG request intercepted");
        return false;
    }

    [HarmonyPatch(typeof(HttpSubAccountLogRequester), nameof(HttpSubAccountLogRequester.DLMFEMGPAEA))]
    [HarmonyPrefix]
    public static bool DLMFEMGPAEA()
    {
        LetheHooks.LOG.LogInfo($"WARNING: SubBattleLog CNLLFHCODBF request intercepted");
        return false;
    }

    [HarmonyPatch(typeof(HttpSubAccountLogRequester), nameof(HttpSubAccountLogRequester.DNCOCMNBELF))]
    [HarmonyPrefix]
    public static bool DNCOCMNBELF()
    {
        LetheHooks.LOG.LogInfo($"WARNING: SubBattleLog DNCOCMNBELF request intercepted");
        return false;
    }

    [HarmonyPatch(typeof(HttpSubAccountLogRequester), nameof(HttpSubAccountLogRequester.ECJECPLMKEH))]
    [HarmonyPrefix]
    public static bool ECJECPLMKEH()
    {
        LetheHooks.LOG.LogInfo($"WARNING: SubBattleLog ECJECPLMKEH request intercepted");
        return false;
    }

    [HarmonyPatch(typeof(HttpSubAccountLogRequester), nameof(HttpSubAccountLogRequester.FBLMPBBMIDN))]
    [HarmonyPrefix]
    public static bool FBLMPBBMIDN()
    {
        LetheHooks.LOG.LogInfo($"WARNING: SubBattleLog FBLMPBBMIDN request intercepted");
        return false;
    }

    [HarmonyPatch(typeof(HttpSubAccountLogRequester), nameof(HttpSubAccountLogRequester.FCOOGBAMAKJ))]
    [HarmonyPrefix]
    public static bool FCOOGBAMAKJ()
    {
        LetheHooks.LOG.LogInfo($"WARNING: SubBattleLog FCOOGBAMAKJ request intercepted");
        return false;
    }

    [HarmonyPatch(typeof(HttpSubAccountLogRequester), nameof(HttpSubAccountLogRequester.FDLJHLJGDIE))]
    [HarmonyPrefix]
    public static bool FDLJHLJGDIE()
    {
        LetheHooks.LOG.LogInfo($"WARNING: SubBattleLog FDLJHLJGDIE request intercepted");
        return false;
    }

    [HarmonyPatch(typeof(HttpSubAccountLogRequester), nameof(HttpSubAccountLogRequester.FDODOCBBHNO))]
    [HarmonyPrefix]
    public static bool FDODOCBBHNO()
    {
        LetheHooks.LOG.LogInfo($"WARNING: SubBattleLog FDODOCBBHNO request intercepted");
        return false;
    }

    [HarmonyPatch(typeof(HttpSubAccountLogRequester), nameof(HttpSubAccountLogRequester.FHAIDOPNKLG))]
    [HarmonyPrefix]
    public static bool FHAIDOPNKLG()
    {
        LetheHooks.LOG.LogInfo($"WARNING: SubBattleLog FHAIDOPNKLG request intercepted");
        return false;
    }

    [HarmonyPatch(typeof(HttpSubAccountLogRequester), nameof(HttpSubAccountLogRequester.FHOPEDMNNNE))]
    [HarmonyPrefix]
    public static bool FHOPEDMNNNE()
    {
        LetheHooks.LOG.LogInfo($"WARNING: SubBattleLog FHOPEDMNNNE request intercepted");
        return false;
    }

    [HarmonyPatch(typeof(HttpSubAccountLogRequester), nameof(HttpSubAccountLogRequester.FLFEPDBENOG))]
    [HarmonyPrefix]
    public static bool FLFEPDBENOG()
    {
        LetheHooks.LOG.LogInfo($"WARNING: SubBattleLog FLFEPDBENOG request intercepted");
        return false;
    }

    [HarmonyPatch(typeof(HttpSubAccountLogRequester), nameof(HttpSubAccountLogRequester.GBKOPOOJHEO))]
    [HarmonyPrefix]
    public static bool GBKOPOOJHEO()
    {
        LetheHooks.LOG.LogInfo($"WARNING: SubBattleLog GBKOPOOJHEO request intercepted");
        return false;
    }

    [HarmonyPatch(typeof(HttpSubAccountLogRequester), nameof(HttpSubAccountLogRequester.GIBMPOMJBCJ))]
    [HarmonyPrefix]
    public static bool GIBMPOMJBCJ()
    {
        LetheHooks.LOG.LogInfo($"WARNING: SubBattleLog GIBMPOMJBCJ request intercepted");
        return false;
    }

    [HarmonyPatch(typeof(HttpSubAccountLogRequester), nameof(HttpSubAccountLogRequester.GMOGAHDFHCA))]
    [HarmonyPrefix]
    public static bool GMOGAHDFHCA()
    {
        LetheHooks.LOG.LogInfo($"WARNING: SubBattleLog GMOGAHDFHCA request intercepted");
        return false;
    }

    [HarmonyPatch(typeof(HttpSubAccountLogRequester), nameof(HttpSubAccountLogRequester.GPNGAHECJPI))]
    [HarmonyPrefix]
    public static bool GPNGAHECJPI()
    {
        LetheHooks.LOG.LogInfo($"WARNING: SubBattleLog GPNGAHECJPI request intercepted");
        return false;
    }

    [HarmonyPatch(typeof(HttpSubAccountLogRequester), nameof(HttpSubAccountLogRequester.HHAIJAMCGBB))]
    [HarmonyPrefix]
    public static bool HHAIJAMCGBB()
    {
        LetheHooks.LOG.LogInfo($"WARNING: SubBattleLog HHAIJAMCGBB request intercepted");
        return false;
    }

    [HarmonyPatch(typeof(HttpSubAccountLogRequester), nameof(HttpSubAccountLogRequester.HHMPOBOJDMB))]
    [HarmonyPrefix]
    public static bool HHMPOBOJDMB()
    {
        LetheHooks.LOG.LogInfo($"WARNING: SubBattleLog HHMPOBOJDMB request intercepted");
        return false;
    }

    [HarmonyPatch(typeof(HttpSubAccountLogRequester), nameof(HttpSubAccountLogRequester.HLOIMGLAJLO))]
    [HarmonyPrefix]
    public static bool HLOIMGLAJLO()
    {
        LetheHooks.LOG.LogInfo($"WARNING: SubBattleLog HLOIMGLAJLO request intercepted");
        return false;
    }

    [HarmonyPatch(typeof(HttpSubAccountLogRequester), nameof(HttpSubAccountLogRequester.IDCGNDPNIKK))]
    [HarmonyPrefix]
    public static bool IDCGNDPNIKK()
    {
        LetheHooks.LOG.LogInfo($"WARNING: SubBattleLog IDCGNDPNIKK request intercepted");
        return false;
    }

    [HarmonyPatch(typeof(HttpSubAccountLogRequester), nameof(HttpSubAccountLogRequester.JBEKGAFGPIC))]
    [HarmonyPrefix]
    public static bool JBEKGAFGPIC()
    {
        LetheHooks.LOG.LogInfo($"WARNING: SubBattleLog JBEKGAFGPIC request intercepted");
        return false;
    }

    [HarmonyPatch(typeof(HttpSubAccountLogRequester), nameof(HttpSubAccountLogRequester.JECANBBEGCA))]
    [HarmonyPrefix]
    public static bool JECANBBEGCA()
    {
        LetheHooks.LOG.LogInfo($"WARNING: SubBattleLog JECANBBEGCA request intercepted");
        return false;
    }

    [HarmonyPatch(typeof(HttpSubAccountLogRequester), nameof(HttpSubAccountLogRequester.JOAAGEGCHDA))]
    [HarmonyPrefix]
    public static bool JOAAGEGCHDA()
    {
        LetheHooks.LOG.LogInfo($"WARNING: SubBattleLog JOAAGEGCHDA request intercepted");
        return false;
    }

    [HarmonyPatch(typeof(HttpSubAccountLogRequester), nameof(HttpSubAccountLogRequester.KJNMILCNIKK))]
    [HarmonyPrefix]
    public static bool KJNMILCNIKK()
    {
        LetheHooks.LOG.LogInfo($"WARNING: SubBattleLog KJNMILCNIKK request intercepted");
        return false;
    }

    [HarmonyPatch(typeof(HttpSubAccountLogRequester), nameof(HttpSubAccountLogRequester.KMHNPFIMCEJ))]
    [HarmonyPrefix]
    public static bool KMHNPFIMCEJ()
    {
        LetheHooks.LOG.LogInfo($"WARNING: SubBattleLog KMHNPFIMCEJ request intercepted");
        return false;
    }

    [HarmonyPatch(typeof(HttpSubAccountLogRequester), nameof(HttpSubAccountLogRequester.KNGOEOFNACJ))]
    [HarmonyPrefix]
    public static bool KNGOEOFNACJ()
    {
        LetheHooks.LOG.LogInfo($"WARNING: SubBattleLog KNGOEOFNACJ request intercepted");
        return false;
    }

    [HarmonyPatch(typeof(HttpSubAccountLogRequester), nameof(HttpSubAccountLogRequester.KPIENPMIJLL))]
    [HarmonyPrefix]
    public static bool KPIENPMIJLL()
    {
        LetheHooks.LOG.LogInfo($"WARNING: SubBattleLog KPIENPMIJLL request intercepted");
        return false;
    }

    [HarmonyPatch(typeof(HttpSubAccountLogRequester), nameof(HttpSubAccountLogRequester.KPNCGCDAKOB))]
    [HarmonyPrefix]
    public static bool KPNCGCDAKOB()
    {
        LetheHooks.LOG.LogInfo($"WARNING: SubBattleLog KPNCGCDAKOB request intercepted");
        return false;
    }

    [HarmonyPatch(typeof(HttpSubAccountLogRequester), nameof(HttpSubAccountLogRequester.LAHFMEEKIEM))]
    [HarmonyPrefix]
    public static bool LAHFMEEKIEM()
    {
        LetheHooks.LOG.LogInfo($"WARNING: SubBattleLog KPNCGCDAKOB request intercepted");
        return false;
    }

    [HarmonyPatch(typeof(HttpSubAccountLogRequester), nameof(HttpSubAccountLogRequester.LCEOPGMJHAE))]
    [HarmonyPrefix]
    public static bool LCEOPGMJHAE()
    {
        LetheHooks.LOG.LogInfo($"WARNING: SubBattleLog LCEOPGMJHAE request intercepted");
        return false;
    }

    [HarmonyPatch(typeof(HttpSubAccountLogRequester), nameof(HttpSubAccountLogRequester.LHBMPGJBKMM))]
    [HarmonyPrefix]
    public static bool LHBMPGJBKMM()
    {
        LetheHooks.LOG.LogInfo($"WARNING: SubBattleLog LHBMPGJBKMM request intercepted");
        return false;
    }

    [HarmonyPatch(typeof(HttpSubAccountLogRequester), nameof(HttpSubAccountLogRequester.LHEFONAMAPP))]
    [HarmonyPrefix]
    public static bool LHEFONAMAPP()
    {
        LetheHooks.LOG.LogInfo($"WARNING: SubBattleLog LHEFONAMAPP request intercepted");
        return false;
    }

    [HarmonyPatch(typeof(HttpSubAccountLogRequester), nameof(HttpSubAccountLogRequester.LNJKLNPDMMH))]
    [HarmonyPrefix]
    public static bool LNJKLNPDMMH()
    {
        LetheHooks.LOG.LogInfo($"WARNING: SubBattleLog LNJKLNPDMMH request intercepted");
        return false;
    }

    [HarmonyPatch(typeof(HttpSubAccountLogRequester), nameof(HttpSubAccountLogRequester.MAJHONHMPDD))]
    [HarmonyPrefix]
    public static bool MAJHONHMPDD()
    {
        LetheHooks.LOG.LogInfo($"WARNING: SubBattleLog MAJHONHMPDD request intercepted");
        return false;
    }

    [HarmonyPatch(typeof(HttpSubAccountLogRequester), nameof(HttpSubAccountLogRequester.MBOLFFPDKIM))]
    [HarmonyPrefix]
    public static bool MBOLFFPDKIM()
    {
        LetheHooks.LOG.LogInfo($"WARNING: SubBattleLog MBOLFFPDKIM request intercepted");
        return false;
    }

    [HarmonyPatch(typeof(HttpSubAccountLogRequester), nameof(HttpSubAccountLogRequester.MHFGDPIFDMB))]
    [HarmonyPrefix]
    public static bool MHFGDPIFDMB()
    {
        LetheHooks.LOG.LogInfo($"WARNING: SubBattleLog MHFGDPIFDMB request intercepted");
        return false;
    }

    [HarmonyPatch(typeof(HttpSubAccountLogRequester), nameof(HttpSubAccountLogRequester.MILGMCELCCP))]
    [HarmonyPrefix]
    public static bool MILGMCELCCP()
    {
        LetheHooks.LOG.LogInfo($"WARNING: SubBattleLog MILGMCELCCP request intercepted");
        return false;
    }

    [HarmonyPatch(typeof(HttpSubAccountLogRequester), nameof(HttpSubAccountLogRequester.MJCEOLMCHBL))]
    [HarmonyPrefix]
    public static bool MJCEOLMCHBL()
    {
        LetheHooks.LOG.LogInfo($"WARNING: SubBattleLog MJCEOLMCHBL request intercepted");
        return false;
    }

    [HarmonyPatch(typeof(HttpSubAccountLogRequester), nameof(HttpSubAccountLogRequester.MMGMIPHGKKD))]
    [HarmonyPrefix]
    public static bool MMGMIPHGKKD()
    {
        LetheHooks.LOG.LogInfo($"WARNING: SubBattleLog MMGMIPHGKKD request intercepted");
        return false;
    }

    [HarmonyPatch(typeof(HttpSubAccountLogRequester), nameof(HttpSubAccountLogRequester.MMHBAJJOEMM))]
    [HarmonyPrefix]
    public static bool MMHBAJJOEMM()
    {
        LetheHooks.LOG.LogInfo($"WARNING: SubBattleLog MMHBAJJOEMM request intercepted");
        return false;
    }

    [HarmonyPatch(typeof(HttpSubAccountLogRequester), nameof(HttpSubAccountLogRequester.MNMEKCMEEIM))]
    [HarmonyPrefix]
    public static bool MNMEKCMEEIM()
    {
        LetheHooks.LOG.LogInfo($"WARNING: SubBattleLog MNMEKCMEEIM request intercepted");
        return false;
    }

    [HarmonyPatch(typeof(HttpSubAccountLogRequester), nameof(HttpSubAccountLogRequester.MOAPGCMJLEN))]
    [HarmonyPrefix]
    public static bool MOAPGCMJLEN()
    {
        LetheHooks.LOG.LogInfo($"WARNING: SubBattleLog MOAPGCMJLEN request intercepted");
        return false;
    }

    [HarmonyPatch(typeof(HttpSubAccountLogRequester), nameof(HttpSubAccountLogRequester.MPGLOACNANH))]
    [HarmonyPrefix]
    public static bool MPGLOACNANH()
    {
        LetheHooks.LOG.LogInfo($"WARNING: SubBattleLog MPGLOACNANH request intercepted");
        return false;
    }

    [HarmonyPatch(typeof(HttpSubAccountLogRequester), nameof(HttpSubAccountLogRequester.NHJALADGOJG))]
    [HarmonyPrefix]
    public static bool NHJALADGOJG()
    {
        LetheHooks.LOG.LogInfo($"WARNING: SubBattleLog NHJALADGOJG request intercepted");
        return false;
    }

    [HarmonyPatch(typeof(HttpSubAccountLogRequester), nameof(HttpSubAccountLogRequester.NJNIPMBCKOD))]
    [HarmonyPrefix]
    public static bool NJNIPMBCKOD()
    {
        LetheHooks.LOG.LogInfo($"WARNING: SubBattleLog NJNIPMBCKOD request intercepted");
        return false;
    }

    [HarmonyPatch(typeof(HttpSubAccountLogRequester), nameof(HttpSubAccountLogRequester.NLIBOAMIMCE))]
    [HarmonyPrefix]
    public static bool NLIBOAMIMCE()
    {
        LetheHooks.LOG.LogInfo($"WARNING: SubBattleLog NLIBOAMIMCE request intercepted");
        return false;
    }

    [HarmonyPatch(typeof(HttpSubAccountLogRequester), nameof(HttpSubAccountLogRequester.OCKIHIGFNOD))]
    [HarmonyPrefix]
    public static bool OCKIHIGFNOD()
    {
        LetheHooks.LOG.LogInfo($"WARNING: SubBattleLog OCKIHIGFNOD request intercepted");
        return false;
    }

    [HarmonyPatch(typeof(HttpSubAccountLogRequester), nameof(HttpSubAccountLogRequester.OGPGGBDCKHO))]
    [HarmonyPrefix]
    public static bool OGPGGBDCKHO()
    {
        LetheHooks.LOG.LogInfo($"WARNING: SubBattleLog OGPGGBDCKHO request intercepted");
        return false;
    }

    [HarmonyPatch(typeof(HttpSubAccountLogRequester), nameof(HttpSubAccountLogRequester.OGPLKABBIIL))]
    [HarmonyPrefix]
    public static bool OGPLKABBIIL()
    {
        LetheHooks.LOG.LogInfo($"WARNING: SubBattleLog OGPLKABBIIL request intercepted");
        return false;
    }

    [HarmonyPatch(typeof(HttpSubAccountLogRequester), nameof(HttpSubAccountLogRequester.OLJBGBMAFCB))]
    [HarmonyPrefix]
    public static bool OLJBGBMAFCB()
    {
        LetheHooks.LOG.LogInfo($"WARNING: SubBattleLog OLJBGBMAFCB request intercepted");
        return false;
    }

    [HarmonyPatch(typeof(HttpSubAccountLogRequester), nameof(HttpSubAccountLogRequester.OPMBHJOBPGN))]
    [HarmonyPrefix]
    public static bool OPMBHJOBPGN()
    {
        LetheHooks.LOG.LogInfo($"WARNING: SubBattleLog OPMBHJOBPGN request intercepted");
        return false;
    }

    [HarmonyPatch(typeof(HttpSubAccountLogRequester), nameof(HttpSubAccountLogRequester.PEJMDIFAOPH))]
    [HarmonyPrefix]
    public static bool PEJMDIFAOPH()
    {
        LetheHooks.LOG.LogInfo($"WARNING: SubBattleLog PEJMDIFAOPH request intercepted");
        return false;
    }

    [HarmonyPatch(typeof(HttpSubAccountLogRequester), nameof(HttpSubAccountLogRequester.PHEPJDEDMBJ))]
    [HarmonyPrefix]
    public static bool PHEPJDEDMBJ()
    {
        LetheHooks.LOG.LogInfo($"WARNING: SubBattleLog PHEPJDEDMBJ request intercepted");
        return false;
    }

    [HarmonyPatch(typeof(HttpSubAccountLogRequester), nameof(HttpSubAccountLogRequester.PKKEANOOGFJ))]
    [HarmonyPrefix]
    public static bool PKKEANOOGFJ()
    {
        LetheHooks.LOG.LogInfo($"WARNING: SubBattleLog PKKEANOOGFJ request intercepted");
        return false;
    }

    [HarmonyPatch(typeof(HttpSubAccountLogRequester), nameof(HttpSubAccountLogRequester.PLGEDGLCELP))]
    [HarmonyPrefix]
    public static bool PLGEDGLCELP()
    {
        LetheHooks.LOG.LogInfo($"WARNING: SubBattleLog PLGEDGLCELP request intercepted");
        return false;
    }

    [HarmonyPatch(typeof(HttpSubAccountLogRequester), nameof(HttpSubAccountLogRequester.PMGPGIOIGII))]
    [HarmonyPrefix]
    public static bool PMGPGIOIGII()
    {
        LetheHooks.LOG.LogInfo($"WARNING: SubBattleLog PMGPGIOIGII request intercepted");
        return false;
    }

    [HarmonyPatch(typeof(HttpSubAccountLogRequester), nameof(HttpSubAccountLogRequester.PNPMJEIDAIM))]
    [HarmonyPrefix]
    public static bool PNPMJEIDAIM()
    {
        LetheHooks.LOG.LogInfo($"WARNING: SubBattleLog PNPMJEIDAIM request intercepted");
        return false;
    }

    [HarmonyPatch(typeof(HttpSubAccountLogRequester), nameof(HttpSubAccountLogRequester.POIBFLHOECO))]
    [HarmonyPrefix]
    public static bool POIBFLHOECO()
    {
        LetheHooks.LOG.LogInfo($"WARNING: SubBattleLog POIBFLHOECO request intercepted");
        return false;
    }

    [HarmonyPatch(typeof(HttpSubAccountLogRequester), nameof(HttpSubAccountLogRequester.PPDAMMIOLKD))]
    [HarmonyPrefix]
    public static bool PPDAMMIOLKD()
    {
        LetheHooks.LOG.LogInfo($"WARNING: SubBattleLog PPDAMMIOLKD request intercepted");
        return false;
    }

    [HarmonyPatch(typeof(HttpSubAccountLogRequester), nameof(HttpSubAccountLogRequester.PPGDJBCAAJP))]
    [HarmonyPrefix]
    public static bool PPGDJBCAAJP()
    {
        LetheHooks.LOG.LogInfo($"WARNING: SubBattleLog PPGDJBCAAJP request intercepted");
        return false;
    }

    [HarmonyPatch(typeof(HttpBattleLogRequester), nameof(HttpBattleLogRequester.SendRequest))]
    [HarmonyPrefix]
    public static bool PreBattleLogSendRequest()
    {
        LetheHooks.LOG.LogInfo($"WARNING: LIMBUS TRIED TO REPORT TO PROJECT MOON");
        return false;
    }

}