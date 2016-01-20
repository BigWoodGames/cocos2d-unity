using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace BBGamelib{
	
	/** Singleton that manages the CCAnimation objects.
	 It saves in a cache the animations. You should use this class if you want to save your animations in a cache.

	 @since v0.99.5
	 */
	public class CCAnimationCache
	{
		NSDictionary _animations;
		
		#region mark CCAnimationCache - Alloc, Init & Dealloc
		
		static CCAnimationCache _sharedAnimationCache=null;
		
		/** Returns the shared instance of the Animation cache */
		public static CCAnimationCache  sharedAnimationCache{
			get{
				if (_sharedAnimationCache == null)
					_sharedAnimationCache = new CCAnimationCache();
				
				return _sharedAnimationCache;			
			}
		}		
		/** Purges the cache. It releases all the CCAnimation objects and the shared instance. */
		public static void PurgeSharedAnimationCache()
		{
			_sharedAnimationCache = null;
		}

		public CCAnimationCache(){
			_animations = new NSDictionary ();
		}

		public override string ToString ()
		{
			return string.Format ("<{0} = {1} | num of animations =  {2}>", GetType().Name, GetHashCode(), _animations.Count);
		}

		#endregion
		
		#region mark CCAnimationCache - load/get/del
		
		/** Adds a CCAnimation with a name.*/
		public void addAnimation(CCAnimation animation, string name){
			_animations.Add (name, animation);
		}
		/** Deletes a CCAnimation from the cache. */
		public void removeAnimationByName(string name){
			if (name == null)
				return;
			_animations.Remove (name);
		}
		
		/** Returns a CCAnimation that was previously added.
		 If the name is not found it will return nil.
		 You should retain the returned copy if you are going to use it.
		 */
		public CCAnimation animationByName(string name){
			CCAnimation anim = _animations [name] as CCAnimation;
			return anim;
		}
		#endregion
		
		#region mark CCAnimationCache - from file
		
		void parseVersion1(NSDictionary animations)
		{
			CCSpriteFrameCache frameCache = CCSpriteFrameCache.sharedSpriteFrameCache;

			var enumerator = animations.GetEnumerator();
			while (enumerator.MoveNext()) {
				KeyValuePair<object, object> kv = enumerator.Current;
				string name = (string)kv.Key;
				NSDictionary animationDict = (NSDictionary)kv.Value;
				ArrayList frameNames = (ArrayList)animationDict["frames"];
				float delay = (float)animationDict["delay"];
				CCAnimation animation = null;
				
				if ( frameNames == null ) {
					CCDebug.Log("cocos2d: CCAnimationCache: Animation '{0}' found in dictionary without any frames - cannot add to animation cache.", name);
					continue;
				}
				
				List<CCAnimationFrame> frames = new List<CCAnimationFrame>(frameNames.Count);

				var framesEnumerator = frameNames.GetEnumerator();
				while (framesEnumerator.MoveNext()) {
					string frameName = (string)framesEnumerator.Current;
					CCSpriteFrame spriteFrame = frameCache.spriteFrameByName(frameName);
					
					if ( spriteFrame == null ) {
						CCDebug.Log("cocos2d: CCAnimationCache: Animation '{0}' refers to frame '{1}' which is not currently in the CCSpriteFrameCache. This frame will not be added to the animation.", name, frameName);
						
						continue;
					}
					
					CCAnimationFrame animFrame = new CCAnimationFrame(spriteFrame, 1, null);
					frames.Add(animFrame);
				}
				
				if ( frames.Count == 0 ) {
					CCDebug.Log("cocos2d: CCAnimationCache: None of the frames for animation '{0}' were found in the CCSpriteFrameCache. Animation is not being added to the Animation Cache.", name);
					continue;
				} else if ( frames.Count != frameNames.Count ) {
					CCDebug.Log("cocos2d: CCAnimationCache: An animation in your dictionary refers to a frame which is not in the CCSpriteFrameCache. Some or all of the frames for the animation '{0}' may be missing.", name);
				}
				
				animation = new CCAnimation(frames, delay, 1);
				
				CCAnimationCache.sharedAnimationCache.addAnimation(animation, name);
			}	
		}
		
		void parseVersion2(NSDictionary animations)
		{
			CCSpriteFrameCache frameCache = CCSpriteFrameCache.sharedSpriteFrameCache;

			var enumerator = animations.GetEnumerator();
			while (enumerator.MoveNext()) {
				KeyValuePair<object, object> kv = enumerator.Current;
				string name = (string)kv.Key;
				NSDictionary animationDict = (NSDictionary)kv.Value;
				
				int loops = 0;
				object loopsObj = loops;
				if(!animationDict.TryGetValue("loops", out loopsObj)){
					loops = 1;
				}else{
					loops = (int)loopsObj;
				}
				bool restoreOriginalFrame = (bool)animationDict["restoreOriginalFrame"];
				NSArray frameArray = (NSArray)animationDict["frames"];
				
				
				if ( frameArray == null ) {
					CCDebug.Log(@"cocos2d: CCAnimationCache: Animation '%@' found in dictionary without any frames - cannot add to animation cache.", name);
					continue;
				}
				
				// Array of AnimationFrames
				List<CCAnimationFrame> array = new List<CCAnimationFrame>(frameArray.Count);
				var frameArrayEnumerator = frameArray.GetEnumerator();
				while (frameArrayEnumerator.MoveNext()) {
					NSDictionary entry = (NSDictionary)frameArrayEnumerator.Current;
					string spriteFrameName = (string)entry["spriteframe"];
					CCSpriteFrame spriteFrame = frameCache.spriteFrameByName(spriteFrameName);
					
					if(  spriteFrame==null ) {
						CCDebug.Log("cocos2d: CCAnimationCache: Animation '{0}' refers to frame '{1}' which is not currently in the CCSpriteFrameCache. This frame will not be added to the animation.", name, spriteFrameName);
						
						continue;
					}
					
					float delayUnits = float.Parse(entry["delayUnits"].ToString());
					NSDictionary userInfo = entry.objectForKey<NSDictionary>("notification");
					
					CCAnimationFrame animFrame = new CCAnimationFrame(spriteFrame, delayUnits, userInfo);
					
					array.Add(animFrame);
				}
				
				float delayPerUnit = (float)animationDict["delayPerUnit"];
				CCAnimation animation = new CCAnimation (array, delayPerUnit, (uint)loops);
				
				animation.restoreOriginalFrame=restoreOriginalFrame;
				
				CCAnimationCache.sharedAnimationCache.addAnimation(animation, name);
			}
		}

		/** Adds an animation from an NSDictionary
		 Make sure that the frames were previously loaded in the CCSpriteFrameCache.
		 @since v1.1
		 */
		public void addAnimationsWithDictionary(NSDictionary dictionary)
		{
			NSDictionary animations = dictionary.objectForKey<NSDictionary>("animations");
			
			if ( animations == null ) {
				CCDebug.Log("cocos2d: CCAnimationCache: No animations were found in provided dictionary.");
				return;
			}
			
			int version = 1;
			NSDictionary properties = dictionary.objectForKey<NSDictionary>("properties");
			if( properties != null )
				version = properties.objectForKey<int>("format");
			
			NSArray spritesheets = properties.objectForKey<NSArray>("spritesheets");

			var enumerator = spritesheets.GetEnumerator();
			while (enumerator.MoveNext()) {
				string name = (string)enumerator.Current;
				CCSpriteFrameCache.sharedSpriteFrameCache.addSpriteFramesWithFile(name);
			}
			switch (version) {
			case 1:
				parseVersion1(animations);
				break;
			case 2:
				parseVersion2(animations);
				break;
			default:
				NSUtils.Assert(false, "Invalid animation format");
				break;
			}
		}
		
		
		/** Adds an animation from a plist file.
		 Make sure that the frames were previously loaded in the CCSpriteFrameCache.
		 @since v1.1
		 */
		public void addAnimationsWithFile(string plist)
		{
			NSUtils.Assert( plist!=null, "Invalid texture file name");

			NSDictionary dict = NSDictionary.DictionaryWithContentsOfFileFromResources(plist);
			
			NSUtils.Assert( dict!=null, "CCAnimationCache: File could not be found: {0}", plist);
			
			
			addAnimationsWithDictionary(dict);
		}
		#endregion
		

	}
}

