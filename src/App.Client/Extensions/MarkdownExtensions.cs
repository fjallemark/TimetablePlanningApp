using Markdig;
using Microsoft.AspNetCore.Components;

namespace Tellurian.Trains.Planning.App.Client.Extensions;

public static class MarkdownExtensions
{
    private static readonly MarkdownPipeline _pipeline = Create();

    private static MarkdownPipeline Create()
    {
        var builder = new MarkdownPipelineBuilder();
        builder.UseAdvancedExtensions();
        return builder.Build();
    }


    public static MarkupString ToHtml(this string? markdown) =>
        new(Markdown.ToHtml(markdown ?? string.Empty, _pipeline));

}
