#if !UNITY_WEBPLAYER && !UNITY_WEBGL
#define USE_FileIO
#endif

using UnityEngine;
using System.Collections;
using System.IO;

namespace BBGamelib{
	public class FileUtils
	{
		public interface ZipDelegate{
			string zip(string text);
			string unZip(string text);
		}
		
		public static string ReadTextFileFromResources(string path, ZipDelegate zip=null){
			string result = null;
            if (Application.isPlaying) {
                string ext = Path.GetExtension(path);
                NSUtils.Assert(string.IsNullOrEmpty(ext) || ext == ".txt" || ext == ".json", "FileUtils: Text file in Resources folder must be '*.txt', '*.json' or none extends format!");
                if(!string.IsNullOrEmpty(ext))
				    path = path.Replace (ext, "");
				TextAsset txt = Resources.Load<TextAsset> (path);
				NSUtils.Assert (txt!=null, "No file found at {0}", path);
				result = txt.text;
			}else if (File.Exists (path)) {
//				result = System.IO.File.ReadAllText(path);
				result = ReadStringFromPath(path);
			}
			if (zip != null)
				result = zip.unZip (result);
			return result;
		}
		
		public static string ReadTextFileFromStreamAssets(string path, ZipDelegate zip=null){
			string filePath = System.IO.Path.Combine(Application.streamingAssetsPath, path);
			string result = null;
			if (filePath.Contains("://")) {
				result = ReadTextFileFromURL(filePath, zip);
			} else{
				result = ReadStringFromPath(filePath);
				if (zip != null)
					result = zip.unZip (result);
			}
			return result;
		}
		
		public static string ReadTextFileFromURL(string url, ZipDelegate zip=null){
			WWW www = new WWW(url);
			while(!www.isDone){}
			string text = www.text;
			if (zip != null)
				text = zip.unZip (text);
			return text;
		}
		
		//StreamingAssets
		public static byte[] ReadBytesFromStreamAssets(string file){
			string filePath = System.IO.Path.Combine(Application.streamingAssetsPath, file);
			byte[] result = null;
			if (filePath.Contains("://")) {
				WWW www = new WWW(filePath);
				while(!www.isDone){}
				result = www.bytes;
			} else
				result = System.IO.File.ReadAllBytes(filePath);
			return result;
		}

		#if USE_FileIO
		public static string ReadTextFileFromExternal(string path, ZipDelegate zip=null){
			string result = null;
			if (File.Exists (path)) {
				result = ReadStringFromPath(path);
			}
			if (zip != null)
				result = zip.unZip (result);
			return result;
		}
		
		public static void WriteToFile(string path, string text, ZipDelegate zip=null){
			CCDebug.Log ("FileUtils:writeToFile {0}", path);
			if (zip != null)
				text = zip.zip (text);
			if (File.Exists (path)) {
				File.Delete(path);
			}
			File.WriteAllText (path, text);
		}
		
		static string ReadStringFromPath(string path){
			string text = System.IO.File.ReadAllText(path);
			return text;
		}

		public static void DeleteFile(string path){
			File.Delete (path);
		}

        public static string GetFilePathWithoutExtends(string path){
            string ext = Path.GetExtension (path);
            if(ext!=null && ext.Length>0)
                path = path.Replace (ext, "");
            return path;
        }

		#else
		public static string ReadTextFileFromExternal(string path, ZipDelegate zip=null){
			string result = PlayerPrefs.GetString (path);
			if(result!=null && result.Trim().Length==0){
				result = null;
			}
			if (zip != null)
				result = zip.unZip (result);
			return result;
		}
		
		public static void WriteToFile(string path, string text, ZipDelegate zip=null){
			if (zip != null)
				text = zip.zip (text);
			PlayerPrefs.SetString (path, text);
		}

		
		static string ReadStringFromPath(string path){
			string text;
			using(var read = System.IO.File.OpenRead(path)) {
				using(var sr = new StreamReader(read)) {
					text = sr.ReadToEnd();
				}
			}
			return text;
		}

		public static void DeleteFile(string path){
			PlayerPrefs.DeleteKey (path);
		}
		#endif
	}
}

