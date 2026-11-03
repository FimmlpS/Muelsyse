using BaseLib.Abstracts;

namespace Muelsyse.Scripts.Powers;

public abstract class AbstractMuelsysePower : CustomPowerModel
{
    public override string? CustomPackedIconPath => $"res://Muelsyse/images/powers/{replaceID(Id.Entry.ToLowerInvariant())}.png";
    public override string? CustomBigIconPath => $"res://Muelsyse/images/powers/{replaceID(Id.Entry.ToLowerInvariant())}.png";

    public string replaceID(string path) => path.Replace("muelsyse-", "");
}
