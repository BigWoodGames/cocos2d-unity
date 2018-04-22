using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System;

namespace BBGamelib{
	public class CCLabelContent
	{
		public CCFactoryGear gear;
		public GameObject gameObject{get{return gear.gameObject;}}
		public MeshRenderer renderer{get{return gear.components[0] as MeshRenderer;}}
		public TextMesh mesh{get{return gear.components[1] as TextMesh;}}
		public Transform transform{get{return gameObject.transform;}}
		//unity font material
		public Material defaultMaterial;
		public CCLabelContent(){
			gear = CCFactory.Instance.takeGear (CCFactory.KEY_LABEL);
			gear.gameObject.name = "content";
			//			gear.gameObject.transform.localScale = new Vector3 (1, 1, 1);
			this.mesh.color = Color.white;
			mesh.characterSize = 0.05f; 
			mesh.anchor = TextAnchor.MiddleCenter;
			mesh.alignment = TextAlignment.Center;
			this.renderer.sortingLayerName = CCFactory.LAYER_DEFAULT;
			this.defaultMaterial = this.renderer.material;
		}
	}

	/** CCLabel is a subclass of CCTextureNode that knows how to render text labels
	 *
	 * All features from CCTextureNode are valid in CCLabel
	 *
	 * CCLabel objects are slow. Consider using CCLabelAtlas or CCLabelBMFont instead.
	 */
	public class CCLabelTTF : CCNodeRGBA, CCLabelProtocol
	{
        public static Type[] kGearTypes = new Type[]
        {
            typeof(MeshRenderer),
            typeof(TextMesh)
        };
		const float kFontSizeToPixel = 0.5f;
		Vector2                       _dimensions;
		CCTextAlignment              _hAlignment;
		CCVerticalTextAlignment      _vAlignment;
		string                    _fontName;
		float                      _fontSize;
		string	_text;
		
		/** font shadow */
//		bool    _shadowEnabled;
//		float   _shadowOpacity;
//		float   _shadowBlur;
//		Vector2  _shadowOffset;
		
		/** font stroke */
//		bool        _strokeEnabled;
//		Color32   _strokeColor;
//		float       _strokeSize;
		
		/** font fill color */
		Color32   _textFillColor;

		CCLabelContent 		_content;

		// opacity and RGB protocol
		bool		_opacityModifyRGB;
		
		// image is flipped
		bool	_flipX;
		bool	_flipY;
		bool _isContentDirty;

		
		/** creates a CCLabelTTF from a fontname, horizontal alignment, vertical alignment, dimension in points, line break mode, and font size in points.
		 Supported lineBreakModes:
		 - iOS: all UILineBreakMode supported modes
		 - Mac: Only NSLineBreakByWordWrapping is supported.
		 @since v1.0
		 */

		protected override void initWithGear (CCFactoryGear gear)
		{
			base.initWithGear (gear);
			_gear.gameObject.name = "LabelTTF";
//			_gear.gameObject.transform.localScale = new Vector3 (1, 1, 1);
			_content = new CCLabelContent ();
			_content.gameObject.transform.parent = gameObject.transform;
            //			_content.renderer.sortingOrder = 1;
            ccUtils.SetRenderColor (_content.renderer, Color.white, new Color32(0, 0, 0, 0));
		}
		public CCLabelTTF(string text, string fontName, float fontSize)
		{
			init (text, fontName, fontSize, Vector2.zero, CCTextAlignment.Left, CCVerticalTextAlignment.Center);
		}
		public CCLabelTTF(string text, string fontName, float fontSize, Vector2 dimensions, 
		                  CCTextAlignment hAlignment=CCTextAlignment.Left, 
		                  CCVerticalTextAlignment vAlignment=CCVerticalTextAlignment.Top)
		{
			init (text, fontName, fontSize, dimensions, hAlignment, vAlignment);
		}

		protected void init(string text, string fontName, float fontSize, Vector2 dimensions, CCTextAlignment hAlignment, CCVerticalTextAlignment vAlignment)
		{
			_opacityModifyRGB = true;
			_flipY = _flipX = false;
			_anchorPoint =  new Vector2(0.5f, 0.5f);

			_hAlignment = CCTextAlignment.Center;
			_vAlignment = CCVerticalTextAlignment.Center;

			this.dimensions = dimensions;
			this.horizontalAlignment = hAlignment;
			this.verticalAlignment = vAlignment;
			this.fontName = getFontName(fontName);
			this.fontSize = fontSize;
			this.text = text;
		}

