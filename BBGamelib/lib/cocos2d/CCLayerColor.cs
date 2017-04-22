using UnityEngine;
using System.Collections;
using UnityEngine.Rendering;

namespace BBGamelib
{
    public class CCLayerColor : CCLayerRGBA
    {
        Vector3[] _squareVertices;
        Color _squareColor;
        Texture2D _texture;
        bool _meshDirty;

        public CCLayerColor(){
        }

        public CCLayerColor(Color32 color)
        {
            init(color);
        }

        public CCLayerColor(Color32 color, float w, float h)
        {
            init(color, w, h);
        }

        protected override void init()
        {
            CCFactoryGear gear = CCFactory.Instance.takeGear (CCFactory.KEY_SPRITE);
            initWithGear (gear);
            this.meshRender.shadowCastingMode = ShadowCastingMode.Off;
            this.meshRender.receiveShadows = false;
            this.meshRender.motionVectors = false;
            _squareVertices = new Vector3[4];

            _texture = new Texture2D(64, 64, TextureFormat.ARGB32, false, false);
            Material mat = new Material (Shader.Find("BBGamelib/CCTexture"));
            mat.name = "CCMaterial-New";
            mat.mainTexture = _texture;
            this.meshRender.sharedMaterial = mat;

            init(new Color(0, 0, 0, 0));
        }

        void init(Color32 color){
            Vector2 s = CCDirector.sharedDirector.winSize;
            init(color, s.x, s.y);
        }

        void init(Color32 color, float w, float h)
        {
            _displayedColor.tint = _realColor.tint = color;
            _displayedOpacity.tint = _realOpacity.tint = color.a;

            if (_squareVertices != null)
            {
                for (int i = 0; i < _squareVertices.Length; i++)
                {
                    _squareVertices [i].x = 0.0f;
                    _squareVertices [i].y = 0.0f;
                    _squareVertices [i].z = 0.0f;
                }
            }
            this.updateColor();
            this.contentSize = new Vector2(w, h);
        }

        protected override void recycleGear()
        {
            Material mat = this.meshRender.sharedMaterial;
            this.meshRender.sharedMaterial = null;
            MonoBehaviour.Destroy(mat);
            MonoBehaviour.Destroy(_texture);
            CCFactory.Instance.recycleGear(CCFactory.KEY_SPRITE, _gear);
        }

        // ------------------------------------------------------------------------------
        //  color & opacity
        // ------------------------------------------------------------------------------
        public void updateColor()
        {
            _squareColor = _displayedColor.tint;
            _squareColor.a = _displayedOpacity.tint/255.0f;
        }

        public override Color32 color
        {
            get
            {
                return base.color;
            }
            set
            {
                base.color = value;
                updateColor();
                _meshDirty = true;
            }
        }

        public override byte opacity
        {
            get
            {
                return base.opacity;
            }
            set
            {
                base.opacity = value;
                updateColor();
                _meshDirty = true;
            }
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

        public override Vector2 contentSize
        {
            get
            {
                return base.contentSize;
            }
            set
            {
                Vector2 vInUnit = value / UIWindow.PIXEL_PER_UNIT;
                _squareVertices [0] = new Vector2(0, 0);
                _squareVertices [1] = new Vector2(0, vInUnit.y);
                _squareVertices [2] = new Vector2(vInUnit.x, vInUnit.y);
                _squareVertices [3] = new Vector2(vInUnit.x, 0);
                base.contentSize = value;
                _meshDirty = true;
            }
        }

        public void changeSize(float w, float h)
        {
            this.contentSize = new Vector2(w, h);
        }

        public void changeWidth(float w)
        {
            this.contentSize = new Vector2(w, _contentSize.x);
        }

        public void changeHeight(float h)
        {
            this.contentSize = new Vector2(_contentSize.x, h);
        }


        // ------------------------------------------------------------------------------
        //  draw & update mesh
        // ------------------------------------------------------------------------------
        protected override void draw ()
        {
            ccUtils.CC_INCREMENT_GL_DRAWS ();
            this.meshRender.sortingOrder = CCDirector.sharedDirector.globolRendererSortingOrder ++;
            updateMesh();
        }

        protected virtual void updateMesh()
        {
            if (_meshDirty)
            {
                for (int x = 0; x < _texture.width; x++)
                {
                    for (int y = 0; y < _texture.height; y++)
                    {
                        _texture.SetPixel(x, y, _squareColor);
                    }
                }
                _texture.Apply();
                Vector3[] vertices = _squareVertices;
                Vector2[] uvs = new Vector2[4]
                {
                    new Vector2(0, 0),
                    new Vector2(0, 1),
                    new Vector2(1, 1),
                    new Vector2(1, 0)
                };
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
    }
}

