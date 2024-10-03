using AdminToys;
using Exiled.API.Features;
using InventorySystem.Items.Pickups;
using MEC;
using Mirror;
using UnityEngine;
using PlayerRoles;

namespace ObjectSanitizer.Handlers;

public static class Coroutine
{
    public static Dictionary<Player, List<NetworkIdentity>> SpawnedNetworkIdentity = new();

    public static float RenderDistance { get; set; } = 75;
    public static float Delay { get; set; } = 0.5f;

    public static IEnumerator<float> Process()
    {
        while (true)
        {
            try
            {
                List<NetworkIdentity> networkIdenitites = UnityEngine.Object.FindObjectsOfType<NetworkIdentity>().ToList();

                // оставляем только пикапы и все спавненные админами штуки
                // примитивы свет вся хуйня
                networkIdenitites = networkIdenitites.Where(x =>
                x.GetComponent<ItemPickupBase>() is not null
                || x.GetComponent<AdminToyBase>() is not null).ToList();

                foreach (Player? player in Player.List)
                {
                    if (!SpawnedNetworkIdentity.ContainsKey(player))
                        SpawnedNetworkIdentity.Add(player, networkIdenitites);

                    if (player.IsDead || player.Role.Type == RoleTypeId.Scp079)
                    {
                        SpawnAllIfNot(networkIdenitites, player);
                        continue;
                    }

                    foreach (NetworkIdentity? networkIdenitity in networkIdenitites.ToList())
                    {
                        if (networkIdenitity.netId == 0) continue;

                        ProcessNetworkIdentity(player, networkIdenitity, RenderDistance);
                    }
                }
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
            yield return Timing.WaitForSeconds(Delay);
        }
    }
    public static void SpawnAllIfNot(List<NetworkIdentity> networkIdentities, Player pl)
    {
        if (SpawnedNetworkIdentity.All(x => x.Value == networkIdentities)) return;

        foreach (NetworkIdentity identity in networkIdentities)
        {
            if (SpawnedNetworkIdentity[pl].Contains(identity)) return;

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

            // Log.Info($"sended [{pl.Id}] spawn message with [{identity.netId}, {identity.name}, is door: {identity.GetComponent<BasicDoor>() is not null}, is room {identity.GetComponent<RoomIdentifier>() is not null}] netid");

            pl.ReferenceHub.connectionToClient.AddToObserving(identity);

            SpawnedNetworkIdentity[pl].Add(identity);
        }
        else
        {
            if (!SpawnedNetworkIdentity[pl].Contains(identity)) return;

            // Log.Info($"sended [{pl.Id}] destroy message with [{identity.NetworkBehaviours.FirstOrDefault()}, {identity.name}, is door: {identity.GetComponent<BasicDoor>() is not null}, is room {identity.GetComponent<RoomIdentifier>() is not null}] netid");

            pl.ReferenceHub.connectionToClient.RemoveFromObserving(identity, false);

            SpawnedNetworkIdentity[pl].Remove(identity);
        }
    }
}
