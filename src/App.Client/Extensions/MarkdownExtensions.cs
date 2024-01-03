using Markdig;
using Microsoft.AspNetCore.Components;

namespace Tellurian.Trains.Planning.App.Client.Extensions;

public static class MarkdownExtensions
{
    private static readonly MarkdownPipelineBuilder _pipelineBuilder = Create();

    private static MarkdownPipelineBuilder Create()
    {
        var builder = new MarkdownPipelineBuilder();
        //builder.Extensions.Add(new Markdig.Extensions.Tables.PipeTableExtension());
        return builder;
    }


    public static MarkupString ToHtml(this string? markdown) =>
        new(Markdown.ToHtml(markdown ?? string.Empty, _pipelineBuilder.Build()));

}
