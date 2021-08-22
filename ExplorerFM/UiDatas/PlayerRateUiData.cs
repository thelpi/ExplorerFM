using System.Windows.Media;
using ExplorerFM.Datas;

namespace ExplorerFM.UiDatas
{
    public class PlayerRateUiData
    {
        public Player Player { get; set; }
        public int PositionRate { get; set; }
        public int SideRate { get; set; }
        public Position Position { get; set; }
        public Side Side { get; set; }
        public int Rate { get; set; }
        public int MaxTheoreticalRate { get; set; }

        public Brush Brush => new SolidColorBrush(GetColorFromRate());

        private Color GetColorFromRate()
        {
            var switchStop = MaxTheoreticalRate / (decimal)3;

            var blue = Rate > switchStop
                ? 0
                : 255 - (Rate / switchStop * 255);

            var green = Rate <= switchStop
                ? 255
                : 255 - ((Rate - switchStop) / (switchStop * 2) * 255);

            return Color.FromArgb(byte.MaxValue, byte.MaxValue, (byte)green, (byte)blue);
        }
    }
}
