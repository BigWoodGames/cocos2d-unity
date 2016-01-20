using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System;

namespace BBGamelib{
	public class CCTPProcessor  : AssetPostprocessor{
//		static string[] textureExtensions = {
//			".png",
//			".jpg",
//			".jpeg",
//			".tiff",
//			".tga",
//			".bmp"
//		};
		/*
	 	 *  Trigger a texture file re-import each time the .tpsheet file changes (or is manually re-imported)
		 */
		static void OnPostprocessAllAssets (string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
		{
//			foreach (var asset in importedAssets) {
//				string assetExt = Path.GetExtension (asset);
//				if (assetExt==null || !assetExt.Equals (".plist"))
//					continue;
//				foreach (string ext in textureExtensions) {
//					string pathToTexture = Path.ChangeExtension (asset, ext);
//					if (File.Exists (pathToTexture)) {
//						AssetDatabase.ImportAsset (pathToTexture, ImportAssetOptions.ForceUpdate);
//						break;
//					}
//				}
//			}
		}
		
		/*
		 *  Trigger a sprite sheet update each time the texture file changes (or is manually re-imported)
	 	 */
		void OnPreprocessTexture ()
		{
			string plist = Path.ChangeExtension (assetPath, ".plist");
			if (File.Exists (plist)) {
				analyze(assetPath, plist);
			}
		}
		void analyze(string image, string plist){
			NSDictionary dictionary = NSDictionary.DictionaryWithContentsOfFileFromResources(plist);
			NSDictionary metadataDict = dictionary.objectForKey<NSDictionary>("metadata");
			NSDictionary framesDict = dictionary.objectForKey<NSDictionary>("frames");

			Vector2 atlasSize;
			int format = 0;
			// get the format
			if (metadataDict != null) {
				format = metadataDict.objectForKey<int> ("format");
				atlasSize = ccUtils.PointFromString (metadataDict.objectForKey<string> ("size"));
			} else {
				NSDictionary texture = dictionary.objectForKey<NSDictionary>("texture");
				float width = texture.objectForKey<float>("width");
				float height = texture.objectForKey<float>("height");
				atlasSize = new Vector2(width, height);
			}

			
			// SpriteFrame info
			Rect rect = new Rect();
			bool isRotated = false;
			
			List<SpriteMetaData> metaData = new List<SpriteMetaData> ();

			// add real frames
			
			var enumerator = framesDict.GetEnumerator();
			while (enumerator.MoveNext()) {
				KeyValuePair<object, object> frameDictKeyValue = enumerator.Current;
				string frameDictKey = (string)frameDictKeyValue.Key;
				NSDictionary frameDict = (NSDictionary)frameDictKeyValue.Value;
				if(format == 0) {
					float x = frameDict.objectForKey<float>("x");
					float y = frameDict.objectForKey<float>("y");
					float w = frameDict.objectForKey<float>("width");
					float h = frameDict.objectForKey<float>("height");
					int ow = frameDict.objectForKey<int>("originalWidth");
					int oh = frameDict.objectForKey<int>("originalHeight");
					// check ow/oh
					if(ow==0 || oh==0)
						CCDebug.Warning("cocos2d: WARNING: originalWidth/Height not found on the CCSpriteFrame. AnchorPoint won't work as expected. Regenerate the .plist");
					
					// abs ow/oh
					ow = Math.Abs(ow);
					oh = Math.Abs(oh);
					
					// set frame info
					rect = new Rect(x, y, w, h);
					isRotated = false;
				} else if(format == 1 || format == 2) {
					Rect frame = ccUtils.RectFromString(frameDict.objectForKey<string>("frame"));
					bool rotated = false;
					
					// rotation
					if(format == 2)
						rotated = frameDict.objectForKey<bool>("rotated");

					
					// set frame info
					rect = frame;
					isRotated = rotated;
				} else if(format == 3) {
					// get values
					Vector2 spriteSize = ccUtils.PointFromString(frameDict.objectForKey<string>("spriteSize"));
					Rect textureRect = ccUtils.RectFromString(frameDict.objectForKey<string>("textureRect"));
					bool textureRotated = frameDict.objectForKey<bool>("textureRotated");

					// set frame info
					rect = new Rect(textureRect.position.x, textureRect.position.y, spriteSize.x, spriteSize.y);
					isRotated = textureRotated;
				}
				
				if(isRotated)
					rect.size = new Vector2(rect.size.y, rect.size.x);

				// add sprite metadata
				{
					SpriteMetaData smd = new SpriteMetaData ();
					smd.name = frameDictKey;
					
					rect.y = atlasSize.y - rect.y - rect.height;
					smd.rect = rect;
					smd.alignment = (int)UnityEngine.SpriteAlignment.Center;
					smd.pivot = smd.rect.center;
					metaData.Add (smd);
				}
			}

			//update texutre importer
			TextureImporter importer = assetImporter as TextureImporter;
			if (importer.textureType != TextureImporterType.Advanced) {
				importer.textureType = TextureImporterType.Sprite;
			}
			importer.maxTextureSize = 4096;
			importer.spriteImportMode = SpriteImportMode.Multiple;
			importer.spritesheet = metaData.ToArray();

			//update #tp.txt
			string textureBase = plist.Replace(Path.GetFileName(plist), "");
			string txtPath = textureBase + Path.GetFileNameWithoutExtension(plist) + "-tp.txt";
			//			dictionary.writeToFile (assetPath);
			if (File.Exists (txtPath))
				File.Delete (txtPath);
			File.Move (plist, txtPath);
            
			//update unity edior widnow
			AssetDatabase.ImportAsset (txtPath);
			AssetDatabase.Refresh ();

			//update asset info
//			CCTPData data = ScriptableObject.CreateInstance<CCTPData> ();
//			data.frames = spriteFrames;
//			data.aliases = spriteFramesAliases;
//
//			string textureBase = plist.Replace(Path.GetFileName(plist), "");
//			string assetPath = textureBase + Path.GetFileNameWithoutExtension(plist) + "#tp.asset";
////			string path = Path.ChangeExtension (plist, "tp") + ".asset";
//			AssetDatabase.CreateAsset (data, assetPath);
//			AssetDatabase.SaveAssets ();
		}
	}
}
