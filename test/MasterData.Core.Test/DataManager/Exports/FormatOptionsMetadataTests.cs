using System.ComponentModel.DataAnnotations;
using JJMasterData.Core.DataManager;
using JJMasterData.Core.DataManager.Exportation;

namespace JJMasterData.Core.Test.DataManager.Exports;

public sealed class FormatOptionsMetadataTests
{
    [Fact]
    public void MetadataIsDerivedFromPublicPropertiesAndDataAnnotations()
    {
        var metadata = FormatOptionsMetadataFactory.CreateOptions(new TestExportOptions());

        var count = Assert.Single(metadata, option => option.Name == nameof(TestExportOptions.Count));
        Assert.Equal("Count", count.DisplayName);
        Assert.Equal(FormatOptionKind.Input, count.Kind);
        Assert.Equal("12", count.DefaultValue);

        var enabled = Assert.Single(metadata, option => option.Name == nameof(TestExportOptions.Enabled));
        Assert.Equal("Include details", enabled.DisplayName);
        Assert.Equal(FormatOptionKind.Boolean, enabled.Kind);
        Assert.Equal("true", enabled.DefaultValue);

        var mode = Assert.Single(metadata, option => option.Name == nameof(TestExportOptions.Mode));
        Assert.Equal(FormatOptionKind.Select, mode.Kind);
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
        var metadata = FormatOptionsMetadataFactory.CreateOptions(new TestExportOptions());

        var defaults = FormatOptionsBinder.Bind<TestExportOptions>(metadata, []);
        Assert.Equal(12, defaults.Count);
        Assert.True(defaults.Enabled);
        Assert.Equal(TestMode.Pretty, defaults.Mode);

        var bound = FormatOptionsBinder.Bind<TestExportOptions>(metadata, new Dictionary<string, string?>
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
        var metadata = FormatOptionsMetadataFactory.CreateOptions(new TestExportOptions());

        Assert.Throws<InvalidOperationException>(() =>
            FormatOptionsBinder.Bind<TestExportOptions>(metadata, new Dictionary<string, string?>
            {
                ["Missing"] = "value"
            }));
        Assert.Throws<InvalidOperationException>(() =>
            FormatOptionsBinder.Bind<TestExportOptions>(metadata, new Dictionary<string, string?>
            {
                [nameof(TestExportOptions.Mode)] = "invalid"
            }));
    }

    [Fact]
    public void MetadataRejectsTypesThatCannotBeBoundFromStrings()
    {
        var exception = Assert.Throws<InvalidOperationException>(() =>
            FormatOptionsMetadataFactory.CreateOptions(new UnsupportedExportOptions()));

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
        protected internal override string Id => "test";
        protected internal override string DisplayName => "Test";
        protected internal override string FileExtension => "test";
        protected internal override string ContentType => "application/test";

        public int Count { get; set; } = 12;

        [Display(Name = "Include details")]
        public bool Enabled { get; set; } = true;

        public TestMode Mode { get; set; } = TestMode.Pretty;
    }

    private sealed class UnsupportedExportOptions : ExportFormatOptions
    {
        protected internal override string Id => "unsupported";
        protected internal override string DisplayName => "Unsupported";
        protected internal override string FileExtension => "unsupported";
        protected internal override string ContentType => "application/unsupported";

        public object Value { get; set; } = new();
    }
}
