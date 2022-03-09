using UnityEngine;
using UnityEngine.SceneManagement;

namespace Assets.Tracking_Example.Scripts
{
    public class LoadSceneButton : MonoBehaviour
    {
        [SerializeField] private string sceneName;

        public void OnClick()
        {
            ExampleSceneChanger.Instance.LoadScene(this.sceneName);
        }
    }
}