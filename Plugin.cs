using Exiled.API.Features;
using HarmonyLib;
using MEC;
using ObjectSanitizer.Handlers;

namespace ObjectSanitizer;

public class ObjectSanitizer : Plugin<Configs.Config>
{
    public override string Name => "ObjectSanitizer";
    public override string Author => "Adarkaz";

    public CoroutineHandle UpdaterCoroutineHandle;
    public const string HarmonyID = "ObjectSanitizer - Adarkaz";
    public static Harmony? Harmony { get; set; }
    public override void OnEnabled()
    {
        Exiled.Events.Handlers.Server.RoundStarted += OnRoundStarted_HandleCoroutine;
        Exiled.Events.Handlers.Server.ReloadedConfigs += OnReloadedConfigs_RefreshConfig;

        Coroutine.Config = Config;

        Harmony = new(HarmonyID);
        Harmony.PatchAll();

        base.OnEnabled();
    }
    public override void OnDisabled()
    {
        Exiled.Events.Handlers.Server.RoundStarted -= OnRoundStarted_HandleCoroutine;
        Exiled.Events.Handlers.Server.ReloadedConfigs -= OnReloadedConfigs_RefreshConfig;

        Timing.KillCoroutines(UpdaterCoroutineHandle);

        Harmony?.UnpatchAll();
        Harmony = null;

        base.OnDisabled();
    }
    public void OnReloadedConfigs_RefreshConfig()
    {
        Coroutine.Config = Config;
    }
    public void OnRoundStarted_HandleCoroutine()
    {
        if (UpdaterCoroutineHandle == null || !UpdaterCoroutineHandle.IsRunning)
        {
            Timing.CallDelayed(Config.DelayAfterRoundStart, () => UpdaterCoroutineHandle = Timing.RunCoroutine(Coroutine.Process()));
        }
    }
}