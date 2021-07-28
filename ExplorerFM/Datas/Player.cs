using System.Collections.Generic;

namespace ExplorerFM.Datas
{
    public class Player : Staff
    {
        public Dictionary<Side, int?> Sides { get; set; }
        public Dictionary<Position, int?> Positions { get; set; }
        public Dictionary<Attribute, int?> Attributes { get; set; }
        public int? LeftFoot { get; set; }
        public int? RightFoot { get; set; }
        public int? SquadNumber { get; set; }
    }
}
