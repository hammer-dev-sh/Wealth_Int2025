using HotChocolate.Types.Pagination;
using Microsoft.Data.Sqlite;
using System.Reflection;
using System.Text.Json;
using System.Xml.Linq;
using Wealth.Assets.API_GQL.Models;

namespace Wealth.Assets.API_GQL.Data
{
    public static class DatabaseSeeder
    {
        public static void SeedDataFromFile(SqliteConnection conn, string jsonFilename)
        {
            if (String.IsNullOrEmpty(jsonFilename))
            {
                throw new ArgumentNullException("There is no source file specified for seeding.");
            }

            var assembly = Assembly.GetExecutingAssembly();
            string resourceName = $"Wealth.Assets.API_GQL.Data.{jsonFilename}";
            String assetsData;

            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            using (StreamReader reader = new StreamReader(stream))
            {
                assetsData = reader.ReadToEnd();
            }

            CreateTables(conn);
            SeedDatabase(conn, assetsData);            

            Console.WriteLine("Database seeding complete.");
        }

        private static void CreateTables(SqliteConnection conn)
        {
            var createAssetsCmd = conn.CreateCommand();
            createAssetsCmd.CommandText = @"
            CREATE TABLE IF NOT EXISTS assets (
                asset_id TEXT PRIMARY KEY,
                asset_description TEXT,
                asset_info TEXT,
                asset_info_type TEXT,
                asset_mask TEXT,
                asset_name TEXT,
                asset_owner_name TEXT,
                balance_as_of TEXT,
                balance_cost_basis REAL,
                balance_cost_from TEXT,
                balance_current REAL,
                balance_from TEXT,
                balance_price REAL,
                balance_price_from TEXT,
                balance_quantity_current REAL,
                beneficiary_composition TEXT,
                cognito_id TEXT,
                creation_date TEXT,
                currency_code TEXT,
                deactivate_by TEXT,
                description_estate_plan TEXT,
                has_investment TEXT,
                include_in_net_worth INTEGER,
                institution_id INTEGER,
                institution_name TEXT,
                integration TEXT,
                integration_account_id TEXT,
                is_active INTEGER,
                is_asset INTEGER,
                is_favorite INTEGER,
                is_linked_vendor TEXT,
                last_update TEXT,
                last_update_attempt TEXT,
                logo_name TEXT,
                modification_date TEXT,
                next_update TEXT,
                nickname TEXT,
                note TEXT,
                note_date TEXT,
                ownership TEXT,
                primary_asset_category TEXT,
                status TEXT,
                status_code TEXT,
                user_institution_id TEXT,
                vendor_account_type TEXT,
                vendor_container TEXT,
                vendor_response TEXT,
                vendor_response_type TEXT,
                wealth_asset_type TEXT,
                wid TEXT
            );
        ";
            createAssetsCmd.ExecuteNonQuery();

            var createHoldingsCmd = conn.CreateCommand();
            createHoldingsCmd.CommandText = @"
            CREATE TABLE IF NOT EXISTS asset_holdings (
                asset_id TEXT,
                balance_as_of TEXT,
                major_class TEXT,
                minor_asset_class TEXT,
                value REAL,
                FOREIGN KEY (asset_id) REFERENCES assets(asset_id)
            );
        ";
            createHoldingsCmd.ExecuteNonQuery();
        }

