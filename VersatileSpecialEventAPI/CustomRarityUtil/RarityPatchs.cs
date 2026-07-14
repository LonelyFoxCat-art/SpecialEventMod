using HarmonyLib;
using LOR_BattleUnit_UI;
using LOR_DiceSystem;
using TMPro;
using UI;
using UnityEngine;
using UnityEngine.UI;
using VersatileSpecialEventAPI.CustomRarityUtil.Effect;
using VersatileSpecialEventAPI.CustomRarityUtil.XML;
using VersatileSpecialEventAPI.CustomRarityUtil.XML.Wrapper;
using VersatileSpecialEventAPI.Utils;
using static BattleDiceCardUI;
using static BattleUnitInformationUI_PassiveList;
using static VersatileSpecialEventAPI.API;

namespace VersatileSpecialEventAPI.CustomRarityUtil;

public class RarityPatchs {
    private static void AddFrameEffect(BattleDiceCardUI __instance, Type component) {
        if (!typeof(MonoBehaviour).IsAssignableFrom(component)) return;

        DestroyFrameEffect(__instance, component);
        __instance.img_Frames[0].gameObject.AddComponent(component);
        foreach (var img in __instance.img_linearDodges) img.gameObject.AddComponent(component);
    }
    private static void AddFrameEffect(Image[] linearDodges, Image frameL, Type component) {
        if (!typeof(MonoBehaviour).IsAssignableFrom(component)) return;

        DestroyFrameEffect(linearDodges, frameL, component);

        frameL?.gameObject.AddComponent(component);
        foreach (var img in linearDodges) img?.gameObject.AddComponent(component);
    }
    private static void DestroyFrameEffect(BattleDiceCardUI __instance, Type component) {
        if (!typeof(MonoBehaviour).IsAssignableFrom(component)) return;

        __instance.img_Frames[0].gameObject.SafeDestroyAll(component);
        foreach (var img in __instance.img_linearDodges) img.gameObject.SafeDestroyAll(component);
    }
    private static void DestroyFrameEffect(Image[] linearDodges, Image frameL, Type component) {
        if (!typeof(MonoBehaviour).IsAssignableFrom(component)) return;

        frameL?.gameObject.SafeDestroyAll(component);
        foreach (var img in linearDodges) img?.gameObject.SafeDestroyAll(component);
    }
    private static NumbersData SetCustomCost(BattleDiceCardUI __instance, NumbersData costNumbers, Sprite[] cardCostAddGlow) {
        int cost = __instance.CardModel.GetCost();
        if (cost < 10) {
            costNumbers.SetOneValue(cost, cardCostAddGlow);
            costNumbers.secondNumbers.SetEnable(false);
        } else {
            if (costNumbers.secondNumbers.content == null) {
                var parent = costNumbers.firstNumbers.content.gameObject.transform.parent;
                var newObj = UnityEngine.Object.Instantiate(costNumbers.firstNumbers.content.gameObject, parent);
                costNumbers.secondNumbers.content = newObj.GetComponent<Image>();
                costNumbers.secondNumbers.content.transform.localPosition = new Vector3(100f, 0f, 0f);
            }
            costNumbers.SetValueHundredLeft(Math.Min(cost, 99), cardCostAddGlow);
            costNumbers.secondNumbers.SetEnable(true);
        }
        return costNumbers;
    }
    private static NumbersData SetCustomCost(DiceCardItemModel cardModel, NumbersData costNumbers) {
        int cost = cardModel.GetSpec().Cost;
        if (cost < 10) {
            costNumbers.SetOneValue(cost, UISpriteDataManager.instance._cardCostNumberSprites);
            costNumbers.secondNumbers.SetEnable(false);
        } else {
            if (costNumbers.secondNumbers.content == null) {
                var firstContent = costNumbers.firstNumbers.content;
                var newObj = UnityEngine.Object.Instantiate(firstContent.gameObject, firstContent.transform.parent);
                costNumbers.secondNumbers.content = newObj.GetComponent<Image>();
                costNumbers.secondNumbers.content.transform.localPosition = new Vector3(100f, 0f, 0f);
            }
            costNumbers.SetValueHundredLeft(cost, UISpriteDataManager.instance._cardCostNumberSprites);
            costNumbers.secondNumbers.SetEnable(true);
        }
        return costNumbers;
    }


    #region 掉落配置相关
    [HarmonyPatch(typeof(BookXmlInfo), "get_Limit")]
    public class BookXmlInfo_get_Limit {
        public static void Postfix(BookXmlInfo __instance, ref int __result) {
            if (__instance != null && __instance.Rarity.IsCustomRarity()) {
                var DropMax = RarityXmlWrapper.GetRarityXmlInfo(__instance.workshopID, __instance.Rarity);
                __result = DropMax != null ? DropMax.GetDropInfo(RarityDropType.Equip).DropMax : 1;
            }
        }
    }

    [HarmonyPatch(typeof(DiceCardItemModel), "GetLimit")]
    public class DiceCardItemModel_GetLimit {
        public static void Postfix(DiceCardItemModel __instance, ref int __result) {
            DiceCardXmlInfo DiceCard = __instance.ClassInfo;
            if (DiceCard != null && DiceCard.Rarity.IsCustomRarity()) {
                __result = RarityXmlWrapper.GetRarityXmlInfo(DiceCard.workshopID, DiceCard.Rarity).DeckLimit;
            }
        }
    }

    [HarmonyPatch(typeof(DiceCardXmlInfo), "get_FloorLimit")]
    public class DiceCardXmlInfo_get_FloorLimit {
        public static void Postfix(DiceCardXmlInfo __instance, ref int __result) {
            if (__instance != null && RarityXmlWrapper.IsRarityLoaded(ModId) && __instance.Rarity.IsCustomRarity()) {
                var RarityInfo = RarityXmlWrapper.GetRarityXmlInfo(__instance.workshopID, __instance.Rarity);
                __result = RarityInfo.GetDropInfo(RarityDropType.Card).DropMax;
            }
        }
    }

    [HarmonyPatch(typeof(DiceCardXmlInfo), "get_Limit")]
    public class DiceCardXmlInfo_get_Limit {
        public static void Postfix(DiceCardXmlInfo __instance, ref int __result) {
            if (__instance != null && __instance.Rarity.IsCustomRarity()) {
                __result = RarityXmlWrapper.GetRarityXmlInfo(__instance.workshopID, __instance.Rarity).DeckLimit;
            }
        }
    }

