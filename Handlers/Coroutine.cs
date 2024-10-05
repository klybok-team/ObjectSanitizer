using AdminToys;
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
            try
            {
                foreach (Player? player in Player.List)
                {
                    if (!SpawnedNetworkIdentity.ContainsKey(player))
                        SpawnedNetworkIdentity.Add(player, CachedNetworkIdentities);

                    if (player.IsDead || player.Role.Type == RoleTypeId.Scp079)
                    {
                        SpawnAllIfNot(CachedNetworkIdentities, player);
                        continue;
                    }

                    foreach (NetworkIdentity? networkIdenitity in CachedNetworkIdentities.ToList())
                    {
                        if (networkIdenitity.netId == 0) continue;

                        ProcessNetworkIdentity(player, networkIdenitity, Config.RefreshDistance);
                    }
                }
            }
            catch (Exception e)
            {
                Log.Error(e);
            }

            yield return Timing.WaitForSeconds(Config.Delay);
        }
    }
    public static void SpawnAllIfNot(List<NetworkIdentity> networkIdentities, Player pl)
    {
        if (SpawnedNetworkIdentity.All(x => x.Value == networkIdentities)) return;

        foreach (NetworkIdentity identity in networkIdentities)
        {
            if (SpawnedNetworkIdentity[pl].Contains(identity)) return;

            Log.Debug($"added object from observing [{identity.netId}, {identity.name}] to [PID: {pl.Id}], spawning all");

            pl.ReferenceHub.connectionToClient.AddToObserving(identity);

            SpawnedNetworkIdentity[pl].Add(identity);
        }
    }
    public static void ProcessNetworkIdentity(Player pl, NetworkIdentity identity, float renderDistance)
    {
        if (identity.transform == null) return;

        if ((pl.Position - identity.transform.position).sqrMagnitude <= (renderDistance * renderDistance))
        {
            if (SpawnedNetworkIdentity[pl].Contains(identity)) return;

            Log.Debug($"added object from observing [{identity.netId}, {identity.name}] to [PID: {pl.Id}]");

            pl.ReferenceHub.connectionToClient.AddToObserving(identity);

            SpawnedNetworkIdentity[pl].Add(identity);
        }
        else
        {
            if (!SpawnedNetworkIdentity[pl].Contains(identity)) return;

            Log.Debug($"removed object from observing [{identity.netId}, {identity.name}] to [PID: {pl.Id}]");

            pl.ReferenceHub.connectionToClient.RemoveFromObserving(identity, false);

            SpawnedNetworkIdentity[pl].Remove(identity);
        }
    }
}