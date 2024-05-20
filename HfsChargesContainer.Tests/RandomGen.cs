using System.Collections.Generic;
using AutoFixture;


namespace HfsChargesContainer.Tests
{
    public static class RandomGen
    {
        private static Fixture _fixture = new Fixture();

        public static TItem Create<TItem>() => _fixture.Create<TItem>();
        public static IEnumerable<TItem> CreateMany<TItem>(int quantity = 3) => _fixture.CreateMany<TItem>(quantity);
    }
}
