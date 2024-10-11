using Exiled.API.Interfaces;
using System.ComponentModel;

namespace ObjectSanitizer.Configs;
public class Config : IConfig
{
    [Description("Indicates plugin is enabled or not.")]
    public bool IsEnabled { get; set; } = true;
    [Description("Indicates debug is enabled or not.")]
    public bool Debug { get; set; } = false;
    [Description("Indicates delay after round start before coroutine will be launched.")]
    public float DelayAfterRoundStart { get; set; } = 15f;
    [Description("Indicates delay objects update.")]
    public float Delay { get; set; } = 5;
    [Description("Indicates object resfresh distance.")]
    public float RefreshDistance { get; set; } = 125f;
    [Description("Ignored root objects." +
        "\nFor scheamtics use - CustomSchematic-nameOfUseSchematic" +
        "\nYou can debug root objects name by enabling debug (!!! DO NOT TURN ON PRODUCTION !!!)")]
    public List<string> IgnoredRootObjects { get; set; } = new();
    [Description("Ignored objects by name." +
        "\nObjects will never be destroyed by plugin (i recommened it if you have teleporters by more than render distance)")]
    public List<string> IgnoredNameObjects { get; set; } = new();
}
