using Verse;

namespace DubheTech;

/// <summary>
/// 天枢科技的显示开关。原版开关行由 PlaySettings 持有状态而没有可挂载的外部扩展点，
/// 故本类自身承担状态保存；开关只改变界面呈现，不影响输电链路与电网拓扑。
/// </summary>
public class DubheTechSettings : GameComponent
{
    /// <summary>是否绘制输电杆之间的电线。</summary>
    public bool showPoleWires = true;

    public DubheTechSettings(Game game)
    {
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref showPoleWires, "showPoleWires", defaultValue: true);
    }
}
