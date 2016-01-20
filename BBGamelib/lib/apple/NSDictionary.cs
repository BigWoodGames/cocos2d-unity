#if !UNITY_WEBPLAYER && !UNITY_WEBGL
#define USE_FileIO
#endif

using System.Text;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System;
using System.IO;

namespace BBGamelib{
	public class NSDictionary : Dictionary<object, object>, ICloneable{
		FileUtils.ZipDelegate _zip;
		public FileUtils.ZipDelegate zip{get{return _zip;} set{_zip=value;}}

		public NSDictionary(FileUtils.ZipDelegate zip=null){
			_zip = zip;
		}

		public static NSDictionary DictionaryWithContentsOfFileFromResources(string path, FileUtils.ZipDelegate zip=null){
			string text = FileUtils.ReadTextFileFromResources (path);
			if(zip!=null)
				text = zip.unZip (text);
			NSDictionary dict = DictionaryWithContentsOfString (text, zip);
			return dict;
		}

		public static NSDictionary DictionaryWithContentsOfFileFromStreamAssetss(string path, FileUtils.ZipDelegate zip=null){
			string text = FileUtils.ReadTextFileFromStreamAssets (path);
			if(zip!=null)
				text = zip.unZip (text);
			return DictionaryWithContentsOfString (text, zip);
        }

		public static NSDictionary DictionaryWithContentsOfFileFromExternal(string path, FileUtils.ZipDelegate zip=null){
			string text = FileUtils.ReadTextFileFromExternal (path);
			if(zip!=null)
				text = zip.unZip (text);
			return DictionaryWithContentsOfString (text, zip);
        }
		
		public static NSDictionary DictionaryWithContentsOfURL(string path, FileUtils.ZipDelegate zip=null){
			string text = FileUtils.ReadTextFileFromURL (path);
			if(zip!=null)
				text = zip.unZip (text);
			return DictionaryWithContentsOfString (text, zip);
        }
        
		public static NSDictionary DictionaryWithContentsOfString(string text){
			if (text == null||text.Length == 0)
				return null;
			
			text = System.Text.RegularExpressions.Regex.Replace(text, "<.*\\.dtd\">", string.Empty);
			XmlReaderSettings settings = new XmlReaderSettings();
			settings.ProhibitDtd = false;
			settings.ValidationType = ValidationType.None;
			XmlDocument xmlDoc = new XmlDocument();
			using (StringReader sr = new StringReader(text))
				using (XmlReader reader = XmlReader.Create(sr, settings))
			{
				xmlDoc.Load(reader);
			}

//			XmlDocument xmlDoc = new XmlDocument();
//			xmlDoc.LoadXml (text);

			XmlNode rootNode = xmlDoc.DocumentElement.ChildNodes[0];
			if (rootNode.Name != "dict")
				return null;
			NSDictionary dict = NSCollectionUtils.ParseDictionary (rootNode);
			return dict;
        }

		static NSDictionary DictionaryWithContentsOfString(string text, FileUtils.ZipDelegate zip=null){
			if (text == null||text.Length == 0)
				return null;
			try{
				text = System.Text.RegularExpressions.Regex.Replace(text, "<.*\\.dtd\">", string.Empty);
				XmlReaderSettings settings = new XmlReaderSettings();
				settings.ProhibitDtd = false;
				settings.ValidationType = ValidationType.None;
				XmlDocument xmlDoc = new XmlDocument();
				using (StringReader sr = new StringReader(text))
					using (XmlReader reader = XmlReader.Create(sr, settings))
				{
					xmlDoc.Load(reader);
				}

//				XmlDocument xmlDoc = new XmlDocument();
//				xmlDoc.LoadXml (text);

				XmlNode rootNode = xmlDoc.DocumentElement.ChildNodes[0];
				if (rootNode.Name != "dict")
					return null;
				NSDictionary dict = NSCollectionUtils.ParseDictionary (rootNode);			
				if(dict!=null)
					dict.zip = zip;
				return dict;
			}catch(Exception e){
				CCDebug.Warning("NSDicitonary:DictionaryWithContentsOfString:Error:{0}", e);
				return null;
			}
		}

		public static NSDictionary DictionaryWithDictionary(NSDictionary dictionary){
			NSDictionary dict = new NSDictionary ();
			var enumerator = dictionary.GetEnumerator();
			while (enumerator.MoveNext()) {
				KeyValuePair<object, object> kv = enumerator.Current;
				dict.Add(kv.Key, kv.Value);
			}
			return dict;
		}

		public static NSDictionary DictionaryWithObjectsAndKeys(object value, object key, params object[] args){
			NSDictionary dict = new NSDictionary ();
			dict [key] = value;		
			if (args.Length % 2 != 0)
			{
				throw new DataMisalignedException("Dictionary elements must have an even number of child nodes");
            }
            
            for (int i=0; i<args.Length-1; i+=2) {
				dict[args[i+1]] = args[i];
			}
			return dict;
		}

		public string convertToXml()
		{
			return NSCollectionUtils.ConvertToXml (this);
		}
				
		public void writeToFile(string path){
			string text = convertToXml ();
			FileUtils.WriteToFile (path, text, _zip);
		}


		public 	T objectForKey<T>(object key){
			object value;
			if (!TryGetValue (key, out value)) {
				value = default(T);
				return (T)value;
			}
			bool isNumberic = false;
			switch (Type.GetTypeCode(typeof(T)))
			{
			case TypeCode.Byte:
			case TypeCode.SByte:
			case TypeCode.UInt16:
			case TypeCode.UInt32:
			case TypeCode.UInt64:
			case TypeCode.Int16:
			case TypeCode.Int32:
			case TypeCode.Int64:
			case TypeCode.Decimal:
			case TypeCode.Double:
			case TypeCode.Single:
				isNumberic = true;
				break;
			default:
				isNumberic = false;
				break;
			}
			
			if (isNumberic) {
				T t  = (T) Convert.ChangeType(value, typeof(T));
				return t;	
			} else {
				return (T)value;
			}
		}

		public object Clone()
		{
			NSDictionary clone = NSDictionary.DictionaryWithDictionary (this);
			return clone;
		}

		public override string ToString ()
		{
			StringBuilder sb = new StringBuilder ("NSDictionary:{");
			var enumerator = this.GetEnumerator();
			while (enumerator.MoveNext()) {
				KeyValuePair<object, object> kv = enumerator.Current;
				sb.Append(kv.Key);
				sb.Append("=");
				sb.Append(kv.Value);
				sb.Append(",");
			}
			sb.Append ("}");
			return sb.ToString();
		}
	}
}

