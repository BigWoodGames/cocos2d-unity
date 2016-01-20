using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System;
using System.IO;
using System.Text;

namespace BBGamelib{
	public class NSArray : List<object>, ICloneable
	{
		FileUtils.ZipDelegate _zip;
		public FileUtils.ZipDelegate zip{get{return _zip;} set{_zip=value;}}

		public NSArray(FileUtils.ZipDelegate zip=null){
			_zip = zip;
		}

		public static NSArray ArrayWithContentsOfFileFromResources(string path, FileUtils.ZipDelegate zip=null){
			string text = FileUtils.ReadTextFileFromResources (path);
			if (zip != null)
				text = zip.unZip (text);
			NSArray ary = ArrayWithContentsOfString (text);
			ary.zip = zip;
			return ary;
		}
		
		public static NSArray ArrayWithContentsOfFileFromStreamAssetss(string path, FileUtils.ZipDelegate zip=null){
			string text = FileUtils.ReadTextFileFromStreamAssets (path);
			if (zip != null)
				text = zip.unZip (text);			
			NSArray ary = ArrayWithContentsOfString (text);
			ary.zip = zip;
			return ary;
		}
		
		public static NSArray ArrayWithContentsOfFileInExternal(string path, FileUtils.ZipDelegate zip=null){
			string text = FileUtils.ReadTextFileFromExternal (path);
			if (zip != null)
				text = zip.unZip (text);
			NSArray ary = ArrayWithContentsOfString (text);
			ary.zip = zip;
			return ary;
		}
		
		public static NSArray ArrayWithContentsOfURL(string path, FileUtils.ZipDelegate zip=null){
			string text = FileUtils.ReadTextFileFromURL (path);
			if (zip != null)
				text = zip.unZip (text);
			NSArray ary = ArrayWithContentsOfString (text);
			ary.zip = zip;
			return ary;
		}
		
		public static NSArray ArrayWithContentsOfString(string text){
			if (text == null)
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
			if (rootNode.Name != "array")
				return null;
			return NSCollectionUtils.ParseArray (rootNode);
		}
		
		public static NSArray ArrayWithArray(NSArray array){
			NSArray ary = new NSArray ();
			ary.AddRange(ary);
			return ary;
		}
		
		public static NSArray ArrayWithObjects(object obj, params object[] args){
			NSArray ary = new NSArray ();
			ary.Add (obj);
			for (int i=0; i<args.Length; i++) {
				ary.Add (args[i]);
			}
			return ary;
		}
		
		public string convertToXml()
		{
			return NSCollectionUtils.ConvertToXml (this);
		}

		public void writeToFile(string path){
			string text = convertToXml ();
			FileUtils.WriteToFile (path, text, _zip);
		}

		public T firstObject<T>(){
			return objectAtIndex<T> (0);
		}

		public T lastObject<T>(){
			return objectAtIndex<T> (this.Count - 1);
		}

		public T objectAtIndex<T>(int index){
			if (index >= Count)
				throw new IndexOutOfRangeException ();
			object val = this [index];
			return convertToValue<T> (val);
		}
		

		T convertToValue<T>(object value){
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
			NSArray clone = NSArray.ArrayWithArray (this);
			return clone;
		}

		public override string ToString ()
		{
			StringBuilder sbuilder = new StringBuilder("NSArray:{"); 
			int count = this.Count;
			for(int i=0; i<count; i++){
				sbuilder.Append(this[i].ToString());
				sbuilder.Append(",");
			}
			sbuilder.Append ("}");
			return sbuilder.ToString();
		}
	}
}