		public CCLabelContent content{
			get{ return _content; }
        }

		public string text{
			get{return _text;}
			set{
				NSUtils.Assert( value!=null, "Invalid string" );
				
				if( _text != value ) {
					_text = value;
					updateTexture();
				}		
			}
		}

		public string getFontName(string fontName)
		{
			return fontName;
//			// Custom .ttf file ?
//			if ([[fontName lowercaseString] hasSuffix:@".ttf"])
//			{
//				// This is a file, register font with font manager
//				NSString* fontFile = [[CCFileUtils sharedFileUtils] fullPathForFilename:fontName];
//				NSURL* fontURL = [NSURL fileURLWithPath:fontFile];
//				CTFontManagerRegisterFontsForURL((CFURLRef)fontURL, kCTFontManagerScopeProcess, NULL);
//				
//				return [[fontFile lastPathComponent] stringByDeletingPathExtension];
//			}
//			
//			return fontName;
		}
		
		/** Font name used in the label */
		public string fontName{
			get{return _fontName;}
			set{
				if(_fontName != value){
					_fontName = value;
					string fn = _fontName;
					string ext = Path.GetExtension (fn);
					if(ext!=null && ext.Length>0)
						fn = fn.Replace (ext, "");
					Font font = Resources.Load<Font> (fn);
					if(font == null){
						fn = _fontName;
						if(ext==null || ext.Length==0){
							fn = _fontName + ".ttf";
						}
						font = (Font)Resources.GetBuiltinResource(typeof(Font), fn);
					}
					if(font!=null){
						_content.mesh.font = font;
						_content.renderer.material = font.material;
					}
					// Force update
					if(_text!=null)
						updateTexture();
				}
			}
		}

		public float fontSize{
			get{return _fontSize;}
			set{
				if(_fontSize != value){
					_fontSize = value;
					_content.mesh.fontSize = (int)(_fontSize / kFontSizeToPixel);
					// Force update
					if(_text!=null)
						updateTexture();
				}
			}
		}
		
		/** Dimensions of the label in Points */
		public Vector2 dimensions{
			get{return _dimensions;}
			set{
				if( value != _dimensions)
				{
					_dimensions = value;
					
					// Force update
					if( _text != null )
						updateTexture();
				}
			}
		}
		
		/** The alignment of the label */
		public CCTextAlignment horizontalAlignment{
			get{ return _hAlignment;}
			set{
				_hAlignment = value;
				if(_hAlignment == CCTextAlignment.Left)
					_content.mesh.alignment = TextAlignment.Left;
				else if(_hAlignment == CCTextAlignment.Center)
					_content.mesh.alignment = TextAlignment.Center;
				else if(_hAlignment == CCTextAlignment.Right)
					_content.mesh.alignment = TextAlignment.Right;
				// Force update
				if( _text != null )
					updateTexture();		
			}
		}
		/** The vertical alignment of the label */
		public CCVerticalTextAlignment verticalAlignment{
			get{ return _vAlignment;}
			set{
				_vAlignment = value;
				
				// Force update
				if( _text != null )
					updateTexture();		
			}
		}

		public override string ToString ()
		{
			return string.Format ("<{0} = {1} | FontSize = {2:0.0}>", GetType().Name, GetHashCode(), _fontSize);
		}

		protected override void recycleGear ()
        {
            //reset default layer
            CCFactory.Instance.materialPropertyBlock.Clear ();
            _content.renderer.SetPropertyBlock (CCFactory.Instance.materialPropertyBlock);
            _content.renderer.material = _content.defaultMaterial;
            _content.renderer.gameObject.layer = LayerMask.NameToLayer(CCFactory.LAYER_DEFAULT);

            CCFactory.Instance.recycleGear (CCFactory.KEY_LABEL, _content.gear);
			base.recycleGear ();
		}

