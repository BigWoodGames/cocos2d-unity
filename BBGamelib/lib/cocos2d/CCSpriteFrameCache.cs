using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;

namespace BBGamelib{
//	public class CCSpriteFrameCache : MonoBehaviour
//	{
//		#region singleton
//		[SerializeField] [HideInInspector]private bool firstPassFlag=true;
//		static CCSpriteFrameCache _Instance=null;
//		//---------singleton------
//		public static CCSpriteFrameCache sharedSpriteFrameCache{
//			get{
//				return _Instance;
//			}
//		}
//		public virtual void Awake() {
//			if (Application.isPlaying) {
//				if (_Instance != null && _Instance != this) {
//					Destroy (this.gameObject);
//					return;
//				} else {
//					_Instance = this;
//				}
//				DontDestroyOnLoad (this.gameObject);
//            } 
//            if (firstPassFlag) {
//                gameObject.transform.position = Vector3.zero;
//				gameObject.name = "CCSpriteFrameCache";
//                firstPassFlag = false;
//            }
//        }
//        #endregion
//        
//    }
    



    public class CCSpriteFrameCache
	{
		Dictionary<string, CCSpriteFrame> _spriteFrames;
		Dictionary<string, string> _spriteFramesAliases;
		HashSet<string>		_loadedFilenames;

		#region mark CCSpriteFrameCache - Alloc, Init & Dealloc
		static CCSpriteFrameCache _sharedSpriteFrameCache=null;

		/** Retruns ths shared instance of the Sprite Frame cache */
		public static CCSpriteFrameCache sharedSpriteFrameCache
        {
            get{
                if (_sharedSpriteFrameCache==null)
					_sharedSpriteFrameCache = new CCSpriteFrameCache ();
                
                return _sharedSpriteFrameCache;
            }
        }
		/** Purges the cache. It releases all the Sprite Frames and the retained instance.*/
		public static void PurgeSharedSpriteFrameCache()
		{
			_sharedSpriteFrameCache = null;
		}

		CCSpriteFrameCache(){
			_spriteFrames = new Dictionary<string, CCSpriteFrame> ();
			_spriteFramesAliases = new Dictionary<string, string> ();
			_loadedFilenames = new HashSet<string> ();
		}
		
		public override string ToString ()
		{
			return string.Format("<{0} = {1} | num of sprite sheets =  {2}>", this.GetType().Name, this.GetHashCode(), _spriteFrames.Count);
		}
        #endregion
		
