﻿using AdminToys;
using Exiled.API.Features;
using InventorySystem.Items.Pickups;
using MEC;
using Mirror;
using UnityEngine;
using PlayerRoles;
using System.Collections.Generic;

namespace ObjectSanitizer.Handlers;

public static class Coroutine
{
    public static List<NetworkIdentity> CachedNetworkIdentities = new();

    public static Dictionary<Player, List<NetworkIdentity>> SpawnedNetworkIdentity = new();

    public static Configs.Config Config { get; set; } = new();

    public static IEnumerator<float> Process()
    {
        while (true)
        {
            foreach (Player? player in Player.List)
            {
                if (!SpawnedNetworkIdentity.ContainsKey(player))
                    SpawnedNetworkIdentity.Add(player, CachedNetworkIdentities.ToList());

                if ((player.IsDead || player.Role.Type == RoleTypeId.Scp079) && !SpawnedNetworkIdentity.All(x => x.Value == CachedNetworkIdentities))
                {
                    try
                    {
                        Timing.RunCoroutine(SpawnAllIfNot(CachedNetworkIdentities.ToList(), player));
                    }
                    catch (Exception e)
                    {
                        Log.Error(e);
                    }

                    continue;
                }

                Timing.RunCoroutine(ProcessNetworkIdentity(player, CachedNetworkIdentities.ToList(), Config.RefreshDistance));
            }

            yield return Timing.WaitForSeconds(Config.Delay);
        }
    }
    public static IEnumerator<float> SpawnAllIfNot(List<NetworkIdentity> networkIdentities, Player pl)
    {
        if (SpawnedNetworkIdentity.All(x => x.Value == networkIdentities)) yield break;

        int count = 0;

        foreach (NetworkIdentity identity in networkIdentities)
        {
            if (identity == null || identity.transform == null || identity.transform.position == null) continue;

            if (identity.netId == 0) continue;

            if (SpawnedNetworkIdentity[pl].Contains(identity)) continue;

            Log.Debug($"added object from observing [{identity.netId}, {identity.name}] to [PID: {pl.Id}], spawning all");

            pl.ReferenceHub.connectionToClient.AddToObserving(identity);

            SpawnedNetworkIdentity[pl].Add(identity);
            if (count > 15)
            {
                yield return Timing.WaitForOneFrame;

                count = 0;
            }

            count++;
        }
    }
    public static IEnumerator<float> ProcessNetworkIdentity(Player pl, List<NetworkIdentity> networkIdentities, float renderDistance)
    {
        // грузим чанками чтоб не лагало круто ZZZVZVZVZVVVZOVZOVPOVZVOZOVZOVOVOZOVOV CBO
        int count = 0;

        foreach (var identity in networkIdentities.ToList())
        {
            try
            {
                if (identity == null || identity.transform == null || identity.transform.position == null) continue;

                if (identity.netId == 0) continue;

                if (Vector3.Distance(pl.Position, identity.transform.position) <= renderDistance)
                {
                    if (SpawnedNetworkIdentity[pl].Contains(identity)) continue;

                    Log.Debug($"added object from observing [{identity.netId}, {identity.name}] to [PID: {pl.Id}]");

                    pl.ReferenceHub.connectionToClient.AddToObserving(identity);

                    SpawnedNetworkIdentity[pl].Add(identity);
                }
                else
                {
                    if (!SpawnedNetworkIdentity[pl].Contains(identity)) continue;

                    Log.Debug($"removed object from observing [{identity.netId}, {identity.name}] to [PID: {pl.Id}]");

                    pl.ReferenceHub.connectionToClient.RemoveFromObserving(identity, false);

                    SpawnedNetworkIdentity[pl].Remove(identity);
                }
            }
            catch(System.Exception ex)
            {
                Log.Error(ex);
            }

            if (count > 15)
            {
                yield return Timing.WaitForOneFrame;

                count = 0;
            }

            count++;
        }
    }
}