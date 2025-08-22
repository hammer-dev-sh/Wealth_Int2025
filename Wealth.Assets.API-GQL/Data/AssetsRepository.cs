using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using Wealth.Assets.API_GQL.Models;

namespace Wealth.Assets.API_GQL.Data
{
    public static class AssetsRepository
    {
        public static List<Wealth.Assets.API_GQL.Models.Assets> QueryAssets()
        {
            var list = new List<Wealth.Assets.API_GQL.Models.Assets>();
            using var conn = new SqliteConnection("Data Source=assets_wealth.db");
            conn.Open();
            var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT asset_id, nickname, asset_info, wealth_asset_type, primary_asset_category FROM assets";
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                list.Add(new Wealth.Assets.API_GQL.Models.Assets
                {
                    AssetId = reader.GetString(0),
                    Nickname = reader.IsDBNull(1) ? null : reader.GetString(1),
                    AssetInfo = reader.IsDBNull(2) ? null : reader.GetString(2),
                    WealthAssetType = reader.IsDBNull(3) ? null : reader.GetString(3),
                    PrimaryAssetCategory = reader.IsDBNull(4) ? null : reader.GetString(4)
                });
            }
            return list;
        }


        public static List<AssetHolding> QueryHoldings()
        {
            var list = new List<AssetHolding>();
            using var conn = new SqliteConnection("Data Source=assets_wealth.db");
            conn.Open();
            var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT asset_id, balance_as_of, major_class, minor_asset_class, value FROM asset_holdings";
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                list.Add(new AssetHolding
                {
                    AssetId = reader.GetString(0),
                    BalanceAsOf = DateTime.Parse(reader.GetString(1)),
                    MajorClass = reader.IsDBNull(2) ? null : reader.GetString(2),
                    MinorAssetClass = reader.IsDBNull(3) ? null : reader.GetString(3),
                    Value = reader.IsDBNull(4) ? 0 : reader.GetDouble(4)
                });
            }
            return list;
        }
    }
}
