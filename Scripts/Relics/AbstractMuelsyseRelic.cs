using BaseLib.Abstracts;

namespace Muelsyse.Scripts.Relics;

public abstract class AbstractMuelsyseRelic : CustomRelicModel
{
    public override string PackedIconPath => $"res://Muelsyse/images/relics/{replaceID(Id.Entry.ToLowerInvariant())}.png";
    protected override string PackedIconOutlinePath => $"res://Muelsyse/images/relics/{replaceID(Id.Entry.ToLowerInvariant())}.png";
    protected override string BigIconPath => $"res://Muelsyse/images/relics/{replaceID(Id.Entry.ToLowerInvariant())}.png";

    public string replaceID(string path) => path.Replace("muelsyse-", "");
}