        static void SeedDatabase(SqliteConnection connection, string jsonText)
        {
            //var assets = jsonText;

            var document = JsonDocument.Parse(jsonText);
            var assets = document.RootElement.EnumerateArray();

            foreach (var asset in assets)
            {
                var cmd = connection.CreateCommand();
                cmd.CommandText = @"
                INSERT OR REPLACE INTO assets (
                    asset_id, asset_description, asset_info, asset_info_type, asset_mask, asset_name, asset_owner_name,
                    balance_as_of, balance_cost_basis, balance_cost_from, balance_current, balance_from, balance_price,
                    balance_price_from, balance_quantity_current, beneficiary_composition, cognito_id, creation_date,
                    currency_code, deactivate_by, description_estate_plan, has_investment, include_in_net_worth,
                    institution_id, institution_name, integration, integration_account_id, is_active, is_asset,
                    is_favorite, is_linked_vendor, last_update, last_update_attempt, logo_name, modification_date,
                    next_update, nickname, note, note_date, ownership, primary_asset_category, status, status_code,
                    user_institution_id, vendor_account_type, vendor_container, vendor_response, vendor_response_type,
                    wealth_asset_type, wid
                ) VALUES (
                    $asset_id, $asset_description, $asset_info, $asset_info_type, $asset_mask, $asset_name, $asset_owner_name,
                    $balance_as_of, $balance_cost_basis, $balance_cost_from, $balance_current, $balance_from, $balance_price,
                    $balance_price_from, $balance_quantity_current, $beneficiary_composition, $cognito_id, $creation_date,
                    $currency_code, $deactivate_by, $description_estate_plan, $has_investment, $include_in_net_worth,
                    $institution_id, $institution_name, $integration, $integration_account_id, $is_active, $is_asset,
                    $is_favorite, $is_linked_vendor, $last_update, $last_update_attempt, $logo_name, $modification_date,
                    $next_update, $nickname, $note, $note_date, $ownership, $primary_asset_category, $status, $status_code,
                    $user_institution_id, $vendor_account_type, $vendor_container, $vendor_response, $vendor_response_type,
                    $wealth_asset_type, $wid
                );";

                foreach (var prop in asset.EnumerateObject())
                {
                    var value = prop.Value.ValueKind switch
                    {
                        JsonValueKind.String => prop.Value.GetString(),
                        JsonValueKind.Number => prop.Value.GetDouble().ToString(),
                        JsonValueKind.True => "1",
                        JsonValueKind.False => "0",
                        JsonValueKind.Object or JsonValueKind.Array => prop.Value.GetRawText(),
                        _ => null
                    };

                    cmd.Parameters.AddWithValue($"${ToSqlName(prop.Name)}", (object?)value ?? DBNull.Value);
                }

                cmd.ExecuteNonQuery();

                if (asset.TryGetProperty("holdings", out var holdings) &&
                    holdings.ValueKind == JsonValueKind.Object &&
                    holdings.TryGetProperty("majorAssetClasses", out var majors) &&
                    majors.ValueKind == JsonValueKind.Array)
                {
                    var assetId = asset.GetProperty("assetId").GetString();
                    var balanceAsOf = asset.GetProperty("balanceAsOf").GetString();

                    foreach (var major in majors.EnumerateArray())
                    {
                        var majorClass = major.GetProperty("majorClass").GetString();
                        foreach (var minor in major.GetProperty("assetClasses").EnumerateArray())
                        {
                            var minorClass = minor.GetProperty("minorAssetClass").GetString();
                            var value = minor.GetProperty("value").GetDouble();

                            var insertHolding = connection.CreateCommand();
                            insertHolding.CommandText = @"
                            INSERT INTO asset_holdings (asset_id, balance_as_of, major_class, minor_asset_class, value)
                            VALUES ($id, $asOf, $major, $minor, $val);";
                            insertHolding.Parameters.AddWithValue("$id", assetId);
                            insertHolding.Parameters.AddWithValue("$asOf", balanceAsOf);
                            insertHolding.Parameters.AddWithValue("$major", majorClass);
                            insertHolding.Parameters.AddWithValue("$minor", minorClass);
                            insertHolding.Parameters.AddWithValue("$val", value);
                            insertHolding.ExecuteNonQuery();
                        }
                    }
                }
            }
        }

        static string ToSqlName(string jsonName)
        // this function is just used to take the preferred casing of each and change
        // json -> sql
        {
            return jsonName switch
            {
                "assetInfo" => "asset_info",
                "assetId" => "asset_id",
                "assetInfoType" => "asset_info_type",
                "balanceAsOf" => "balance_as_of",
                "balanceCostBasis" => "balance_cost_basis",
                "balanceCostFrom" => "balance_cost_from",
                "balanceCurrent" => "balance_current",
                "balanceFrom" => "balance_from",
                "balancePrice" => "balance_price",
                "balancePriceFrom" => "balance_price_from",
                "balanceQuantityCurrent" => "balance_quantity_current",
                "beneficiaryComposition" => "beneficiary_composition",
                "cognitoId" => "cognito_id",
                "creationDate" => "creation_date",
                "currencyCode" => "currency_code",
                "deactivateBy" => "deactivate_by",
                "descriptionEstatePlan" => "description_estate_plan",
                "hasInvestment" => "has_investment",
                "includeInNetWorth" => "include_in_net_worth",
                "institutionId" => "institution_id",
                "institutionName" => "institution_name",
                "integration" => "integration",
                "integrationAccountId" => "integration_account_id",
                "isActive" => "is_active",
                "isAsset" => "is_asset",
                "isFavorite" => "is_favorite",
                "isLinkedVendor" => "is_linked_vendor",
                "lastUpdate" => "last_update",
                "lastUpdateAttempt" => "last_update_attempt",
                "logoName" => "logo_name",
                "modificationDate" => "modification_date",
                "nextUpdate" => "next_update",
                "nickname" => "nickname",
                "note" => "note",
                "noteDate" => "note_date",
                "ownership" => "ownership",
                "primaryAssetCategory" => "primary_asset_category",
                "status" => "status",
                "statusCode" => "status_code",
                "userInstitutionId" => "user_institution_id",
                "vendorAccountType" => "vendor_account_type",
                "vendorContainer" => "vendor_container",
                "vendorResponse" => "vendor_response",
                "vendorResponseType" => "vendor_response_type",
                "wealthAssetType" => "wealth_asset_type",
                "wid" => "wid",
                "assetMask" => "asset_mask",
                "assetDescription" => "asset_description",
                "assetName" => "asset_name",
                "assetOwnerName" => "asset_owner_name",
                _ => jsonName
            };
        }
    }
}
