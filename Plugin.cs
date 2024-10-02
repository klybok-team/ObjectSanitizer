using Exiled.API.Features;
using HarmonyLib;
using MEC;
using ObjectSanitizer.Handlers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ObjectSanitizer;

public class ObjectSanitizer : Plugin<Configs.Config>
{
    public override string Name => "ObjectSanitizer";
    public override string Author => "Adarkaz";

    public CoroutineHandle UpdaterCoroutineHandle;
    public override void OnEnabled()
    {
        Exiled.Events.Handlers.Server.RoundStarted += OnRoundStarted_HandleCoroutine;

        Coroutine.Delay = Config.Delay;
        Coroutine.RenderDistance = Config.RefreshDistance;

        base.OnEnabled();
    }
    public override void OnDisabled()
    {
        Exiled.Events.Handlers.Server.RoundStarted -= OnRoundStarted_HandleCoroutine;

        Timing.KillCoroutines(UpdaterCoroutineHandle);

        base.OnDisabled();
    }

    public void OnRoundStarted_HandleCoroutine()
    {
        if (UpdaterCoroutineHandle == null || !UpdaterCoroutineHandle.IsRunning)
            UpdaterCoroutineHandle = Timing.RunCoroutine(Coroutine.Process());
    }
}