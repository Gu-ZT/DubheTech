using HarmonyLib;
using Verse;

namespace DubheTech;

/// <summary>
/// 天枢科技 Mod 入口：负责装载 Harmony 补丁并完成全局初始化。
/// </summary>
public class DubheTechMod : Mod
{
    /// <summary>
    /// Mod 的唯一标识，与 About/About.xml 中的 packageId 保持一致。
    /// </summary>
    public const string Id = "dubhe.dubhetech";

    /// <summary>
    /// 游戏加载 Mod 程序集时调用：应用本程序集中的全部 Harmony 补丁并输出加载日志。
    /// </summary>
    /// <param name="content">RimWorld 传入的 Mod 内容包。</param>
    public DubheTechMod(ModContentPack content) : base(content)
    {
        new Harmony(Id).PatchAll();
        var version = typeof(DubheTechMod).Assembly.GetName().Version;
        Log.Message($"[DubheTech] 天枢科技已加载，程序集版本 {version}");
    }
}
