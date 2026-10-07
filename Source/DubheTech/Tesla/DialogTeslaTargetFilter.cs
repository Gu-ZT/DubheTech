using UnityEngine;
using Verse;

namespace DubheTech.Tesla;

/// <summary>
/// 特斯拉塔的目标筛选清单窗口：勾选允许攻击的目标类别，改动立即生效并随存档保存。
/// </summary>
public class DialogTeslaTargetFilter : Window
{
    private static readonly TeslaTargetCategory[] Categories =
    {
        TeslaTargetCategory.HostileCharacter,
        TeslaTargetCategory.HostileMechanoid,
        TeslaTargetCategory.HostileAnimal,
        TeslaTargetCategory.WildAnimal,
        TeslaTargetCategory.NonHostileCharacter,
    };

    private readonly TeslaTower tower;

    public DialogTeslaTargetFilter(TeslaTower tower)
    {
        this.tower = tower;
        doCloseX = true;
        closeOnClickedOutside = true;
        absorbInputAroundWindow = false;
    }

    public override Vector2 InitialSize => new(420f, 340f);

    public override void DoWindowContents(Rect inRect)
    {
        Listing_Standard listing = new();
        listing.Begin(inRect);
        Text.Font = GameFont.Medium;
        listing.Label("DubheTech_TeslaFilterTitle".Translate());
        Text.Font = GameFont.Small;
        listing.GapLine();
        foreach (TeslaTargetCategory category in Categories)
        {
            bool enabled = tower.IsCategoryEnabled(category);
            listing.CheckboxLabeled(("DubheTech_TeslaFilter_" + category).Translate(), ref enabled);
            if (enabled != tower.IsCategoryEnabled(category))
            {
                tower.SetCategoryEnabled(category, enabled);
            }
        }
        listing.Gap();
        Text.Font = GameFont.Tiny;
        listing.Label("DubheTech_TeslaFilterNote".Translate());
        Text.Font = GameFont.Small;
        listing.End();
    }
}
