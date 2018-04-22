using UnityEngine;
using System.Collections;
using UnityEngine.Rendering;

namespace BBGamelib
{
    public class CC3TileSprite : CC3Node
    {
        public class Tile{
            public CCSpriteFrame spriteFrame;
            public CGAffineTransform transform;
        }
        private Texture2D _texture;
        private utList<Tile> _tiles;
        private bool    _flipX;
        private bool    _flipY;
        private bool _meshDirty;

        // ------------------------------------------------------------------------------
        //  init
        // ------------------------------------------------------------------------------
        public CC3TileSprite(string textureName) : this(Resources.Load<Texture2D>(textureName))
        {
        }

        public CC3TileSprite(Texture2D texture)
        {
            this.nameInHierarchy = string.Concat(GetType().Name, "-", texture.name);
            _texture = texture;
            _tiles = new utList<Tile>();
            _meshDirty = false;
            this.meshRender.sharedMaterial = CCMaterialCache.sharedMaterialCache.getMaterial(texture);
            this.renderQueue = RenderQueue.Geometry;
            this.renderType = "Opaque";
            ccUtils.SetRenderValue (this.meshRender, "_CullMode", (int)CullMode.Back);
            ccUtils.SetRenderValue (this.meshRender, "_ZWrite", 1);
            ccUtils.SetRenderColor (this.meshRender, Color.white, new Color32(0, 0, 0, 0));
        }

        protected override void init()
        {
            CCFactoryGear gear = CCFactory.Instance.takeGear (CCFactory.KEY_SPRITE);
            initWithGear (gear);
            this.meshRender.shadowCastingMode = ShadowCastingMode.Off;
            this.meshRender.receiveShadows = false;
            this.meshRender.motionVectors = false;   
        }  

        protected override void recycleGear()
        {
            this.meshRender.sharedMaterial = null;
            CCFactory.Instance.recycleGear(CCFactory.KEY_SPRITE, _gear);
        }

        // ------------------------------------------------------------------------------
        //  edit tile
        // ------------------------------------------------------------------------------
        public Tile addTile(string frameName, Vector2 centerPosInNodeSpace)
        {
            CCSpriteFrame frame = CCSpriteFrameCache.sharedSpriteFrameCache.spriteFrameByName(frameName);
            return addTile(frame, centerPosInNodeSpace);
        }

        public Tile addTile(string frameName, Vector2 centerPosInNodeSpace, float scaleX, float scaleY, float degree)
        {
            CCSpriteFrame frame = CCSpriteFrameCache.sharedSpriteFrameCache.spriteFrameByName(frameName);
            return addTile(frame, centerPosInNodeSpace, scaleX, scaleY, degree);
        }

        public Tile addTile(CCSpriteFrame frame, Vector2 centerPosInNodeSpace)
        {
            return addTile(frame, centerPosInNodeSpace, 1, 1, 0);
        }

        public Tile addTile(CCSpriteFrame frame, Vector2 centerPosInNodeSpace, float scaleX, float scaleY, float degree)
        {
            centerPosInNodeSpace /= UIWindow.PIXEL_PER_UNIT;
            CGAffineTransform t = CGAffineTransform.MakeTranslation(centerPosInNodeSpace.x, centerPosInNodeSpace.y);
            t = CGAffineTransform.Scale(t, scaleX, scaleY);
            t = CGAffineTransform.Rotate(t, -ccUtils.CC_DEGREES_TO_RADIANS(degree));
            return addTile(frame, t);
        }

        Tile addTile(CCSpriteFrame frame, CGAffineTransform transform)
        {
            NSUtils.Assert(_texture == frame.texture, "cocos2d:CCTileSprite#setTile: only support one texture in a tile sprite.");
            Tile tile = new Tile();
            tile.spriteFrame = frame;
            tile.transform = transform;
            _tiles.DL_APPEND(tile);
            _meshDirty = true;
            return tile;
        }