    [HarmonyPatch(typeof(DropBoxListModel), "GetEquipDropBoxCountInfoTable")]
    public class DropBoxListModel_GetEquipDropBoxCountInfoTable {
        public static void Postfix(DropBoxListModel __instance, LorId dropBookId, ref List<DropBoxCount> __result, Dictionary<LorId, DropBoxInfo> ____dropBoxTable) {
            if (__result == null || __result.Count == 0) return;

            var itemDataList = ItemXmlDataList.instance;
            var bookXmlList = Singleton<BookXmlList>.Instance;
            ____dropBoxTable.TryGetValue(dropBookId, out var dropBoxInfo);

            foreach (var dropBoxCount in __result) {
                if (dropBoxCount?.itemInfo is not { } itemInfo) continue;

                int? newMax = null;
                int currentCount = 1;

                if (itemInfo.itemType == DropItemType.Card) {
                    var cardInfo = itemDataList.GetCardItem(itemInfo.id, false);
                    if (cardInfo.Rarity.IsCustomRarity() == true && RarityXmlWrapper.IsRarityLoaded(cardInfo.workshopID)) {
                        var RarityInfo = RarityXmlWrapper.GetRarityXmlInfo(cardInfo.workshopID, cardInfo.Rarity);
                        var DropInfo = RarityInfo.GetDropInfo(RarityDropType.Card);
                        if (DropInfo != null) {
                            newMax = DropInfo.DropMax;
                            currentCount = (dropBoxInfo != null) ? dropBoxInfo.GetCurrentCardCount(cardInfo.id) : 0;
                        }
                    }
                } else if (itemInfo.itemType == DropItemType.Equip) {
                    var equipInfo = bookXmlList.GetData(itemInfo.id, false);
                    if (equipInfo?.Rarity.IsCustomRarity() == true && RarityXmlWrapper.IsRarityLoaded(equipInfo.workshopID)) {
                        var RarityInfo = RarityXmlWrapper.GetRarityXmlInfo(equipInfo.workshopID, equipInfo.Rarity);
                        var DropInfo = RarityInfo.GetDropInfo(RarityDropType.Equip);
                        if (DropInfo != null) {
                            newMax = equipInfo.Limit;
                            currentCount = Singleton<BookInventoryModel>.Instance.GetBookCount(itemInfo.id);
                        }
                    }
                }

                if (newMax.HasValue) {
                    dropBoxCount.max = newMax.Value;
                    dropBoxCount.remain = Math.Max(0, dropBoxCount.max - currentCount);
                }
            }
        }
    }

    [HarmonyPatch(typeof(RandomGachaGenerator), "SelectRandomItem")]
    public class RandomGachaGenerator_SelectRandomItem {
        private static float GetRate(Rarity r) => r switch {
            Rarity.Common => 0.4f,
            Rarity.Uncommon => 0.3f,
            Rarity.Rare => 0.2f,
            Rarity.Unique => 0.1f,
            _ => 0f
        };

        public static bool Prefix(DropBookXmlInfo book, ref BookDropItemInfo __result) {
            var table = Singleton<DropBoxListModel>.Instance.GetEquipDropBoxCountInfoTable(book.id);
            if (table == null || table.Count == 0) return true;

            float equipProb = (book.equipProb > 0f && book.equipProb < 1f) ? book.equipProb : 0.3f;
            float cardProb = 1f - equipProb;

            var dict = new Dictionary<string, RandomGachaGenerator.ProbInfo>();
            bool hasCustom = false;

            foreach (var drop in table) {
                if (drop.remain == 0) continue;

                var info = drop.itemInfo;
                bool isCard = info.itemType == DropItemType.Card;
                float mult = isCard ? cardProb : equipProb;
                object data = isCard ? ItemXmlDataList.instance.GetCardItem(info.id, false) : Singleton<BookXmlList>.Instance.GetData(info.id, false);
                if (data == null) continue;

                string rName; float dRate;

                if (isCard) {
                    var DiceCard = (DiceCardXmlInfo)data;
                    var RarityInfo = RarityXmlWrapper.GetRarityXmlInfo(DiceCard.workshopID, DiceCard.Rarity);
                    var DropInfo = RarityInfo.GetDropInfo(RarityDropType.Card);
                    if (DiceCard.Rarity.IsCustomRarity() && RarityInfo != null && DropInfo != null) {
                        rName = RarityInfo.RarityName;
                        dRate = DropInfo.DropRate * mult;
                        hasCustom = true;
                    } else {
                        rName = DiceCard.Rarity.ToString();
                        dRate = GetRate(DiceCard.Rarity) * mult;
                    }
                } else {
                    var Book = (BookXmlInfo)data;
                    var RarityInfo = RarityXmlWrapper.GetRarityXmlInfo(Book.workshopID, Book.Rarity);
                    var DropInfo = RarityInfo.GetDropInfo(RarityDropType.Equip);
                    if (Book.Rarity.IsCustomRarity() && RarityInfo != null && DropInfo != null) {
                        rName = RarityInfo.RarityName;
                        dRate = DropInfo.DropRate * mult;
                        hasCustom = true;
                    } else {
                        rName = Book.Rarity.ToString();
                        dRate = GetRate(Book.Rarity) * mult;
                    }
                }

                string key = $"{info.itemType}_{rName}";
                if (!dict.TryGetValue(key, out var p)) {
                    p = new RandomGachaGenerator.ProbInfo {
                        type = info.itemType,
                        prob = dRate,
                        dropList = new List<DropBoxCount>()
                    };
                    dict[key] = p;
                }
                p.dropList.Add(drop);
            }

            if (!hasCustom) return true;

            var valid = dict.Values.Where(p => p.dropList.Count > 0).ToList();
            if (valid.Count == 0) { __result = null!; return false; }

            float total = valid.Sum(p => p.prob);
            float roll = RandomUtil.RangeFloat(0f, total), sum = 0f;

            foreach (var p in valid) {
                if ((sum += p.prob) > roll) {
                    __result = RandomUtil.SelectOne(p.dropList).itemInfo;
                    return false;
                }
            }

            __result = null!;
            return false;
        }
    }
    #endregion

    #region 能力描述相关
    [HarmonyPatch(typeof(BattleDiceCard_BehaviourDescUI), "SetBehaviourInfo")]
    public static class BattleDiceCard_BehaviourDescUI_SetBehaviourInfo {
        public static void Postfix(BattleDiceCard_BehaviourDescUI __instance, DiceBehaviour behaviour, LorId cardId, List<DiceBehaviour> behaviourList, bool isHide) {
            if (__instance?.txt_ability == null) return;
            if (ItemXmlDataList.instance.GetCardItem(cardId, false) is not DiceCardXmlInfo diceCard) return;

            string txt = __instance.txt_ability.text;
            var rarity = RarityXmlWrapper.GetRarityXmlInfo(diceCard.workshopID, diceCard.Rarity);

            if (rarity == null) return;
            if (rarity.AbilityKeywordColor != Color.yellow) txt = txt.Replace("<color=yellow>", $"<color={rarity.AbilityKeywordColor.ToHex()}>");
            if (rarity.AbilityDescColor != Color.white) txt = $"<color={rarity.AbilityDescColor.ToHex()}>{txt}</color>";

            __instance.txt_ability.text = txt;
        }
    }

    [HarmonyPatch(typeof(UIDetailCardDescSlot), "SetBehaviourInfo")]
    public class UIDetailCardDescSlot_SetBehaviourInfo {
        public static void Postfix(UIDetailCardDescSlot __instance, DiceBehaviour behaviour, LorId cardId, List<DiceBehaviour> behaviourList, bool isHide) {
            if (__instance?.txt_ability == null) return;
            if (ItemXmlDataList.instance.GetCardItem(cardId, false) is not DiceCardXmlInfo diceCard) return;

            string txt = __instance.txt_ability.text;
            var rarity = RarityXmlWrapper.GetRarityXmlInfo(diceCard.workshopID, diceCard.Rarity);

            if (rarity == null) return;
            if (rarity.AbilityKeywordColor != Color.yellow) txt = txt.Replace("<color=yellow>", $"<color={rarity.AbilityKeywordColor.ToHex()}>");
            if (rarity.AbilityDescColor != Color.white) txt = $"<color={rarity.AbilityDescColor.ToHex()}>{txt}</color>";

            __instance.txt_ability.text = txt;
        }
    }
    #endregion

