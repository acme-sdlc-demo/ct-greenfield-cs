using Core;
using Xunit;

namespace Core.Tests;

public class GreetingTests
{
    [Fact]
    public void Greet_names_the_person() => Assert.Equal("Hello, CT", Greeting.Greet("CT"));

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    public void Greet_refuses_a_blank_name(string name) =>
        Assert.Throws<ArgumentException>(() => Greeting.Greet(name));
}
