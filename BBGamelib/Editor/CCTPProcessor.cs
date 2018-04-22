using UnityEditor;
using System.IO;
using BBGamelib;
using UnityEngine;
using System;
using System.Collections.Generic;

namespace BBGamelib{
	public class CCTPProcessor  : AssetPostprocessor{
		/*
	 	 *  renamed plist file of texture packer to .plist.txt
		 */
		static void OnPostprocessAllAssets (string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
		{
            bool hasNewAsset = false;
			foreach (var asset in importedAssets) {
				string assetExt = Path.GetExtension (asset);
                if (assetExt == ".plist")
                {
                    NSDictionary plist = NSDictionary.DictionaryWithContentsOfFileFromResources(asset);
                    NSDictionary metadataDict = plist.objectForKey<NSDictionary>("metadata");
                    if (metadataDict != null)
                    {
                        string textureFileName = metadataDict.objectForKey<string>("textureFileName");
                        string texturePath = asset.Replace(Path.GetFileName(asset), "") + textureFileName;
                        if (File.Exists(texturePath))
                        {
                            byte[] fileData = File.ReadAllBytes(texturePath);
                            Texture2D texture = new Texture2D(2, 2);
                            texture.LoadImage(fileData); 
                            CheckSemiTransparentSprite(plist, texture);
                            //update *.plist.txt
                            string txtPath = asset.Replace(".plist", ".txt");
//                            if (File.Exists (txtPath))
                            plist.writeToFile(txtPath);
                            File.Delete (asset);
//                            File.Move (asset, txtPath);
                            hasNewAsset = true;
                        }
                    }
                }
            }
            if(hasNewAsset)
                AssetDatabase.Refresh ();
		}

        static void CheckSemiTransparentSprite(NSDictionary dictionary, Texture2D texture)
        {
            NSDictionary metadataDict = dictionary.objectForKey<NSDictionary>("metadata");
            NSDictionary framesDict = dictionary.objectForKey<NSDictionary>("frames");

            // get the format
            int format = 0;
            if (metadataDict != null) {
                format = metadataDict.objectForKey<int> ("format");
            }

            // get texture size
            Vector2 textureSize = new Vector2(texture.width, texture.height);

            // check the format
            NSUtils.Assert( format >= 0 && format <= 3, @"cocos2d: WARNING: format is not supported for CCSpriteFrameCache addSpriteFramesWithDictionary:texture:");

            // SpriteFrame info
            Rect rect = new Rect();
            bool textureRotated = false;

            // add real frames
            var enumerator = framesDict.GetEnumerator();
            while (enumerator.MoveNext()) {
                KeyValuePair<object, object> frameDictKeyValue = enumerator.Current;
                NSDictionary frameDict = (NSDictionary)frameDictKeyValue.Value;
                if(format == 0) 
                {
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
                    textureRotated = false;
                } else if(format == 1 || format == 2) {
                    Rect frame = ccUtils.RectFromString(frameDict.objectForKey<string>("frame"));
                    bool rotated = false;

                    // rotation
                    if(format == 2)
                        rotated = frameDict.objectForKey<bool>("rotated");
                    
                    // set frame info
                    rect = frame;
                    textureRotated = rotated;
                } else if(format == 3) {
                    // get values
                    Vector2 spriteSize = ccUtils.PointFromString(frameDict.objectForKey<string>("spriteSize"));
                    Rect textureRect = ccUtils.RectFromString(frameDict.objectForKey<string>("textureRect"));
                    bool textureRotated_ = frameDict.objectForKey<bool>("textureRotated");


                    // set frame info
                    rect = new Rect(textureRect.position.x, textureRect.position.y, spriteSize.x, spriteSize.y);
                    textureRotated = textureRotated_;
                }
                if (textureRotated)
                {
                    rect.size = new Vector2(rect.size.y, rect.size.x);
                }
                rect.y = textureSize.y - rect.y - rect.height;

                // add sprite frame
                int rectX = Mathf.RoundToInt(rect.xMin);
                int rectY = Mathf.RoundToInt(rect.yMin);
                int rectW = Mathf.RoundToInt(rect.width);
                int rectH = Mathf.RoundToInt(rect.height);
                bool isSemi = false;
                for (int x = 0; x < rectW; x++)
                {
                    for (int y = 0; y < rectH; y++)
                    {
                        Color color = texture.GetPixel(rectX + x, rectY + y);
                        if (FloatUtils.Big(color.a, 0) && FloatUtils.Small(color.a, 1))
                        {
                            isSemi = true;
                            break;
                        }
                    }
                    if (isSemi)
                    {
                        break;
                    }
                }
                frameDict.Add("semi", isSemi);
            }
        }
	}
}
