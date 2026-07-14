using HarmonyLib;
using static VersatileSpecialEventAPI.API;

namespace VersatileSpecialEventAPI.Utils;

public static class HarmonyTraverseExtensions {
    public static FieldAccessor Field(this object target, string name) => new(Traverse.Create(target).Field(name));
    public static MethodAccessor Method(this object target, string name, params object[] args) => new(Traverse.Create(target).Method(name, args));

    public static FieldAccessor Field(this Type type, string name) => new(Traverse.Create(type).Field(name));
    public static MethodAccessor Method(this Type type, string name, params object[] args) => new(Traverse.Create(type).Method(name, args));
}

public class FieldAccessor {
    private readonly Traverse _traverse;
    internal FieldAccessor(Traverse traverse) => _traverse = traverse;

    public T Get<T>() => _traverse.GetValue<T>();
    public void Set<T>(T value) => _traverse.SetValue(value);
    public FieldAccessor Field(string name) => new(_traverse.Field(name));
}

public class MethodAccessor {
    private readonly Traverse _traverse;
    internal MethodAccessor(Traverse traverse) => _traverse = traverse;
    public T GetDelegate<T>() => _traverse.GetValue<T>();
    public void Call() => _traverse.GetValue();
}

/// <summary>
/// 提供 Harmony 补丁注入与反射访问的辅助扩展方法。
/// <br/>
/// 旨在简化 Mod 开发中常见的批量补丁应用与非公开成员访问操作。
/// </summary>
public static class HarmonyHelper {
    /// <summary>
    /// 递归扫描指定类型 <typeparamref name="T"/> 及其所有嵌套类型，
    /// 自动应用所有标记了 <see cref="HarmonyPatch"/> 特性的类。
    /// </summary>
    /// <typeparam name="T">包含补丁定义的根类型（通常为 Mod 的主类或补丁容器类）。</typeparam>
    /// <param name="harmony">当前的 Harmony 实例。</param>
    /// <remarks>
    /// <para><b>设计意图</b>：Harmony 补丁常以私有嵌套类的形式组织在目标类内部(此方法通过递归确保所有嵌套补丁都能被正确发现和加载)。</para>
    /// <para><b>故障隔离</b>：方法内部捕获了所有异常并记录日志，防止单个补丁的格式错误或目标方法签名变更导致整个 Mod 崩溃。</para>
    /// </remarks>
    public static void HarmonyPatchAll<T>(this Harmony harmony) {
        // 局部函数：递归收集当前类型及所有嵌套层级中带有 [HarmonyPatch] 特性的类型
        void CollectHarmonyPatchTypes(Type type, List<Type> result) {
            if (Attribute.IsDefined(type, typeof(HarmonyPatch))) result.Add(type);
            foreach (var nested in type.GetNestedTypes(AccessTools.all)) CollectHarmonyPatchTypes(nested, result);
        }

        var patchTypes = new List<Type>();
        CollectHarmonyPatchTypes(typeof(T), patchTypes);

        if (patchTypes.Count == 0) {
            APILogger.LogInfo($"{ModId} 未找到任何 HarmonyPatch 类型");
            return;
        }

        try {
            foreach (var type in patchTypes) {
                APILogger.LogInfo($"{ModId} 正在应用 HarmonyPatch: {type.FullName}");
                harmony.CreateClassProcessor(type).Patch();
            }
        } catch (Exception ex) {
            APILogger.LogError($"[{ModId}] Harmony 注入失败。原因: {ex.Message}\n来源: {ex.Source}\nStackTrace: {ex.StackTrace}");
        }
    }

    /// <summary>
    /// 使用 Harmony <see cref="Traverse"/> 安全地获取指定类型的私有或受保护字段值。
    /// </summary>
    /// <typeparam name="T">期望返回的字段类型。</typeparam>
    /// <param name="field">字段名称。</param>
    /// <returns>字段的值；若未找到或类型不匹配，则返回 <typeparamref name="T"/> 的默认值。</returns>
    public static K GetField<T, K>(string field) => Traverse.Create(typeof(T)).Field(field).GetValue<K>();

    /// <summary>
    /// 使用 Harmony <see cref="Traverse"/> 安全地设置指定类型的私有或受保护字段值。
    /// </summary>
    /// <typeparam name="T">要设置的值的类型。</typeparam>
    /// <param name="field">字段名称。</param>
    /// <param name="value">要赋予的新值。</param>
    public static void SetField<T, K>(string field, K value) => Traverse.Create(typeof(T)).Field(field).SetValue(value);

    /// <summary>
    /// 使用 Harmony <see cref="Traverse"/> 获取指定类型的方法（通常用于获取方法委托以便后续调用）。
    /// </summary>
    /// <typeparam name="T">期望返回的委托类型 (如 <see cref="Func{TResult}"/> 或 <see cref="Action"/>)。</typeparam>
    /// <param name="method">方法名称。</param>
    /// <returns>方法的委托表示；若未找到，则返回默认值。</returns>
    public static K GetMethod<T, K>(string method) => Traverse.Create(typeof(T)).Method(method).GetValue<K>();

    /// <summary>
    /// 使用 Harmony <see cref="Traverse"/> 替换或设置指定类型的方法实现（常用于 Detour 或方法劫持）。
    /// </summary>
    /// <typeparam name="T">新方法的委托类型。</typeparam>
    /// <param name="method">目标方法名称。</param>
    /// <param name="value">用于替换的新方法委托。</param>
    public static K SetMethod<T, K, S>(string method, S value) => Traverse.Create(typeof(T)).Method(method).SetValue(value).GetValue<K>();
}