using UnityEngine;
using System.Collections;
using UnityEngine.Rendering;

namespace BBGamelib
{
    public class CC3Sprite : CC3Node
    {
        // ------------------------------------------------------------------------------
        //  init
        // ------------------------------------------------------------------------------
        CCSpriteFrame _spriteFrame;
        bool    _flipX;
        bool    _flipY;
        bool _meshDirty;
        bool _clippingEnabled;
        Rect _clippingRect;

        public CC3Sprite(string imagedName)
        {
            initWithImageNamed(imagedName);
        }

        public CC3Sprite(CCSpriteFrame spriteFrame)
        {
            gameObject.name = spriteFrame.frameFileName;
            initWithSpriteFrame(spriteFrame);
        }

        protected override void init()
        {
            CCFactoryGear gear = CCFactory.Instance.takeGear (CCFactory.KEY_SPRITE);
            initWithGear (gear);
            this.meshRender.sortingOrder = 0;
            this.meshRender.shadowCastingMode = ShadowCastingMode.Off;
            this.meshRender.receiveShadows = true;
            this.meshRender.motionVectors = true;
            _clippingEnabled = false;
        }      

        public void initWithImageNamed(string imageName)
        {
            gameObject.name = imageName;
            CCSpriteFrame frame = CCSpriteFrameCache.sharedSpriteFrameCache.spriteFrameByName (imageName);
            if (frame == null) {
                CCDebug.Info("cocos2d:CC3Sprite: try to load '{0}' as a file.", imageName);
                string path = FileUtils.GetFilePathWithoutExtends(imageName);
                Texture2D texture = Resources.Load<Texture2D> (path);
                NSUtils.Assert(texture != null, "cocos2d:CC3Sprite: '{0}' not found.", path);
                frame = new CCSpriteFrame (texture, new Rect(Vector2.zero, new Vector2(texture.width, texture.height)));
                frame.semiTransparent = false;
            }
            initWithSpriteFrame (frame);
        }

        public void initWithSpriteFrame(CCSpriteFrame spriteFrame)
        { 
            _flipY = _flipX = false;
            _anchorPoint = new Vector2(0.5f, 0.5f);
            this.displayedFrame = spriteFrame;
        }

        protected override void recycleGear()
        {
            this.meshRender.sharedMaterial = null;
            CCFactory.Instance.recycleGear(CCFactory.KEY_SPRITE, _gear);
        }
        // ------------------------------------------------------------------------------
        //  public methods
        // ------------------------------------------------------------------------------
        public MeshFilter meshFilter
        { 
            get { return _gear.components [0] as MeshFilter; } 
        }

        public MeshRenderer meshRender
        { 
            get { return _gear.components [1] as MeshRenderer; } 
        }

        public CCSpriteFrame displayedFrame{
            get{ return _spriteFrame;}
            set{ 
                _spriteFrame = value;
                this.contentSize = value.originalSize;

                if (this.meshRender.sharedMaterial == null || this.meshRender.sharedMaterial.mainTexture != _spriteFrame.texture)
                {
                    this.meshRender.sharedMaterial = CCMaterialCache.sharedMaterialCache.getMaterial(_spriteFrame.texture);
                    if (_spriteFrame.semiTransparent)
                    {
                        ccUtils.SetRenderValue(this.meshRender, "_ZWrite", 0);
                        this.renderQueue = RenderQueue.Transparent;
                        this.renderType = "Transparent";
                    } else
                    {
                        ccUtils.SetRenderValue(this.meshRender, "_ZWrite", 1);
                        this.renderQueue = RenderQueue.Geometry;
                        this.renderType = "Opaque";
                    }
                    ccUtils.SetRenderValue (this.meshRender, "_CullMode", (int)CullMode.Back);
                    ccUtils.SetRenderColor (this.meshRender, Color.white, new Color32(0, 0, 0, 0));
                }
                _meshDirty = true;
            }
        }

        public CullMode cullMode
        {
            get{ return (CullMode)this.meshRender.sharedMaterial.GetInt("_CullMode"); }
            set{ ccUtils.SetRenderValue (this.meshRender, "_CullMode", (int)value);}
        }

