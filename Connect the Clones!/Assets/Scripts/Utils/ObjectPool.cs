using System.Collections.Generic;
using UnityEngine;

public class ObjectPool : MonoBehaviour 
{
	private struct poolEntry 
	{
		public GameObject 	gameObject;
		public float 		lifetime;
	}

	public	GameObject		prefab;
	public 	int 			numberOfCopies;
	public 	float			lifetime = 0; // After this time objects will be returned to pool automatically. 0 means "immortal".
	public 	bool 			stretchBeyondLimit = false; // When out of stuff in pull to return, it will instantiate new objects when trying to get them.

	private poolEntry[]		pool;

	void Awake () 
	{
		pool = new poolEntry[numberOfCopies];
		for(int i = 0; i < numberOfCopies; ++i)
		{
			pool[i].gameObject = Instantiate(prefab, Vector3.zero, Quaternion.identity, transform);
			pool[i].gameObject.SetActive(false);
		}
	}

	void Update () 
	{
		if(lifetime > 0)
		{
			int len = pool.Length;
			for(int i = 0; i < len; ++i)
			{
				if(pool[i].gameObject.activeSelf && pool[i].lifetime > 0)
				{
					pool[i].lifetime -= Time.deltaTime;
					if(pool[i].lifetime <= 0.0f)
					{
						pool[i].gameObject.SetActive(false);
					}
				}
			}
		}
	}

	public bool HasFreeObjects()
	{
		if(stretchBeyondLimit)
		{
			return true;
		}

		int len = pool.Length;

		for(int i = 0; i < len; ++i)
		{
			if(!pool[i].gameObject.activeSelf)
			{
				return true;
			}
		}

		return false;
	}

	public GameObject GetFreeObject()
	{
		int len = pool.Length;

		for(int i = 0; i < len; ++i)
		{
			if(!pool[i].gameObject.activeSelf)
			{
				pool[i].gameObject.SetActive(true);
				pool[i].lifetime = lifetime;
				return pool[i].gameObject;
			}
		}

		if(stretchBeyondLimit)
		{
			// resize pool and add a new child
			List<poolEntry> newList = new List<poolEntry>(pool);
			poolEntry newEntry = new poolEntry();
			newEntry.gameObject = Instantiate(prefab, Vector3.zero, Quaternion.identity, transform);
			newEntry.gameObject.SetActive(false);
			newList.Add(newEntry);
			pool = newList.ToArray();
			return GetFreeObject();
		}

		return null;
	}

	public static void ReturnObject(GameObject obj)
	{
		obj.SetActive(false);
	}
}
