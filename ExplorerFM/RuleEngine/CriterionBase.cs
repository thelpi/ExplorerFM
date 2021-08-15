using System;
using System.Collections.Generic;

namespace ExplorerFM.RuleEngine
{
    public abstract class CriterionBase
    {
        protected const string SqlNullString = "NULL";
        protected const string SqlIsNotString = "IS NOT";
        protected const string SqlIsString = "IS";
        protected const string SqlAnd = "AND";
        protected const string SqlOr = "OR";
        protected const string SqlAll = "1";

        private const string ClubNestedSql = "SELECT club.{0} FROM club WHERE club.ID = player.ClubContractID";
        private const string CountryNestedSql = "SELECT country.{0} FROM country WHERE country.ID = player.NationID1";
        private static readonly string ConfederationNestedSql = "SELECT confederation.{0} FROM confederation WHERE confederation.ID = (" + string.Format(CountryNestedSql, "ContinentID") + ")";

        protected static readonly IReadOnlyDictionary<Type, string> NestedQueries = new Dictionary<Type, string>
        {
            { typeof(Datas.Club), string.Concat("(", ClubNestedSql, ")") },
            { typeof(Datas.Country), string.Concat("(", CountryNestedSql, ")") },
            { typeof(Datas.Confederation), string.Concat("(", ConfederationNestedSql, ")") }
        };
    }
}
