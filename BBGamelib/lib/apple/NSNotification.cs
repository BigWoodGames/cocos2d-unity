using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

namespace BBGamelib{
	#region NSNotification
	public class NSNotification
	{
		public readonly string name;
		public readonly System.Object sender;
		public readonly NSDictionary userInfo;
		public NSNotification(string aName, System.Object aObj, NSDictionary aUserInfo=null){
			name = aName;
			sender = aObj;
			userInfo = aUserInfo;
		}
		public override string ToString (){
			return string.Format ("[NSNotification: name={0}, obj={1}, userInfo={2}]", name, sender, userInfo);
		}
	}
	#endregion

	#region NSNotificationCenter
	public class NSNotificationCenter
	{
		class NSNotificationSelectorAndSender{
			public Action<NSNotification> selector;
			public System.Object sender;
		}
		class NSNotificationObserver{
			public System.Object target;
			public List<NSNotificationSelectorAndSender> selectorAndSenders;
		}

		Dictionary<string, List<NSNotificationObserver>> _name_observers;

		#region Singleton
		static NSNotificationCenter _defaultCenter;
		public static NSNotificationCenter defaultCenter{
			get{
				if(_defaultCenter == null)
					_defaultCenter = new NSNotificationCenter();
				return _defaultCenter;
			}
		}
		#endregion

		public NSNotificationCenter(){
			_name_observers = new Dictionary<string, List<NSNotificationObserver>> (32);
		}

		public void addObserver(System.Object target, Action<NSNotification> aSelector, string aName, System.Object sender=null){
			List<NSNotificationObserver> observers = null;
			if (!_name_observers.TryGetValue (aName, out observers)) {
				_name_observers[aName] = observers = new List<NSNotificationObserver>();		
			}
			NSNotificationObserver observer = null;
			var enumerator = observers.GetEnumerator();
			while (enumerator.MoveNext()) {
				NSNotificationObserver aObserver = enumerator.Current;
				if(aObserver.target == target){
					observer = aObserver;
				}
			}
			if (observer == null) {
				observer = new NSNotificationObserver ();
				observer.target = target;
				observer.selectorAndSenders = new List<NSNotificationSelectorAndSender> ();
				observers.Add(observer);
			} else {
				var eu = observer.selectorAndSenders.GetEnumerator();
				while (eu.MoveNext()) {
					NSNotificationSelectorAndSender selectorAndSender = eu.Current;
					NSUtils.Assert(selectorAndSender.selector != aSelector || selectorAndSender.sender != sender, 
					               "NSNotificationDelegate#addObserver : Dont' add observer with same target/method/sender !");			
				}	
			}
			NSNotificationSelectorAndSender selAndSender = new NSNotificationSelectorAndSender();
			selAndSender.selector = aSelector;
			selAndSender.sender = sender;
			observer.selectorAndSenders.Add (selAndSender);
		}

		public System.Object addObserverForName(string aName, System.Object sender, Action<NSNotification> block){
			System.Object target = new System.Object ();
			addObserver (target, block, aName, sender);
			return target;
		}

		public void postNotification(NSNotification n){
			List<NSNotificationObserver> observers = observersForName (n.name);
			if (observers == null) return;
			for(int i=observers.Count-1; i>=0; i--){
				NSNotificationObserver observer = observers[i];
				List<NSNotificationSelectorAndSender> selectorAndSenders = observer.selectorAndSenders;
				for(int j=selectorAndSenders.Count - 1; j>=0; j--){
					NSNotificationSelectorAndSender selectorAndSender = selectorAndSenders[j];
					if(selectorAndSender.sender == null || selectorAndSender.sender == n.sender){
						selectorAndSender.selector(n);
					}
				}
			}

		}
		public void postNotification(string name, System.Object sender, NSDictionary userInfo=null){
			NSNotification notification = new NSNotification (name, sender, userInfo);
			postNotification (notification);
		}

		public void removeObserver(System.Object target){
			Dictionary<string ,List<NSNotificationObserver>> name_observers_copy = new Dictionary<string, List<NSNotificationObserver>> (_name_observers);

			var enumerator = name_observers_copy.GetEnumerator();
			while (enumerator.MoveNext()) {
				KeyValuePair<string ,List<NSNotificationObserver>> keyValuePair = enumerator.Current;
				List<NSNotificationObserver> observers = keyValuePair.Value;
				observers.RemoveAll(observer=>observer.target==target);
				if(observers.Count==0){
					_name_observers.Remove(keyValuePair.Key);
				}
			}
		}
		public void removeObserver(System.Object target, string name){
			List<NSNotificationObserver> observers = observersForName (name);
			if (observers == null) return;
			observers.RemoveAll (observer => observer.target == target);
			if(observers.Count==0){
				_name_observers.Remove(name);
			}
		}


		public void removeObserver(System.Object target, string name, System.Object sender){
			List<NSNotificationObserver> observers = observersForName (name);
			if (observers == null) return;
			NSNotificationObserver observer = null;


			var enumerator = observers.GetEnumerator();
			while (enumerator.MoveNext()) {
				NSNotificationObserver aObserver = enumerator.Current;
				if(aObserver.target == target){
					observer = aObserver;
					break;
				}
			}

			List<NSNotificationSelectorAndSender> selectorAndSenders = observer.selectorAndSenders;
			selectorAndSenders.RemoveAll (selectorAndSender => selectorAndSender.sender == sender);
			if (selectorAndSenders.Count==0) 
				observers.Remove(observer);
			if(observers.Count==0)
				_name_observers.Remove(name);
		}

		List<NSNotificationObserver> observersForName(string aName){
			List<NSNotificationObserver> observers = null;
			_name_observers.TryGetValue (aName, out observers);
			return observers;
		}
	}
	#endregion
}
