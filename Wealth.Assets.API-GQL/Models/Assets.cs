namespace Wealth.Assets.API_GQL.Models
{
    public record Assets
    {
        public string AssetId { get; set; }
        public string Nickname { get; set; }
        public string AssetInfo { get; set; } // JSON
        public string WealthAssetType { get; set; }
        public string PrimaryAssetCategory { get; set; }
    }


    public record AssetHolding
    {
        public string AssetId { get; set; }
        public DateTime BalanceAsOf { get; set; }
        public string MajorClass { get; set; }
        public string MinorAssetClass { get; set; }
        public double Value { get; set; }
    }


    public record HistoricalAssetBalance
    {
        public string AssetId { get; set; }
        public string Nickname { get; set; }
        public DateTime BalanceAsOf { get; set; }
        public double Value { get; set; }
    }
}
