using HarmonyLib;
using LOR_XML;
using Mod;
using VersatileSpecialEventAPI.CustomEmotionUtil;
using VersatileSpecialEventAPI.CustomRarityUtil;
using VersatileSpecialEventAPI.CustomRarityUtil.XML.Wrapper;
using VersatileSpecialEventAPI.LocalizationUtil;
using VersatileSpecialEventAPI.Utils;

namespace VersatileSpecialEventAPI;

public enum FileType { ArtWork, Audio, Asset }

public class API {
    public static string ModId = string.Empty;
    public static string APIPath = string.Empty;
    public static string Language = GlobalGameManager.Instance.CurrentOption.language;
    public static ModLogger APILogger = null!;

    private static void RemoveError() {
        var errorLogs = Singleton<ModContentManager>.Instance.GetErrorLogs();
        var FilterKeywords = new List<string>();
        FilterKeywords.Add(
            "0Harmony", "VersatileSpecialEventAPI",
            "Mono.Cecil", "MonoMod.Common", "MonoMod.RuntimeDetour", "MonoMod.Utils",
            "NAudio"
        );
        errorLogs.RemoveAll(log => FilterKeywords.Any(keyword => log.Contains(keyword)));
    }

    public static void Init(string Id, Harmony harmony) {
        RemoveError();
        ModId = Id;
        APIPath = Singleton<ModContentManager>.Instance.GetModPath(ModId);
        APILogger = new ModLogger("VersatileSpecialEventAPI", APIPath);

        harmony.HarmonyPatchAll<MainPatchs>();
        harmony.HarmonyPatchAll<RarityPatchs>();
        harmony.HarmonyPatchAll<EmotionPatchs>();
    }

