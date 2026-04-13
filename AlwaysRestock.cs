using System.Reflection;
using GHPC.Weapons;
using GHPC.Crew;
using MelonLoader;
using HarmonyLib;
using AlwaysRestock;

[assembly: MelonInfo(typeof(AlwaysRestockClass), "Always Restock Ammunition", "1.0.0", "Bluehawk")]
[assembly: MelonGame("Radian Simulations LLC", "GHPC")]

namespace AlwaysRestock
{
    public class AlwaysRestockClass : MelonMod
    {
        private static MelonPreferences_Entry<float> extraDelay;
        private static float extraDelayCorrected;
        public override void OnInitializeMelon()
        {
            MelonLogger.Msg("Patching LoadoutManager...");
            MelonPreferences_Category cfg = MelonPreferences.CreateCategory("AlwaysRestock");
            extraDelay = cfg.CreateEntry<float>("Restocking delay", 3f);
            extraDelay.Comment = "The penalty in seconds for restocking ammo with a dead loader";

            if (extraDelay.Value < 0) { extraDelayCorrected = 0f; } else { extraDelayCorrected = extraDelay.Value; }
            MelonLogger.Msg("Delay set at " + extraDelayCorrected + " secs.");
        }
    

        [HarmonyPatch(typeof(LoadoutManager))]
        [HarmonyPatch("_crewAvailableForRestock", MethodType.Getter)]
        public static class ForceRestock
        {
            public static bool Prefix(ref bool __result, LoadoutManager __instance)
            {
                __result = true;
                return false;
            }
        }

        [HarmonyPatch(typeof(CrewManager), "LoaderKilled")]
        public static class AddDelay
        {
            private static void Postfix(CrewManager __instance)
            {
                MelonLogger.Msg("Loader killed");
                FieldInfo field1 = typeof(CrewManager).GetField("_loadoutManager", BindingFlags.Instance | BindingFlags.NonPublic);
                LoadoutManager lm = field1.GetValue(__instance) as LoadoutManager;
                MelonLogger.Msg("LoadoutManager found: " + lm);
                GHPC.Weapons.AmmoRack rack = lm.RackLoadouts[0].Rack;
                MelonLogger.Msg("Rack found: " + rack);
                FieldInfo field2 = typeof(GHPC.Weapons.AmmoRack).GetField("_storageDelaySeconds", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                float temp = (float) field2.GetValue(rack);
                temp += extraDelayCorrected;
                field2.SetValue(rack, temp);
                //._storageDelaySeconds += extraDelayCorrected
                MelonLogger.Msg("loader killed, restock delay added");
            }
        }
    }
}
