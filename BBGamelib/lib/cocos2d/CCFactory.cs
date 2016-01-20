using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BBGamelib{
	#region Data Structure
	[Serializable]
	public class CCFactoryGear{
		public GameObject gameObject;
		public Component[] components;
	}
	#endregion

	[ExecuteInEditMode]
	[AddComponentMenu("")]
	public class CCFactory : MonoBehaviour
	{
		#region default categories
		public const string KEY_NODE = "CCFactory.NODE";
		public const string KEY_SPRITE = "CCFactory.SPRITE";
		public const string KEY_SPRITE_BOXCOLLIDER2D = "CCFactory.SPRITE_BOXCOLLIDER2D";
		public const string KEY_SPRITE_CIRCLECOLLIDER2D = "CCFactory.SPRITE_CIRCLECOLLIDER2D";
		public const string KEY_LABEL = "CCFactory.LABEL";
		#endregion

		#region singleton
		[SerializeField] [HideInInspector]private bool firstPassFlag=true;
		static CCFactory _Instance=null;
		//---------singleton------
		public static CCFactory Instance{
			get{
				return _Instance;
			}
		}
		public virtual void Awake() {
			if (Application.isPlaying) {
				if (_Instance != null && _Instance != this) {
					Destroy (this.gameObject);
					return;
				} else {
					_Instance = this;
				}
				DontDestroyOnLoad (this.gameObject);
			} 
			if (firstPassFlag) {
				gameObject.transform.position = Vector3.zero;
				gameObject.name = "CCFactory";
				firstPassFlag = false;
			}
		}
		#endregion
		
		#region properties
		[Serializable]
		class Storage{
			[SerializeField] public string category;
			[SerializeField] public string[] componentTypeNames;
			[SerializeField] public List<CCFactoryGear> gears = new List<CCFactoryGear>(); 
		}
		[Serializable]  class DictionaryOfStringAndStorage : NSSerializableDictionary<string, Storage> {}
		[SerializeField] DictionaryOfStringAndStorage _storages = new DictionaryOfStringAndStorage();
		#endregion

		
		#region public method
		public void generateNodeGearsInEditMode(int num){
			generateGearsInEditMode (KEY_NODE, null, num);
		}
		public void generateSpriteGearsInEditMode(int num){
			generateGearsInEditMode (KEY_SPRITE, new Type[1]{typeof(SpriteRenderer)}, num);
		}
		public void generateLabelGearsInEditMode(int num){
			generateGearsInEditMode (KEY_LABEL, new Type[2]{typeof(MeshRenderer), typeof(TextMesh)}, num);
		}

		public void generateGearsInEditMode(string category, Type[] componentTypes, int num){
			NSUtils.Assert (!Application.isPlaying, "CCFactory:generateGearsInEditMode works in edit mode only!");

			if(componentTypes==null)
				componentTypes = new Type[0];

			Storage storage = getStorage (category, true);
			storage.componentTypeNames = new string[componentTypes.Length];
			for(int i=0; i<componentTypes.Length; i++){
				storage.componentTypeNames[i] = componentTypes[i].AssemblyQualifiedName;
			}

			for(int i=0; i<num; i++){
				CCFactoryGear gear = buildGear(componentTypes);
				gear.gameObject.name = string.Format("{0}-{1}", category, i);
				gear.gameObject.transform.parent = transform;
				gear.gameObject.hideFlags = HideFlags.HideInHierarchy;
				gear.gameObject.SetActive(false);
				storage.gears.Add(gear);
			}
		}

		public CCFactoryGear takeGear(string category){
			Storage storage = getStorage (category, false);
			if (storage!=null) {
				if(storage.gears.Count>0){
//					return storage.gears.Dequeue();
					CCFactoryGear gear = storage.gears[0];
					storage.gears.RemoveAt(0);
					gear.gameObject.hideFlags = HideFlags.None;
					gear.gameObject.SetActive(true);
					return gear;
				}else{
					Type[] componentTypes = new Type[storage.componentTypeNames.Length];
					for(int i=0; i<componentTypes.Length; i++){
						componentTypes[i] = Type.GetType(storage.componentTypeNames[i]);
					}
					CCFactoryGear gear = buildGear(componentTypes);
					gear.gameObject.hideFlags = HideFlags.None;
					gear.gameObject.SetActive(true);
					return gear;
				}
			}
			return null;
		}


		public bool recycleGear(string category, CCFactoryGear gear, bool constraint=false){
			Storage storage = getStorage (category, true);
			if (storage.componentTypeNames == null) {
				string[] componentTypeNames = new string[gear.components.Length];
				for (int i=0; i<componentTypeNames.Length; i++) {
					componentTypeNames [i] = gear.components [i].GetType ().FullName;
				}
				storage.componentTypeNames = componentTypeNames;
			} else if(constraint){
				if(storage.componentTypeNames.Length != gear.components.Length){
					return false;
				}else{
					HashSet<string> typeSet = new HashSet<string>(storage.componentTypeNames);

					int count = gear.components.Length;
					for(int i=0; i<count; i++){
						Component com = gear.components[i];
						string tName = com.GetType().FullName;
						if(!typeSet.Remove(tName)){
							return false;
						}
					}
				}
			}
			gear.gameObject.transform.parent = transform;
			gear.gameObject.transform.localEulerAngles = Vector3.zero;
			gear.gameObject.transform.localScale = new Vector3 (1, 1, 1);
			gear.gameObject.transform.localPosition = Vector3.zero;
			gear.gameObject.name = string.Format("{0}-{1}", category, storage.gears.Count);
			gear.gameObject.hideFlags = HideFlags.HideInHierarchy;
			gear.gameObject.SetActive(false);
			storage.gears.Add(gear);
			return true;
		}
		#endregion

		#region inner method
		CCFactoryGear buildGear(Type[] componentTypes){
			CCFactoryGear gear = new CCFactoryGear();
			
			GameObject obj = new GameObject ();
			gear.gameObject = obj;
			
			Component[] coms = new Component[componentTypes.Length];
			for(int j=0; j<componentTypes.Length; j++){
				coms[j] = obj.AddComponent(componentTypes[j]);
			}
			gear.components = coms;	

			return gear;
		}
		
		Storage getStorage(string category, bool createIfNotExist){
			Storage storage = null;
			if (!_storages.TryGetValue (category, out storage)) {
				if(createIfNotExist){
					storage = new Storage();
					storage.category = category;
					_storages[category] = storage;
				}else{
					storage = null;
				}
			}
			return storage;
		}
		#endregion
	}
}

