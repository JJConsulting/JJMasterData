using System.Collections;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Xunit.Tester;
using Xunit;

namespace JJMasterData.Xunit.Assertions;

public static class DataDictionaryAssertions 
{
    public static Metadata AssertAllOperations(this Metadata metadata, Hashtable? values = null)
    {
        var tester = new DataDictionaryTester(metadata);
        var result = tester.AllOperations(values);
        Assert.True(result.IsValid);
        return metadata;
    }
    
    public static Metadata AssertAllOperations(this Metadata metadata, Action<DataDictionaryTesterValues> configure)
    {
        var tester = new DataDictionaryTester(metadata);
        var result = tester.AllOperations(configure);
        Assert.True(result.IsValid);
        return metadata;
    }
    
    public static Metadata AssertInsert(this Metadata metadata, Hashtable? values = null)
    {
        var tester = new DataDictionaryTester(metadata);
        var result = tester.Insert(values);
        Assert.True(result.IsValid);
        return metadata;
    }
    
    public static Metadata AssertUpdate(this Metadata metadata, Hashtable values)
    {
        var tester = new DataDictionaryTester(metadata);
        var result = tester.Update(values);
        Assert.True(result.IsValid);
        return metadata;
    }
    
    public static Metadata AssertRead(this Metadata metadata, Hashtable? values = null)
    {
        var tester = new DataDictionaryTester(metadata);
        var result = tester.Read(values);
        Assert.True(result.IsValid);
        return metadata;
    }
    
    public static Metadata AssertDelete(this Metadata metadata, Hashtable values)
    {
        var tester = new DataDictionaryTester(metadata);
        var result = tester.Delete(values);
        Assert.True(result.IsValid);
        return metadata;
    }
}