    public static void LoadData(string path = "") {
        var DataPath = Path.Combine(APIPath, path);
        RarityXmlWrapper.AddRarityByFile(ModId, DataPath, "RarityList.xml");
        PassiveXmlWrapper.AddPassiveByFile(ModId, DataPath, "PassiveList.xml");
        BookXmlWrapper.AddBookByFile(ModId, DataPath, "BookEnemyList.xml");
        BookXmlWrapper.AddBookByFile(ModId, DataPath, "BookLibrarianList.xml");
        DiceCardXmlWrapper.AddDiceByFile(ModId, DataPath, "DiceCardList.xml");
    }
    public static void LoadResource(FileType type, string path = "") {
        var ResourcePath = Path.Combine(APIPath, "Resource", path);
        var directorie = new DirectoryInfo(ResourcePath);
        if (!directorie.Exists) {
            APILogger.LogInfo($"AddResource 未找到资源目录: {directorie.Name}");
            return;
        }

        var Files = directorie.GetFiles();
        if (Files.Length == 0) {
            APILogger.LogInfo("AddResource 未找到资源文件");
            return;
        }

        DirectoryInfo[] directories = directorie.GetDirectories();
        for (int i = 0; i < directories.Length; i++) LoadResource(type, directories[i].Name);

        foreach (FileInfo fileInfo in Files) {
            if (!fileInfo.Exists) continue;

            var FileName = Path.GetFileNameWithoutExtension(fileInfo.Name);
            var FilePath = fileInfo.FullName;

            switch (type) {
                case FileType.ArtWork: SpriteHelper.AddSpriteByFile(FilePath, FileName); break;
                case FileType.Audio: AudioHelper.AddAudioByFile(FilePath, FileName); break;
                case FileType.Asset: AssetBundleHelper.AddAssetBundleByFile(FilePath, FileName); break;
            }
        }
    }
    public static void LoadLocalize(string path = "") {
        string basePath = Path.Combine(APIPath, path, Language);

        if (!Directory.Exists(basePath)) basePath = Path.Combine(APIPath, path, "cn");
        if (!Directory.Exists(basePath)) Directory.CreateDirectory(basePath);

        // ================= 效果文本加载 =================
        XmlHelper.Merge<BattleEffectTextRoot>(basePath, "BattleEffectText", root => {
            var Instance = Singleton<BattleEffectTextsXmlList>.Instance;

            foreach (var text in root.effectTextList) {
                var EffectDict = HarmonyHelper.GetField<BattleEffectTextsXmlList, Dictionary<string, BattleEffectText>>("_dictionary");
                EffectDict[text.ID] = text;
            }
        });

        // ================= 战斗书页描述加载 =================
        XmlHelper.Merge<BattleCardDescRoot>(basePath, "BattleCardDesc", root => {
            var descDict = root.cardDescList.ToDictionary(x => x.cardID);
            var workshopCards = ItemXmlDataList.instance.GetAllWorkshopData()[ModId];
            var allCards = ItemXmlDataList.instance.GetCardList().FindAll(x => x.id.packageId == ModId);

            foreach (var card in workshopCards) LocalizationManager.GetName(card, descDict);
            foreach (var card in allCards) LocalizationManager.GetName(card, descDict);
        });

        // ================= 卡牌能力描述加载 =================
        XmlHelper.Merge<BattleCardAbilityDescRoot>(basePath, "BattleCardAbilityDesc", root => {
            foreach (var desc in root.cardDescList) {
                var data = Singleton<BattleCardAbilityDescXmlList>.Instance.GetData(desc.id);
                if (data != null) data.desc = desc.desc;
            }
        });


        // ================= 情感书页描述加载 =================
        XmlHelper.Merge<AbnormalityCardsRoot>(basePath, "AbnormalityCards", root => {
            var Instance = Singleton<AbnormalityCardDescXmlList>.Instance;
            var AbnormalityCardList = Instance.Field("_dictionary").Get<Dictionary<string, AbnormalityCard>>();
            foreach (var abnormalityCard in root.sephirahList.SelectMany((Sephirah x) => x.list)) {
                AbnormalityCardList[abnormalityCard.id] = abnormalityCard;
            }
        });

        // ================= 情感书页能力文本 =================
        XmlHelper.Merge<AbnormalityAbilityRoot>(basePath, "AbnormalityAbility", root => {
            var Instance = Singleton<AbnormalityAbilityTextXmlList>.Instance;
            var AbnormalityAbilityList = Instance.Field("_dictionary").Get<Dictionary<string, AbnormalityAbilityText>>();
            foreach (var abnormalityAbility in root.abnormalityList) {
                AbnormalityAbilityList[abnormalityAbility.id] = abnormalityAbility;
            }
        });


        // ================= 角色名称加载 =================
        XmlHelper.Merge<CharactersNameRoot>(basePath, "CharactersName", root => {
            var nameDict = root.nameList.ToDictionary(x => x.ID);
            var workshopEnemies = Singleton<EnemyUnitClassInfoList>.Instance.GetAllWorkshopData()[ModId];

            foreach (var enemy in workshopEnemies) {
                if (nameDict.TryGetValue(enemy.id.id, out var nameData)) {
                    enemy.name = nameData.name;
                    Singleton<EnemyUnitClassInfoList>.Instance.GetData(enemy.id).name = enemy.name;
                }
            }
        });


        // ================= 被动技能描述加载 =================
        XmlHelper.Merge<PassiveDescRoot>(basePath, "PassiveDesc", root => {
            var descDict = root.descList.ToDictionary(x => x.ID);
            var passives = Singleton<PassiveXmlList>.Instance.GetDataAll().FindAll(x => x.id.packageId == ModId);

            foreach (var passive in passives) {
                if (descDict.TryGetValue(passive.id, out var desc)) {
                    passive.name = desc.name;
                    passive.desc = desc.desc;
                }
            }
        });


        // ================= 书籍描述加载 =================
        XmlHelper.Merge<BookDescRoot>(basePath, "BookDesc", root => {
            var descDict = root.bookDescList.ToDictionary(x => x.bookID);
            var workshopBooks = Singleton<BookXmlList>.Instance.GetAllWorkshopData()[ModId];
            var allBooks = Singleton<BookXmlList>.Instance.GetList().FindAll(x => x.id.packageId == ModId);

            foreach (var book in workshopBooks) LocalizationManager.GetName(book, descDict);
            foreach (var book in allBooks) LocalizationManager.GetName(book, descDict);

            var workshopDict = HarmonyHelper.GetField<BookXmlList, Dictionary<string, List<BookDesc>>>("_dictionaryWorkshop");
            workshopDict[ModId] = root.bookDescList;
        });

        // ================= 舞台名称加载 =================
        XmlHelper.Merge<CharactersNameRoot>(basePath, "StageName", root => {
            var nameDict = root.nameList.ToDictionary(x => x.ID);
            var workshopStages = Singleton<StageClassInfoList>.Instance.GetAllWorkshopData()[ModId];

            foreach (var stage in workshopStages) {
                if (nameDict.TryGetValue(stage.id.id, out var nameData)) stage.stageName = nameData.name;
            }
        });


        // ================= 其他通用文本加载 =================
        XmlHelper.Merge<TextXmlRoot>(basePath, "Text", root => {
            var Dict = LocalizationManager.LocalizeDict;
            if (!Dict.ContainsKey(ModId)) Dict[ModId] = new Dictionary<string, string>();
            foreach (var info in root.TextXmlList) Dict[ModId][info.Name] = info.Text;
        });
    }
}