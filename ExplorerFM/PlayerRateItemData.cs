using System.Windows.Media;
using ExplorerFM.Datas;

namespace ExplorerFM
{
    public class PlayerRateItemData
    {
        public Player Player { get; set; }
        public int PositionRate { get; set; }
        public int SideRate { get; set; }
        public int Rate { get; set; }
        public int AttributesCount { private get; set; }

        public Brush Brush => new SolidColorBrush(
            GuiExtensions.GetColorFromRate(Rate, AttributesCount * 20));
    }
}