        public Tile getTile(Vector2 positionInNodeSpace)
        {
            Vector2 uPositionInNodeSpace = positionInNodeSpace / UIWindow.PIXEL_PER_UNIT;
            for (var ent = _tiles.head; ent != null; ent = ent.next)
            {
                Vector2 sptSize = ent.obj.spriteFrame.originalSize  / UIWindow.PIXEL_PER_UNIT;
                Rect sptRect = new Rect(- sptSize /2, sptSize);
                sptRect = CGAffineTransform.CGRectApplyAffineTransform(sptRect, ent.obj.transform);
                if (sptRect.Contains(uPositionInNodeSpace))
                {
                    return ent.obj;
                }
            }
            return null;
        }

        public utList<Tile> getTilesAt(Vector2 positionInNodeSpace)
        {
            Vector2 uPositionInNodeSpace = positionInNodeSpace / UIWindow.PIXEL_PER_UNIT;
            utList<Tile> tiles = new utList<Tile>();
            for (var ent = _tiles.head; ent != null; ent = ent.next)
            {
                Vector2 sptSize = ent.obj.spriteFrame.originalSize  / UIWindow.PIXEL_PER_UNIT;
                Rect sptRect = new Rect(- sptSize /2, sptSize);
                sptRect = CGAffineTransform.CGRectApplyAffineTransform(sptRect, ent.obj.transform);
                if (sptRect.Contains(uPositionInNodeSpace))
                {
                    tiles.DL_APPEND(ent.obj);
                }
            }
            return tiles;
        }

        public void removeTile(Tile tile)
        {
            _tiles.DL_DELETE(tile);
            _meshDirty = true;
        }

        public void removeTiles(Vector2 positionInNodeSpace)
        {
            Vector2 uPositionInNodeSpace = positionInNodeSpace / UIWindow.PIXEL_PER_UNIT;
            for (var ent = _tiles.head; ent != null; ent = ent.next)
            {
                Vector2 sptSize = ent.obj.spriteFrame.originalSize  / UIWindow.PIXEL_PER_UNIT;
                Rect sptRect = new Rect(- sptSize /2, sptSize);
                sptRect = CGAffineTransform.CGRectApplyAffineTransform(sptRect, ent.obj.transform);
                if (sptRect.Contains(uPositionInNodeSpace))
                {
                    _tiles.DL_DELETE(ent.obj);
                }
            }
            _meshDirty = true;
        }

        public Rect getBounds(){
            Rect rect = new Rect();
            for (var ent = _tiles.head; ent != null; ent = ent.next)
            {
                Vector2 sptSize = ent.obj.spriteFrame.originalSize  / UIWindow.PIXEL_PER_UNIT;
                Rect sptRect = new Rect(- sptSize /2, sptSize);
                sptRect = CGAffineTransform.CGRectApplyAffineTransform(sptRect, ent.obj.transform);
                rect = ccUtils.RectUnion(rect, sptRect);
            }
            rect.position *= UIWindow.PIXEL_PER_UNIT;
            rect.size *= UIWindow.PIXEL_PER_UNIT;
            return rect;
        }

        // ------------------------------------------------------------------------------
        //  draw & updateTransform
        // ------------------------------------------------------------------------------
        protected override void draw ()
        {
            ccUtils.CC_INCREMENT_GL_DRAWS ();
            updateMesh();
        }

