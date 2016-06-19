
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace SimpleLibrary
{
	public class SimplePoolManager : MonoBehaviour
	{
		#region SpecialSingleton
		private static SimplePoolManager instance = null;
		public static SimplePoolManager Instance
		{
			get
			{
				if (instance == null)
					FindInstance();
				return instance;
			}
			set
			{
				instance = value;
			}
		}
		private static void FindInstance()
		{
			instance = FindObjectOfType<SimplePoolManager>();
			if (instance == null)
			{
				Debug.Log("No SimplePoolManager found, generating one");
				GameObject go = new GameObject();
				SimplePoolManager.Instance = go.AddComponent<SimplePoolManager>();
				go.name = "SimplePoolManager";
			}
		}
		public static bool HasInstance
		{
			get
			{
				return instance != null;
			}
		}
		#endregion

		Queue<SimplePool.DestroyInfo> destroyQueue = new Queue<SimplePool.DestroyInfo>();
		Queue<SimplePool.InstantiateInfo> instantiateQueue = new Queue<SimplePool.InstantiateInfo>();

		public bool SplitWorkload = true;
		public int UpdateEveryXFrames = 1;
		private int frameCounter = 0;

		public bool Pools_FoldOut = false;

		/// <summary>
		/// Check SimplePool.ExpectedTimePerInstantiate ExpectedTimePerDestroy
		/// </summary>
		public float MaxUsedDeltaTimePerFrame = 0.02f;


		public bool EnableProfiler = false;
		public int UsedTimesCount = 1000;
		protected Queue<float> LastUsedTimes = new Queue<float>();

		public float StartTime = 0f;
		public float CurrentUsedTime = 0f;
		public float MinUsedTime = 0f;
		public float MaxUsedTime = 0f;
		public float AverageUsedTime = 0f;


		public delegate void MoveEvent(int from, int to);
		[SerializeField]
		public MoveEvent NameIndexChanged;


		private bool HasMoreTime
		{
			get
			{
				return TimeDiff(StartTime) < MaxUsedDeltaTimePerFrame;
			}
		}

		void Update()
		{
			if (!SplitWorkload)
				return;

			StartTime = Time.realtimeSinceStartup;

			frameCounter++;
			if (frameCounter >= UpdateEveryXFrames)
			{
				frameCounter = 0;

				//First we instantiate needed objects
				InstantiaterWork();
				//If we have more time to use, destroy objects
				if (HasMoreTime) DestroyerWork();
			}

			if (EnableProfiler)
			{
				CurrentUsedTime = TimeDiff(StartTime);

				if (LastUsedTimes.Count == UsedTimesCount)
					LastUsedTimes.Dequeue();
				LastUsedTimes.Enqueue(CurrentUsedTime);

				MinUsedTime = LastUsedTimes.Min();
				MaxUsedTime = LastUsedTimes.Max();
				AverageUsedTime = LastUsedTimes.Average();
			}
		}

		private float TimeDiff(float last)
		{
			return Time.realtimeSinceStartup - last;
		}

		private void InstantiaterWork()
		{
			for (int i = 0; i < instantiateQueue.Count; i++)
			{
				if (HasMoreTime == false)
					return;
				if (instantiateQueue.Count > 0)
				{
					SimplePool.InstantiateInfo info = instantiateQueue.Dequeue();
					DoInstantiate(info);
				}
			}
		}

		private void DestroyerWork()
		{
			for (int i = 0; i < destroyQueue.Count; i++)
			{
				if (HasMoreTime == false)
					return;
				if (destroyQueue.Count > 0)
				{
					SimplePool.DestroyInfo info = destroyQueue.Dequeue();
					DoDestroy(info);
				}
			}
		}
		public void AddDestroyInfo(SimplePool.DestroyInfo info)
		{
			if (SplitWorkload)
				destroyQueue.Enqueue(info);
			else
				DoDestroy(info);
		}
		public void AddInstantiateInfo(SimplePool.InstantiateInfo info)
		{
			if (SplitWorkload)
				instantiateQueue.Enqueue(info);
			else
				DoInstantiate(info);
		}

		private void DoDestroy(SimplePool.DestroyInfo info)
		{
			info.pool.DestroyOneObject();
		}
		private void DoInstantiate(SimplePool.InstantiateInfo info)
		{
			info.pool.InstantiateNewObject();
		}

		#region Static
		[SerializeField]
		public List<SimplePool> Pools = new List<SimplePool>();
		[SerializeField]
		public string[] PoolNames = { };

		[ContextMenu("FindAllPools")]
		public void FindAllPools()
		{
			SimplePool[] pools = FindObjectsOfType<SimplePool>();
			Pools.Clear();
			for (int i = 0; i < pools.Length; i++)
			{
				if (!Pools.Contains(pools[i]))
				{
					Pools.Add(pools[i]);
				}
			}
			UpdatePoolNames();
		}

		public static void MovePool(int from, int to)
		{
			if (from < 0 || from >= Instance.Pools.Count)
				return;
			if (to < 0 || to >= Instance.Pools.Count)
				return;

			SimplePool tmp = Instance.Pools[to];
			Instance.Pools[to] = Instance.Pools[from];
			Instance.Pools[from] = tmp;
			IUpdatePoolNames();

			if (Instance.NameIndexChanged != null)
				Instance.NameIndexChanged(from, to);
		}

		public static void IUpdatePoolNames()
		{
			if (Instance == null)
				return;
			Instance.UpdatePoolNames();
		}

		public void UpdatePoolNames()
		{
			PoolNames = new string[Pools.Count];
			for (int i = 0; i < Pools.Count; i++)
			{
				if (Pools[i])
					PoolNames[i] = Pools[i].PoolName;
			}
		}

		public static bool Contains(SimplePool pool)
		{
			return Instance.Pools.Contains(pool);
		}
		public static void Add(SimplePool pool)
		{
			if (!Contains(pool))
				Instance.Pools.Add(pool);
			Instance.UpdatePoolNames();
		}
		public static void Remove(SimplePool pool)
		{
			if (Contains(pool))
				Instance.Pools.Remove(pool);
			Instance.UpdatePoolNames();
		}
		public static void RemoveAt(int index)
		{
			if (index >= 0 && index < Instance.Pools.Count)
				Instance.Pools.RemoveAt(index);
			Instance.UpdatePoolNames();
		}

		/// <summary>
		/// Spawns GameObject from first pool containing given prefab, creates new pool, or instantiates
		/// </summary>
		/// <param name="prefab">GameObject to Spawn</param>
		/// <param name="autoCreatePool">false = Instantiate Object</param>
		/// <returns></returns>
		public static GameObject Spawn(GameObject prefab, Vector3 localPosition, Quaternion localRotation, Vector3 localScale, bool autoCreatePool = true)
		{
			GameObject go = null;
			foreach (var pool in Instance.Pools)
			{
				if (pool.Uses(prefab))
				{
					go = pool.Spawn(localPosition, localRotation, localScale);
					pool.AfterSpawn(go);
					return go;
				}
			}
			if (autoCreatePool)
			{
				SimplePool newPool = CreatePool(prefab);
				go = newPool.Spawn(localPosition, localRotation, localScale);
				newPool.AfterSpawn(go);
			}
			else
			{
				go = GameObject.Instantiate<GameObject>(prefab);
				go.SetActive(true);
				go.SendMessage("OnSpawn", SendMessageOptions.DontRequireReceiver);
			}
			return go;
		}
		public static GameObject Spawn(PoolInfo info, Vector3 localPosition, Quaternion localRotation, Vector3 localScale)
		{
			GameObject go = Instance.Pools[info.SelectedPoolIndex].Spawn(localPosition, localRotation, localScale);
			Instance.Pools[info.SelectedPoolIndex].AfterSpawn(go);
			return go;
		}

		public static bool Despawn(GameObject go, bool allowDestroy = true)
		{
			foreach (var pool in Instance.Pools)
			{
				if (pool.TryDespawnObject(go))
					return true;
			}
			if(allowDestroy)
			{
				Destroy(go);
				return true;
			}
			return false;
		}
		#endregion


		public static SimplePool CreatePool(GameObject prefab)
		{
			GameObject go = new GameObject();
			SimplePool pool = go.AddComponent<SimplePool>();
			pool.ThisPrefab = prefab;
			go.name = "SimplePool";
			pool.PoolName = go.name;

			SimplePoolManager.Add(pool);
			if (go.transform.parent == null)
				go.transform.SetParent(SimplePoolManager.Instance.transform);

			return pool;
		}
	}

}
