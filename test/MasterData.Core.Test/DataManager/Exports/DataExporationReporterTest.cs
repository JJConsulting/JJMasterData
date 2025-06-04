using JJMasterData.Core.DataManager.Exportation;

namespace JJMasterData.Core.Test.DataManager.Exports;

public class DataExpReporterTests
{
    [Fact]
    public void Percentage_Is_Calculated_Correctly()
    {
        // Arrange
        var reporter = new DataExportationReporter
        {
            TotalOfRecords = 100,
            TotalProcessed = 50
        };

        // Act
        int percentage = reporter.Percentage;

        // Assert
        Assert.Equal(50, percentage);
    }

    [Fact]
    public void Percentage_Is_Zero_When_No_Records_Processed()
    {
        // Arrange
        var reporter = new DataExportationReporter
        {
            TotalOfRecords = 100,
            TotalProcessed = 0
        };

        // Act
        int percentage = reporter.Percentage;

        // Assert
        Assert.Equal(0, percentage);
    }

    [Theory]
    [InlineData(1,1)]
    [InlineData(10,1)]
    [InlineData(10,9)]
    [InlineData(1221212123,1221212122)]
    public void Percentage_Is_Capped_At_100(int totalRecords, int totalProcessed)
    {
        // Arrange
        var reporter = new DataExportationReporter
        {
            TotalOfRecords = totalRecords,
            TotalProcessed = totalProcessed
        };

        // Act
        int percentage = reporter.Percentage;

        // Assert
        Assert.True(percentage <= 100);
    }
}