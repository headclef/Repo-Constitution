using System;
using static Character_Stats.Character_Stats;
using HarmonyLib;
using UnityEngine;

namespace Constitution.Patches;

[HarmonyPatch]
internal static class HealthRegenPatch
{
    private static float _lastDamageTime;
    private static float _lastLogTime;
    private static int _lastKnownHealth = -1;

    /// <summary>
    /// Detect when the player takes damage by tracking health changes.
    /// Resets the regen delay timer.
    /// </summary>
    [HarmonyPatch(typeof(PlayerAvatar), nameof(PlayerAvatar.PlayerDeath))]
    [HarmonyPrefix]
    private static void PlayerDeath_Prefix()
    {
        // Disable regen on death
        _lastDamageTime = float.MaxValue;
    }

    [HarmonyPatch(typeof(PlayerAvatar), nameof(PlayerAvatar.Revive))]
    [HarmonyPostfix]
    private static void Revive_Postfix()
    {
        // Re-enable regen after revive
        _lastDamageTime = Time.time;
        _lastKnownHealth = -1;
    }

    /// <summary>
    /// Postfix on PlayerController.Update — adds slow, passive health regen
    /// based on the player's Health upgrade level from Character Stats.
    /// </summary>
    [HarmonyPatch(typeof(PlayerController), "Update")]
    [HarmonyPostfix]
    private static void PlayerController_Update_Postfix(PlayerController __instance)
    {
        if (!Constitution.EnableHealthRegen.Value)
            return;

        if (!AreStatsReady)
            return;

        try
        {
            // Only apply to the local player
            if (__instance != PlayerController.instance)
                return;

            string? steamId = GetLocalSteamId();
            if (steamId == null)
                return;

            // Read Health level from Character Stats — no regen if 0
            int healthLevel = GetUpgradeLevel(steamId, "Health");
            if (healthLevel <= 0)
                return;

            // Get player health reference
            var playerAvatar = __instance.playerAvatarScript;
            if (playerAvatar == null || playerAvatar.playerHealth == null)
                return;

            int currentHealth = playerAvatar.playerHealth.health;
            int maxHealth = playerAvatar.playerHealth.maxHealth;

            // Detect damage taken — reset regen delay
            if (_lastKnownHealth >= 0 && currentHealth < _lastKnownHealth)
            {
                _lastDamageTime = Time.time;
            }
            _lastKnownHealth = currentHealth;

            // Don't regen if dead or at max
            if (currentHealth <= 0 || currentHealth >= maxHealth)
                return;

            // Don't regen during delay after taking damage
            float delay = Constitution.RegenDelay.Value;
            if (Time.time - _lastDamageTime < delay)
                return;

            // Calculate regen rate
            float regenPerSecond = Constitution.RegenPerHealthLevel.Value * healthLevel;

            float maxRegen = Constitution.MaxRegenPerSecond.Value;
            if (maxRegen > 0f)
                regenPerSecond = Math.Min(regenPerSecond, maxRegen);

            if (regenPerSecond <= 0f)
                return;

            // Accumulate fractional HP over frames using unscaled time
            float hpThisFrame = regenPerSecond * Time.deltaTime;

            // Use a static accumulator for fractional HP
            _hpAccumulator += hpThisFrame;

            // Only heal when we've accumulated at least 1 full HP
            if (_hpAccumulator >= 1f)
            {
                int healAmount = (int)_hpAccumulator;
                _hpAccumulator -= healAmount;

                int newHealth = Math.Min(currentHealth + healAmount, maxHealth);
                playerAvatar.playerHealth.health = newHealth;

                Constitution.Logger.LogDebug(
                    $"Health regen: +{healAmount} HP ({currentHealth} -> {newHealth}/{maxHealth})");
            }

            // Log periodically
            if (Time.time - _lastLogTime > 30f)
            {
                _lastLogTime = Time.time;
                Constitution.Logger.LogDebug(
                    $"Health regen active: {regenPerSecond:F2}/sec (Health level: {healthLevel})");
            }
        }
        catch (Exception ex)
        {
            Constitution.Logger.LogError($"HealthRegen exception: {ex.Message}");
        }
    }

    private static float _hpAccumulator;
}
