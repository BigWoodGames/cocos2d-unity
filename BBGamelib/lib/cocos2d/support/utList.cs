using UnityEngine;
using System.Collections;
using System;

namespace BBGamelib{
	public class utNode<T>{
		public utNode<T>	prev, next;
		public T obj;
	}
	public class utList<T>
	{
		utNode<T> _head;
		public utNode<T> head{get{return _head;}}

		public void DL_APPEND(T t){
			utNode<T> listElement = new utNode<T> ();
			listElement.next = listElement.prev = null;
			listElement.obj = t;
			DL_APPEND (listElement);
		}

		public void DL_APPEND(utNode<T> add)
		{
			if (_head != null) {
				add.prev = _head.prev;
				_head.prev.next = add;
				_head.prev = add;
				add.next = null; 
			} else {
				_head = add;
				_head.prev = _head;
				_head.next = null;
			}
		}

		public void DL_PREPEND(T t){
			utNode<T> listElement = new utNode<T> ();
			listElement.next = listElement.prev = null;
			listElement.obj = t;
			DL_PREPEND (listElement);
		}

		public void DL_PREPEND(utNode<T> add){
			(add).next = _head;                                                                           
			if (_head != null) {                                                                                   
				(add).prev = (_head).prev;                                                                 
				(_head).prev = (add);                                                                       
			} else {                                                                                      
				(add).prev = (add);                                                                        
			} 
			(_head) = (add);   
		}

		public void DL_DELETE(utNode<T> del){
			NSUtils.Assert((del).prev != null, "utList#DL_DELETE: del.prev should not be null.");                                                                 
			if ((del).prev == (del)) {                                                                  
				(_head)=null;                                                                             
			} else if ((del)==(_head)) {         
				if(del.next != null)
					(del).next.prev = (del).prev;                                                         
				(_head) = (del).next;                                                                    
			} else {                                                                                     
				(del).prev.next = (del).next;                                                         
				if ((del).next != null) {                                                                       
					(del).next.prev = (del).prev;                                                     
				} else {                                                                                 
					(_head).prev = (del).prev;                                                          
				}                                                                                        
			}  
		}

		public void DL_DELETE(T t){
			for (utNode<T> tmp = this.head; tmp != null; tmp = tmp.next) {
				utNode<T> entry = tmp;
				if (entry.obj.Equals(t)) {
					DL_DELETE (entry);
				}
			}
		}

		public void DL_REPLACE_ELEM(utNode<T> el, utNode<T> add){
			NSUtils.Assert (head != null, "utList#DL_REPLACE_ELEM: head should not be null");
			NSUtils.Assert (el != null, "utList#DL_REPLACE_ELEM: el should not be null");
			NSUtils.Assert (add != null, "utList#DL_REPLACE_ELEM: add should not be null");
			if ((_head) == (el)) {                                                                         
				_head = add;                                                                              
					(add).next = (el).next;                                                                    
				if ((el).next == null) {                                                                    
					(add).prev = (add);                                                                        
				} else {                                                                                     
					(add).prev = (el).prev;                                                                   
						(add).next.prev = (add);                                                                  
				}                                                                                            
			} else {                                                                                      
				(add).next = (el).next;                                                                    
					(add).prev = (el).prev;                                                                    
					(add).prev.next = (add);                                                                   
				if ((el).next == null) {                                                                    
					(head).prev = (add);                                                                       
				} else {                                                                                     
					(add).next.prev = (add);                                                                  
				}                                                                                            
			}    
		}

		public void DL_REPLACE_ELEM(T elT, T addT){
			utNode<T> el = null;
			for (utNode<T> tmp = this.head; tmp != null; tmp = tmp.next) {
				utNode<T> entry = tmp;
				if (entry.obj.Equals(elT)) {
					el = entry;
				}
			}
			utNode<T> add = new utNode<T> ();
			add.next = add.prev = null;
			add.obj = addT;
			DL_REPLACE_ELEM (el, add);
		}

		public void DL_CONTACT(utList<T> list2){
			var head1 = _head;
			var head2 = list2.head;
			if (head2 != null) {
				if(head1 != null){
					var tmp = head2.prev;
					head2.prev = head1.prev;
					head1.prev.next = head2;
					head1.prev = tmp;
				}else{
					_head = head2;
				}
			}
		}

		public void DL_CLEAR(){
			_head = null;
		}

		public bool DL_CONTAINS(T t){
			for (utNode<T> tmp = this.head; tmp != null; tmp = tmp.next) {
				utNode<T> entry = tmp;
				if (entry.obj.Equals(t)) {
					return true;
				}
			}
			return false;
		}

        public int DL_COUNT(){
            int count = 0;
            for (utNode<T> ent = this.head; ent != null; ent = ent.next) {
                count ++;
            }
            return count;
        }
	}
}

