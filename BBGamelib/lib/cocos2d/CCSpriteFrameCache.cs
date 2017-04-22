using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;

namespace BBGamelib{
    public class CCSpriteFrameCache
    {
        utHash<string, CCSpriteFrame> _spriteFrames;
        utHash<string, string> _spriteFramesAliases;
        HashSet<string>     _loadedFilenames;


        // ------------------------------------------------------------------------------
        //  singleton
        // ------------------------------------------------------------------------------
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
            _spriteFrames = new utHash<string, CCSpriteFrame> ();
            _spriteFramesAliases = new utHash<string, string> ();
            _loadedFilenames = new HashSet<string> ();
        }

        // ------------------------------------------------------------------------------
        //  Adds multiple Sprite Frames from a plist file.
        // ------------------------------------------------------------------------------
        /** Adds multiple Sprite Frames from a plist file.
         * A texture will be loaded automatically. The texture name will composed by replacing the .plist suffix with .png .
         * If you want to use another texture, you should use the addSpriteFramesWithFile:texture method.
         */
        public void addSpriteFramesWithFile(string path){
            path = FileUtils.GetFilePathWithoutExtends(path);
            if (_loadedFilenames.Contains(path))
            {
                CCDebug.Warning("cocos2d: CCSpriteFrameCache: file already loaded: {0}", path);
            } else
            {
                _loadedFilenames.Add(path);
                NSDictionary dict = NSDictionary.DictionaryWithContentsOfFileFromResources(string.Concat(path, ".txt"));
                Texture2D texture = Resources.Load<Texture2D> (path);
                if (texture != null)
                {
                    addSpriteFrames(dict, texture, path);
                } else
                {
                    CCDebug.Log ("cocos2d: CCSpriteFrameCache: Couldn't load texture: {0}", path);
                }
            }
        }

        void addSpriteFrames(NSDictionary dictionary, Texture2D texture, string textureFileName)
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
            Vector2 spriteOffset = Vector2.zero;
            Vector2 originalSize = Vector2.zero;
            bool semi = false;