    #region 书页相关
    [HarmonyPatch(typeof(BattleDiceCardUI), "SetCard")]
    public class BattleDiceCardUI_SetCard {
        public static void Postfix(BattleDiceCardUI __instance, BattleDiceCardModel cardModel, Option[] options,
            ref NumbersData ___costNumbers,
            ref Sprite[] ___costNumberSprite,
            ref Color ___colorFrame, ref Color ___colorLineardodge, ref Color ___colorLineardodge_deactive,
            ref int ____cost, ref int ____originCost) {
            try {
                DiceCardXmlInfo? xmlInfo = (cardModel != null) ? cardModel.XmlData : null;
                var frontImg = __instance.img_artwork.transform.parent.parent.GetChild(1).GetComponent<Image>();

                if (xmlInfo == null || !xmlInfo.Rarity.IsCustomRarity()) {
                    if (__instance.img_Frames[0].overrideSprite.name == "CustomRarityUtil_LFrame") __instance.img_Frames[0].overrideSprite = null;
                    if (__instance.img_Frames[4].overrideSprite.name == "CustomRarityUtil_RFrame") __instance.img_Frames[4].overrideSprite = null;
                    if (__instance.img_icon.overrideSprite.name == "CustomRarityUtil_RangeIcon") __instance.img_icon.overrideSprite = null;

                    frontImg.color = Color.white;

                    if (frontImg.overrideSprite.name == "CustomRarityUtil_FFrame") frontImg.overrideSprite = null;
                    foreach (FrameEffect effect in Enum.GetValues(typeof(FrameEffect))) DestroyFrameEffect(__instance, effect.GetFrameEffect());

                    ___costNumbers.secondNumbers.SetEnable(false);
                    ___colorLineardodge.a = 1f;
                    __instance.Method("SetLinearDodgeColor", ___colorLineardodge).Call();
                    return;
                }

                // --- 自定义稀有度逻辑 ---
                var rarityInfo = RarityXmlWrapper.GetRarityXmlInfo(xmlInfo.workshopID, xmlInfo.Rarity);
                APILogger.LogInfo($"Rarity {xmlInfo.Rarity} {rarityInfo.FrameColor}");

                // 边框与图标 Sprite 设置
                var LeftFrameArtwork = rarityInfo.GetArtworkName(RarityArtworkType.LeftFrame);
                if (!string.IsNullOrEmpty(LeftFrameArtwork) && SpriteHelper.IsSpriteExist(LeftFrameArtwork)) {
                    __instance.img_Frames[0].overrideSprite = SpriteHelper.GetData(LeftFrameArtwork);
                    __instance.img_Frames[0].overrideSprite.name = "CustomRarityUtil_LFrame";
                } else {
                    __instance.img_Frames[0].overrideSprite = null;
                }

                var RightFrameArtwork = rarityInfo.GetArtworkName(RarityArtworkType.RightFrame);
                if (!string.IsNullOrEmpty(RightFrameArtwork) && SpriteHelper.IsSpriteExist(RightFrameArtwork)) {
                    __instance.img_Frames[4].overrideSprite = SpriteHelper.GetData(RightFrameArtwork);
                    __instance.img_Frames[4].overrideSprite.name = "CustomRarityUtil_RFrame";
                } else {
                    __instance.img_Frames[4].overrideSprite = null;
                }

                var FrontFrameArtwork = rarityInfo.GetArtworkName(RarityArtworkType.FrontFrame);
                if (!string.IsNullOrEmpty(FrontFrameArtwork) && SpriteHelper.IsSpriteExist(FrontFrameArtwork)) {
                    frontImg.overrideSprite = SpriteHelper.GetData(FrontFrameArtwork);
                    frontImg.overrideSprite.name = "CustomRarityUtil_FFrame";
                    if (rarityInfo.frontFrameApplyColor) frontImg.color = rarityInfo.FrameColor;
                } else {
                    frontImg.overrideSprite = null;
                }

                var iPath = rarityInfo.GetRangeIconName(__instance.CardModel.GetSpec().Ranged);
                if (!string.IsNullOrEmpty(iPath) && SpriteHelper.IsSpriteExist(iPath)) {
                    __instance.img_icon.overrideSprite = SpriteHelper.GetData(iPath);
                    __instance.img_icon.overrideSprite.name = "CustomRarityUtil_RangeIcon";
                } else {
                    __instance.img_icon.overrideSprite = null;
                }

                // 费用数字
                var costGlow = (____cost != ____originCost) ? UISpriteDataManager.instance.CardCostAddGlow : ___costNumberSprite;
                ___costNumbers = SetCustomCost(__instance, ___costNumbers, costGlow);

                // 颜色与特效参数
                ___colorFrame = rarityInfo.FrameColor;
                ___colorLineardodge = rarityInfo.FrameLinearColor;
                ___colorLineardodge_deactive = rarityInfo.FrameColor;
                __instance.Method("SetRangeIconHsv", rarityInfo.RangeIconColorHsv).Call();
                __instance.Method("SetFrameColor", ___colorFrame).Call();
                __instance.Method("SetLinearDodgeColor", ___colorLineardodge).Call();

                // 边框特效
                foreach (FrameEffect effect in Enum.GetValues(typeof(FrameEffect))) DestroyFrameEffect(__instance, effect.GetFrameEffect());

                var frameEffectType = rarityInfo.FrameEffect.GetFrameEffect();
                if (frameEffectType != null) AddFrameEffect(__instance, frameEffectType);

                // 文本颜色替换
                var txt = __instance.txt_selfAbility;
                if (rarityInfo.AbilityKeywordColor != Color.yellow) {
                    var hex = ColorUtility.ToHtmlStringRGB(rarityInfo.AbilityKeywordColor);
                    txt.text = txt.text.Replace("<color=yellow>", $"<color=#{hex}>");
                }

                if (rarityInfo.AbilityDescColor != Color.white) {
                    var hex = ColorUtility.ToHtmlStringRGB(rarityInfo.AbilityDescColor);
                    txt.text = $"<color=#{hex}>{txt.text}</color>";
                }
            } catch (Exception ex) {
                Debug.LogError(ex);
            }
        }
    }

    [HarmonyPatch(typeof(UIBattleSettingLibrarianInfoPanel), "SetData")]
    public class UIBattleSettingLibrarianInfoPanel_SetData {
        public static void Postfix(UIBattleSettingLibrarianInfoPanel __instance, UnitDataModel data,
            ref Image ___img_BookIconGlow, ref TextMeshProMaterialSetter ___setter_bookname, ref TextMeshProUGUI ___txt_BookName) {
            BookXmlInfo? Book = ((data != null) ? data.bookItem.ClassInfo : null);
            if (Book != null && Book.Rarity.IsCustomRarity()) {
                Color frameColor = RarityXmlWrapper.GetRarityXmlInfo(Book.workshopID, Book.Rarity).FrameColor;
                ___img_BookIconGlow.color = frameColor;
                ___setter_bookname.underlayColor = frameColor;
                ___setter_bookname.InitMaterialProperty();
                __instance.SetEquipPageSlotColor(frameColor);
            }
        }
    }

