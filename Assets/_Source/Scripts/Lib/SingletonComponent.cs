/*  ----------------------------------------------------------------------------
 *  Author:     Burak Göncü
 *  E-Mail:     bmgoncu@gmail.com
 *  ----------------------------------------------------------------------------
 *  Description:
 */

using UnityEngine;

public abstract class SingletonComponent<T> : MonoBehaviour where T : MonoBehaviour
{
    static T _instance = null;

	public static T Instance
	{
		get
		{
			if (_instance == null)
			{
				_instance = (T)FindObjectOfType(typeof(T));
				if (_instance == null)
				{
					string goName = "_" + typeof(T).Name;

					GameObject go = GameObject.Find(goName);
					if (go == null)
					{
                        go = new GameObject
                        {
                            name = goName
                        };
                    }

					_instance = go.AddComponent<T>();
				}
			}
			return _instance;
		}
	}

	protected virtual void Awake()
	{
		if (Instance == null)
		{
			Debug.LogError("Awake Error -->", this);
		}
	}

	protected virtual void OnDestroy()
	{
		_instance = null;
	}

	public static bool IsManagerActive()
	{
		return _instance != null;
	}
}
