using System.Diagnostics;
using System.Text;
using FluentAssertions;
using NUnit.Framework;

namespace Markdown.Tests;

[TestFixture]
public class MdPerformanceTests
{
    private readonly Md md = new();

    [TestCaseSource(nameof(PerformanceFragments))]
    public void Render_ScalesLinearlyWithInputSize(string fragment)
    {
        var smallInput = Repeat(fragment, 1_000);
        var largeInput = Repeat(fragment, 10_000);

        md.Render(smallInput);
        md.Render(largeInput);

        var smallTime = MeasureMinTime(() => md.Render(smallInput));
        var largeTime = MeasureMinTime(() => md.Render(largeInput));

        largeTime.TotalMilliseconds.Should().BeLessThan(
            Math.Max(smallTime.TotalMilliseconds * 25, 50),
            "при увеличении входа в 10 раз время работы линейного алгоритма не должно расти квадратично");
    }

    [Test]
    public void Render_HandlesDeeplyNestedMarkup()
    {
        const int depth = 20_000;
        var markdown = Repeat("_a ", depth) + "x" + Repeat(" b_", depth);

        var html = md.Render(markdown);

        html.Should().StartWith("<p><em>a ");
        html.Should().EndWith("</em></p>");
        html.Split("<em>").Length.Should().Be(depth + 1, "каждый открывающий подчёрк даёт свой тег em");
    }

    [Test]
    public void Render_HandlesLongTailAfterSingleOpener()
    {
        var markdown = "_a" + Repeat("b", 600_000) + "_";

        var html = md.Render(markdown);

        html.Should().StartWith("<p><em>a");
        html.Should().EndWith("</em></p>");
        html.Should().NotContain("_");
    }

    private static string Repeat(string fragment, int count)
    {
        var builder = new StringBuilder(fragment.Length * count);

        for (var i = 0; i < count; i++)
            builder.Append(fragment);

        return builder.ToString();
    }

    private static TimeSpan MeasureMinTime(Action action, int runs = 5)
    {
        var minElapsed = TimeSpan.MaxValue;

        for (var i = 0; i < runs; i++)
        {
            var stopwatch = Stopwatch.StartNew();
            action();
            stopwatch.Stop();

            if (stopwatch.Elapsed < minElapsed)
                minElapsed = stopwatch.Elapsed;
        }

        return minElapsed;
    }

    public static IEnumerable<TestCaseData> PerformanceFragments => Fragments(
        "# Заголовок __с _разными_ символами__ и [ссылкой](url)\n\n",
        "_это ра_ботает так_ __пересечение _двойных__ и одинарных_ [a _b](u) c_ ",
        "текст](без закрытия скобки ](еще ](и еще _непарный __подчерк ");

    private static IEnumerable<TestCaseData> Fragments(params string[] fragments)
    {
        for (var index = 0; index < fragments.Length; index++)
            yield return new TestCaseData(fragments[index])
                .SetName($"фрагмент {index + 1}: \"{fragments[index].Replace("\n", "\\n")}\"");
    }
}
