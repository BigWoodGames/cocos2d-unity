using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;

namespace BBGamelib{
	public class CCTMXProcessor  : AssetPostprocessor{

		static void OnPostprocessAllAssets (string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
		{
			for(int i=0; i<importedAssets.Length; i++){
				var asset = importedAssets[i];
				string assetExt = Path.GetExtension (asset);
				if (assetExt==null || !assetExt.Equals (".tmx"))
					continue;
//				AssetDatabase.ImportAsset (asset, ImportAssetOptions.ForceUpdate);
				string textureBase = asset.Replace(Path.GetFileName(asset), "");
				string txtPath = textureBase + Path.GetFileNameWithoutExtension(asset) + ".txt";
				//			dictionary.writeToFile (assetPath);
				if (File.Exists (txtPath))
                    File.Delete (txtPath);
                File.Copy (asset, txtPath);
				AssetDatabase.ImportAsset (txtPath);
				AssetDatabase.Refresh ();
            }
		}

//		void OnPreprocessTexture ()
//		{
//			//assetPath
//			string asset = assetPath;
//			string textureBase = asset.Replace(Path.GetFileName(asset), "");
//			string txtPath = textureBase + Path.GetFileNameWithoutExtension(asset) + "-tmx.txt";
//			//			dictionary.writeToFile (assetPath);
//			if (File.Exists (txtPath))
//				File.Delete (txtPath);
//			File.Copy (asset, txtPath);
//			
////			AssetDatabase.ImportAsset (txtPath);
////            AssetDatabase.Refresh ();
//        }
    }
}
