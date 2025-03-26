using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using ExplorerFM.FieldsAttributes;
using ExplorerFM.Providers;

namespace ExplorerFM.Datas
{
    public class Player : Staff
    {
        public const int PositioningTolerance = 15;
        
        [NestedSelectorField(1, 20, typeof(Side))]
        public Dictionary<Side, int?> Sides { get; set; }

        [GridView("Best pos.", 2.5, typeof(Converters.PositioningDisplayConverter), true, true)]
        [GridView("Altern. pos.", 2.6, typeof(Converters.PositioningDisplayConverter), true, false)]
        [NestedSelectorField(1, 20, typeof(Position))]
        public Dictionary<Position, int?> Positions { get; set; }

        [NestedSelectorField(1, 20, nameof(DataProvider.Attributes), nameof(Attribute.Name))]
        public Dictionary<Attribute, int?> Attributes { get; set; }

        [Field(1, 20)]
        public int? LeftFoot { get; set; }

        [Field(1, 20)]
        public int? RightFoot { get; set; }

        [Field(0, 999)]
        public int? SquadNumber { get; set; }

        public bool Loaded { get; set; }

        [GridView("Tot. attr.", 12.5)]
        [AggregateField(1, 1000)]
        public int AttributesTotal => GetAttributesTotal();

        // the identifier of attribute_type is hardcoded here
        [AggregateField(1, 1000)]
        public int AttributesGoalkeeperTotal => GetAttributesTotal(AttributeType.Goalkeeper);

        [AggregateField(1, 1000)]
        public int AttributesKickoffTotal => GetAttributesTotal(AttributeType.Technical);

        [AggregateField(1, 1000)]
        public int AttributesPhysicalTotal => GetAttributesTotal(AttributeType.Physical);

        [AggregateField(1, 1000)]
        public int AttributesPsychologicalTotal => GetAttributesTotal(AttributeType.Psychological);

        [AggregateField(1, 1000)]
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

        private Dictionary<(Position, Side), int> _positionFinalRateCache = new Dictionary<(Position, Side), int>(30);
        private object _lockerPositionFinalRateCache = new object();

        public int GetPositionFinalRate((Position, Side) ps)
        {
            if (_positionFinalRateCache.TryGetValue(ps, out var result))
            {
                return result;
            }

            lock (_lockerPositionFinalRateCache)
            {
                if (_positionFinalRateCache.TryGetValue(ps, out result))
                {
                    return result;
                }

                result = (int)Math.Round(GetAttributesTotal() * GetPositionSideRate(ps.Item1, ps.Item2) / (decimal)20);

                _positionFinalRateCache.Add(ps, result);
            }

            return result;
        }

        private Dictionary<int, int> _attributesCache = new Dictionary<int, int>(48);
        private object _lockerAttributesCache = new object();

        public int GetAttributesTotal(AttributeType? type = null)
        {
            var tInt = type.HasValue ? (int)type.Value : -1;

            if (_attributesCache.TryGetValue(tInt, out var result))
            {
                return result;
            }

            lock (_lockerAttributesCache)
            {
                if (_attributesCache.TryGetValue(tInt, out result))
                {
                    return result;
                }

                var attributesToConsider = Attributes
                    .Where(_ => !type.HasValue || type == _.Key.Type)
                    .ToList();
                var knownRates = attributesToConsider
                    .Where(a => a.Value.HasValue)
                    .Select(a => a.Value.Value)
                    .ToArray();
                var nullRateBehavior = knownRates.Length < attributesToConsider.Count / 2
                    ? 10
                    : (int)Math.Round(knownRates.Average());

                result = attributesToConsider
                    .Sum(_ => _.Value ?? nullRateBehavior);

                _attributesCache.Add(tInt, result);
            }

            return result;
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
