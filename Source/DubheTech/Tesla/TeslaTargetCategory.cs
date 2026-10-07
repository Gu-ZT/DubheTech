using System;
using RimWorld;
using Verse;

namespace DubheTech.Tesla;

/// <summary>
/// 特斯拉塔可攻击目标的类别。筛选清单按类别勾选，未勾选类别的目标不会被攻击；
/// 敌对与否以目标是否敌对本塔所属派系为准。
/// </summary>
[Flags]
public enum TeslaTargetCategory
{
    None = 0,
    HostileCharacter = 1,
    HostileMechanoid = 2,
    HostileAnimal = 4,
    WildAnimal = 8,
    NonHostileCharacter = 16,
}

public static class TeslaTargetCategories
{
    /// <summary>筛选清单的默认勾选：敌对角色、敌对机械体与敌对生物。</summary>
    public const TeslaTargetCategory Default =
        TeslaTargetCategory.HostileCharacter | TeslaTargetCategory.HostileMechanoid | TeslaTargetCategory.HostileAnimal;

    /// <summary>
    /// 将小人归入筛选清单的类别；不属于任何类别（非敌对的机械体、殖民地的动物等）时返回 null。
    /// </summary>
    public static TeslaTargetCategory? Of(Pawn pawn, Faction faction)
    {
        RaceProperties race = pawn.RaceProps;
        bool hostile = pawn.HostileTo(faction);
        if (race.IsMechanoid)
        {
            return hostile ? TeslaTargetCategory.HostileMechanoid : null;
        }
        if (race.Humanlike)
        {
            return hostile ? TeslaTargetCategory.HostileCharacter : TeslaTargetCategory.NonHostileCharacter;
        }
        if (race.Animal)
        {
            if (hostile)
            {
                return TeslaTargetCategory.HostileAnimal;
            }
            if (pawn.Faction == null)
            {
                return TeslaTargetCategory.WildAnimal;
            }
        }
        return null;
    }
}
