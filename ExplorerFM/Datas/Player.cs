using System;
using System.Collections.Generic;
using System.Linq;
using ExplorerFM.Extensions;
using ExplorerFM.FieldsAttributes;

namespace ExplorerFM.Datas
{
    public class Player : Staff
    {
        public const int PositioningTolerance = 15;
        
        [NestedSelectorField("SELECT player_side.rate FROM player_side WHERE player_side.side_ID = {0} AND player_side.player_ID = player.ID", 1, 20, typeof(Side))]
        public Dictionary<Side, int?> Sides { get; set; }

        [GridView("Best pos.", 2.5, typeof(Converters.PositioningDisplayConverter), true, true)]
        [GridView("Altern. pos.", 2.6, typeof(Converters.PositioningDisplayConverter), true, false)]
        [NestedSelectorField("SELECT player_position.rate FROM player_position WHERE player_position.position_ID = {0} AND player_position.player_ID = player.ID", 1, 20, typeof(Position))]
        public Dictionary<Position, int?> Positions { get; set; }

        [NestedSelectorField("SELECT player_attribute.rate FROM player_attribute WHERE player_attribute.attribute_ID = {0} AND player_attribute.player_ID = player.ID", 1, 20, nameof(DataProvider.Attributes), nameof(Attribute.Name))]
        public Dictionary<Attribute, int?> Attributes { get; set; }

        [Field("LeftFoot", 1, 20)]
        public int? LeftFoot { get; set; }

        [Field("RightFoot", 1, 20)]
        public int? RightFoot { get; set; }

        [Field("SquadNumber", 0, 999)]
        public int? SquadNumber { get; set; }

        public bool Loaded { get; set; }

        [GridView("Tot. attr.", 12.5)]
        [AggregateField("SELECT SUM(IFNULL(player_attribute.rate, {0})) FROM player_attribute WHERE player_attribute.player_ID = player.ID", 1, 1000)]
        public int AttributesTotal => GetAttributesTotal();

        // the identifier of attribute_type is hardcoded here
        [AggregateField("SELECT SUM(IFNULL(player_attribute.rate, {0})) FROM player_attribute WHERE player_attribute.player_ID = player.ID AND player_attribute.attribute_ID IN (SELECT attribute.ID FROM attribute where attribute.type_ID = 1)", 1, 1000)]
        public int AttributesGoalkeeperTotal => GetAttributesTotal(AttributeType.Goalkeeper);

        [AggregateField("SELECT SUM(IFNULL(player_attribute.rate, {0})) FROM player_attribute WHERE player_attribute.player_ID = player.ID AND player_attribute.attribute_ID IN (SELECT attribute.ID FROM attribute where attribute.type_ID = 2)", 1, 1000)]
        public int AttributesKickoffTotal => GetAttributesTotal(AttributeType.Technical);

        [AggregateField("SELECT SUM(IFNULL(player_attribute.rate, {0})) FROM player_attribute WHERE player_attribute.player_ID = player.ID AND player_attribute.attribute_ID IN (SELECT attribute.ID FROM attribute where attribute.type_ID = 3)", 1, 1000)]
        public int AttributesPhysicalTotal => GetAttributesTotal(AttributeType.Physical);

        [AggregateField("SELECT SUM(IFNULL(player_attribute.rate, {0})) FROM player_attribute WHERE player_attribute.player_ID = player.ID AND player_attribute.attribute_ID IN (SELECT attribute.ID FROM attribute where attribute.type_ID = 4)", 1, 1000)]
        public int AttributesPsychologicalTotal => GetAttributesTotal(AttributeType.Psychological);

        [AggregateField("SELECT SUM(IFNULL(player_attribute.rate, {0})) FROM player_attribute WHERE player_attribute.player_ID = player.ID AND player_attribute.attribute_ID IN (SELECT attribute.ID FROM attribute where attribute.type_ID = 5)", 1, 1000)]
        public int AttributesTechnicalTotal => GetAttributesTotal(AttributeType.Technical);

        public int GetPositionSideRate(Position p, Side s)
        {
            var sNote = p == Position.GoalKeeper
                ? 20
                : (Sides.ContainsKey(s)
                    ? Sides[s].GetValueOrDefault(1)
                    : 1);
            var pNote = Positions.ContainsKey(p)
                ? Positions[p].GetValueOrDefault(1)
                : 1;
            return Math.Min(sNote, pNote);
        }

        public int GetAttributesTotal(AttributeType? type = null, NullRateBehavior nullRateBehavior = NullRateBehavior.Minimal)
        {
            var attributesToConsider = Attributes
                .Where(_ => !type.HasValue || type == _.Key.Type)
                .ToList();
            var knownRates = attributesToConsider
                .Where(a => a.Value.HasValue)
                .Select(a => a.Value.Value)
                .ToArray();
            return attributesToConsider
                .Sum(_ => _.Value ?? nullRateBehavior.ToRate(attributesToConsider.Count, knownRates));
        }

        public object GetSortablePropertyValue(
            System.Reflection.PropertyInfo columnField,
            GridViewAttribute gridViewAttribute)
        {
            object sourceValue = null;
            if (columnField.DeclaringType == typeof(Confederation))
                sourceValue = Nationality?.Confederation == null ? null : columnField.GetValue(Nationality?.Confederation);
            else if (columnField.DeclaringType == typeof(Country))
                sourceValue = Nationality == null ? null : columnField.GetValue(Nationality);
            else if (columnField.DeclaringType == typeof(Club))
                sourceValue = ClubContract == null ? null : columnField.GetValue(ClubContract);
            else
                sourceValue = columnField.GetValue(this);

            var sourceType = Nullable.GetUnderlyingType(columnField.PropertyType) ?? columnField.PropertyType;
            if (typeof(IComparable).IsAssignableFrom(sourceType))
                return sourceValue;
            else
                return gridViewAttribute.Converter?.Convert(
                    gridViewAttribute.NoPath ? this : sourceValue,
                    null,
                    gridViewAttribute.ConverterParameter,
                    null);
        }
    }
}
