using HarmonyLib;
using VersatileSpecialEventAPI;
using static VersatileSpecialEventAPI.API;

namespace SpecialEvent;

public class Initializer : ModInitializer {
    public static string ModId = "SpecialEvent";
    public static Harmony ModHarmony = new Harmony(ModId);

    public override void OnInitializeMod() {
        Init(ModId, ModHarmony);
        LoadData();
        LoadResource(FileType.ArtWork);
        LoadLocalize("Localize");
    }
}