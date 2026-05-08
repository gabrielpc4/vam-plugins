using System.Collections.Generic;
using UnityEngine;

public class CollisionTriggerEventHandler : MonoBehaviour
{
	public CollisionTrigger collisionTrigger;

	public Dictionary<Collider, bool> collidingWithDictionary;

	public List<Collider> collidingWith;

	public string atomFilterUID;

	public bool invertAtomFilter;

	public bool debug;

	protected List<Collider> removeList;

	protected bool PassFilter(Collider c)
	{
		if (atomFilterUID != null && atomFilterUID != "None")
		{
			Atom componentInParent = c.GetComponentInParent<Atom>();
			if (invertAtomFilter)
			{
				if (componentInParent == null || componentInParent.uid != atomFilterUID)
				{
					return true;
				}
			}
			else if (componentInParent != null && componentInParent.uid == atomFilterUID)
			{
				return true;
			}
			return false;
		}
		return true;
	}

	protected void AddCollidingWith(Collider c)
	{
		if (!collidingWithDictionary.ContainsKey(c))
		{
			collidingWithDictionary.Add(c, value: true);
		}
		collisionTrigger.trigger.active = true;
		collisionTrigger.trigger.transitionInterpValue = 1f;
	}

	protected void RemoveCollidingWith(Collider c)
	{
		collidingWithDictionary.Remove(c);
		if (collidingWithDictionary.Count == 0)
		{
			collisionTrigger.trigger.transitionInterpValue = 0f;
			collisionTrigger.trigger.active = false;
		}
	}

	protected void RemoveAllCollidingWith()
	{
		if (collidingWithDictionary != null)
		{
			collidingWithDictionary.Clear();
		}
		collisionTrigger.trigger.transitionInterpValue = 0f;
		collisionTrigger.trigger.active = false;
	}

	private void OnCollisionEnter(Collision collision)
	{
		if (PassFilter(collision.collider) && (SuperController.singleton == null || !SuperController.singleton.isLoading))
		{
			AddCollidingWith(collision.collider);
		}
	}

	private void OnCollisionStay(Collision collision)
	{
		if (PassFilter(collision.collider) && (SuperController.singleton == null || !SuperController.singleton.isLoading))
		{
			AddCollidingWith(collision.collider);
		}
	}

	private void OnCollisionExit(Collision collision)
	{
		if (SuperController.singleton == null || !SuperController.singleton.isLoading)
		{
			RemoveCollidingWith(collision.collider);
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		if (PassFilter(other) && (SuperController.singleton == null || !SuperController.singleton.isLoading))
		{
			AddCollidingWith(other);
		}
	}

	private void OnTriggerStay(Collider other)
	{
		if (PassFilter(other) && (SuperController.singleton == null || !SuperController.singleton.isLoading))
		{
			AddCollidingWith(other);
		}
	}

	private void OnTriggerExit(Collider other)
	{
		if (SuperController.singleton == null || !SuperController.singleton.isLoading)
		{
			RemoveCollidingWith(other);
		}
	}

	public void Reset()
	{
		collisionTrigger.trigger.transitionInterpValue = 0f;
		collisionTrigger.trigger.active = false;
		collidingWithDictionary = new Dictionary<Collider, bool>();
	}

	private void OnDisable()
	{
		RemoveAllCollidingWith();
	}

	private void FixedUpdate()
	{
		if (removeList == null)
		{
			removeList = new List<Collider>();
		}
		else
		{
			removeList.Clear();
		}
		if (collidingWithDictionary.Count > 0)
		{
			foreach (Collider key in collidingWithDictionary.Keys)
			{
				if (key == null)
				{
					removeList.Add(key);
				}
				else if (!key.gameObject.activeInHierarchy)
				{
					removeList.Add(key);
				}
			}
		}
		foreach (Collider remove in removeList)
		{
			RemoveCollidingWith(remove);
		}
		if (debug)
		{
			collidingWith = new List<Collider>(collidingWithDictionary.Keys);
		}
	}

	protected void OnDestroy()
	{
		RemoveAllCollidingWith();
	}
}
