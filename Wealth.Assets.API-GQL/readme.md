[ Please also see KnownIssues at bottom ]
# 
## Database Considerations
### SQL vs NoSQL
- The assets.json is relatively structured for the most part.  And we will want to be able to filter on common fields.  There are some JSON fields, but a relational database can accomodate that.
### Database Choice
- I went with sqlite, local db for simplicity and local store.  If this were to be designed for usage, I would use PostgreSQL with some type of a hybrid schema.  A managed PostgreSQL instance in the cloud can be setup for infinte scale, reliability and security.
### Assumptions/Customizations
- The data structure will not change.  If it were to change, we would shift to a nonrelational NoSQL db.
- We added an 'ingestion' date column so we can track what date the data was actually implorted into the db compared to the other date columns
## GraphQL Implementation
- I went with HotChocolate as it's the most popular for .NET.
- I have used GraphQL before, but not recently, so I went with the most popular and will have no problem getting up to expert level and being able to determine best practices
- I went with GQL because it was preferred, I could have done this in a fraction of the amount of time with REST, but did not want to use a non-preferred method
## Project Dependencies
- Microsoft.Data.Sqlite
- HotChocolate.AspNetCore
- HotChocolate.Data

# Assets input file
- I just used this as an embedded resource, ideally I would have a separate process for this consumption (see considerations below).

# Local GraphQL Endpoint (Nitro UI)
- https://localhost:7160/graphql

# Instructions/SampleQuery:
- query {
  assetsAsOf(asOf: "2025-03-30T00:00:00Z") {
    assetId
    nickname
    balanceAsOf
    value
  }
}

# Considerations
- In a realistic environment, we would be pulling this feed from an API or somehow pulling (or be sent it), so rather than a simple db seed, we would want to incorporate a process that does that.  It could be done in some type of serverless function in the cloud that has a prime responsibility of syncing/transforming the data, and remove that functionality from this app and have it's primary focus be serving up a GraphQL endpoint.

# KnownIssues
- Due to time contstraints, I was not able to get all fields into the query/contracts so you cannot select all of the fields for your return object.  This would just be adding them to the model so a user can include whatever field(s) they want - hence a benefit of GQL.
