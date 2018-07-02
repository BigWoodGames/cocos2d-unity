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
		// ------------------------------------------------------------------------------
		//  macros
		// ------------------------------------------------------------------------------
		public const string KEY_NODE = "CCFactory.NODE";
		public const string KEY_SPRITE = "CCFactory.SPRITE";
		public const string KEY_LABEL = "CCFactory.LABEL";
		public const string LAYER_DEFAULT = "Default";
		
		// ------------------------------------------------------------------------------
		//  singleton
		// ------------------------------------------------------------------------------
		[SerializeField] [HideInInspector]private bool firstPassFlag=true;
		static CCFactory _Instance=null;
		public static CCFactory Instance{
			get{
				return _Instance;
			}
		}
		public virtual void Awake() {
			if (Application.isPlaying) {
				_materialPropertyBlock=new MaterialPropertyBlock();
				if (_Instance != null && _Instance != this) {
					Destroy (this.gameObject);
					return;
				} else {
					_Instance = this;
				}
			} 
			if (firstPassFlag) {
				gameObject.transform.position =Vector3.zero;
				gameObject.name = "CCFactory";
				firstPassFlag = false;
			}
		}
		
		// ------------------------------------------------------------------------------
		//  properties
		// ------------------------------------------------------------------------------
		[Serializable]
		public class Storage{
			[SerializeField] public string category;
			[SerializeField] public string[] componentTypeNames;
			[SerializeField] public List<CCFactoryGear> gears = new List<CCFactoryGear>(); 
		}
		[Serializable]public  class DictionaryOfStringAndStorage : NSSerializableDictionary<string, Storage> {}
		[SerializeField] DictionaryOfStringAndStorage _storages = new DictionaryOfStringAndStorage();
		/**
		 public the storages for preloading
		 example of how preload gear: 
			splash scene:
		 	gear.gameObject.SetActive (true); 
		 	...
		 	next frame:
		 	gear.gameObject.SetActive (false); 
		 */
		public DictionaryOfStringAndStorage storages{ get { return _storages; } }
		
		// shared proerties block of material
		MaterialPropertyBlock _materialPropertyBlock;
		public MaterialPropertyBlock materialPropertyBlock{ get { return _materialPropertyBlock; } }

		// ------------------------------------------------------------------------------
		//  public edit method
		// ------------------------------------------------------------------------------
		public void generateNodeGearsInEditMode(int num){
			generateGearsInEditMode (KEY_NODE, null, num);
		}
		public void generateSpriteGearsInEditMode(int num){
            generateGearsInEditMode (KEY_SPRITE, CCSprite.kGearTypes, num);
		}
		public void generateLabelGearsInEditMode(int num){
            generateGearsInEditMode (KEY_LABEL, CCLabelTTF.kGearTypes, num);
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
				gear.gameObject.transform.SetParent(transform);
				gear.gameObject.hideFlags = HideFlags.HideInHierarchy;
				gear.gameObject.SetActive(false);
				storage.gears.Add(gear);
			}
		}
		
		// ------------------------------------------------------------------------------
		//  public runtime metod
		// ------------------------------------------------------------------------------
		public CCFactoryGear takeGear(string category){
			Storage storage = getStorage (category, false);
			if (storage!=null) {
				while(storage.gears.Count>0){
					CCFactoryGear gear = storage.gears[0];
					storage.gears.RemoveAt(0);
					gear.gameObject.hideFlags = HideFlags.None;
					gear.gameObject.SetActive(true);
					if (gear.gameObject.transform.childCount != 0) {
						CCDebug.Warning("CCFactory try to take a not empty gear: {0}-{1}.", gear.gameObject, gear.gameObject.transform.GetChild(0));
						DestroyObject(gear.gameObject);
					}else{
						return gear;
					}
				}
				//no gear caches
				{
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
			if (gear.gameObject.transform.childCount != 0) {
				CCDebug.Warning("CCFactory try to recyle a not empty gear: {0}-{1}.", gear.gameObject, gear.gameObject.transform.GetChild(0));
				DestroyObject(gear.gameObject);
				return false;
			}


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

			gear.gameObject.layer = LayerMask.NameToLayer ("Default");
			gear.gameObject.transform.SetParent(this.transform);
			gear.gameObject.transform.localEulerAngles =Vector3.zero;
			gear.gameObject.transform.localScale = new Vector3 (1, 1, 1);
			gear.gameObject.transform.localPosition =Vector3.zero;
			gear.gameObject.name = string.Format("{0}-{1}", category, storage.gears.Count);
			gear.gameObject.hideFlags = HideFlags.HideInHierarchy;
			gear.gameObject.SetActive(false);
			storage.gears.Add(gear);

			if (gear.gameObject.transform.parent == null) {
				NSUtils.Assert(gear.gameObject.transform.parent != null, "Recyle# set parent fail!");
			}

			return true;
		}
		
		// ------------------------------------------------------------------------------
		//  private
		// ------------------------------------------------------------------------------
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
	}
}