		#region mark CCSpriteFrameCache - loading sprite frames
		void addSpriteFrames(NSDictionary dictionary, Sprite[] sprites){
			Dictionary<string ,Sprite> spritesDict = new Dictionary<string, Sprite> ();
			for(int i=0; i<sprites.Length; i++){
				Sprite s = sprites[i];
				spritesDict.Add(s.name, s);			
			}

			NSDictionary metadataDict = dictionary.objectForKey<NSDictionary>("metadata");
			NSDictionary framesDict = dictionary.objectForKey<NSDictionary>("frames");

			int format = 0;
			// get the format
			if (metadataDict != null) {
				format = metadataDict.objectForKey<int> ("format");
			}
			
			
			// SpriteFrame info
//			Rect rect = new Rect();
			bool isRotated = false;
			Vector2 frameOffset = Vector2.zero;
			Vector2 originalSize = Vector2.zero;
			
			// add real frames
			
			var enumerator = framesDict.GetEnumerator();
			while (enumerator.MoveNext()) {
				KeyValuePair<object, object> frameDictKeyValue = enumerator.Current;
				string frameDictKey = (string)frameDictKeyValue.Key;
				NSDictionary frameDict = (NSDictionary)frameDictKeyValue.Value;
				CCSpriteFrame spriteFrame=null;
				if(format == 0) {
//					float x = frameDict.objectForKey<float>("x");
//					float y = frameDict.objectForKey<float>("y");
//					float w = frameDict.objectForKey<float>("width");
//					float h = frameDict.objectForKey<float>("height");
					float ox = frameDict.objectForKey<float>("offsetX");
					float oy = frameDict.objectForKey<float>("offsetY");
					int ow = frameDict.objectForKey<int>("originalWidth");
					int oh = frameDict.objectForKey<int>("originalHeight");
					// check ow/oh
					if(ow==0 || oh==0)
						CCDebug.Warning("cocos2d: WARNING: originalWidth/Height not found on the CCSpriteFrame. AnchorPoint won't work as expected. Regenerate the .plist");
					
					// abs ow/oh
					ow = Math.Abs(ow);
					oh = Math.Abs(oh);
					
					// set frame info
//					rect = new Rect(x, y, w, h);
					isRotated = false;
					frameOffset = new Vector2(ox, oy);
					originalSize = new Vector2(ow, oh);
					
//					if(isRotated)
//						rect.size = new Vector2(rect.size.y, rect.size.x);
				} else if(format == 1 || format == 2) {
//					Rect frame = ccUtils.RectFromString(frameDict.objectForKey<string>("frame"));
					bool rotated = false;
					
					// rotation
					if(format == 2)
						rotated = frameDict.objectForKey<bool>("rotated");
					
					Vector2 offset = ccUtils.PointFromString(frameDict.objectForKey<string>("offset"));
					Vector2 sourceSize = ccUtils.PointFromString(frameDict.objectForKey<string>("sourceSize"));
					
					// set frame info
//					rect = frame;
					isRotated = rotated;
					frameOffset = offset;
					originalSize = sourceSize;
				} else if(format == 3) {
					// get values
//					Vector2 spriteSize = ccUtils.PointFromString(frameDict.objectForKey<string>("spriteSize"));
					Vector2 spriteOffset = ccUtils.PointFromString(frameDict.objectForKey<string>("spriteOffset"));
					Vector2 spriteSourceSize = ccUtils.PointFromString(frameDict.objectForKey<string>("spriteSourceSize"));
//					Rect textureRect = ccUtils.RectFromString(frameDict.objectForKey<string>("textureRect"));
					bool textureRotated = frameDict.objectForKey<bool>("textureRotated");
					
					// get aliases
					NSArray aliases = frameDict.objectForKey<NSArray>("aliases");
					
					var aliasesEnumerator = aliases.GetEnumerator();
					while (aliasesEnumerator.MoveNext()) {
						string alias = (string)aliasesEnumerator.Current;
						if( _spriteFramesAliases.ContainsKey(alias) )
							CCDebug.Warning("cocos2d: WARNING: an alias with name {0} already exists",alias);
						
						_spriteFramesAliases[alias] = frameDictKey;
					}
					
					// set frame info
//					rect = new Rect(textureRect.position.x, textureRect.position.y, spriteSize.x, spriteSize.y);
					isRotated = textureRotated;
					frameOffset = spriteOffset;
					originalSize = spriteSourceSize;
				}
				
				Sprite spt;
				if(!spritesDict.TryGetValue(frameDictKey, out spt)){
					CCDebug.Warning("cocos2d: WARNING: a sprite frame with name {0} not found", frameDictKey);
					continue;
				}
				// add sprite frame
				spriteFrame = new CCSpriteFrame(spt, originalSize, frameOffset, isRotated);
				_spriteFrames.Add(frameDictKey, spriteFrame);
			}
		}


