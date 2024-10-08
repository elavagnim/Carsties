using System;

namespace AuctionService.IntegrationTests.Fixture;

[CollectionDefinition("Shared Collection")]
public class SharedFixture : ICollectionFixture<CustomWebAppFactory>
{

}
