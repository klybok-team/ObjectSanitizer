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
            foreach (Player? player in Player.List)
            {
                new Thread(() =>
                {

                    if (!SpawnedNetworkIdentity.ContainsKey(player))
                        SpawnedNetworkIdentity.Add(player, CachedNetworkIdentities.ToList());

                    if (player.IsDead || player.Role.Type == RoleTypeId.Scp079)
                    {
                        try
                        {
                            SpawnAllIfNot(CachedNetworkIdentities, player);
                        }
                        catch (Exception e)
                        {
                            Log.Error(e);
                        }

                        return;
                    }

                    foreach (NetworkIdentity? networkIdenitity in CachedNetworkIdentities.ToList())
                    {
                        if (networkIdenitity == null || networkIdenitity.transform == null || networkIdenitity.transform.position == null) continue;

                        if (networkIdenitity.netId == 0) continue;

                        int count = 0;

                        try
                        {
                            ProcessNetworkIdentity(player, networkIdenitity, Config.RefreshDistance);
                        }
                        catch (Exception e)
                        {
                            Log.Error(e);
                        }

                        count++;

                        if (count > 60)
                        {
                            Thread.Sleep(1);
                            count = 0;
                        }
                    }
                })
                {
                    Priority = System.Threading.ThreadPriority.AboveNormal,
                    IsBackground = true,
                    Name = $"ObjectSanitizer task for {player.Id}."
                }.Start();
            }

            yield return Timing.WaitForSeconds(Config.Delay);
        }
    }
    public static void SpawnAllIfNot(List<NetworkIdentity> networkIdentities, Player pl)
    {
        if (SpawnedNetworkIdentity.All(x => x.Value == networkIdentities)) return;

        foreach (NetworkIdentity identity in networkIdentities)
        {
            if (identity.transform == null) continue;
            
            if (SpawnedNetworkIdentity[pl].Contains(identity)) return;

            Log.Debug($"added object from observing [{identity.netId}, {identity.name}] to [PID: {pl.Id}], spawning all");

            pl.ReferenceHub.connectionToClient.AddToObserving(identity);

            SpawnedNetworkIdentity[pl].Add(identity);
        }
    }
    public static void ProcessNetworkIdentity(Player pl, NetworkIdentity identity, float renderDistance)
    {
        if (identity.transform == null) return;

        if (Vector3.Distance(pl.Position, identity.transform.position) <= renderDistance)
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