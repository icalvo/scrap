using System;
using System.Linq;
using FluentAssertions;
using Scrap.JobDefinitions;
using Xunit;

namespace LiteDB.Tests.Mapper
{
    public class CustomMapping_Tests
    {
        public class ClassWithArray
        {
            [BsonCtor]
            public ClassWithArray(string id, string[] stringArray)
            {
                Id = id;
                StringArray = stringArray;
            }


            public string Id { get; }
            public string[] StringArray { get; }
        }

        private readonly BsonMapper _mapper = new();

        [Fact]
        public void Custom_Ctor2()
        {
            var doc = new BsonDocument { ["_id"] = "trol", ["stringArray"] = new BsonArray("First", "Second")  };

            Func<ClassWithArray> action = () => _mapper.ToObject<ClassWithArray>(doc);

            action.Should().Throw<LiteException>().WithMessage($"They seem to have fixed this. Change {nameof(LiteDbJobDefinition)}'s constructor to accept string[] directly instead of BsonArray, and remove this test.");
        }
    }
}