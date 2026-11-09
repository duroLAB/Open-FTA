using OpenFTA.Tests.Helpers;

namespace OpenFTA.Tests;

/// <summary>Tags usable in the minimal cut set expression and text escaped in the HTML report (B12).</summary>
public class TagValidationTests : SettingsTestBase
{
    [Theory]
    [InlineData("B1")]
    [InlineData("TE")]
    [InlineData("P-101")]
    [InlineData("LIA_1.2")]
    public void Valid_tags(string tag) => Assert.Null(TagValidation.GetError(tag));

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData("A B")]
    [InlineData("A+B")]
    [InlineData("A*B")]
    [InlineData("(A")]
    [InlineData("A)")]
    public void Invalid_tags(string? tag) => Assert.NotNull(TagValidation.GetError(tag));

    [Fact]
    public void Only_events_without_children_are_reported()
    {
        var b = new TreeBuilder(Gates.OR);
        FTAitem gate = b.AddGate(b.Top, "G 1", Gates.AND);   // tag of a gate is not used in the expression
        b.AddBasic(gate, "A+B", 0.1);
        b.AddBasic(gate, "C", 0.2);
        var logic = b.Build();

        var invalid = TagValidation.FindInvalidBasicEventTags(logic.FTAStructure.Values);

        Assert.Equal(new[] { "A+B" }, invalid.Select(e => e.Tag));
    }

    [Fact]
    public void Analysis_check_fails_for_invalid_tags()
    {
        var logic = new TreeBuilder(Gates.OR).Basic("A", 0.1).Basic("B*", 0.2).Build();

        Assert.False(logic.CheckTree(forMinimalCutSets: true));
        Assert.True(logic.CheckTree(forMinimalCutSets: false));
    }

    [Fact]
    public void Report_escapes_names_tags_and_references()
    {
        var b = new TreeBuilder(Gates.OR);
        FTAitem a = b.AddBasic(b.Top, "A", 0.1);
        a.Name = "Pump <P1> & valve";
        a.Reference = "Smith & Jones <2020>";
        b.Basic("B", 0.2);
        var logic = b.Build();

        logic.GenerateHTMLreport();
        string html = logic.html.ToString();

        Assert.Contains("Pump &lt;P1&gt; &amp; valve", html);
        Assert.Contains("Smith &amp; Jones &lt;2020&gt;", html);
        Assert.DoesNotContain("<P1>", html);
    }
}
