using AutoFixture;
using Goober.Core.Extensions;
using System;
using Xunit;

namespace Goober.Core.Tests.RequiredExtentions
{
    public class RequiredArgumentNotDefaultValueTests
    {
        class TestClass
        {
            public int IntProperty { get; set; }

            public int? NullableIntProperty { get; set; }
        }

        [Fact]
        public void IntPropertyDefaultValue_ShouldThrowArgumentOutOfRangeException()
        {
            //arrange
            var sut = new Fixture().Build<TestClass>().With(x => x.IntProperty, 0).Create();

            //act+assert
            var exc = Assert.Throws<ArgumentException>(() => sut.RequiredArgumentNotDefaultValue(() => sut.IntProperty));
            Assert.Contains("sut.IntProperty", exc.Message);
        }

        [Fact]
        public void IntPropertyNotDefaultValue_NoExceptionThrows()
        {
            //arrange
            var sut = new Fixture().Build<TestClass>().With(x => x.IntProperty, 1).Create();

            //act+assert
            sut.RequiredArgumentNotDefaultValue(() => sut.IntProperty);
        }

        [Fact]
        public void NullableIntPropertyDefaultUnderlyingTypeValue_ShouldThrowArgumentOutOfRangeException()
        {
            //arrange
            var sut = new Fixture().Build<TestClass>().With(x => x.NullableIntProperty, 0).Create();

            //act+assert
            var exc = Assert.Throws<ArgumentException>(() => sut.RequiredArgumnetNotNullAndNotDefaultValue(() => sut.NullableIntProperty));
            Assert.Contains("sut.NullableIntProperty", exc.Message);
        }

        [Fact]
        public void NullableIntPropertyNotDefaultUnderlyingTypeValue_NoExceptionThrows()
        {
            //arrange
            var sut = new Fixture().Build<TestClass>().With(x => x.NullableIntProperty, 1).Create();

            //act+assert
            sut.RequiredArgumnetNotNullAndNotDefaultValue(() => sut.NullableIntProperty);
        }
    }
}