    [HarmonyPatch(typeof(UIBattleSettingLibrarianInfoPanel), "SetEquipPageSlotState")]
    public class UIBattleSettingLibrarianInfoPanel_SetEquipPageSlotState {
        public static void Postfix(UIBattleSettingLibrarianInfoPanel __instance, UnitDataModel ___unitdata) {
            BookXmlInfo? Book = ((___unitdata != null) ? ___unitdata.bookItem.ClassInfo : null);
            if (Book != null && Book.Rarity.IsCustomRarity()) {
                Color frameColor = RarityXmlWrapper.GetRarityXmlInfo(Book.workshopID, Book.Rarity).FrameColor;
                __instance.SetEquipPageSlotColor(frameColor);
            }
        }
    }

    [HarmonyPatch(typeof(UICharacterBookSlot), "SetHighlighted")]
    public class UICharacterBookSlot_SetHighlighted {
        public static void Postfix(UICharacterBookSlot __instance, bool on, ref TextMeshProUGUI ___BookName, ref List<Graphic> ____defaultGraphics, List<Graphic> ____targetGraphics) {
            BookModel bookModel = __instance.BookModel;
            BookXmlInfo? Book = ((bookModel != null) ? bookModel.ClassInfo : null);
            if (Book != null && Book.Rarity.IsCustomRarity()) {
                Color frameColor = RarityXmlWrapper.GetRarityXmlInfo(Book.workshopID, Book.Rarity).FrameColor;

                if (!on) {
                    foreach (var graphic in ____targetGraphics) {
                        graphic.CrossFadeColor(frameColor, 0.1f, true, true);
                    }
                } else {
                    foreach (var graphic in ____defaultGraphics) {
                        graphic.CrossFadeColor(frameColor, 0.1f, true, true);
                    }
                }

                ___BookName.color = frameColor;
                if (___BookName.TryGetComponent<TextMeshProMaterialSetter>(out var setter)) {
                    setter.independentSetting = true;
                    setter.underlayColor = frameColor;
                    setter.enabled = false;
                    setter.enabled = true;
                } else {
                    ___BookName.fontMaterial.SetColor("_UnderlayColor", frameColor);
                }

                ___BookName.gameObject.SetActive(false);
                ___BookName.gameObject.SetActive(true);
            }
        }
    }

    [HarmonyPatch(typeof(UICustomCoreBookInfoPanel), "SetBookContentData")]
    public class UICustomCoreBookInfoPanel_SetBookContentData {
        public static void Postfix(UICustomCoreBookInfoPanel __instance, BookModel book, ref TextMeshProUGUI ___txt_rarity) {
            BookXmlInfo? Book = ((book != null) ? book.ClassInfo : null);
            if (Book != null && Book.Rarity.IsCustomRarity()) {
                Color frameColor = RarityXmlWrapper.GetRarityXmlInfo(Book.workshopID, Book.Rarity).FrameColor;
                ___txt_rarity.fontMaterial.SetColor("_UnderlayColor", frameColor);
            }
        }
    }

    [HarmonyPatch(typeof(UIDetailCardSlot), "SetData")]
    public class UIDetailCardSlot_SetData {
        private static readonly Dictionary<int, Image> _bgFrameCache = new Dictionary<int, Image>();

        public static void Postfix(UIDetailCardSlot __instance, DiceCardItemModel cardmodel, GameObject ___ob_selfAbility, TextMeshProUGUI ___txt_selfAbility) {
            try {
                var DiceCard = cardmodel?.ClassInfo;
                var RarityInfo = DiceCard != null ? RarityXmlWrapper.GetRarityXmlInfo(DiceCard.workshopID, DiceCard.Rarity) : null;

                if (RarityInfo != null && ___txt_selfAbility != null) {
                    string original = ___txt_selfAbility.text;
                    bool needKeyword = RarityInfo.AbilityKeywordColor != Color.yellow && original.Contains("<color=yellow>");
                    bool needDesc = RarityInfo.AbilityDescColor != Color.white;

                    if (needKeyword || needDesc) {
                        string processed = needKeyword
                            ? original.Replace("<color=yellow>", $"<color={RarityInfo.AbilityKeywordColor.ToHex()}>")
                            : original;

                        ___txt_selfAbility.text = needDesc
                            ? $"<color={RarityInfo.AbilityDescColor.ToHex()}>{processed}</color>"
                            : processed;
                    }
                }

                if (___ob_selfAbility == null) return;

                int instanceId = ___ob_selfAbility.GetInstanceID();
                if (!_bgFrameCache.TryGetValue(instanceId, out Image bgFrameImage)) {
                    var currentTransform = ___ob_selfAbility.transform;
                    for (int i = 0; i < 3; i++) {
                        if (currentTransform.parent == null) return;
                        currentTransform = currentTransform.parent;
                    }

                    var root = currentTransform.gameObject;
                    if (root != null) bgFrameImage = root.GetComponentsInChildren<Image>().FirstOrDefault(x => x.name.Contains("[Image]BgFrame"));
                    _bgFrameCache[instanceId] = bgFrameImage;
                }

                if (bgFrameImage == null) return;

                bgFrameImage.color = Color.white;
                bgFrameImage.overrideSprite = null;

                if (DiceCard != null && RarityInfo != null && RarityInfo.Rarity.IsCustomRarity()) {
                    string frameName = RarityInfo.GetArtworkName(RarityArtworkType.RightFrame);
                    if (!string.IsNullOrEmpty(frameName) && SpriteHelper.IsSpriteExist(frameName)) {
                        var sprite = SpriteHelper.GetData(frameName);
                        if (sprite != null) {
                            bgFrameImage.overrideSprite = sprite;
                            bgFrameImage.color = RarityInfo.FrameColor;
                        }
                    }
                }
            } catch (Exception ex) {
                APILogger.LogError(ex.ToString());
            }
        }
    }

    [HarmonyPatch(typeof(UIEquipPageModelPreviewPanel), "SetData")]
    public class UIEquipPageModelPreviewPanel_SetData {
        public static void Postfix(UIEquipPageModelPreviewPanel __instance, BookModel book, ref Graphic[] ___graphic_Frames, ref TextMeshProMaterialSetter ___setter_bookname, ref TextMeshProUGUI ___txt_BookName) {
            BookXmlInfo? Book = ((book != null) ? book.ClassInfo : null);
            if (Book != null && Book.Rarity.IsCustomRarity()) {
                Color frameColor = RarityXmlWrapper.GetRarityXmlInfo(Book.workshopID, Book.Rarity).FrameColor;
                for (int i = 0; i < ___graphic_Frames.Length; i++) ___graphic_Frames[i].color = frameColor;
                ___setter_bookname.underlayColor = frameColor;
                ___setter_bookname.enabled = false;
                ___setter_bookname.enabled = true;
            }
        }
    }

    [HarmonyPatch(typeof(UIEquipPagePreviewPanel), "SetData")]
    public class UIEquipPagePreviewPanel_SetData {
        public static void Postfix(UIEquipPagePreviewPanel __instance, BookModel book, ref Graphic[] ___graphic_Frames, ref TextMeshProMaterialSetter ___setter_bookname) {
            BookXmlInfo? Book = ((book != null) ? book.ClassInfo : null);
            if (Book != null && Book.Rarity.IsCustomRarity()) {
                Color frameColor = RarityXmlWrapper.GetRarityXmlInfo(Book.workshopID, Book.Rarity).FrameColor;
                for (int i = 0; i < ___graphic_Frames.Length; i++) ___graphic_Frames[i].color = frameColor;
                ___setter_bookname.underlayColor = frameColor;
                ___setter_bookname.enabled = false;
                ___setter_bookname.enabled = true;
            }
        }
    }

