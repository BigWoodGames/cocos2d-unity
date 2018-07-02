using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace BBGamelib{
	[ExecuteInEditMode]
	[AddComponentMenu("")]
	public class CC3SpriteFactory : MonoBehaviour
    {
		#region singleton
		static CC3SpriteFactory _Instance;
		[SerializeField] [HideInInspector]private bool firstPassFlag=true;
		//---------singleton------
		public static CC3SpriteFactory Instance
        {
			get{
				return _Instance;
			}
		}
		public virtual void Awake() 
        {
            if (Application.isPlaying)
            {
                if (_Instance != null && _Instance != this)
                {
                    Destroy(this.gameObject);
                    return;
                } else
                {
                    _Instance = this;
                }
            }
            if (firstPassFlag)
            {
                gameObject.transform.position =Vector3.zero;
                gameObject.name = "CC3SpriteFactory";
                firstPassFlag = false;
            }
		}
		#endregion
		
        Dictionary<string, GameObject> _prefabs = new Dictionary<string, GameObject>();
		Dictionary<string, AnimationClip[]> _animationClips = new Dictionary<string, AnimationClip[]>();
		Dictionary<string, List<GameObject>> _fbxs_path_list = new Dictionary<string, List<GameObject>>();
		
		public void preloadPrefab(string path)
        {
            if (!_prefabs.ContainsKey(path))
            {
                GameObject obj = Resources.Load<GameObject>(path);
                if (obj == null)
                    CCDebug.Warning("CC3SpriteFactory#preloadPrefab: {0} not found.", path);
                _prefabs [path] = obj;
            }
		}
		public void removePrefab(string path)
        {
			_prefabs.Remove (path);
		}
		
		
		public void preloadAnimationClips(string path)
        {
            if (!_animationClips.ContainsKey(path))
            {
                AnimationClip[] obj = Resources.LoadAll<AnimationClip>(path);
                if (obj == null)
                    CCDebug.Warning("CC3SpriteFactory#preloadAnimationClips: {0} not found.", path);
                _animationClips [path] = obj;
            }
		}
		public void removeAnimationClip(string path)
        {
			_animationClips.Remove (path);
		}
		
		public AnimationClip[] getAnimationClips(string path, bool createIfNeed){
			AnimationClip[] clips;
            if (_animationClips.TryGetValue(path, out clips))
            {
                return clips;
            } else if (createIfNeed)
            {
                clips = Resources.LoadAll<AnimationClip>(path);
                _animationClips [path] = clips;
                return clips;
            }
			return null;
		}
		
		public void preloadCache(string path, int num)
        {
            GameObject prefab;
            if (!_prefabs.TryGetValue(path, out prefab))
            {
                preloadPrefab(path);
                prefab = _prefabs [path];
            }
			List<GameObject> fbxs;
            if (!_fbxs_path_list.TryGetValue(path, out fbxs))
            {
                fbxs = new List<GameObject>();
                _fbxs_path_list [path] = fbxs;
            }
			int existCount = fbxs.Count;
            for (int i = existCount; i < num; i++)
            {
                GameObject obj = Instantiate(prefab) as GameObject; 
                obj.SetActive(false);
                obj.transform.SetParent(transform, false);
                fbxs.Add(obj);
            }
		}
		public void removeCache(string path)
        {
			List<GameObject> fbxs;
            if (_fbxs_path_list.TryGetValue(path, out fbxs))
            {
                var fbxsEnu = fbxs.GetEnumerator();
                while (fbxsEnu.MoveNext())
                {
                    var fbx = fbxsEnu.Current;
                    Destroy(fbx);
                }
            }
			_fbxs_path_list.Remove (path);
		}

        public GameObject getPrefab(string path)
        {
            GameObject prefab;
            if (!_prefabs.TryGetValue(path, out prefab))
            {
                preloadPrefab(path);
                prefab = _prefabs [path];
            }
            if (prefab == null)
                CCDebug.Warning("CC3SpriteFactory {0} not found.", path);
            return prefab;
        }

		public GameObject getPrefabObject(string path, bool createIfNeed=true)
        {
            List<GameObject> fbxs;
            if (_fbxs_path_list.TryGetValue(path, out fbxs) && fbxs.Any())
            {
                GameObject obj = fbxs [0];
                fbxs.RemoveAt(0);
                obj.transform.localPosition =Vector3.zero;
                obj.SetActive(true);
                return obj;
            } else if (createIfNeed)
            {
                GameObject prefab = getPrefab(path);
                GameObject obj = Instantiate(prefab) as GameObject; 
                return obj;
            }
            return null;
        }

		public void recyclePrefabObject(string path, GameObject obj)
        {
            //obj.transform.parent=xxx will change the prefab's local transform
            obj.transform.SetParent(this.transform, false);
            obj.SetActive(false);
            List<GameObject> fbxs;
            if (!_fbxs_path_list.TryGetValue(path, out fbxs))
            {
                fbxs = new List<GameObject>();
                _fbxs_path_list [path] = fbxs;
            }
            fbxs.Add(obj);
        }

		public void cleanup()
        {
            var fbxsListEnu = _fbxs_path_list.Values.GetEnumerator();
            while (fbxsListEnu.MoveNext())
            {
                var fbxs = fbxsListEnu.Current;
                var fbxsEnu = fbxs.GetEnumerator();
                while (fbxsEnu.MoveNext())
                {
                    var fbx = fbxsEnu.Current;
                    if (Application.isEditor)
                        DestroyImmediate(fbx, true);
                    else
                        Destroy(fbx);
                }
            }
            _fbxs_path_list.Clear();
            _prefabs.Clear();
            _animationClips.Clear();
        }
	}
}