using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Serialization;

namespace JJMasterData.Commons.Util;

public class XmlHelper
{
    private static string RemoveText(Match m) { return ""; }

    public static T Deserialize<T>(string xml)
    {
        if (xml == null)
            throw new ArgumentNullException(nameof(xml), "Xml to Deserialize");

        object oRet = null;
        string cleanXml = Regex.Replace(xml, @"<[a-zA-Z].[^(><.)]+/>", RemoveText);
        XmlSerializer serializer = new XmlSerializer(typeof(T));
        using (StringReader reader = new StringReader(cleanXml))
        {
            oRet = serializer.Deserialize(reader);
        }

        return (T)oRet;
    }


    public static string Serialize(object value)
    {
        if (value == null)
            throw new ArgumentNullException(nameof(value), "Serialize");

        var stringwriter = new Utf8StringWriter();
        var serializer = new XmlSerializer(value.GetType());
        serializer.Serialize(stringwriter, value);
        return stringwriter.ToString();
    }

    public class Utf8StringWriter : StringWriter
    {
        public override Encoding Encoding
        {
            get { return Encoding.UTF8; }
        }
    }

}