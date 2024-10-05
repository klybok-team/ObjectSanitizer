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

namespace ObjectSanitizer.Patches;

[HarmonyPatch(typeof(NetworkIdentity), nameof(NetworkIdentity.OnStartServer))]
public static class AddNewNetworkIdentityInList
{
    [HarmonyPrefix]
    public static void Postfix(NetworkIdentity __instance)
    {
        // не добавляем в список если это не пикап  и не админтой дрочит емне
        if (__instance.GetComponent<ItemPickupBase>() is null && __instance.GetComponent<AdminToyBase>() is null) return;

        Log.Debug($"recived new networkidenity, OnStartServer, [{__instance.netId}]");

        // добавляем в кеш ну потом учто каждый раз обрабатывать 1488 объектов это не круто -тпс
        Coroutine.CachedNetworkIdentities.Add(__instance);
        
        // добавляем в заспавненные нетврокайденити ну потому что они заспавнены по умолчанию ебать открыл америку
        Coroutine.SpawnedNetworkIdentity.ForEach(x => x.Value.Add(__instance));
    }
}