        public RenderQueue renderQueue
        {
            get{ return (RenderQueue)this.meshRender.sharedMaterial.renderQueue; }
            set{ this.meshRender.material.renderQueue = (int)value; }
        }

        public string renderType
        {
            get{ return this.meshRender.sharedMaterial.GetTag("RenderType", false); }
            set{ this.meshRender.material.SetOverrideTag("RenderType", value); }
        }

        public int zWrite
        {
            get{ return this.meshRender.sharedMaterial.GetInt("_ZWrite"); }
            set{ ccUtils.SetRenderValue (this.meshRender, "_ZWrite", value); }
        }

        public bool receiveShadows
        {
            get{ return this.meshRender.receiveShadows; }
            set{ this.meshRender.receiveShadows = value; }
        }

        public virtual bool flipX
        {
            get{ return _flipX; }
            set
            {
                if (_flipX != value)
                {
                    _flipX = value;
                    _isUpdateTransformDirty = _isTransformDirty = _isInverseDirty = true;
                }
            }
        }

        public virtual bool flipY
        {
            get{ return _flipY; }
            set
            {
                if (_flipY != value)
                {
                    _flipY = value;
                    _isUpdateTransformDirty = _isTransformDirty = _isInverseDirty = true;
                }
            }
        }

        public override Vector2 anchorPoint
        {
            set
            {
                base.anchorPoint = value;
                _meshDirty = true;
            }
        }

        public void enableClipping(Rect clippingRect)
        {
            _clippingEnabled = true;
            _clippingRect = clippingRect;
            _meshDirty = true;
        }

        public void disableClipping()
        {
            _clippingEnabled = false;
            _meshDirty = true;
        }


        public override string ToString ()
        {
            return string.Format ("<{0} = {1} | Size = ({2:0.00},{3:0.00},{4:0.00},{5:0.00}) | tag = {6}>", 
                GetType().Name, GetHashCode(),
                position.x, position.y,
                contentSize.x, contentSize.y,
                _userTag);
        }

        // ------------------------------------------------------------------------------
        //  color & opacity
        // ------------------------------------------------------------------------------
        public void updateColor()
        {
            Color32 tint = _displayedColor.tint;
            tint.a = _displayedOpacity.tint;
            Color32 add = _displayedColor.add;
            add.a = _displayedOpacity.add;
            ccUtils.SetRenderColor (this.meshRender, tint, add);
        }

        public override Color32 color
        {
            set
            {
                base.color = value;
                updateColor();
            }
        }

        public override ColorTransform colorTransform
        {
            get
            {
                return base.colorTransform;
            }
            set
            {
                base.colorTransform = value;
                updateColor();
            }
        }


        public override void updateDisplayedColor (ColorTransform parentColor)
        {
            base.updateDisplayedColor (parentColor);
            updateColor ();
        }

        public override byte opacity
        {
            set
            {
                base.opacity = value;
                updateColor();
            }
        }

        public override void updateDisplayedOpacity (OpacityTransform parentOpacity)
        {
            base.updateDisplayedOpacity (parentOpacity);
            updateColor ();
        }

        // ------------------------------------------------------------------------------
        //  draw & updateTransform
        // ------------------------------------------------------------------------------
        protected override void draw ()
        {
            ccUtils.CC_INCREMENT_GL_DRAWS ();
            updateMesh();
        }

