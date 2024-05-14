using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using AutoFixture.Dsl;
using HfsChargesContainer.Domain;

namespace HfsChargesContainer.Tests
{
    public static class RandomGen
    {
        private static Fixture _fixture = new Fixture();

        // public BatchLogDomain 

        public static TItem Create<TItem>() => _fixture.Create<TItem>();
        // public static TItem CreateCustom<TItem>(this IPostprocessComposer<TItem> itemComposer) => itemComposer.Create();
        // public static ICustomizationComposer<TItem> Build<TItem>() => _fixture.Build<TItem>();
        public static IEnumerable<TItem> CreateMany<TItem>(int quantity = 3) => _fixture.CreateMany<TItem>(quantity);

        
    }
}