		/** Adds multiple Sprite Frames from a plist file.
		 * A texture will be loaded automatically. The texture name will composed by replacing the .plist suffix with .png .
		 * If you want to use another texture, you should use the addSpriteFramesWithFile:texture method.
		 */
		public void addSpriteFramesWithFile(string path){
			string ext = Path.GetExtension (path);
			if(ext!=null && ext.Length>0)
				path = path.Replace (ext, "");
			if (_loadedFilenames.Contains (path)){
				CCDebug.Info ("cocos2d: CCSpriteFrameCache: file already loaded: {0}", path);
				return;
			}else 
				_loadedFilenames.Add (path);
            
			string cfgPath = path;
			if(!cfgPath.EndsWith("-tp"))
				cfgPath += "-tp.txt";
			NSDictionary dict = NSDictionary.DictionaryWithContentsOfFileFromResources(cfgPath);

			string texturePath = path;
			Sprite[] sprites = Resources.LoadAll<Sprite> (texturePath);
			addSpriteFrames(dict, sprites);
		}

		
		/** Adds an sprite frame with a given name.
		 If the name already exists, then the contents of the old name will be replaced with the new one.
		 */
		public void addSpriteFrame(CCSpriteFrame frame){
			_spriteFrames[frame.textureFilename] = frame;
		}
		#endregion
		
		
		#region mark CCSpriteFrameCache - removing
		/** Purges the dictionary of loaded sprite frames.
		 * Call this method if you receive the "Memory Warning".
		 * In the short term: it will free some resources preventing your app from being killed.
		 * In the medium term: it will allocate more resources.
		 * In the long term: it will be the same.
		 */
		public void removeSpriteFrames(){
			_spriteFrames.Clear ();
			_spriteFramesAliases.Clear ();
			_loadedFilenames.Clear ();
		}

		
		/** Deletes an sprite frame from the sprite frame cache. */
		public void removeSpriteFrameByName(string name){
			// explicit nil handling
			if(  name==null )
				return;
			
			// Is this an alias ?
			string key;
			
			if( _spriteFramesAliases.TryGetValue(name, out key) ) {
				_spriteFrames.Remove(key);
				_spriteFramesAliases.Remove(name);
				
			} else
				_spriteFrames.Remove(name);
			
			// XXX. Since we don't know the .plist file that originated the frame, we must remove all .plist from the cache
			_loadedFilenames.Clear();
		}

		/** Removes multiple Sprite Frames from a plist file.
		* Sprite Frames stored in this file will be removed.
		* It is convenient to call this method when a specific texture needs to be removed.
		* @since v0.99.5
		*/
		public void removeSpriteFramesFromFile(string plist)
		{	
			string path = plist;
			string ext = Path.GetExtension (path);
			if(ext!=null && ext.Length>0)
				path = path.Replace (ext, "");
			if(!path.EndsWith("-tp"))
				path += "-tp.txt";
			NSDictionary dict = NSDictionary.DictionaryWithContentsOfFileFromResources(path);
			
			removeSpriteFramesFromDictionary(dict);
			
			// remove it from the cache
			if(_loadedFilenames.Contains(plist))
				_loadedFilenames.Remove(plist);
		}

		void removeSpriteFramesFromDictionary(NSDictionary dictionary)
		{
			NSDictionary framesDict = dictionary.objectForKey<NSDictionary>("frames");
//			List<string> keysToRemove = new List<string> ();

			var enumerator = framesDict.GetEnumerator();
			while (enumerator.MoveNext()) {
				KeyValuePair<object, object> kv = enumerator.Current;
				string frameDictKey = (string)kv.Key;
				_spriteFrames.Remove(frameDictKey);
			}
		}
		/** Removes unused sprite frames.
		 * Sprite Frames that have a retain count of 1 will be deleted.
		 * It is convenient to call this method after when starting a new Scene.
		 */
		public void removeUnusedSpriteFrames(){
			NSUtils.Assert (false, "CCSpriteFrameCache:removeUnusedSpriteFrames is not implemented yet.");		
		}
		#endregion

		#region mark CCSpriteFrameCache - getting
		/** Returns an Sprite Frame that was previously added.
		 If the name is not found it will return nil.
		 You should retain the returned copy if you are going to use it.
		 */
		public CCSpriteFrame spriteFrameByName(string name)
		{
			CCSpriteFrame frame;
			if( ! _spriteFrames.TryGetValue(name, out frame) ) {
				// try alias dictionary
				string key;
				if(_spriteFramesAliases.TryGetValue(name, out key))
					_spriteFrames.TryGetValue(key, out frame);
				
				if(  frame==null )
					CCDebug.Info("cocos2d: CCSpriteFrameCache: Frame '{0}' not found", name);
			}
			
			return frame;
		}
		#endregion
    }
}