    [HarmonyPatch(typeof(UIEquipPagePreviewPanel), "SetPassiveBookInfoPanel")]
    public class UIEquipPagePreviewPanel_SetPassiveBookInfoPanel {
        public static void Postfix(UIEquipPagePreviewPanel __instance, ref BookModel ___bookDataModel, ref Image ___img_givebookFrame, ref Image ___img_givebookIconGlow, ref TextMeshProMaterialSetter ___setter_givebookname,
            ref TextMeshProUGUI ___txt_givebookname) {
            BookXmlInfo? Book = ((___bookDataModel != null) ? ___bookDataModel.ClassInfo : null);
            if (Book != null && Book.Rarity.IsCustomRarity()) {
                Color frameColor = RarityXmlWrapper.GetRarityXmlInfo(Book.workshopID, Book.Rarity).FrameColor;
                ___img_givebookFrame.color = frameColor;
                ___img_givebookIconGlow.color = frameColor;
                ___setter_givebookname.underlayColor = frameColor;
                ___txt_givebookname.gameObject.SetActive(false);
                ___txt_givebookname.gameObject.SetActive(true);
            }
        }
    }

    [HarmonyPatch(typeof(UIGachaEquipSlot), "SetDefaultColor")]
    public class UIGachaEquipSlot_SetDefaultColor {
        public static void Postfix(UIGachaEquipSlot __instance) {
            BookModel book = __instance._book;
            var Book = ((book != null) ? book.ClassInfo : null) as BookXmlWrapper;
            if (Book != null && Book.Rarity.IsCustomRarity()) {
                Color frameColor = RarityXmlWrapper.GetRarityXmlInfo(Book.workshopID, Book.Rarity).FrameColor;
                __instance.Method("SetGlowColor", frameColor).Call();
            }
        }
    }

    [HarmonyPatch(typeof(UIInvenLeftEquipPageSlot), "SetColorFrame")]
    public class UIInvenLeftEquipPageSlot_SetColorFrame {
        public static void Postfix(UIInvenLeftEquipPageSlot __instance, UIEquipPageSlotState type) {
            BookModel bookDataModel = __instance.BookDataModel;
            BookXmlInfo? Book = ((bookDataModel != null) ? bookDataModel.ClassInfo : null);
            if (Book != null && Book.Rarity.IsCustomRarity()) {
                Color frameColor = RarityXmlWrapper.GetRarityXmlInfo(Book.workshopID, Book.Rarity).FrameColor;
                __instance.Method("SetGlowColor", frameColor).Call();
            }
        }
    }

    [HarmonyPatch(typeof(UILibrarianEquipBookInfoPanel), "SetUnitData")]
    public class UILibrarianEquipBookInfoPanel_SetUnitData {
        public static void Postfix(UILibrarianEquipBookInfoPanel __instance, UnitDataModel data, ref Image ___icon, ref TextMeshProUGUI ___bookName, ref List<Graphic> ___targetGraphics) {
            BookXmlInfo? Book = ((data != null) ? data.bookItem.ClassInfo : null);
            if (Book == null) return;
            string text = (string.IsNullOrEmpty(Book.workshopID)) ? $"No.{Book._id}" : $"{Book.workshopID} - No.{Book._id}";
            ___bookName.text = $"({text}) {Book.Name}";

            if (Book.Rarity.IsCustomRarity()) {
                Color frameColor = RarityXmlWrapper.GetRarityXmlInfo(Book.workshopID, Book.Rarity).FrameColor;
                ___icon.color = frameColor;
                ___bookName.color = frameColor;
                ___bookName.GetComponent<TextMeshProMaterialSetter>().underlayColor = frameColor;
                ___bookName.GetComponent<TextMeshProMaterialSetter>().InitMaterialProperty();
                foreach (Graphic graphic in ___targetGraphics) {
                    graphic.CrossFadeColor(frameColor, 0.01f, true, true);
                }
                ___bookName.GetComponent<TextMeshProMaterialSetter>().InitMaterialProperty();
            }
        }
    }

    [HarmonyPatch(typeof(UILibrarianEquipDeckPanel), "SetData")]
    public class UILibrarianEquipDeckPanel_SetData {
        public static void Postfix(UILibrarianEquipDeckPanel __instance) {
            UnitDataModel unitdata = __instance.Unitdata;
            BookXmlInfo? Book = (unitdata != null) ? unitdata.bookItem.ClassInfo : null;
            if (Book != null && Book.Rarity.IsCustomRarity()) {
                Color frameColor = RarityXmlWrapper.GetRarityXmlInfo(Book.workshopID, Book.Rarity).FrameColor;
                __instance.Method("SetRarityColor", frameColor).Call();
            }
        }
    }

    [HarmonyPatch(typeof(UILibrarianInfoInCardPhase), "SetData")]
    public class UILibrarianInfoInCardPhase_SetData {
        public static void Postfix(UILibrarianInfoInCardPhase __instance, UnitDataModel data, ref TextMeshProUGUI ___txt_BookName, ref TextMeshProMaterialSetter ___setter_bookname, ref Image ___img_BookIcon, ref Image ___img_BookIconGlow, ref Graphic[] ___graphic_Frames) {
            BookXmlInfo? Book = ((data != null) ? data.bookItem.ClassInfo : null);
            if (Book != null && Book.Rarity.IsCustomRarity()) {
                Color frameColor = RarityXmlWrapper.GetRarityXmlInfo(Book.workshopID, Book.Rarity).FrameColor;
                foreach (var graphic in ___graphic_Frames) { graphic.color = frameColor; }
                ___txt_BookName.color = frameColor;
                ___img_BookIcon.color = frameColor;
                ___img_BookIconGlow.color = frameColor;
                ___setter_bookname.underlayColor = frameColor;
                ___setter_bookname.enabled = false;
                ___setter_bookname.enabled = true;
            }
        }
    }

    [HarmonyPatch(typeof(UILibrarianInfoInCardPhase), "OnPointerExitEquipPage")]
    public class UILibrarianInfoInCardPhase_OnPointerExitEquipPage {
        public static void Postfix(UILibrarianInfoInCardPhase __instance, ref UnitDataModel ___unitdata, ref Graphic[] ___graphic_Frames) {
            BookXmlInfo? Book = ((___unitdata != null) ? ___unitdata.bookItem.ClassInfo : null);
            if (Book != null && Book.Rarity.IsCustomRarity()) {
                Color frameColor = RarityXmlWrapper.GetRarityXmlInfo(Book.workshopID, Book.Rarity).FrameColor;
                foreach (var graphic in ___graphic_Frames) { graphic.color = frameColor; }
            }
        }
    }

