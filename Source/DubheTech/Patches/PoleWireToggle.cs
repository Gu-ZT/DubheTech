using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace DubheTech.Patches;

/// <summary>
/// 状态栏开关补丁：在右下角原版开关行中追加输电杆电线的显示开关。
/// 必须补丁 DoPlaySettingsGlobalControls 的原因：该行由 PlaySettings 的私有方法逐项绘制，
/// 且 GlobalControls 不持有可插入控件的扩展点，经反编译确认没有其他公共入口能追加开关。
/// </summary>
[StaticConstructorOnStartup]
[HarmonyPatch(typeof(PlaySettings), nameof(PlaySettings.DoPlaySettingsGlobalControls))]
public static class PoleWireToggle
{
    private static readonly Texture2D Icon = ContentFinder<Texture2D>.Get("UI/Buttons/DubheTech_ShowPoleWires");

    private static void Postfix(WidgetRow row, bool worldView)
    {
        if (worldView)
        {
            return;
        }
        DubheTechSettings settings = Current.Game.GetComponent<DubheTechSettings>();
        row.ToggleableIcon(ref settings.showPoleWires, Icon, "DubheTech_ShowPoleWiresToggle".Translate(), SoundDefOf.Mouseover_ButtonToggle);
    }
}
