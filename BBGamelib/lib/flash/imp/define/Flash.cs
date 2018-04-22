using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace BBGamelib.flash.imp{
	public class Flash
	{
		public const int LIB_VERSION = 0x00020001;
		public int version;
		public int flashVersion;
		public int frameRate;
		public Vector2 frameSize;
		public string prefix;
		public TagDefine[] chId_defs;
		public Dictionary<string, TagDefine> classNameDefs;
		public readonly DisplayFactory displayFactory;

		public Flash(string path, DisplayFactory aFactory){
			displayFactory = aFactory;
			string ext = Path.GetExtension (path);
			if(ext!=null && ext.Length>0)
				path = path.Replace (ext, "");
			TextAsset asset = Resources.Load<TextAsset> (path);
			NSUtils.Assert (asset != null, "BBGamelib:flash:{0} file not found!", path);

			byte[] data = asset.bytes;

			Cursor cursor = new Cursor ();
			readHeader (data, cursor);
			readDefines (data, cursor);
			Resources.UnloadAsset (asset);
		}
		
		void readHeader(byte[] data, Cursor cursor){
			int len = Utils.ReadLength (data, cursor);
			int newIndex = cursor.index + len;

			version = Utils.ReadInt32(data, cursor);

			int supportedVersion = Utils.ReadInt32 (data, cursor);
			NSUtils.Assert (LIB_VERSION >= supportedVersion, "BBGamelib:flash: library {0} is not supported by swf parser {1} ",
			                versionToString(LIB_VERSION), versionToString(supportedVersion)); 

			flashVersion = Utils.ReadByte (data, cursor);
			frameRate = Utils.ReadByte (data, cursor);
			frameSize = Utils.ReadVector2 (data, cursor);
			prefix = Utils.ReadString (data, cursor);

			cursor.index = newIndex;
		}
		void readDefines(byte[] data, Cursor cursor){
			int len = Utils.ReadLength (data, cursor);
			int newIndex = cursor.index + len;

			int maxcharacterId = Utils.ReadInt32 (data, cursor);
			int definesCount = Utils.ReadInt32 (data, cursor);

			chId_defs = new TagDefine[maxcharacterId + 1];
			classNameDefs = new Dictionary<string, TagDefine> (definesCount/4);

			for (int i=0; i<definesCount; i++) {
				TagDefine define = parseDefine(this, data, cursor);
				
				if(define!=null){
					chId_defs[define.characterId] = define;
					if(define is TagDefine && (define as TagDefine).className!=null){
						classNameDefs[(define as TagDefine).className] = define;
					}
				}
			}
			cursor.index = newIndex;
		}

		public TagDefine parseDefine(Flash flash, byte[] data, Cursor cursor){
			//find nextIndex
			int dataLength = Utils.ReadInt32 (data, cursor);
			int nextIndex = cursor.index + dataLength;
			
			//parse
			byte type = Utils.ReadByte(data, cursor);
			TagDefine def = null;
			if(type==TagDefine.DEF_TYPE_GRAPHIC){
				def = new TagDefineGraphic(flash, data, cursor);
			}else if(type==TagDefine.DEF_TYPE_SPRITE){
				def = new TagDefineMovie(flash, data, cursor);
			}

			//nextIndex
			cursor.index = nextIndex;
			
			return def;
		}

		public TagDefine getDefine(int characterId){
			return chId_defs [characterId];
		}

		public TagDefine getDefine(string className){
			TagDefine define;
			classNameDefs.TryGetValue (className, out define);
			return define;
		}

		public string trace(int indent=0){
			string indent0 = Utils.RepeatString(indent);
			string indent2 = Utils.RepeatString(indent + 2);
			string indent4 = Utils.RepeatString(indent + 4);

			string s = indent0 + "[SWF:"+ prefix+ "]\n" +
				indent2 + "Header:\n" +
					indent4 + "Version: " + version + "\n" +
					indent4 + "FlashVersion: " + flashVersion + "\n" +
					indent4 + "FrameRate: " + frameRate + "\n" +
					indent4 + "FrameSize: " + frameSize + "\n" +
					indent4 + "FrameRate: " + frameRate + "\n";
			int count = 0;
			string defineString = "";
			for(int i=0; i<chId_defs.Length; i++){
				TagDefine define = chId_defs[i];
				if(define != null){
					count ++;
					defineString += define.trace(indent + 4);
				}
			}
			
			s += indent2 + "Defines(" + chId_defs.Length + "):\n";
			s += defineString;
			return s;
		}

		string versionToString(int version){
			StringBuilder sb = new StringBuilder();
			sb.Append (((version & 0x00FF0000) >> 16));
			sb.Append (".");
			sb.Append (((version & 0x0000FF00) >> 8));
			sb.Append (".");
			sb.Append (version & 0x000000FF);
			return sb.ToString();
		}
	}
}