    [HarmonyPatch(typeof(UILibrarianInfoPanel), "UpdatePanel")]
    public class UILibrarianInfoPanel_UpdatePanel {
        public static void Postfix(UILibrarianInfoPanel __instance) {
            UnitDataModel selectedUnit = __instance.SelectedUnit;
            BookXmlInfo? Book = (selectedUnit != null) ? selectedUnit.bookItem.ClassInfo : null;
            if (Book != null && Book.Rarity.IsCustomRarity() && UI.UIController.Instance.CurrentUIPhase != UIPhase.Main_ItemList) {
                Color frameColor = RarityXmlWrapper.GetRarityXmlInfo(Book.workshopID, Book.Rarity).FrameColor;
                __instance.Method("SetFrameColor", frameColor).Call();
            }
        }
    }

    [HarmonyPatch(typeof(UIOriginCardSlot), "SetData")]
    public class UIOriginCardSlot_SetData {
        private static readonly FrameEffect[] AllFrameEffects = (FrameEffect[])Enum.GetValues(typeof(FrameEffect));

        public static void Postfix(UIOriginCardSlot __instance, DiceCardItemModel cardmodel, ref NumbersData ___costNumbers,
            ref Image ___img_Artwork, ref Image ___img_RangeIcon, ref Image[] ___img_Frames, ref Image[] ___img_linearDodge,
            ref Color ___colorFrame, ref Color ___colorLineardodge) {
            try {
                if (cardmodel == null) return;
                DiceCardXmlInfo DiceCard;
                try {
                    if (cardmodel.ClassInfo is not DiceCardXmlInfo info) return;
                    DiceCard = info;
                } catch (NullReferenceException) {
                    return;
                }

                var frontImg = ___img_Artwork.transform.parent.parent.GetChild(1).GetComponent<Image>();
                var img_Frame = ___img_Frames.FirstOrDefault((Image x) => x.name.Contains("[Image]NormalFrame"));

                if (DiceCard == null || !DiceCard.Rarity.IsCustomRarity()) {
                    foreach (var effect in AllFrameEffects) DestroyFrameEffect(___img_linearDodge, img_Frame, effect.GetFrameEffect());

                    ___costNumbers.secondNumbers.SetEnable(false);
                    frontImg.color = Color.white;

                    if (img_Frame.overrideSprite.name == "CustomRarityUtil_LFrame") img_Frame.overrideSprite = null;
                    if (___img_RangeIcon.overrideSprite.name == "CustomRarityUtil_RangeIcon") ___img_RangeIcon.overrideSprite = null;
                    if (frontImg.overrideSprite.name == "CustomRarityUtil_FFrame") frontImg.overrideSprite = null;

                    return;
                }

                // --- 自定义稀有度逻辑 ---
                var RarityInfo = RarityXmlWrapper.GetRarityXmlInfo(DiceCard.workshopID, DiceCard.Rarity);

                // 左侧边框
                var lPath = RarityInfo.GetArtworkName(RarityArtworkType.LeftFrame);
                if (!string.IsNullOrEmpty(lPath) && SpriteHelper.IsSpriteExist(lPath)) {
                    img_Frame.overrideSprite = SpriteHelper.GetData(lPath);
                    img_Frame.overrideSprite.name = "CustomRarityUtil_LFrame";
                } else {
                    img_Frame.overrideSprite = null;
                }

                // 前置卡面
                frontImg.color = Color.white;
                var fPath = RarityInfo.GetArtworkName(RarityArtworkType.FrontFrame);
                if (!string.IsNullOrEmpty(fPath) && SpriteHelper.IsSpriteExist(fPath)) {
                    frontImg.overrideSprite = SpriteHelper.GetData(fPath);
                    frontImg.overrideSprite.name = "CustomRarityUtil_FFrame";
                    if (RarityInfo.frontFrameApplyColor) frontImg.color = RarityInfo.FrameColor;
                } else {
                    frontImg.overrideSprite = null;
                }

                // 边框特效
                foreach (var effect in AllFrameEffects) DestroyFrameEffect(___img_linearDodge, img_Frame, effect.GetFrameEffect());

                var frameEffectType = RarityInfo.FrameEffect.GetFrameEffect();
                if (frameEffectType != null) AddFrameEffect(___img_linearDodge, img_Frame, frameEffectType);

                // 费用
                ___costNumbers = SetCustomCost(cardmodel!, ___costNumbers);

                // 距离图标
                var iPath = RarityInfo.GetRangeIconName(__instance.CardModel.GetSpec().Ranged);
                if (!string.IsNullOrEmpty(iPath) && SpriteHelper.IsSpriteExist(iPath)) {
                    ___img_RangeIcon.overrideSprite = SpriteHelper.GetData(iPath);
                    ___img_RangeIcon.overrideSprite.name = "CustomRarityUtil_RangeIcon";
                } else {
                    ___img_RangeIcon.overrideSprite = null;
                }

                // 颜色与 HSV
                ___colorFrame = RarityInfo.FrameColor;
                ___colorLineardodge = RarityInfo.FrameLinearColor;
                __instance.Method("SetRangeIconHsv", RarityInfo.RangeIconColorHsv).Call();
                __instance.Method("SetFrameColor", ___colorFrame).Call();
                __instance.Method("SetLinearDodgeColor", ___colorLineardodge).Call();
                ___costNumbers.SetContentColor(___colorFrame);
            } catch (Exception ex) {
                APILogger.LogError($"exception {ex.ToString()}");
            }
        }
    }

    [HarmonyPatch(typeof(UIOriginCardSlot), "SetGrayScale")]
    public class UIOriginCardSlot_SetGrayScale {
        public static void Postfix(UIOriginCardSlot __instance, bool on, ref Image[] ___img_BehaviourIcons) {
            var cardModel = __instance.CardModel;
            DiceCardXmlInfo diceCard;
            try {
                if (cardModel.ClassInfo is not DiceCardXmlInfo info) return;
                diceCard = info;
            } catch (NullReferenceException) {
                return;
            }

            if (on || cardModel == null || diceCard == null || !diceCard.Rarity.IsCustomRarity()) return;

            var rarityInfo = RarityXmlWrapper.GetRarityXmlInfo(diceCard.workshopID, diceCard.Rarity);
            if (rarityInfo == null) return;

            if (___img_BehaviourIcons != null) {
                foreach (var icon in ___img_BehaviourIcons) {
                    if (icon == null) continue;
                    icon.material = null;
                    if (icon.enabled) {
                        icon.enabled = false;
                        icon.enabled = true;
                    }
                }
            }

            __instance.Method("SetFrameColor", rarityInfo.FrameColor).Call();
            __instance.Method("SetLinearDodgeColor", rarityInfo.FrameLinearColor).Call();
            __instance.Method("SetRangeIconHsv", rarityInfo.RangeIconColorHsv).Call();
        }
    }

    [HarmonyPatch(typeof(UIOriginCardSlot), "SetHighlightedSlot")]
    public class UIOriginCardSlot_SetHighlightedSlot {
        public static void Postfix(UIOriginCardSlot __instance, bool on) {
            DiceCardItemModel cardModel = __instance.CardModel;
            DiceCardXmlInfo? DiceCard = ((cardModel != null) ? cardModel.ClassInfo : null);
            if (DiceCard != null && DiceCard.Rarity.IsCustomRarity() && !on) {
                Vector3 rangeIconColorHsv = RarityXmlWrapper.GetRarityXmlInfo(DiceCard.workshopID, DiceCard.Rarity).RangeIconColorHsv;
                __instance.Method("SetRangeIconHsv", rangeIconColorHsv).Call();
            }
        }
    }

