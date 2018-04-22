using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace BBGamelib{
    public class CCMaterialCache
    {
        utHash<Texture2D, Material> _materials;

        // ------------------------------------------------------------------------------
        //  singleton
        // ------------------------------------------------------------------------------
        static CCMaterialCache _sharedMaterialCache=null;
        /** Retruns ths shared instance of the material cache */
        public static CCMaterialCache sharedMaterialCache
        {
            get{
                if (_sharedMaterialCache==null)
                    _sharedMaterialCache = new CCMaterialCache ();

                return _sharedMaterialCache;
            }
        }
        /** Purges the cache. It releases all the materials.*/
        public static void PurgeMaterialCache()
        {
            if (_sharedMaterialCache != null)
            {
                var enumerator = _sharedMaterialCache._materials.Values.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    Material mat = enumerator.Current;
                    MonoBehaviour.Destroy(mat);
                }
                _sharedMaterialCache = null;
            }
        }

        CCMaterialCache()
        {
            _materials = new utHash<Texture2D, Material> ();
        }

        // ------------------------------------------------------------------------------
        //  find, add and remove material
        // ------------------------------------------------------------------------------
        public Material getMaterial(Texture2D texture)
        {
            Material mat = _materials [texture];
            if (mat == null)
            {
                mat = new Material (Shader.Find("BBGamelib/CCSprite"));
                mat.mainTexture = texture;
                mat.name = string.Concat("CCMaterial-", texture.name);
                _materials [texture] = mat;
            }
            return mat;
        }

        public void addMaterial(Texture2D texture, Material mat)
        {
            _materials [texture] = mat;
        }

        public void removeMaterial(Texture2D texture)
        {
            Material existMat = _materials [texture];
            if (existMat != null)
            {
                MonoBehaviour.Destroy(existMat);
                _materials.Remove(texture);
            }
        }

    }
}
