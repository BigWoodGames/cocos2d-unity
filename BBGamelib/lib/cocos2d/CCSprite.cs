using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace BBGamelib{
	public class CCSpriteContent
	{
		public CCFactoryGear gear;
		public GameObject gameObject{get{return gear.gameObject;}}
		public SpriteRenderer renderer{get{return gear.components[0] as SpriteRenderer;}}
		public Transform transform{get{return gameObject.transform;}}

		public CCSpriteContent(){
			gear = CCFactory.Instance.takeGear (CCFactory.KEY_SPRITE);
			gear.gameObject.name = "content";
			gear.gameObject.transform.localPosition = Vector3.zero;
//			gear.gameObject.transform.localScale = new Vector3 (1, 1, 1);
			this.renderer.color = Color.white;
			this.renderer.sortingLayerName = CCFactory.LAYER_DEFAULT;
		}
	}
	public class CCSprite : CCNodeRGBA, CCSpriteProtocol
	{
		
		//
		// Data used when the sprite is rendered using a CCSpriteBatchNode
		//
//		CCTextureAtlas			*_textureAtlas;			// Sprite Sheet texture atlas (weak reference)
//		NSUInteger				_atlasIndex;			// Absolute (real) Index on the batch node
//		CCSpriteBatchNode		*_batchNode;			// Used batch node (weak reference)
		CGAffineTransform		_transformToBatch;		//
//		bool					_dirty;					// Sprite needs to be updated
//		bool					_recursiveDirty;		// Subchildren needs to be updated
//		bool					_hasChildren;			// optimization to check if it contain children
		bool					_shouldBeHidden;		// should not be drawn because one of the ancestors is not visible
		
		//
		// Data used when the sprite is self-rendered
		//
//		ccBlendFunc				_blendFunc;				// Needed for the texture protocol
//		Texture2D				_texture;				// Texture used to render the sprite
		CCSpriteFrame			_spriteFrame;
		CCSpriteContent 		_content;

		//
		// Shared data
		//
		
		// sprite rectangle
//		Rect	_rect;
		
		// texture
//		bool	_rectRotated;
		
		// Offset Position (used by Zwoptex)
//		Vector2	_offsetPosition;
//		Vector2 _unflippedOffsetPositionFromCenter;
		
		// vertex coords, texture coords and color info
//		ccV3F_C4B_T2F_Quad _quad;
		Color _quadColor;
		
		// opacity and RGB protocol
		bool		_opacityModifyRGB;
		
		// image is flipped
		bool	_flipX;
		bool	_flipY;

		bool _isContentDirty;

		#region CCSprite

		//empty sprite
		public CCSprite(){}

		protected override void initWithGear (CCFactoryGear gear)
		{
			base.initWithGear (gear);
			_gear.gameObject.name = "Sprite";
			_gear.gameObject.transform.localPosition = Vector3.zero;
//			_gear.gameObject.transform.localScale = new Vector3 (1, 1, 1);
			_content = new CCSpriteContent ();
			_content.gameObject.transform.parent = gameObject.transform;
//			_content.renderer.sortingOrder = 1;
		}
		
		//sprite with file
		public CCSprite(string imagedName){
			initWithImageNamed (imagedName);
		}
		public void initWithImageNamed(string imageName){
			gameObject.name = imageName;
			CCSpriteFrame frame = CCSpriteFrameCache.sharedSpriteFrameCache.spriteFrameByName (imageName);
			if (frame == null) {
				CCDebug.Info("cocos2d:CCSprite: Try to load '{0}' as a file.", imageName);
				frame = new CCSpriteFrame (imageName);
			}
			initWithSpriteFrame (frame);
		}

		public CCSprite(CCSpriteFrame frame){
			initWithSpriteFrame (frame);
		}
		public void initWithSpriteFrame(CCSpriteFrame spriteFrame){

			// shader program
//			self.shaderProgram = [[CCShaderCache sharedShaderCache] programForKey:kCCShader_PositionTextureColor];
			
//			_dirty = _recursiveDirty = false;
			
			_opacityModifyRGB = true;
			
//			_blendFunc.src = CC_BLEND_SRC;
//			_blendFunc.dst = CC_BLEND_DST;
			
			_flipY = _flipX = false;
			
			// default transform anchor: center
			_anchorPoint =  new Vector2(0.5f, 0.5f);
			
			// zwoptex default values
//			_offsetPosition = CGPointZero;
			
//			_hasChildren = false;
//			_batchNode = null;
			
			// clean the Quad
//			bzero(&_quad, sizeof(_quad));
			
			// Atlas: Color
//			ccColor4B tmpColor = {255,255,255,255};
//			_quad.bl.colors = tmpColor;
//			_quad.br.colors = tmpColor;
//			_quad.tl.colors = tmpColor;
//			_quad.tr.colors = tmpColor;
			_quadColor = new Color32 (255, 255, 255, 255);


			this.displayedFrame = spriteFrame;
//			[self setTexture:texture];
//			[self setTextureRect:rect rotated:rotated untrimmedSize:rect.size];
			
			
			// by default use "Self Render".
			// if the sprite is added to a batchnode, then it will automatically switch to "batchnode Render"
//			[self setBatchNode:nil];
		}

		public override string ToString ()
		{
			return string.Format ("<{0} = {1} | Size = ({2:0.00},{3:0.00},{4:0.00},{5:0.00}) | tag = {6}>", 
			                      GetType().Name, GetHashCode(),
			                      position.x, position.y,
			                      contentSize.x, contentSize.y,
			                      _userTag);
		}

		public override void cleanup ()
		{
			base.cleanup ();
		}

		protected override void recycleGear ()
		{
			base.recycleGear ();
			_content.renderer.sprite = null;
			_content.renderer.color = Color.white;
			_content.renderer.sortingLayerName = CCFactory.LAYER_DEFAULT;
			//reset default layer
			_content.renderer.gameObject.layer = LayerMask.NameToLayer(CCFactory.LAYER_DEFAULT);
		
			CCFactory.Instance.recycleGear (CCFactory.KEY_SPRITE, _content.gear);
		}

		public CCSpriteFrame displayedFrame{
			get{ return _spriteFrame;}
			set{ 
				_spriteFrame = value;
				_content.renderer.sprite = value.sprite;
//				Vector2 sizeInUnits = value.bounds.size;
//				Vector2 sizeInPixels = ccUtils.UnitsToPixels(sizeInUnits);
				this.contentSize = value.originalSize;
				_isContentDirty = true;
			}
		}


		public override void updateTransform ()
		{
			base.updateTransform ();
			if (_isContentDirty) {
				Vector2 contentPosition = _contentSize / 2 + _spriteFrame.offset;
				contentPosition -= _anchorPointInPixels;
				Vector2 pInUIUnits = ccUtils.PixelsToUnits (contentPosition);
//				_content.transform.localPosition = new Vector3(pInUIUnits.x, pInUIUnits.y, _content.transform.localPosition.z);
				Vector3 pos = _content.transform.localPosition;
				pos.x = pInUIUnits.x;
				pos.y = pInUIUnits.y;
				_content.transform.localPosition = pos;
		
				//rotation
				Vector3 rotation = _content.transform.localEulerAngles;
				rotation.y = 0;
				rotation.z = 0;
				if (_flipX) {
					rotation.y = 180;
				} 
				if (_flipY) {
					rotation.y = _flipX ? 0 : 180;
					rotation.z = 180;
				}
				if(_spriteFrame.rotated)
					rotation.z += 90;
				_content.transform.localEulerAngles = rotation;
				_isContentDirty = false;
			}
		}

		protected override void draw ()
		{
			ccUtils.CC_INCREMENT_GL_DRAWS ();
			_content.renderer.sortingOrder = CCDirector.sharedDirector.globolRendererSortingOrder ++;
		}

		public CCSpriteContent content{
			get{ return _content;}
		}
		#endregion

		
		
		#region mark CCSprite - CCNode overrides

		public override void addChild (CCNode child, int z, string tag)
		{
			NSUtils.Assert( child != null, "Argument must be non-nil");

			//CCNode already sets _isReorderChildDirty so this needs to be after batchNode check
			base.addChild (child, z, tag);
			
//			_hasChildren = true;
		}

		public override void reorderChild (CCNode child, int z)
		{
			NSUtils.Assert( child != null, "Child must be non-nil");
			NSUtils.Assert( _children.Contains(child), "Child doesn't belong to Sprite" );
			
			if( z == child.zOrder )
				return;
			base.reorderChild (child, z);
		}

		#endregion

		#region CCSprite - property overloads
//		public virtual void setDirtyRecursively(bool b)
//		{
//			_dirty = _recursiveDirty = b;
//			// recursively set dirty
//			if( _hasChildren ) {
//				foreach(CCSprite child in _children)
//					child.setDirtyRecursively(true);
//			}
//		}

		public virtual bool flipX{
			get{return _flipX;}
			set{
				if(_flipX != value){
					_flipX = value;
					_isContentDirty = true;
				}
			}
		}
		public virtual bool flipY{
			get{return _flipY;}
			set{
				if(_flipY != value){
					_flipY = value;
					_isContentDirty = true;
				}
			}
		}

		public override Vector2 anchorPoint {
			set {
				base.anchorPoint = value;
				_isContentDirty = true;
			}
		}

		#endregion
		
		#region CCSprite - RGBA protocol
		public void updateColor()
		{
			Color32 color4 = new Color32(_displayedColor.r, _displayedColor.g, _displayedColor.b, _displayedOpacity);

			// special opacity for premultiplied textures
			if ( _opacityModifyRGB ) {
				color4.r = (byte)(color4.r * _displayedOpacity/255.0f);
				color4.g = (byte)(color4.g * _displayedOpacity/255.0f);
				color4.b = (byte)(color4.b * _displayedOpacity/255.0f);
			}
			_quadColor = color4;
			_content.renderer.color = _quadColor;
		}

		public override Color32 color {
			set {
				base.color = value;
				updateColor();
			}
		}

		public override void updateDisplayedColor (Color32 parentColor)
		{
			base.updateDisplayedColor (parentColor);
			updateColor ();
		}

		public override byte opacity {
			set {
				base.opacity = value;
				updateColor();
			}
		}
		public override bool opacityModifyRGB{
			get{return _opacityModifyRGB;}
			set{
				if( _opacityModifyRGB != value ) {
					_opacityModifyRGB = value;
					updateColor();
				}
			}
		}

		public override void updateDisplayedOpacity (byte parentOpacity)
		{
			base.updateDisplayedOpacity (parentOpacity);
			updateColor ();
		}

		#endregion

		#region CCSpriteProtocol
		public CCSpriteFrame displayFrame{
			get{ return displayedFrame;}
			set{ 
				displayedFrame = value;
			}
		}
		#endregion
	}
}
