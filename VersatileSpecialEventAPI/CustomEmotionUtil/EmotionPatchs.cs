using HarmonyLib;
using UnityEngine.UI;
using VersatileSpecialEventAPI.Utils;
using static VersatileSpecialEventAPI.API;

namespace VersatileSpecialEventAPI.CustomEmotionUtil;

public class EmotionPatchs {
    [HarmonyPatch(typeof(StageController), "RoundEndPhase_ChoiceEmotionCard")]
    public class StageController_RoundEndPhase_ChoiceEmotionCard {
        public static void Postfix(StageController __instance, ref bool __result) {
            if (__result) {
                SingletonBehavior<BattleManagerUI>.Instance.ui_levelup.SetRootCanvas(true);
                SingletonBehavior<BattleManagerUI>.Instance.ui_levelup.Init(0, [
                    new EmotionCardXmlInfo() { id = 0, Name = "Not Found" },
                    new EmotionCardXmlInfo() { id = 0, Name = "Not Found" },
                    new EmotionCardXmlInfo() { id = 0, Name = "Not Found" }
                ]);
                __result = true;
            }
        }
    }

    [HarmonyPatch(typeof(EmotionPassiveCardUI), "SetSprites")]
    public class EmotionPassiveCardUI_SetSprites {
        public static void Postfix(MentalState state, EmotionCardXmlInfo ____card, ref Image ____artwork) {
            if (____card == null || ____artwork == null) return;
            if (string.IsNullOrEmpty(____card._artwork)) return;
            if (!SpriteHelper.IsSpriteExist(____card._artwork)) return;

            APILogger.LogInfo($"SetSprites: {____card.Artwork}");
            ____artwork.sprite = SpriteHelper.GetData(____card.Artwork);
        }
    }
}