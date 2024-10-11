using Exiled.API.Features;
using HarmonyLib;
using Mirror;
using ObjectSanitizer.Handlers;
using Utils.NonAllocLINQ;
using static Mirror.NetworkServer;

namespace ObjectSanitizer.Patches;

[HarmonyPatch(typeof(NetworkServer), nameof(NetworkServer.DestroyObject), [typeof(NetworkIdentity), typeof(DestroyMode)])]
public static class DestroyNetworkIdentityInListWhenDestroyed
{
    [HarmonyPrefix]
    public static void Postfix(NetworkIdentity __instance)
    {
        Log.Warn($"Called {nameof(NetworkServer.DestroyObject)}, removing..");

        // УДАЛЯЕМ ВСЕ НАХУЙЙЙ ЕБАТЬ Я ТИПО КАК ЧАТ ГПТ ПИШУ ЕБАТЬ
        Coroutine.SpawnedNetworkIdentity.ForEach(x => x.Value.Remove(__instance));

        // В следствии удаления объектов на сервере удаляем их из списка, чтобы не создавать проблемы при дальнейшей обработке.
        Coroutine.CachedNetworkIdentities.Remove(__instance);
    }
}