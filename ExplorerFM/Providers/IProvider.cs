using System.Collections.Generic;
using ExplorerFM.Datas;
using ExplorerFM.RuleEngine;

namespace ExplorerFM.Providers
{
    internal interface IProvider
    {
        IReadOnlyList<Club> GetClubs(IReadOnlyDictionary<int, Country> countries);

        IReadOnlyList<Country> GetCountries(IReadOnlyDictionary<int, Confederation> confederations);

        IReadOnlyList<Confederation> GetConfederations();

        IReadOnlyList<Player> GetPlayersByCriteria(CriteriaSet criteria,
            IReadOnlyDictionary<int, Club> clubs,
            IReadOnlyDictionary<int, Country> countries);

        IReadOnlyList<Player> GetPlayersByClub(int? clubId,
            IReadOnlyDictionary<int, Club> clubs,
            IReadOnlyDictionary<int, Country> countries);

        IReadOnlyList<Player> GetPlayersByCountry(int? countryId,
            bool selectionEligible,
            IReadOnlyDictionary<int, Club> clubs,
            IReadOnlyDictionary<int, Country> countries);
    }
}