    [HarmonyPatch(typeof(UIOriginEquipPageSlot), "SetColorFrame")]
    public class UIOriginEquipPageSlot_SetColorFrame {
        public static void Postfix(UIOriginEquipPageSlot __instance, UIEquipPageSlotState type) {
            BookModel bookDataModel = __instance.BookDataModel;
            BookXmlInfo? Book = ((bookDataModel != null) ? bookDataModel.ClassInfo : null);
            if (Book != null && Book.Rarity.IsCustomRarity()
                && (type == UIEquipPageSlotState.None || type == UIEquipPageSlotState.Succession || type == UIEquipPageSlotState.SuccessionMatter)) {
                Color frameColor = RarityXmlWrapper.GetRarityXmlInfo(Book.workshopID, Book.Rarity).FrameColor;
                __instance.Method("SetGlowColor", frameColor).Call();
            }
        }
    }

    [HarmonyPatch(typeof(UIPassiveEquipBookSlot), "SetData")]
    public class UIPassiveEquipBookSlot_SetData {
        public static void Postfix(UIPassiveEquipBookSlot __instance, BookModel book) {
            BookXmlInfo? Book = (book != null) ? book.ClassInfo : null;
            if (Book != null && Book.Rarity.IsCustomRarity()) {
                Color frameColor = RarityXmlWrapper.GetRarityXmlInfo(Book.workshopID, Book.Rarity).FrameColor;
                __instance.SetRarityColor(frameColor);
            }
        }
    }

    [HarmonyPatch(typeof(UIPassiveSuccessionBookSlot), "SetDefaultColor")]
    public class UIPassiveSuccessionBookSlot_SetDefaultColor {
        public static void Postfix(UIPassiveSuccessionBookSlot __instance) {
            BookModel currentbookmodel = __instance.CurrentBookModel;
            BookXmlInfo? Book = (currentbookmodel != null) ? currentbookmodel.ClassInfo : null;
            if (Book != null && Book.Rarity.IsCustomRarity()) {
                Color frameColor = RarityXmlWrapper.GetRarityXmlInfo(Book.workshopID, Book.Rarity).FrameColor;
                __instance.SetColor(frameColor);
                __instance.SetGlowColor(frameColor);
            }
        }
    }

    [HarmonyPatch(typeof(UIPassiveSuccessionBookSlot), "SetHighlightColor")]
    public class UIPassiveSuccessionBookSlot_SetHighlightColor {
        public static void Postfix(UIPassiveSuccessionBookSlot __instance) {
            BookModel currentbookmodel = __instance.CurrentBookModel;
            BookXmlInfo? Book = (currentbookmodel != null) ? currentbookmodel.ClassInfo : null;
            if (Book != null && Book.Rarity.IsCustomRarity()) {
                Color frameColor = RarityXmlWrapper.GetRarityXmlInfo(Book.workshopID, Book.Rarity).FrameColor;
                __instance.SetColor(frameColor);
                __instance.SetGlowColor(frameColor);
            }
        }
    }

    [HarmonyPatch(typeof(UIPassiveSuccessionCenterEquipBookSlot), "SetColorByRarity")]
    public class UIPassiveSuccessionCenterEquipBookSlot_SetColorByRarity {
        public static void Postfix(UIPassiveSuccessionCenterEquipBookSlot __instance, Rarity rare, ref Image ___img_Frame, ref Image ___img_IconGlow, ref TextMeshProMaterialSetter ___setter_name) {
            BookModel currentbookmodel = __instance.CurrentBookModel;
            BookXmlInfo? Book = (currentbookmodel != null) ? currentbookmodel.ClassInfo : null;
            if (Book != null && Book.Rarity.IsCustomRarity()) {
                Color frameColor = RarityXmlWrapper.GetRarityXmlInfo(Book.workshopID, Book.Rarity).FrameColor;
                ___img_Frame.color = frameColor;
                ___img_IconGlow.color = frameColor;
                ___setter_name.underlayColor = frameColor;
            }
        }
    }

    [HarmonyPatch(typeof(UIPassiveSuccessionEquipBookSlot), "SetData")]
    public class UIPassiveSuccessionEquipBookSlot_SetData {
        public static void Postfix(UIPassiveSuccessionEquipBookSlot __instance, BookModel book) {
            BookXmlInfo? Book = (book != null) ? book.ClassInfo : null;
            if (Book != null && Book.Rarity.IsCustomRarity()) {
                Color frameColor = RarityXmlWrapper.GetRarityXmlInfo(Book.workshopID, Book.Rarity).FrameColor;
                __instance.SetRarityColor(frameColor);
            }
        }
    }

    [HarmonyPatch(typeof(UIPassiveSuccessionPreviewBookPanel), "SetColorByRarity")]
    public class UIPassiveSuccessionPreviewBookPanel_SetColorByRarity {
        public static void Postfix(UIPassiveSuccessionCenterEquipBookSlot __instance, Rarity rare, ref Image ___img_Frame, ref Image ___img_IconGlow, ref TextMeshProMaterialSetter ___setter_name) {
            BookModel currentbookmodel = __instance.CurrentBookModel;
            BookXmlInfo Book = (currentbookmodel != null) ? currentbookmodel.ClassInfo : null!;
            if (Book != null && Book.Rarity.IsCustomRarity()) {
                Color frameColor = RarityXmlWrapper.GetRarityXmlInfo(Book.workshopID, rare).FrameColor;
                ___img_Frame.color = frameColor;
                ___img_IconGlow.color = frameColor;
                ___setter_name.underlayColor = frameColor;
            }
        }
    }

    [HarmonyPatch(typeof(UISettingInvenEquipPageLeftSlot), "SetColorFrame")]
    public class UISettingInvenEquipPageLeftSlot_SetColorFrame {
        public static void Postfix(UISettingInvenEquipPageLeftSlot __instance, UIEquipPageSlotState type) {
            BookModel currentbookmodel = __instance.BookDataModel;
            BookXmlInfo? Book = (currentbookmodel != null) ? currentbookmodel.ClassInfo : null;
            if (Book != null && Book.Rarity.IsCustomRarity()) {
                Color frameColor = RarityXmlWrapper.GetRarityXmlInfo(Book.workshopID, Book.Rarity).FrameColor;
                __instance.Method("SetGlowColor", frameColor).Call();
            }
        }
    }
    #endregion

