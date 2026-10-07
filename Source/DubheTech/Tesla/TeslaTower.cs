using System;
using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace DubheTech.Tesla;

/// <summary>
/// 特斯拉塔：通电后周期扫描攻击范围内允许类别的目标，以闪电攻击最近的可见目标。
/// 开火时充能半径内其他通电的特斯拉塔会向本塔充能以增强伤害；充能塔自身还会获得其
/// 邻近塔的充能加成。充能与攻击均以塔头到目标的闪电特效呈现，线宽随充能等级增加。
/// </summary>
public class TeslaTower : Building
{
    private TeslaTargetCategory filter = TeslaTargetCategories.Default;
    private int cooldownLeft;
    private int scanLeft;
    private CompPowerTrader powerComp;

    private TeslaTowerSettings Settings => def.GetModExtension<TeslaTowerSettings>();

    public override void SpawnSetup(Map map, bool respawningAfterLoad)
    {
        base.SpawnSetup(map, respawningAfterLoad);
        powerComp = GetComp<CompPowerTrader>();
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref filter, "filter", TeslaTargetCategories.Default);
        Scribe_Values.Look(ref cooldownLeft, "cooldownLeft", 0);
    }

    protected override void Tick()
    {
        base.Tick();
        if (!powerComp.PowerOn)
        {
            return;
        }
        if (cooldownLeft > 0)
        {
            cooldownLeft--;
            return;
        }
        if (--scanLeft > 0)
        {
            return;
        }
        scanLeft = Settings.scanIntervalTicks;
        Pawn target = FindTarget();
        if (target != null)
        {
            Fire(target);
        }
    }

    /// <summary>
    /// 扫描攻击范围内允许类别、未倒地且对塔可见的目标，取距离最近者；没有合法目标时返回 null。
    /// </summary>
    private Pawn FindTarget()
    {
        TeslaTowerSettings settings = Settings;
        Pawn best = null;
        float bestDistSq = settings.attackRange * settings.attackRange;
        foreach (Pawn pawn in Map.mapPawns.AllPawnsSpawned)
        {
            if (pawn.Dead || pawn.Downed)
            {
                continue;
            }
            TeslaTargetCategory? category = TeslaTargetCategories.Of(pawn, Faction);
            if (category == null || (filter & category.Value) == TeslaTargetCategory.None)
            {
                continue;
            }
            float distSq = (pawn.Position - Position).LengthHorizontalSquared;
            if (distSq > bestDistSq)
            {
                continue;
            }
            if (!GenSight.LineOfSight(Position, pawn.Position, Map, skipFirstCell: true))
            {
                continue;
            }
            best = pawn;
            bestDistSq = distSq;
        }
        return best;
    }

    private void Fire(Pawn target)
    {
        TeslaTowerSettings settings = Settings;
        TeslaBoltRenderer renderer = Map.GetComponent<TeslaBoltRenderer>();
        Vector3 head = HeadPosition();
        int chargeLevel = 0;
        foreach (TeslaTower supporter in FindSupporters(settings))
        {
            // 充能塔的充能等级 = 1（自身放电）+ 其邻近通电塔数；攻击塔不计入，避免两塔互充循环
            int supporterCharge = 1 + supporter.CountNearbyTowers(settings, exclude: this);
            chargeLevel += supporterCharge;
            renderer.AddBolt(supporter.HeadPosition(), head, supporterCharge);
        }
        float damage = settings.baseDamage + settings.damagePerCharge * chargeLevel;
        target.TakeDamage(new DamageInfo(DubheTechDefOf.DubheTech_TeslaShock, damage, 0f, -1f, this, null, null,
            DamageInfo.SourceCategory.ThingOrUnknown, target));
        renderer.AddBolt(head, target.DrawPos, chargeLevel);
        SoundDefOf.Thunder_OnMap.PlayOneShot(SoundInfo.InMap(new TargetInfo(Position, Map)));
        cooldownLeft = settings.cooldownTicks;
    }

    /// <summary>枚举充能半径内为本塔充能的其他通电特斯拉塔。</summary>
    private IEnumerable<TeslaTower> FindSupporters(TeslaTowerSettings settings)
    {
        foreach (TeslaTower tower in TowersOnMap())
        {
            if (tower != this && tower.Powered() && WithinChargeRange(tower.Position, Position, settings.chargeRadius))
            {
                yield return tower;
            }
        }
    }

    /// <summary>充能半径内除本塔与 exclude 外的通电特斯拉塔数量。</summary>
    private int CountNearbyTowers(TeslaTowerSettings settings, TeslaTower exclude)
    {
        int count = 0;
        foreach (TeslaTower tower in TowersOnMap())
        {
            if (tower != this && tower != exclude && tower.Powered()
                && WithinChargeRange(tower.Position, Position, settings.chargeRadius))
            {
                count++;
            }
        }
        return count;
    }

    private IEnumerable<TeslaTower> TowersOnMap()
    {
        foreach (Thing thing in Map.listerThings.ThingsOfDef(DubheTechDefOf.DubheTech_TeslaTower))
        {
            if (thing.Spawned)
            {
                yield return (TeslaTower)thing;
            }
        }
    }

    private bool Powered() => powerComp.PowerOn;

    /// <summary>充能协作为直线互联，距离与接闪杆的覆盖判定一致使用切比雪夫距离。</summary>
    private static bool WithinChargeRange(IntVec3 a, IntVec3 b, int radius)
    {
        int dist = Math.Max(Math.Abs(a.x - b.x), Math.Abs(a.z - b.z));
        return dist <= radius;
    }

    /// <summary>塔头放电点的世界坐标；y 分量无效，绘制高度由 <see cref="TeslaBoltRenderer"/> 统一。</summary>
    private Vector3 HeadPosition()
    {
        Vector3 center = DrawPos;
        return new Vector3(center.x, 0f, center.z + Settings.headHeight);
    }

    public bool IsCategoryEnabled(TeslaTargetCategory category) => (filter & category) != TeslaTargetCategory.None;

    public void SetCategoryEnabled(TeslaTargetCategory category, bool enabled)
    {
        filter = enabled ? filter | category : filter & ~category;
    }

    public override IEnumerable<Gizmo> GetGizmos()
    {
        foreach (Gizmo gizmo in base.GetGizmos())
        {
            yield return gizmo;
        }
        yield return new Command_Action
        {
            defaultLabel = "DubheTech_TeslaFilterGizmo".Translate(),
            defaultDesc = "DubheTech_TeslaFilterGizmoDesc".Translate(),
            icon = (Texture2D)Graphic.MatSingle.mainTexture,
            action = () => Find.WindowStack.Add(new DialogTeslaTargetFilter(this))
        };
    }
}
