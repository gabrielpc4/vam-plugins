using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using SimpleJSON;

namespace MVRPlugin {

	public class MyCollection<T> : IList<T> {
		private readonly IList<T> _list = new List<T>();

		#region Implementation of IEnumerable

		public IEnumerator<T> GetEnumerator() {
			return _list.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator() {
			return GetEnumerator();
		}

		#endregion

		#region Implementation of ICollection<T>

		public void Add(T item) {
			Debug.Log("Add " + item.ToString());
			_list.Add(item);
		}

		public void Clear() {
			_list.Clear();
		}

		public bool Contains(T item) {
			return _list.Contains(item);
		}

		public void CopyTo(T[] array, int arrayIndex) {
			_list.CopyTo(array, arrayIndex);
		}

		public bool Remove(T item) {
			return _list.Remove(item);
		}

		public int Count {
			get { return _list.Count; }
		}

		public bool IsReadOnly {
			get { return _list.IsReadOnly; }
		}

		#endregion

		#region Implementation of IList<T>

		public int IndexOf(T item) {
			return _list.IndexOf(item);
		}

		public void Insert(int index, T item) {
			_list.Insert(index, item);
		}

		public void RemoveAt(int index) {
			_list.RemoveAt(index);
		}

		public T this[int index] {
			get { return _list[index]; }
			set { _list[index] = value; }
		}

		#endregion

		#region Your Added Stuff

		// Add new features to your collection.

		#endregion
	}

	public class Test1 : MVRScript {

		// IMPORTANT - DO NOT make custom enums. The dynamic C# complier crashes Unity when it encounters these for
		// some reason

		// IMPORTANT - DO NOT OVERRIDE Awake() as it is used internally by MVRScript - instead use Init() function which
		// is called right after creation
		public override void Init() {
			try {
				// put init code in here
				SuperController.LogMessage("Template Loaded");

				MyCollection<float> mfs = new MyCollection<float>();
				mfs.Add(0.1f);

				// create custom JSON storable params here if you want them to be stored with scene JSON
				// types are JSONStorableFloat, JSONStorableBool, JSONStorableString, JSONStorableStringChooser
				// JSONStorableColor

			}
			catch (Exception e) {
				SuperController.LogError("Exception caught: " + e);
			}
		}

		// Start is called once before Update or FixedUpdate is called and after Init()
		void Start() {
			try {
				// put code in here
			}
			catch (Exception e) {
				SuperController.LogError("Exception caught: " + e);
			}
		}

		// Update is called with each rendered frame by Unity
		void Update() {
			try {
				// put code in here
			}
			catch (Exception e) {
				SuperController.LogError("Exception caught: " + e);
			}
		}

		// FixedUpdate is called with each physics simulation frame by Unity
		void FixedUpdate() {
			try {
				// put code in here
			}
			catch (Exception e) {
				SuperController.LogError("Exception caught: " + e);
			}
		}

		// OnDestroy is where you should put any cleanup
		// if you registered objects to supercontroller or atom, you should unregister them here
		void OnDestroy() {
		}

	}
}