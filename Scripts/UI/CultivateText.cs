using Muelsyse.Scripts.Mechanics.Cultivate;

namespace Muelsyse.Scripts.UI;

public static class CultivateText
{
    public static string Name(CultivateAim aim) => aim switch
    {
        CultivateAim.Attack => "攻击值栽培",
        CultivateAim.Block => "格挡值栽培",
        CultivateAim.Cost => "耗能栽培",
        CultivateAim.Draw => "抽牌栽培",
        CultivateAim.Energy => "能量栽培",
        CultivateAim.Water => "润化栽培",
        CultivateAim.Spread => "花粉栽培：弥散",
        CultivateAim.Parasitism => "花粉栽培：寄生",
        CultivateAim.Cure => "花粉栽培：疗愈",
        CultivateAim.WaterGrow => "花粉栽培：水培",
        _ => "栽培"
    };

    public static string Tooltip(string icon, int amount)
    {
        CultivateAim aim = icon switch
        {
            "attack" => CultivateAim.Attack,
            "block" => CultivateAim.Block,
            "draw" => CultivateAim.Draw,
            "energy" => CultivateAim.Energy,
            "water" => CultivateAim.Water,
            "spread" => CultivateAim.Spread,
            "parasitism" => CultivateAim.Parasitism,
            "cure" => CultivateAim.Cure,
            "water2" => CultivateAim.WaterGrow,
            _ => CultivateAim.Attack
        };

        return $"{Name(aim)} {amount}";
    }
}
