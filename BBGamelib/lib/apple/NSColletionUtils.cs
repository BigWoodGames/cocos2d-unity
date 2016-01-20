using System.Xml;
using System;
using System.IO;
using System.Collections.Generic;


namespace BBGamelib{
	public class NSCollectionUtils{

		#region static read functions
		public static NSDictionary ParseDictionary(XmlNode node)
		{
			XmlNodeList children = node.ChildNodes;
			if (children.Count % 2 != 0)
			{
				throw new DataMisalignedException("Dictionary elements must have an even number of child nodes");
			}
			
			NSDictionary dict = new NSDictionary();
			
			for (int i = 0; i < children.Count; i += 2)
			{
				XmlNode keynode = children[i];
				XmlNode valnode = children[i + 1];
				
				if (keynode.Name != "key")
				{
					throw new ApplicationException("expected a key node");
				}
				
				object result = Parse(valnode);
				
				if (result != null)
                {
                    dict[keynode.InnerText] = result;
                    
                }
            }
            
            return dict;
		}

		public static NSArray ParseArray(XmlNode node)
		{
			NSArray array = new NSArray();
			int count = node.ChildNodes.Count;
			for (int i = 0; i < count; i++) {
				object result = Parse(node.ChildNodes[i]);
				if (result != null)
				{
					array.Add(result);
				}		
			}
            return array;
        }
        
        static object Parse(XmlNode node)
        {
            switch (node.Name) {
                case "dict":
                    return ParseDictionary (node);
                case "array":
                    return ParseArray (node);
			case "string":
				return node.InnerText;
			case "integer":
				//  int result;
				return (int)(Convert.ToDouble (node.InnerText, System.Globalization.NumberFormatInfo.InvariantInfo));
			case "real":
				return (float)(Convert.ToDouble (node.InnerText, System.Globalization.NumberFormatInfo.InvariantInfo));
			case "false":
				return false;
			case "true":
				return true;
			case "null":
				return null;
			case "date":
				if(node.InnerText == "1-01-01T00:00:00Z")
					return XmlConvert.ToDateTime ("0001-01-01T00:00:00Z", XmlDateTimeSerializationMode.Utc);
                    else
                        return XmlConvert.ToDateTime (node.InnerText, XmlDateTimeSerializationMode.Utc);
                case "data":
                    return Convert.FromBase64String (node.InnerText);
            }
            throw new ApplicationException(String.Format("Plist Node `{0}' is not supported", node.Name));
        }
		#endregion
		
		#region static write functions
		public static string ConvertToXml(object value)
		{
			using (MemoryStream ms = new MemoryStream())
			{
				XmlWriterSettings xmlWriterSettings = new XmlWriterSettings();
				xmlWriterSettings.Encoding = new System.Text.UTF8Encoding(false);
				xmlWriterSettings.ConformanceLevel = ConformanceLevel.Document;
				xmlWriterSettings.Indent = true;
				
				using (XmlWriter xmlWriter = XmlWriter.Create(ms, xmlWriterSettings))
				{
					xmlWriter.WriteStartDocument(); 
					//xmlWriter.WriteComment("DOCTYPE plist PUBLIC \"-//Apple//DTD PLIST 1.0//EN\" " + "\"http://www.apple.com/DTDs/PropertyList-1.0.dtd\"");
					xmlWriter.WriteDocType("plist", "-//Apple Computer//DTD PLIST 1.0//EN", "http://www.apple.com/DTDs/PropertyList-1.0.dtd", null);
					xmlWriter.WriteStartElement("plist");
					xmlWriter.WriteAttributeString("version", "1.0");
					Compose(value, xmlWriter);
					xmlWriter.WriteEndElement();
					xmlWriter.WriteEndDocument();
					xmlWriter.Flush();
					xmlWriter.Close();
					return System.Text.Encoding.UTF8.GetString(ms.ToArray());
				}
			}
		}
		static void Compose(object value, XmlWriter writer)
		{
			
			if (value is string)
			{
				writer.WriteElementString("string", value as string);
			}
			else if (value is int || value is long)
			{
				writer.WriteElementString("integer", ((int)value).ToString(System.Globalization.NumberFormatInfo.InvariantInfo));
			}
			else if (value is NSDictionary)
			{
				NSDictionary dic = value as NSDictionary;
				WriteDictionaryValues(dic, writer);
			}
			else if (value is NSArray)
			{
				ComposeArray((NSArray)value, writer);
			}
			else if (value is byte[])
			{
				writer.WriteElementString("data", Convert.ToBase64String((Byte[])value));
			}
			else if ( value is double)
			{
				writer.WriteElementString("real", ((double)value).ToString(System.Globalization.NumberFormatInfo.InvariantInfo));
			}
			else if (value is float)
			{
				writer.WriteElementString("real", ((float)value).ToString(System.Globalization.NumberFormatInfo.InvariantInfo));
			}
			else if (value is DateTime)
			{
				DateTime time = (DateTime)value;
				string theString = XmlConvert.ToString(time, XmlDateTimeSerializationMode.Utc);
				theString = theString.Substring(0, 19) + "Z";
				writer.WriteElementString("date", theString);//, "yyyy-MM-ddTHH:mm:ssZ"));
			}
			else if (value is bool)
			{
				writer.WriteElementString(value.ToString().ToLower(), "");
			}else if (value == null)
			{
			}
			else
			{
				throw new Exception(String.Format("Value type '{0}' is unhandled", value.GetType().ToString()));
			}
		}		
		static void WriteDictionaryValues(NSDictionary dictionary, XmlWriter writer)
		{
			writer.WriteStartElement("dict");

			var enumerator = dictionary.GetEnumerator();
			while (enumerator.MoveNext()) {
				KeyValuePair<object, object> kv = enumerator.Current;
				object value = kv.Value;
				if(value!=null){
					writer.WriteElementString("key", kv.Key.ToString());
					Compose(value, writer);
				}
			}

			writer.WriteEndElement();
		}
		static void ComposeArray(NSArray value, XmlWriter writer)
		{
			writer.WriteStartElement("array");

			var enumerator = value.GetEnumerator();
			while (enumerator.MoveNext()) {
				var obj = enumerator.Current;
				Compose(obj, writer);
			}

			writer.WriteEndElement();
		}
		#endregion

    }
}
