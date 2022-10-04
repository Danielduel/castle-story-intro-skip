using System;
using System.Collections;
using BepInEx;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace IntroSkip
{
  [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
  public class Plugin : BaseUnityPlugin
  {
    private static string SplashScreenSceneName = "SceneSplashScreen";
    private static float SplashScreenScanInterval = 0.2F;
  
    // Game starts with loading state, let's give it a bit of time to load before scanning
    private static float SplashScreenFirstScanDelay = 2F; 
    private static Boolean FirstScan = true;

    private static Plugin _instance;
    public static Plugin Instance
    {
      get
      {
        if (_instance == null)
        {
          throw new InvalidOperationException();
        }
        return _instance;
      }
    }

    private void Awake()
    {
      _instance = this;
      Plugin.Instance.StartCoroutine(TakeMeToTheMenu());
    }


    private IEnumerator TakeMeToTheMenu()
    {
      while (true)
      {
        float ScanDelay = FirstScan ? SplashScreenFirstScanDelay : SplashScreenScanInterval;
        yield return new WaitForSecondsRealtime(ScanDelay);
        FirstScan = false;
        try
        {
          int CountLoaded = SceneManager.sceneCount;
          Debug.Log($"Populating {CountLoaded} scenes");
          Scene[] LoadedScenes = new Scene[CountLoaded];
          for (int i = 0; i < CountLoaded; i++)
          {
            LoadedScenes[i] = SceneManager.GetSceneAt(i);
          }
          Debug.Log("Scenes populated");

          foreach (var Scene in LoadedScenes)
          {
            var SceneName = Scene.name;
            Debug.Log($"Looking for {SplashScreenSceneName} (got {SceneName})");
            if (SceneName.Equals(SplashScreenSceneName))
            {
              var rgos = Scene.GetRootGameObjects();
              if (rgos.Length == 0) {
                Debug.LogError($"{SceneName} root game objects are 0 length");
                break;
              }
              var canvas = Array.Find(rgos, element => element.name.Equals("Canvas"));
              if (!canvas) {
                Debug.LogError($"{SceneName}/Canvas desn't exist");
                break;
              }
              var canSkip = canvas.transform.GetChild(1).gameObject.activeSelf;
              if (!canSkip) {
                Debug.Log($"{SceneName} - can't skip yet (it is expected if game is still loading)");
                break;
              }
              var lastStageEventTrigger = canvas.transform.GetChild(3).GetComponent(typeof(KeyEventTrigger)) as KeyEventTrigger;
              if (!lastStageEventTrigger) {
                Debug.LogError($"{SceneName}/3rd child:KeyEventTrigger - doesn't exist");
                break;
              }

              if (canSkip) // it is technically checked for before, leaving this "if" just for readability reasons
              {
                Debug.Log("Cleaning up Coroutine");
                Plugin.Instance.StopAllCoroutines();
                Debug.Log("Skipping to menu, thank you for using IntroSkip! GLHF");
                lastStageEventTrigger.m_OnEvent.Invoke();
              }
            }
          }
        }
        catch (Exception ex)
        {
          Debug.LogException(ex);
          break;
        }
      }
    }
  }
}