        protected virtual void updateMesh()
        {
            if (_meshDirty)
            {
                CCSpriteFrame spriteFrame = _spriteFrame;
                Rect textureRect = spriteFrame.textureRect;
                float textureWidth = spriteFrame.texture.width;
                float textureHeight = spriteFrame.texture.height;
                float textureRectWidth = textureRect.width;
                float textureRectHeigh = textureRect.height;

                Vector2 anchorPointInPixels = this.anchorPointInPixels;
                Vector2 contentSize = this.contentSize;
                Vector2 spriteOffset = spriteFrame.offset;
                float viewWidth = textureRectWidth, viewHeight = textureRectHeigh;
                if (spriteFrame.rotated)
                {
                    viewWidth = textureRectHeigh;
                    viewHeight = textureRectWidth;
                }
                Vector2 center = spriteOffset + contentSize / 2 - anchorPointInPixels;
                Rect viewRect = new Rect(center.x -  viewWidth / 2, center.y -  viewHeight / 2, viewWidth, viewHeight);
                Rect clippingViewRect = viewRect;
                if (_clippingEnabled)
                {
                    clippingViewRect = ccUtils.RectIntersection(viewRect, _clippingRect);
                }

                Vector3[] vertices = new Vector3[4]
                {
                    new Vector3(clippingViewRect.xMin, clippingViewRect.yMax, 0) / UIWindow.PIXEL_PER_UNIT, //left-top
                    new Vector3(clippingViewRect.xMax, clippingViewRect.yMax, 0) / UIWindow.PIXEL_PER_UNIT, //right-top
                    new Vector3(clippingViewRect.xMax, clippingViewRect.yMin, 0) / UIWindow.PIXEL_PER_UNIT, //right-bottom
                    new Vector3(clippingViewRect.xMin, clippingViewRect.yMin, 0) / UIWindow.PIXEL_PER_UNIT  //left-bottom
                };

                Vector2[] uvs = new Vector2[4];
                float uLeft = textureRect.xMin / textureWidth;
                float uRight = textureRect.xMax / textureWidth;
                float vBottom = textureRect.yMin / textureHeight;
                float vTop = textureRect.yMax / textureHeight;

                if (!spriteFrame.rotated)
                {
                    if (_clippingEnabled)
                    {
                        float uLeftPadding = (clippingViewRect.xMin - viewRect.xMin) / textureWidth;
                        float uRightPadding = (clippingViewRect.xMax - viewRect.xMax) / textureWidth;
                        float vBottomPadding = (clippingViewRect.yMin - viewRect.yMin) / textureHeight;
                        float vTopPadding = (clippingViewRect.yMax - viewRect.yMax) / textureHeight;

                        uLeft += uLeftPadding;
                        uRight += uRightPadding;
                        vBottom += vBottomPadding;
                        vTop += vTopPadding;
                    }
                    uvs [0] = new Vector2(uLeft, vTop); //top-left
                    uvs [1] = new Vector2(uRight, vTop); //top-right
                    uvs [2] = new Vector2(uRight, vBottom); //bottom-right
                    uvs [3] = new Vector2(uLeft, vBottom); //bottom-left
                } else
                {
                    if (_clippingEnabled)
                    {
                        float vTopPadding = (viewRect.xMin - clippingViewRect.xMin) / textureHeight;
                        float vBottomPadding = (viewRect.xMax - clippingViewRect.xMax) / textureHeight;
                        float uLeftPadding = (clippingViewRect.yMin - viewRect.yMin) / textureWidth;
                        float uRightPadding = (clippingViewRect.yMax - viewRect.yMax) / textureWidth;

                        uLeft += uLeftPadding;
                        uRight += uRightPadding;
                        vBottom += vBottomPadding;
                        vTop += vTopPadding;
                    }
                    uvs [0] = new Vector2(uRight, vTop); //top-right
                    uvs [1] = new Vector2(uRight, vBottom); //bottom-right
                    uvs [2] = new Vector2(uLeft, vBottom); //bottom-left
                    uvs [3] = new Vector2(uLeft, vTop); //top-left
                }


                int[] triangles = new int[]{ 0, 1, 2, 2, 3, 0 };

                Vector3[] normals = new Vector3[]
                {
                    Vector3.back,
                    Vector3.back,
                    Vector3.back,
                    Vector3.back
                };

                Mesh mesh = this.meshFilter.sharedMesh;
                if (mesh == null)
                {
                    mesh = new Mesh();
                }
                mesh.vertices = vertices;
                mesh.triangles = triangles;
                mesh.normals = normals;
                mesh.uv = uvs;

                this.meshFilter.sharedMesh = mesh;
                _meshDirty = false;
            }
        }

        protected override Vector3 calculateRotation()
        {
            Vector3 rotation = base.calculateRotation();
            if (_flipX) {
                rotation.y += 180;
            } 
            if (_flipY) {
                rotation.y += 180;
                rotation.z += 180;
            }
            return rotation;
        }
    }
}

