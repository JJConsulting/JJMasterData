using System.ComponentModel.DataAnnotations;
using JJMasterData.Core.DataManager.Exportation;
using JJMasterData.Core.DataManager.Exportation.Abstractions;

namespace JJMasterData.Core.Test.DataManager.Exports;

public sealed class FormatOptionsMetadataTests
{
    [Fact]
    public void MetadataIsDerivedFromPublicPropertiesAndDataAnnotations()
    {
        var metadata = ExportFormatOptionsMetadataFactory.CreateOptions(new TestExportFormat());

        var count = Assert.Single(metadata, option => option.Name == nameof(TestExportOptions.Count));
        Assert.Equal("Count", count.DisplayName);
        Assert.Equal(ExportFormatOptionKind.Input, count.Kind);
        Assert.Equal("12", count.DefaultValue);

        var enabled = Assert.Single(metadata, option => option.Name == nameof(TestExportOptions.Enabled));
        Assert.Equal("Include details", enabled.DisplayName);
        Assert.Equal(ExportFormatOptionKind.Boolean, enabled.Kind);
        Assert.Equal("true", enabled.DefaultValue);

        var mode = Assert.Single(metadata, option => option.Name == nameof(TestExportOptions.Mode));
        Assert.Equal(ExportFormatOptionKind.Select, mode.Kind);
        Assert.Equal("p", mode.DefaultValue);
        Assert.Collection(mode.Choices,
            choice =>
            {
                Assert.Equal("Compact", choice.Value);
                Assert.Equal("Compact mode", choice.DisplayName);
            },
            choice =>
            {
                Assert.Equal("p", choice.Value);
                Assert.Equal("Pretty mode", choice.DisplayName);
            });
    }

    [Fact]
    public void BinderUsesDefaultsAndBindsNamesAndEnumValuesCaseInsensitively()
    {
        var metadata = ExportFormatOptionsMetadataFactory.CreateOptions(new TestExportFormat());

        var defaults = (TestExportOptions)ExportFormatOptionsBinder.Bind(metadata, typeof(TestExportOptions), []);
        Assert.Equal(12, defaults.Count);
        Assert.True(defaults.Enabled);
        Assert.Equal(TestMode.Pretty, defaults.Mode);

        var bound = (TestExportOptions)ExportFormatOptionsBinder.Bind(metadata, typeof(TestExportOptions), new Dictionary<string, string?>
        {
            ["count"] = "42",
            ["ENABLED"] = "false",
            ["mode"] = "P"
        });
        Assert.Equal(42, bound.Count);
        Assert.False(bound.Enabled);
        Assert.Equal(TestMode.Pretty, bound.Mode);
    }

    [Fact]
    public void BinderRejectsUnknownOptionsAndInvalidEnumValues()
    {
        var metadata = ExportFormatOptionsMetadataFactory.CreateOptions(new TestExportFormat());

        Assert.Throws<InvalidOperationException>(() =>
            ExportFormatOptionsBinder.Bind(metadata, typeof(TestExportOptions), new Dictionary<string, string?>
            {
                ["Missing"] = "value"
            }));
        Assert.Throws<InvalidOperationException>(() =>
            ExportFormatOptionsBinder.Bind(metadata, typeof(TestExportOptions), new Dictionary<string, string?>
            {
                [nameof(TestExportOptions.Mode)] = "invalid"
            }));
    }

    [Fact]
    public void MetadataRejectsTypesThatCannotBeBoundFromStrings()
    {
        var exception = Assert.Throws<InvalidOperationException>(() =>
            ExportFormatOptionsMetadataFactory.CreateOptions(new UnsupportedExportFormat()));

        Assert.Contains(nameof(UnsupportedExportOptions.Value), exception.Message);
    }

    private enum TestMode
    {
        [Display(Name = "Compact mode")]
        Compact,
        [Display(Name = "Pretty mode", ShortName = "p")]
        Pretty
    }

    private sealed class TestExportOptions : ExportFormatOptions
    {
        public int Count { get; set; } = 12;

        [Display(Name = "Include details")]
        public bool Enabled { get; set; } = true;

        public TestMode Mode { get; set; } = TestMode.Pretty;
    }

    private sealed class TestExportFormat : IExportFormat<TestExportOptions>
    {
        public string Id => "test";
        public string DisplayName => "Test";
        public string FileExtension => "test";

        public Task WriteAsync(
            ExportContext context,
            TestExportOptions options,
            Stream output,
            CancellationToken cancellationToken) => Task.CompletedTask;
    }

    private sealed class UnsupportedExportOptions : ExportFormatOptions
    {
        public object Value { get; set; } = new();
    }

    private sealed class UnsupportedExportFormat : IExportFormat<UnsupportedExportOptions>
    {
        public string Id => "unsupported";
        public string DisplayName => "Unsupported";
        public string FileExtension => "unsupported";

        public Task WriteAsync(
            ExportContext context,
            UnsupportedExportOptions options,
            Stream output,
            CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
