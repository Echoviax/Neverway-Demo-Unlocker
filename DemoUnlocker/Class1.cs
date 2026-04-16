using HarmonyLib;
using Road;
using Road.StateMachines;
using System;
using System.Reflection.Emit;
using Wayfinder.Core;

public class ModEntry
{
    public static void Start()
    {
        try
        {
            var harmony = new Harmony("com.echoviax.DemoDisabler");
            harmony.PatchAll();
            LoaderCore.LogSuccess("Successfully injected.");
        }
        catch (Exception ex)
        {
            LoaderCore.LogError("Failed to inject: " + ex);
        }
    }
}

public static class TranspilerHelper
{
    public static IEnumerable<CodeInstruction> ReplaceDemoWithFalse(IEnumerable<CodeInstruction> instructions, string patchName)
    {
        var demoField = AccessTools.Field(typeof(RoadGame), nameof(RoadGame.DEMO));
        bool foundAndReplaced = false;

        foreach (var instruction in instructions)
        {
            if (instruction.LoadsField(demoField))
            {
                yield return new CodeInstruction(OpCodes.Ldc_I4_0);
                foundAndReplaced = true;
            }
            else
            {
                yield return instruction;
            }
        }

        if (!foundAndReplaced)
            LoaderCore.LogError($"Could not find the demo check in {patchName}.");
    }
}

[HarmonyPatch(typeof(MainMenu))]
[HarmonyPatch("Main", MethodType.Enumerator)]
public static class MainMenu_Main_Patch
{
    static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        return TranspilerHelper.ReplaceDemoWithFalse(instructions, "MainMenu.Main");
    }
}

[HarmonyPatch(typeof(MainMenu))]
[HarmonyPatch("LoadNewGame", MethodType.Enumerator)]
public static class MainMenu_LoadNewGame_Patch
{
    static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        return TranspilerHelper.ReplaceDemoWithFalse(instructions, "MainMenu.LoadNewGame");
    }
}

[HarmonyPatch(typeof(Road.StateMachines.Cutscenes.SleepCutscene))]
[HarmonyPatch("SleepAtFirstNight", MethodType.Enumerator)]
public static class SleepCutscene_FirstNight_Patch
{
    static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        return TranspilerHelper.ReplaceDemoWithFalse(instructions, "SleepCutscene.SleepAtFirstNight");
    }
}

[HarmonyPatch(typeof(SaveStateMachine))]
[HarmonyPatch("Save", MethodType.Enumerator)]
public static class SaveState_CanSave_Patch
{
    static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        var canSaveGetter = AccessTools.PropertyGetter(typeof(Murder.Game), "CanSave");
        bool foundAndReplaced = false;

        foreach (var instruction in instructions)
        {
            if (instruction.Calls(canSaveGetter))
            {
                yield return new CodeInstruction(OpCodes.Ldc_I4_1);
                foundAndReplaced = true;
            }
            else
            {
                yield return instruction;
            }
        }

        if (!foundAndReplaced)
            LoaderCore.LogError("Could not find the save state save demo check.");
    }
}

[HarmonyPatch(typeof(Road.RoadGame))]
[HarmonyPatch("CanSave", MethodType.Getter)]
public static class RoadGame_CanSave_Patch
{
    static void Postfix(ref bool __result)
    {
        __result = true;
    }
}