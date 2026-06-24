using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;

namespace Constitution;

[BepInPlugin(PluginGuid, PluginName, PluginVersion)]
[BepInDependency("headclef.CharacterStats", BepInDependency.DependencyFlags.HardDependency)]
public class Constitution : BaseUnityPlugin
{
    private const string PluginGuid = "headclef.Constitution";
    private const string PluginName = "Constitution";
    private const string PluginVersion = "1.1.0";

    internal static Constitution Instance { get; private set; } = null!;
    internal new static ManualLogSource Logger => Instance._logger;
    private ManualLogSource _logger => base.Logger;
    internal Harmony? Harmony { get; set; }

    // ── Config ──
    internal static ConfigEntry<bool> EnableHealthRegen = null!;
    internal static ConfigEntry<float> RegenPerHealthLevel = null!;
    internal static ConfigEntry<float> MaxRegenPerSecond = null!;
    internal static ConfigEntry<float> RegenDelay = null!;

    private void Awake()
    {
        Instance = this;
        this.gameObject.transform.parent = null;
        this.gameObject.hideFlags = HideFlags.HideAndDontSave;

        BindConfiguration();
        Harmony ??= new Harmony(Info.Metadata.GUID);
        Harmony.PatchAll();

        Logger.LogInfo($"{Info.Metadata.GUID} v{Info.Metadata.Version} has loaded!");
    }

    private void OnDestroy()
    {
        Harmony?.UnpatchSelf();
    }

    private void BindConfiguration()
    {
        const string section = "Health Regeneration";

        EnableHealthRegen = Config.Bind(section, "Enable", true,
            "Enable passive health regeneration based on Health stat level.");

        RegenPerHealthLevel = Config.Bind(section, "Regen Per Health Level", 0.05f,
            new ConfigDescription(
                "Health regeneration per second for each Health upgrade level. Intentionally slow — designed for long-run sustain.",
                new AcceptableValueRange<float>(0.01f, 1f)));

        MaxRegenPerSecond = Config.Bind(section, "Max Regen Per Second", 0f,
            new ConfigDescription(
                "Maximum health regeneration per second. 0 = no cap.",
                new AcceptableValueRange<float>(0f, 5f)));

        RegenDelay = Config.Bind(section, "Regen Delay", 10f,
            new ConfigDescription(
                "Seconds after taking damage before health regeneration starts again.",
                new AcceptableValueRange<float>(0f, 60f)));
    }
}
