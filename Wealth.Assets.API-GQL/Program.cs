using HotChocolate;
using HotChocolate.AspNetCore;
using HotChocolate.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Linq;
using Wealth.Assets.API_GQL.Data;

namespace Wealth.Assets.API_GQL
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddAuthorization();

            builder.Services
                .AddGraphQLServer()
                .AddQueryType<Wealth.Assets.API_GQL.GraphQL.AssetQuery>()
                .AddFiltering()
                .AddSorting();

            // When the app starts, we will create an in-memory Sqlite connection to hold our data
            var conn = new SqliteConnection("Data Source=assets_wealth.db");
            conn.Open();

            DatabaseSeeder.SeedDataFromFile(conn, "assets.json");

            var app = builder.Build();

            app.MapGraphQL();

            // Configure the HTTP request pipeline.

            app.UseHttpsRedirection();

            app.UseAuthorization();

            app.Run();
        }
    }
}