    #region 被动相关
    [HarmonyPatch(typeof(BattleUnitInformationPassiveSlot), "SetData")]
    public class BattleUnitInformationPassiveSlot_SetData {
        public static bool Prefix(BattleUnitInformationPassiveSlot __instance, PassiveAbilityBase passive) {
            PassiveXmlInfo Passive = Singleton<PassiveXmlList>.Instance.GetData(passive.id);

            if (Passive != null && Passive.rare.IsCustomRarity()) {
                var RarityInfo = RarityXmlWrapper.GetRarityXmlInfo(Passive.workshopID, Passive.rare);
                var frameColor = RarityInfo.FrameColor;
                var bookStatsIcon = UISpriteDataManager.instance.GetBookStatsIcon(UIBookStatsIconType.Passive);

                __instance.Rect.gameObject.SetActive(true);
                __instance.txt_PassiveDesc.text = Passive.name + " - " + Passive.desc;
                __instance.txt_PassiveDesc.gameObject.SetActive(false);
                __instance.txt_PassiveDesc.gameObject.SetActive(true);

                float preferredHeight = __instance.txt_PassiveDesc.preferredHeight;
                Vector2 sizeDelta = __instance.Rect.sizeDelta;
                sizeDelta.y = preferredHeight;
                __instance.Rect.sizeDelta = sizeDelta;

                if (__instance.img_Icon != null) {
                    var Icon = RarityInfo.GetArtwork(RarityArtworkType.PassiveIcon);
                    __instance.img_Icon.sprite = Icon ?? bookStatsIcon.icon;
                    __instance.img_Icon.color = frameColor;
                }

                if (__instance.img_IconGlow == null) {
                    __instance.img_IconGlow = __instance.Rect.Find("[Image]IconGlow").GetComponent<Image>();
                } else {
                    __instance.img_IconGlow.sprite = bookStatsIcon!.iconGlow;
                    __instance.img_IconGlow.color = frameColor;
                }

                __instance.img_Icon?.rectTransform.anchoredPosition = new Vector2(0, 5f);
                __instance.img_IconGlow.rectTransform.anchoredPosition = new Vector2(0, 5f);

                return false;
            }

            return true;
        }
    }

    [HarmonyPatch(typeof(UILibrarianEquipInfoSlot), "SetData")]
    public class UILibrarianEquipInfoSlot_SetData {
        public static void Postfix(UILibrarianEquipInfoSlot __instance, UIIconManager.IconSet iconSet, string name, string desc, BookPassiveInfo passive, bool noCost, ref Image ___Frame) {
            PassiveXmlInfo Passive = Singleton<PassiveXmlList>.Instance.GetData(__instance.Currentpassive.passive.id);
            if (Passive != null && Passive.rare.IsCustomRarity()) {
                Color frameColor = RarityXmlWrapper.GetRarityXmlInfo(Passive.workshopID, Passive.rare).FrameColor;
                ___Frame.color = frameColor;
                string txt = passive.passive.cost.ToString();
                if (passive.passive.cost > 99) txt = "?";
                __instance.txt_cost.text = txt;
                __instance.txt_cost.color = frameColor;
            }
        }
    }

    [HarmonyPatch(typeof(UIPassiveSuccessionCenterPassiveSlot), "SetData")]
    public class UIPassiveSuccessionCenterPassiveSlot_SetData {
        public static void Postfix(UIPassiveSuccessionCenterPassiveSlot __instance, PassiveModel passive) {
            PassiveXmlInfo Passive = Singleton<PassiveXmlList>.Instance.GetData(passive.reservedData.currentpassive.id);
            if (Passive != null && Passive.rare.IsCustomRarity()) {
                Color frameColor = RarityXmlWrapper.GetRarityXmlInfo(Passive.workshopID, Passive.rare)!.FrameColor;
                __instance.SetColorByRarity(frameColor);
            }
        }
    }

    [HarmonyPatch(typeof(UIPassiveSuccessionCenterPassiveSlot), "SetDataOriginBook")]
    public class UIPassiveSuccessionCenterPassiveSlot_SetDataOriginBook {
        public static void Postfix(UIPassiveSuccessionCenterPassiveSlot __instance, PassiveModel passive) {
            PassiveXmlInfo Passive = Singleton<PassiveXmlList>.Instance.GetData(passive.reservedData.currentpassive.id);
            if (Passive != null && Passive.rare.IsCustomRarity()) {
                Color frameColor = RarityXmlWrapper.GetRarityXmlInfo(Passive.workshopID, Passive.rare)!.FrameColor;
                __instance.SetColorByRarity(frameColor);
            }
        }
    }

    [HarmonyPatch(typeof(UIPassiveSuccessionPreviewPassiveSlot), "SetColorByRarity", [typeof(Rarity)])]
    public class UIPassiveSuccessionPreviewPassiveSlot_SetColorByRarity {
        public static void Postfix(UIPassiveSuccessionPreviewPassiveSlot __instance, Rarity rare, ref PassiveModel ___passivemodel, ref List<Graphic> ___graphics_Rarity) {
            var Passive = Singleton<PassiveXmlList>.Instance.GetData(___passivemodel.reservedData.currentpassive.id);
            if (Passive != null && Passive.rare.IsCustomRarity()) {
                Color frameColor = RarityXmlWrapper.GetRarityXmlInfo(Passive.workshopID, Passive.rare).FrameColor;
                foreach (Graphic graphic in ___graphics_Rarity) {
                    if (graphic != null) graphic.color = frameColor;
                }
            }
        }
    }

    [HarmonyPatch(typeof(UIPassiveSuccessionSlot), "SetDataModel")]
    public class UIPassiveSuccessionSlot_SetColorByRarity {
        public static void Postfix(UIPassiveSuccessionSlot __instance, PassiveModel passive, ref Color ___originalColor) {
            PassiveXmlInfo Passive = Singleton<PassiveXmlList>.Instance.GetData(passive.reservedData.currentpassive.id);
            if (Passive != null && Passive.rare.IsCustomRarity()) {
                Color frameColor = RarityXmlWrapper.GetRarityXmlInfo(Passive.workshopID, Passive.rare).FrameColor;

                ___originalColor = frameColor;
                __instance.SetColorByRarity(___originalColor);
            }
        }
    }
    #endregion

    #region 速度骰子
    [HarmonyPatch(typeof(SpeedDiceUI), "Init")]
    public class SpeedDiceUI_Init {
        public static void Postfix(SpeedDiceUI __instance, int index, int diceMin, int diceFace, Faction faction,
            ref Text ____txtSpeedRange, ref RawImage ____rouletteImg, TextMeshProUGUI ____txtSpeedMax, ref Image ___img_tensNum, ref Image ___img_unitsNum, ref Image ___img_breakedFrame,
            ref Graphic ___img_breakedLinearDodge, ref Image ___img_lockedFrame, ref Graphic ___img_lockedIcon, ref Image ___img_normalFrame, ref Image ___img_lightFrame,
            ref Image ___img_highlightFrame) {

            if (__instance.view.model.Book.ClassInfo is not BookXmlWrapper bookItem) return;
            if (string.IsNullOrEmpty(bookItem.SpeedDice)) return;
            if (!SpriteHelper.IsSpriteExist(bookItem.SpeedDice)) return;

            var baseSprite = SpriteHelper.GetData(bookItem.SpeedDice);
            var glowSprite = SpriteHelper.GetData(bookItem.SpeedDice + "_Glow");
            var hoverSprite = SpriteHelper.GetData(bookItem.SpeedDice + "_Hovered");
            var color = bookItem.SpeedDiceColor;

            ___img_normalFrame.sprite = baseSprite;
            ___img_lightFrame.sprite = glowSprite;
            ___img_highlightFrame.sprite = hoverSprite;

            ____txtSpeedRange.color = color;
            ____rouletteImg.color = color;
            ____txtSpeedMax.color = color;
            ___img_tensNum.color = color;
            ___img_unitsNum.color = color;

            color.a = Mathf.Max(0f, color.a - 0.6f);

            ___img_breakedFrame.color = color;
            ___img_breakedLinearDodge.color = color;
            ___img_lockedIcon.color = color;
            ___img_lockedFrame.color = color;
        }
    }
    #endregion
}