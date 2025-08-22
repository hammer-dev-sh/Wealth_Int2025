using System;
using System.Collections.Generic;
using System.Linq;
using Wealth.Assets.API_GQL.Data;
using Wealth.Assets.API_GQL.Models;

namespace Wealth.Assets.API_GQL.GraphQL
{
    public class AssetQuery
    {
        public IEnumerable<Wealth.Assets.API_GQL.Models.Assets> GetAssets() => AssetsRepository.QueryAssets();


        public IEnumerable<HistoricalAssetBalance> GetAssetsAsOf(DateTime asOf)
        {
            var latestByAsset = Wealth.Assets.API_GQL.Data.AssetsRepository.QueryHoldings()
            .Where(h => h.BalanceAsOf <= asOf)
            .GroupBy(h => h.AssetId)
            .Select(g => g.OrderByDescending(h => h.BalanceAsOf).First());


            var assets = Wealth.Assets.API_GQL.Data.AssetsRepository.QueryAssets();


            return from holding in latestByAsset
                   join asset in assets on holding.AssetId equals asset.AssetId
                   select new HistoricalAssetBalance
                   {
                       AssetId = asset.AssetId,
                       Nickname = asset.Nickname,
                       BalanceAsOf = holding.BalanceAsOf,
                       Value = holding.Value
                   };
        }
    }
}
