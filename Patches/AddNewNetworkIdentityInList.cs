using AdminToys;
using Exiled.API.Features;
using HarmonyLib;
using InventorySystem.Items.Pickups;
using Mirror;
using ObjectSanitizer.Handlers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utils.NonAllocLINQ;
using static Mirror.NetworkServer;

namespace ObjectSanitizer.Patches;

[HarmonyPatch(typeof(NetworkIdentity), nameof(NetworkIdentity.OnStartServer))]
public static class AddNewNetworkIdentityInList
{
    [HarmonyPrefix]
    public static void Postfix(NetworkIdentity __instance)
    {
        // не добавляем в список если это не пикап  и не админтой дрочит емне
        if (!__instance.TryGetComponent<ItemPickupBase>(out _) && !__instance.TryGetComponent<AdminToyBase>(out _)) return;

        // не добавляем объекты если они в игнОРЕ (НИХУЯ ЛЕВ НА КОНДИЦИЯХ)
        if (__instance.transform.root != null && Coroutine.Config.IgnoredRootObjects.Contains(__instance.transform.root.name))
        {
            Log.Error($"DON'T adding {__instance.transform.name} cuz {__instance.transform.root.name} is ignored");
            return;
        }

        if (Coroutine.Config.IgnoredNameObjects.Contains(__instance.transform.name))
        {
            Log.Error($"DON'T adding {__instance.transform.name} cuz it ignored");
            return;
        }

        Log.Debug($"recived new networkidenity, OnStartServer, [{__instance.netId}, {__instance.transform.name}]");

        // добавляем в заспавненные нетврокайденити ну потому что они заспавнены по умолчанию ебать открыл америку
        Coroutine.SpawnedNetworkIdentity.ForEach(x => x.Value.Add(__instance));

        // добавляем в кеш ну потом учто каждый раз обрабатывать 1488 объектов это не круто -тпс
        Coroutine.CachedNetworkIdentities.Add(__instance);
    }
}

[HarmonyPatch(typeof(NetworkServer), nameof(NetworkServer.DestroyObject), [typeof(NetworkIdentity), typeof(DestroyMode)] )]
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