        void updateMesh()
        {
            if (_meshDirty)
            {
                int tilesCount = _tiles.DL_COUNT();
                Vector3[] vertices = new Vector3[tilesCount * 4];
                Vector3[] normals = new Vector3[vertices.Length];
                Vector2[] uvs = new Vector2[vertices.Length];
                int[] triangles = new int[tilesCount * 6];

                int tileIndex = 0;
                for (var ent = _tiles.head; ent != null; ent = ent.next)
                {  
                    Tile tile = ent.obj;
                    CCSpriteFrame spriteFrame = tile.spriteFrame;
                    Rect uTextureRect = spriteFrame.textureRect;
                    uTextureRect.position /= UIWindow.PIXEL_PER_UNIT;
                    uTextureRect.size /= UIWindow.PIXEL_PER_UNIT;
                    float uTextureWidth = spriteFrame.texture.width / UIWindow.PIXEL_PER_UNIT;
                    float uTextureHeight = spriteFrame.texture.height / UIWindow.PIXEL_PER_UNIT;
                    float uTextureRectWidth = uTextureRect.width;
                    float uTextureRectHeigh = uTextureRect.height;


                    Vector2 uSpriteOffset = spriteFrame.offset / UIWindow.PIXEL_PER_UNIT;
                    Vector2 center = uSpriteOffset;
                    float centerX = center.x;
                    float centerY = center.y;
                    float uViewWidth = uTextureRectWidth, uViewHeight = uTextureRectHeigh;
                    if (spriteFrame.rotated)
                    {
                        uViewWidth = uTextureRectHeigh;
                        uViewHeight = uTextureRectWidth;
                    }
                    int verticesIndex = tileIndex * 4;
                    vertices [verticesIndex + 0] = new Vector3(centerX - uViewWidth / 2, centerY + uViewHeight / 2, 0);
                    vertices [verticesIndex + 1] = new Vector3(centerX + uViewWidth / 2, centerY + uViewHeight / 2, 0);
                    vertices [verticesIndex + 2] = new Vector3(centerX + uViewWidth / 2, centerY - uViewHeight / 2, 0);
                    vertices [verticesIndex + 3] = new Vector3(centerX - uViewWidth / 2, centerY - uViewHeight / 2, 0);

                    int trianglesIndex = tileIndex * 6;
                    triangles [trianglesIndex + 0] = verticesIndex + 0;
                    triangles [trianglesIndex + 1] = verticesIndex + 1;
                    triangles [trianglesIndex + 2] = verticesIndex + 2;
                    triangles [trianglesIndex + 3] = verticesIndex + 2;
                    triangles [trianglesIndex + 4] = verticesIndex + 3;
                    triangles [trianglesIndex + 5] = verticesIndex + 0;

                    float uLeft = uTextureRect.xMin / uTextureWidth;
                    float vBottom = uTextureRect.yMin / uTextureHeight;
                    float uRight = uTextureRect.xMax / uTextureWidth;
                    float vTop = uTextureRect.yMax / uTextureHeight;
                    if (!spriteFrame.rotated)
                    {
                        uvs [verticesIndex + 0] = new Vector2(uLeft, vTop); //top-left
                        uvs [verticesIndex + 1] = new Vector2(uRight, vTop); //top-right
                        uvs [verticesIndex + 2] = new Vector2(uRight, vBottom); //bottom-right
                        uvs [verticesIndex + 3] = new Vector2(uLeft, vBottom); //bottom-left
                    } else
                    {
                        uvs [verticesIndex + 0] = new Vector2(uRight, vTop); //top-right
                        uvs [verticesIndex + 1] = new Vector2(uRight, vBottom); //bottom-right
                        uvs [verticesIndex + 2] = new Vector2(uLeft, vBottom); //bottom-left
                        uvs [verticesIndex + 3] = new Vector2(uLeft, vTop); //top-left
                    }

                    //apply transform
                    if (!CGAffineTransform.IsIdentity(tile.transform))
                    {
                        for (int i = 0; i < 4; i++)
                        {
                            Vector2 vertex = vertices [verticesIndex + i];
                            vertex = CGAffineTransform.CGPointApplyAffineTransform(vertex, tile.transform);
                            vertices [verticesIndex + i] = vertex;
                        }
                    }
                    tileIndex++;
                }
                for (int i = 0; i < normals.Length; i++)
                {
                    normals [i] = Vector3.back;
                }

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
        //  public method
        // ------------------------------------------------------------------------------
        public MeshFilter meshFilter
        { 
            get { return _gear.components [0] as MeshFilter; } 
        }

        public MeshRenderer meshRender
        { 
            get { return _gear.components [1] as MeshRenderer; } 
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
            set{  this.meshRender.material.SetOverrideTag("RenderType", value);}
        }

        public int zWrite
        {
            get{ return this.meshRender.sharedMaterial.GetInt("_ZWrite"); }
            set{ ccUtils.SetRenderValue (this.meshRender, "_ZWrite", 0); }
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

        public override string ToString ()
        {
            return string.Format ("<{0} = {1} | Size = ({2:0.00},{3:0.00},{4:0.00},{5:0.00}) | tag = {6}>", 
                GetType().Name, GetHashCode(),
                position.x, position.y,
                contentSize.x, contentSize.y,
                _userTag);
        }
    }
}

