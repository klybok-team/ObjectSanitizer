using Exiled.API.Interfaces;
using System.ComponentModel;

namespace ObjectSanitizer.Configs;
public class Config : IConfig
{
    [Description("Indicates plugin is enabled or not.")]
    public bool IsEnabled { get; set; } = true;
    [Description("Indicates debug is enabled or not.")]
    public bool Debug { get; set; } = false;
    [Description("Indicates delay objects update.")]
    public float Delay { get; set; } = 15f;
    [Description("Indicates object resfresh distance.")]
    public float RefreshDistance { get; set; } = 125f;
}
