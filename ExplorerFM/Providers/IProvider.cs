using System.Collections.Generic;
using ExplorerFM.Datas;
using ExplorerFM.RuleEngine;

namespace ExplorerFM.Providers
{
    internal interface IProvider
    {
        IReadOnlyList<Club> GetClubs(IReadOnlyDictionary<int, Country> countries,
            IReadOnlyDictionary<int, Competition> competitions);

        IReadOnlyList<Country> GetCountries(IReadOnlyDictionary<int, Confederation> confederations);

        IReadOnlyList<Confederation> GetConfederations();

        IReadOnlyList<Competition> GetCompetitions(Dictionary<int, Country> countryDatas);

        IReadOnlyList<Player> GetPlayersByCriteria(CriteriaSet criteria,
            IReadOnlyDictionary<int, Club> clubs,
            IReadOnlyDictionary<int, Country> countries,
            bool potentialEnabled);

        IReadOnlyList<Player> GetPlayersByClub(int? clubId,
            IReadOnlyDictionary<int, Club> clubs,
            IReadOnlyDictionary<int, Country> countries,
            bool potentialEnabled);

        IReadOnlyList<Player> GetPlayersByCountry(int? countryId,
            bool selectionEligible,
            IReadOnlyDictionary<int, Club> clubs,
            IReadOnlyDictionary<int, Country> countries,
            bool potentialEnabled);
    }
}