		// Helper
		protected void updateTexture()
		{			
			if (string.IsNullOrEmpty (_text)) {
				_content.mesh.text = _text;
				return;
			}
			if (FloatUtils.EQ(_dimensions.x , 0)) {
				_content.mesh.text = _text;
				Bounds localBounds = getLocalbounds();
				Vector2 textSize = ccUtils.UnitsToPixels (localBounds.size);
				this.contentSize = textSize;
			} else {
				StringBuilder finalText = new StringBuilder();
				string originalText = _text;
				int preEmptyCharIndex = -1;
				for(int i=1; i<=originalText.Length; i++){
					char c = originalText[i-1];
					if(char.IsWhiteSpace(c)){
						preEmptyCharIndex = i-1;
					}
					string tmpStr = originalText.Substring(0, i);
					if(c == '\n'){
						finalText.Append(tmpStr);
						originalText = originalText.Substring(i);
						i = 0;
						preEmptyCharIndex = -1;
					}

					_content.mesh.text = tmpStr;
					Bounds localBounds = getLocalbounds();
					Vector2 csize = ccUtils.UnitsToPixels (localBounds.size);
					if(FloatUtils.Big(csize.x , _dimensions.x)){
						if(preEmptyCharIndex==-1)
							tmpStr = originalText.Substring(0, --i);
						else{
							tmpStr = originalText.Substring(0, preEmptyCharIndex);
							i = preEmptyCharIndex + 1;
							preEmptyCharIndex = -1;
						}
						finalText.Append(tmpStr);
						if(i<originalText.Length){
							finalText.Append("\n");
							originalText = originalText.Substring(i);
							i = 0;
						}
					}else if(i==originalText.Length){
						tmpStr = originalText.Substring(0, i);
						finalText.Append(tmpStr);
						break;
					}

//					string tmpStr = originalText.Substring(0, i);
//					_content.mesh.text = tmpStr;
//					Vector2 csize = _content.renderer.bounds.size;
//					csize = ccUtils.UnitsToPixels(csize);
//					if(FloatUtils.Small(csize.x , _dimensions.x) || i==1){
//						tmpStr = originalText.Substring(0, i);
//						finalText += tmpStr;
//						if(i<originalText.Length){
//							finalText += "\n";
//							originalText = originalText.Substring(i);
//							i = originalText.Length+1;
//						}else{
//							break;
//						}
//					}
				}
				_content.mesh.text = finalText.ToString();
				Bounds bounds = getLocalbounds();
				Vector2 textSize = ccUtils.UnitsToPixels (bounds.size);
				this.contentSize = textSize;
			}
			_isContentDirty = true;
		}

		protected override void draw ()
		{
			ccUtils.CC_INCREMENT_GL_DRAWS ();
			_content.renderer.sortingOrder = CCDirector.sharedDirector.globolRendererSortingOrder ++;
		}
		
		#region CCSprite - property overloads
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

		
		#region CCLabelTTF - updateTransform
		public override void updateTransform ()
		{
			base.updateTransform ();
			if (_isContentDirty) {
				Vector2 contentPosition = _contentSize / 2;
				Bounds localBounds = getLocalbounds();
				Vector2 textSize = ccUtils.UnitsToPixels (localBounds.size);
				if(verticalAlignment == CCVerticalTextAlignment.Top){
					contentPosition.y = _contentSize.y - textSize.y/2;
				}else if(verticalAlignment == CCVerticalTextAlignment.Bottom){
					contentPosition.y = textSize.y/2;
				}
				if(horizontalAlignment == CCTextAlignment.Right){
					contentPosition.x = _contentSize.x - textSize.x/2;
				}else if(horizontalAlignment == CCTextAlignment.Left){
					contentPosition.x = textSize.x/2;
				}

				contentPosition -= _anchorPointInPixels;
				Vector2 pInUIUnits = ccUtils.PixelsToUnits (contentPosition);
				_content.transform.localPosition = new Vector3(pInUIUnits.x, pInUIUnits.y, _content.transform.localPosition.z);

		
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
				_content.transform.localEulerAngles = rotation;
				_isContentDirty = false;
			}
		}
		#endregion
		
		#region CCLabelTTF - RGBA protocol
		public void updateColor()
		{
			// special opacity for premultiplied textures
			Color32 tint = _displayedColor.tint;
			tint.a = _displayedOpacity.tint;
			Color32 add = _displayedColor.add;
			add.a = _displayedOpacity.add;
			
			ccUtils.SetRenderColor (_content.renderer, tint, add);
		}
		
		public override Color32 color {
			set {
				base.color = value;
				updateColor();
			}
		}
		
		public override void updateDisplayedColor (ColorTransform parentColor)
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
		
		public override void updateDisplayedOpacity (OpacityTransform parentOpacity)
		{
			base.updateDisplayedOpacity (parentOpacity);
			updateColor ();
		}
		
		#endregion

		public Bounds getLocalbounds(){
			Bounds bounds = _content.renderer.bounds;

			//Fixed: convert bounds to content space
			bounds = cc3Utils.ConvertToLocalBounds(_content.transform, bounds);
			return bounds;
		}
	}
}

