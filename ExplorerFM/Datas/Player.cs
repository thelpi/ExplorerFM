using System.Collections.Generic;
using System.Linq;
using ExplorerFM.FieldsAttributes;

namespace ExplorerFM.Datas
{
    public class Player : Staff
    {
        [Field("Sides", 1, 20, false)]
        public Dictionary<Side, int?> Sides { get; set; }
        [Field("Positions", 1, 20, false)]
        public Dictionary<Position, int?> Positions { get; set; }
        [Field("Attributes", 1, 20, false)]
        public Dictionary<Attribute, int?> Attributes { get; set; }

        [Field("LeftFoot", 1, 20)]
        public int? LeftFoot { get; set; }
        [Field("RightFoot", 1, 20)]
        public int? RightFoot { get; set; }
        [Field("SquadNumber", 0, 999)]
        public int? SquadNumber { get; set; }

        [AggregateField("(SELECT SUM(IFNULL(player_attribute.rate, {0})) FROM player_attribute WHERE player_attribute.player_ID = player.ID)", 1, 1000)]
        public int AttributesTotal => GetAttributesTotal();

        // the identifier of attribute_type is hardcoded here
        [AggregateField("(SELECT SUM(IFNULL(player_attribute.rate, {0})) FROM player_attribute WHERE player_attribute.player_ID = player.ID AND player_attribute.attribute_ID IN (SELECT attribute.ID FROM attribute where attribute.type_ID = 1))", 1, 1000)]
        public int AttributesGoalkeeperTotal => GetAttributesTotal(AttributeType.Goalkeeper);
        [AggregateField("(SELECT SUM(IFNULL(player_attribute.rate, {0})) FROM player_attribute WHERE player_attribute.player_ID = player.ID AND player_attribute.attribute_ID IN (SELECT attribute.ID FROM attribute where attribute.type_ID = 2))", 1, 1000)]
        public int AttributesKickoffTotal => GetAttributesTotal(AttributeType.Kickoff);
        [AggregateField("(SELECT SUM(IFNULL(player_attribute.rate, {0})) FROM player_attribute WHERE player_attribute.player_ID = player.ID AND player_attribute.attribute_ID IN (SELECT attribute.ID FROM attribute where attribute.type_ID = 3))", 1, 1000)]
        public int AttributesPhysicalTotal => GetAttributesTotal(AttributeType.Physical);
        [AggregateField("(SELECT SUM(IFNULL(player_attribute.rate, {0})) FROM player_attribute WHERE player_attribute.player_ID = player.ID AND player_attribute.attribute_ID IN (SELECT attribute.ID FROM attribute where attribute.type_ID = 4))", 1, 1000)]
        public int AttributesPsychologicalTotal => GetAttributesTotal(AttributeType.Psychological);
        [AggregateField("(SELECT SUM(IFNULL(player_attribute.rate, {0})) FROM player_attribute WHERE player_attribute.player_ID = player.ID AND player_attribute.attribute_ID IN (SELECT attribute.ID FROM attribute where attribute.type_ID = 5))", 1, 1000)]
        public int AttributesTechnicalTotal => GetAttributesTotal(AttributeType.Technical);

        public int GetPositionSideRate(Position p, Side s)
        {
            var sNote = p == Position.GoalKeeper ? 20 : (Sides.ContainsKey(s) ? Sides[s].GetValueOrDefault(1) : 1);
            var pNote = Positions.ContainsKey(p) ? Positions[p].GetValueOrDefault(1) : 1;
            return System.Math.Min(sNote, pNote);
        }

        private int GetAttributesTotal(AttributeType? type = null)
        {
            return Attributes
                .Where(_ => !type.HasValue || type == _.Key.Type)
                .Sum(_ => _.Value ?? 0);
        }
    }
}