            var enumerator = framesDict.GetEnumerator();
            while (enumerator.MoveNext()) {
                KeyValuePair<object, object> frameDictKeyValue = enumerator.Current;
                string frameDictKey = (string)frameDictKeyValue.Key;
                NSDictionary frameDict = (NSDictionary)frameDictKeyValue.Value;
                if(format == 0) 
                {
                    float x = frameDict.objectForKey<float>("x");
                    float y = frameDict.objectForKey<float>("y");
                    float w = frameDict.objectForKey<float>("width");
                    float h = frameDict.objectForKey<float>("height");
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
                    rect = new Rect(x, y, w, h);
                    textureRotated = false;
                    spriteOffset = new Vector2(ox, oy);
                    originalSize = new Vector2(ow, oh);
                    semi = frameDict.objectForKey<bool>("semi");
                } else if(format == 1 || format == 2) {
                    Rect frame = ccUtils.RectFromString(frameDict.objectForKey<string>("frame"));
                    bool rotated = false;

                    // rotation
                    if(format == 2)
                        rotated = frameDict.objectForKey<bool>("rotated");

                    Vector2 offset = ccUtils.PointFromString(frameDict.objectForKey<string>("offset"));
                    Vector2 sourceSize = ccUtils.PointFromString(frameDict.objectForKey<string>("sourceSize"));

                    // set frame info
                    rect = frame;
                    textureRotated = rotated;
                    spriteOffset = offset;
                    originalSize = sourceSize;
                    semi = frameDict.objectForKey<bool>("semi");
                } else if(format == 3) {
                    // get values
                    Vector2 spriteSize = ccUtils.PointFromString(frameDict.objectForKey<string>("spriteSize"));
                    Vector2 spriteOffset_ = ccUtils.PointFromString(frameDict.objectForKey<string>("spriteOffset"));
                    Vector2 spriteSourceSize = ccUtils.PointFromString(frameDict.objectForKey<string>("spriteSourceSize"));
                    Rect textureRect = ccUtils.RectFromString(frameDict.objectForKey<string>("textureRect"));
                    bool textureRotated_ = frameDict.objectForKey<bool>("textureRotated");

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
                    rect = new Rect(textureRect.position.x, textureRect.position.y, spriteSize.x, spriteSize.y);
                    textureRotated = textureRotated_;
                    spriteOffset = spriteOffset_;
                    originalSize = spriteSourceSize;
                    semi = frameDict.objectForKey<bool>("semi");
                }
                if (textureRotated)
                {
                    rect.size = new Vector2(rect.size.y, rect.size.x);
                }
                rect.y = textureSize.y - rect.y - rect.height;

                // add sprite frame
                CCSpriteFrame spriteFrame = new CCSpriteFrame(texture, rect, textureRotated, spriteOffset, originalSize, semi);
                spriteFrame.frameFileName = frameDictKey;
                spriteFrame.textureFilename = textureFileName;
                _spriteFrames [frameDictKey] = spriteFrame;
            }
        }

        /** Returns an Sprite Frame that was previously added.
         If the name is not found it will return nil.
         You should retain the returned copy if you are going to use it.
         */
        public CCSpriteFrame spriteFrameByName(string name)
        {
            CCSpriteFrame frame = _spriteFrames [name];
            if( frame == null ) {
                // try alias dictionary
                string key = _spriteFramesAliases[name];
                if (key != null)
                    frame = _spriteFrames [key];

                if (frame == null)
                    CCDebug.Info("cocos2d: CCSpriteFrameCache: Frame '{0}' not found", name);
            }

            return frame;
        }

        // ------------------------------------------------------------------------------
        //  Remove frame
        // ------------------------------------------------------------------------------
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

            CCSpriteFrame frame = spriteFrameByName(name);

            // Is this an alias ?
            string key;

            if( _spriteFramesAliases.TryGetValue(name, out key) ) {
                _spriteFrames.Remove(key);
                _spriteFramesAliases.Remove(name);

            } else
                _spriteFrames.Remove(name);

            _loadedFilenames.Remove(frame.textureFilename);
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
            NSDictionary dict = NSDictionary.DictionaryWithContentsOfFileFromResources(string.Concat(path, ".txt"));

            removeSpriteFramesFromDictionary(dict);

            // remove it from the cache
            if(_loadedFilenames.Contains(plist))
                _loadedFilenames.Remove(plist);
        }      
        public void removeSpriteFramesFromDictionary(NSDictionary dictionary)
        {
            NSDictionary framesDict = dictionary.objectForKey<NSDictionary>("frames");
            //          List<string> keysToRemove = new List<string> ();

            var enumerator = framesDict.GetEnumerator();
            while (enumerator.MoveNext()) {
                KeyValuePair<object, object> kv = enumerator.Current;
                string frameDictKey = (string)kv.Key;
                _spriteFrames.Remove(frameDictKey);
            }
        }
        public void removeSpriteFramesFromTexture(Texture2D texture)
        {
            utList<string> keysToRemove = new utList<string> ();
            var enumerator = _spriteFrames.GetEnumerator();
            while (enumerator.MoveNext()) {
                KeyValuePair<string, CCSpriteFrame> kv = enumerator.Current;
                string spriteFrameKey = kv.Key;
                CCSpriteFrame frame = kv.Value;
                if (frame.texture == texture)
                    keysToRemove.DL_APPEND(spriteFrameKey);
            }
            for (var ent = keysToRemove.head; ent != null; ent = ent.next)
            {
                _spriteFrames.Remove(ent.obj);
            }
        }


        public override string ToString ()
        {
            return string.Format("<{0} = {1} | num of sprite sheets =  {2}>", this.GetType().Name, this.GetHashCode(), _spriteFrames.Count);
        }
    }
}
