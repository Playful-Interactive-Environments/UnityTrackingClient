using Assets.UnityPharusAPI.Helper;
using Assets.UnityPharusAPI.Managers;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Assets.Tracking_Example.Scripts
{
    public class ExampleSceneChanger : AManager<ExampleSceneChanger>
    {
        private string currentLevel;
        public string CurrentLevel => this.currentLevel;

        private string previousLevel;

        public static event Action<float> LoadingProgressUpdate;

        public static event Action LoadingStart;

        public static event Action<string> LoadingComplete;

        public void LoadScene(string level)
        {
            ATrackingManager.Instance.ShutDown();
            this.previousLevel = this.currentLevel;
            this.currentLevel = level;
            StartCoroutine(this.LoadSceneAsync());
        }

        public void ReloadCurrent()
        {
            ATrackingManager.Instance.ShutDown();
            SceneManager.LoadScene(this.currentLevel);
        }

        private IEnumerator LoadSceneAsync()
        {
            LoadingStart?.Invoke();

            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(this.currentLevel);
            while (!asyncLoad.isDone)
            {
                float progress = Mathf.Clamp01(asyncLoad.progress / 0.9f);
                LoadingProgressUpdate?.Invoke(progress);
                yield return new WaitForEndOfFrame();
            }

            LoadingComplete?.Invoke(this.currentLevel);
        }
    }
}