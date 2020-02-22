 using System.Collections;
 using UnityEngine;

 public class MainScene : MonoBehaviour
{
    public IEnumerator Start()
    {
        yield return UIManager.Instance.Init();
    }
}