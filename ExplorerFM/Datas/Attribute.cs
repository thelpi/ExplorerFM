using System.Collections.Generic;
using System.Linq;
using ExplorerFM.Datas.Dtos;

namespace ExplorerFM.Datas
{
    public class Attribute : BaseData
    {
        private static List<Attribute> _playerInstances = null;

        public string Name { get; set; }
        public AttributeType Type { get; set; }

        private Attribute() { }

        public static IReadOnlyList<Attribute> PlayerInstances
        {
            get
            {
                if (_playerInstances == null)
                {
                    _playerInstances = new List<Attribute>(50)
                    {
                        new Attribute { Id = 01, Name = nameof(PlayerAttributesDto.Acceleration), Type = AttributeType.Physical },
                        new Attribute { Id = 02, Name = nameof(PlayerAttributesDto.Agility), Type = AttributeType.Physical },
                        new Attribute { Id = 03, Name = nameof(PlayerAttributesDto.Balance), Type = AttributeType.Physical },
                        new Attribute { Id = 04, Name = nameof(PlayerAttributesDto.InjuryProneness), Type = AttributeType.Physical },
                        new Attribute { Id = 05, Name = nameof(PlayerAttributesDto.Jumping), Type = AttributeType.Physical },
                        new Attribute { Id = 06, Name = nameof(PlayerAttributesDto.NaturalFitness), Type = AttributeType.Physical },
                        new Attribute { Id = 07, Name = nameof(PlayerAttributesDto.Pace), Type = AttributeType.Physical },
                        new Attribute { Id = 08, Name = nameof(PlayerAttributesDto.Stamina), Type = AttributeType.Physical },
                        new Attribute { Id = 09, Name = nameof(PlayerAttributesDto.Strength), Type = AttributeType.Physical },

                        new Attribute { Id = 10, Name = nameof(PlayerAttributesDto.Handling), Type = AttributeType.Goalkeeper },
                        new Attribute { Id = 11, Name = nameof(PlayerAttributesDto.OneOnOnes), Type = AttributeType.Goalkeeper },
                        new Attribute { Id = 12, Name = nameof(PlayerAttributesDto.Reflexes), Type = AttributeType.Goalkeeper },

                        new Attribute { Id = 13, Name = nameof(PlayerAttributesDto.Corners), Type = AttributeType.Technical },
                        new Attribute { Id = 14, Name = nameof(PlayerAttributesDto.FreeKicks), Type = AttributeType.Technical },
                        new Attribute { Id = 15, Name = nameof(PlayerAttributesDto.ThrowIns), Type = AttributeType.Technical },
                        new Attribute { Id = 16, Name = nameof(PlayerAttributesDto.Crossing), Type = AttributeType.Technical },
                        new Attribute { Id = 17, Name = nameof(PlayerAttributesDto.Dribbling), Type = AttributeType.Technical },
                        new Attribute { Id = 18, Name = nameof(PlayerAttributesDto.Finishing), Type = AttributeType.Technical },
                        new Attribute { Id = 19, Name = nameof(PlayerAttributesDto.Heading), Type = AttributeType.Technical },
                        new Attribute { Id = 20, Name = nameof(PlayerAttributesDto.LongShots), Type = AttributeType.Technical },
                        new Attribute { Id = 21, Name = nameof(PlayerAttributesDto.Marking), Type = AttributeType.Technical },
                        new Attribute { Id = 22, Name = nameof(PlayerAttributesDto.Movement), Type = AttributeType.Technical },
                        new Attribute { Id = 23, Name = nameof(PlayerAttributesDto.Passing), Type = AttributeType.Technical },
                        new Attribute { Id = 24, Name = nameof(PlayerAttributesDto.Penalties), Type = AttributeType.Technical },
                        new Attribute { Id = 25, Name = nameof(PlayerAttributesDto.Positioning), Type = AttributeType.Technical },
                        new Attribute { Id = 26, Name = nameof(PlayerAttributesDto.Tackling), Type = AttributeType.Technical },
                        new Attribute { Id = 27, Name = nameof(PlayerAttributesDto.Technique), Type = AttributeType.Technical },
                        new Attribute { Id = 28, Name = nameof(PlayerAttributesDto.Vision), Type = AttributeType.Technical },

                        new Attribute { Id = 29, Name = nameof(PlayerAttributesDto.Adaptability), Type = AttributeType.Psychological },
                        new Attribute { Id = 30, Name = nameof(PlayerAttributesDto.Agression), Type = AttributeType.Psychological },
                        new Attribute { Id = 31, Name = nameof(PlayerAttributesDto.Ambition), Type = AttributeType.Psychological },
                        new Attribute { Id = 32, Name = nameof(PlayerAttributesDto.Anticipation), Type = AttributeType.Psychological },
                        new Attribute { Id = 33, Name = nameof(PlayerAttributesDto.Bravery), Type = AttributeType.Psychological },
                        new Attribute { Id = 34, Name = nameof(PlayerAttributesDto.Consistency), Type = AttributeType.Psychological },
                        new Attribute { Id = 35, Name = nameof(PlayerAttributesDto.Decisions), Type = AttributeType.Psychological },
                        new Attribute { Id = 36, Name = nameof(PlayerAttributesDto.Determination), Type = AttributeType.Psychological },
                        new Attribute { Id = 37, Name = nameof(PlayerAttributesDto.Dirtiness), Type = AttributeType.Psychological },
                        new Attribute { Id = 38, Name = nameof(PlayerAttributesDto.Flair), Type = AttributeType.Psychological },
                        new Attribute { Id = 39, Name = nameof(PlayerAttributesDto.ImportantMatchs), Type = AttributeType.Psychological },
                        new Attribute { Id = 40, Name = nameof(PlayerAttributesDto.Leadership), Type = AttributeType.Psychological },
                        new Attribute { Id = 41, Name = nameof(PlayerAttributesDto.Loyalty), Type = AttributeType.Psychological },
                        new Attribute { Id = 42, Name = nameof(PlayerAttributesDto.Pressure), Type = AttributeType.Psychological },
                        new Attribute { Id = 43, Name = nameof(PlayerAttributesDto.Professionalism), Type = AttributeType.Psychological },
                        new Attribute { Id = 44, Name = nameof(PlayerAttributesDto.Sportsmanship), Type = AttributeType.Psychological },
                        new Attribute { Id = 45, Name = nameof(PlayerAttributesDto.TeamWork), Type = AttributeType.Psychological },
                        new Attribute { Id = 46, Name = nameof(PlayerAttributesDto.Temperament), Type = AttributeType.Psychological },
                        new Attribute { Id = 47, Name = nameof(PlayerAttributesDto.Versatility), Type = AttributeType.Psychological },
                        new Attribute { Id = 48, Name = nameof(PlayerAttributesDto.WorkRate), Type = AttributeType.Psychological }
                    };
                }

                return _playerInstances;
            }
        }

        public static Attribute GetPlayerAttributeBy(string exactName)
        {
            return PlayerInstances.FirstOrDefault(x => x.Name == exactName);
        }
    }
}
