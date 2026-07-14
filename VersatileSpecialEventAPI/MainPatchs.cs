using HarmonyLib;
using TMPro;
using UI;
using UnityEngine;
using VersatileSpecialEventAPI.CustomRarityUtil.XML.Wrapper;
using VersatileSpecialEventAPI.Utils;
using static VersatileSpecialEventAPI.API;

namespace VersatileSpecialEventAPI;

public class MainPatchs {
    [HarmonyPatch(typeof(GameSceneManager), "ActivateUIController")]
    public class GameSceneManager_ActivateUIController {
        public static void Postfix() {
            if (Singleton<InventoryModel>.Instance.GetCardCount(new LorId(ModId, 1)) == 0) {
                Singleton<InventoryModel>.Instance.AddCard(new LorId(ModId, 1), 3);
            } else {
                if (3 != Singleton<InventoryModel>.Instance.GetCardCount(new LorId(ModId, 1))) {
                    Singleton<InventoryModel>.Instance.GetCardList().Find((DiceCardItemModel x) => x.GetID() == new LorId(ModId, 1)).num = 3;
                }
            }

            if (Singleton<BookInventoryModel>.Instance.GetBookCount(new LorId(ModId, 1)) < 1) {
                Singleton<BookInventoryModel>.Instance.CreateBook(new LorId(ModId, 1));
            }
        }
    }

    [HarmonyPatch(typeof(UICardListDetailFilterGroup), "Init")]
    public static void Postfix(UICardListDetailFilterGroup __instance, UICardListDetailFilterPopup p, int groupIdx, RectTransform ___slotGroup) {
        if (groupIdx == 0 && __instance.detailSlots.Count < 5) {
            var go = UnityEngine.Object.Instantiate(__instance.detailSlots.Last().gameObject);
            var tr = go.transform;

            tr.SetParent(___slotGroup);
            tr.localPosition += new Vector3(5f, 0f, 0f);
            go.GetComponentInChildren<TextMeshProUGUI>().SetText("Custom");

            var slot = go.GetComponent<UICardListDetailFilterSlot>();
            __instance.detailSlots.Add(slot);
            slot.Init(__instance, 5);
        }
    }

    [HarmonyPatch(typeof(UIInvenCardListScroll), "GetCardsByDetailFilterUI")]
    public class UIInvenCardListScroll_GetCardsByDetailFilterUI {
        public static void Postfix(UIInvenCardListScroll __instance, List<DiceCardItemModel> cards, List<DiceCardItemModel> __result) {
            var detailFilter = __instance.CardFilter.GetDetailFilter();
            var rarityFilter = detailFilter.CheckRarityDetailFilter();

            if (!rarityFilter.Any(x => !Enum.IsDefined(typeof(RarityFilterDetails), x))) return;
            var filteredCards = cards.Where(c => c.GetRarity().IsCustomRarity());

            var diceFilter = detailFilter.CheckDiceDetailFilter();
            if (diceFilter.Count > 0) {
                filteredCards = filteredCards.Where(c => c.GetBehaviourList().Any(b => diceFilter.Contains(b.Detail.ToString()) || diceFilter.Contains(b.Type.ToString())));
            }

            var buffFilter = detailFilter.CheckBufDetailFilter();
            if (buffFilter.Count > 0) {
                filteredCards = filteredCards.Where(c => buffFilter.Any(buf => DiceCardKeywordFilter(buf, c)));
            }

            var abilityFilter = detailFilter.CheckAbilityDetailFilter();
            if (abilityFilter.Count > 0) {
                filteredCards = filteredCards.Where(c => abilityFilter.Any(ability => DiceCardKeywordFilter(ability, c)));
            }

            var diceCountFilter = detailFilter.CheckDiceCountDetailFilter();
            if (diceCountFilter.Count > 0) {
                filteredCards = filteredCards.Where(c => diceCountFilter.Contains(c.GetBehaviourList().Count));
            }

            __result.AddRange(filteredCards);
        }

        protected static bool DiceCardKeywordFilter(string keywordName, DiceCardItemModel card) {
            var abilityList = Singleton<BattleCardAbilityDescXmlList>.Instance;

            return card.GetBehaviourList().Any(b => abilityList.GetAbilityKeywords_byScript(b.Script).Contains(keywordName)) ||
                   abilityList.GetAbilityKeywords(card.ClassInfo).Contains(keywordName);
        }
    }

    [HarmonyPatch(typeof(BattleCardAbilityDescXmlList), "GetAbilityKeywords_byScript")]
    [HarmonyBefore(["BongBong Enterprises"])]
    [HarmonyPriority(800)]
    public class BattleCardAbilityDescXmlList_GetAbilityKeywords_byScript {
        public static void Prefix(BattleCardAbilityDescXmlList __instance, string scriptName, ref Dictionary<string, List<string>> ____dictionaryKeywordCache) {
            if (____dictionaryKeywordCache.ContainsKey(scriptName)) return;

            var manager = Singleton<AssemblyManager>.Instance;
            string trimmedName = scriptName.Trim();

            var ability = manager.CreateInstance_DiceCardAbility(trimmedName);
            var keywords = ability?.Keywords ?? (ability == null ? manager.CreateInstance_DiceCardSelfAbility(trimmedName)?.Keywords : null);

            ____dictionaryKeywordCache[scriptName] = keywords != null ? new List<string>(keywords) : new List<string>();
        }
    }

    [HarmonyPatch(typeof(BookModel), "SetXmlInfo")]
    public class BookModel_SetXmlInfo {
        public static void Postfix(BookModel __instance, BookXmlInfo classInfo) {
            if (classInfo.EquipEffect is not BookEquipEffectWrapper diceCard) return;

            var onlyCards = __instance.GetOnlyCards();
            var itemDataList = ItemXmlDataList.instance;
            int count = Math.Min(diceCard.OnlyCard.Count, diceCard.OnlyCardPackageId.Count);

            for (int i = 0; i < count; i++) {
                int id = diceCard.OnlyCard[i];
                string workshopId = diceCard.OnlyCardPackageId[i];

                LorId targetLorId = workshopId == "@origin" ? new LorId(id) : string.IsNullOrEmpty(workshopId) ? new LorId(classInfo.workshopID, id) : new LorId(workshopId, id);
                onlyCards.Add(itemDataList.GetCardItem(targetLorId, false));
            }
        }
    }
}