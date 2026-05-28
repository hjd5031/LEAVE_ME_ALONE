using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class MultiplayerSceneCleaner
{
    private const string LogTag = "[MultiplayerSceneCleaner]";

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Bootstrap()
    {
        SceneManager.sceneLoaded -= HandleSceneLoaded;
        SceneManager.sceneLoaded += HandleSceneLoaded;
        CleanIfMultiplayerScene(SceneManager.GetActiveScene());
    }

    private static void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        CleanIfMultiplayerScene(scene);
    }

    private static void CleanIfMultiplayerScene(Scene scene)
    {
        if (!IsMultiplayerScene(scene.name))
        {
            return;
        }

        int removed = 0;
        removed += DestroyObjectsWithComponent<GameManager>();
        removed += DestroyObjectsWithComponent<EnemyCtrl>();
        removed += DestroyObjectsWithComponent<EnemyTomatoCtrl>();
        removed += DestroyObjectsWithComponent<EnemyBoostDrone>();
        removed += DestroyObjectsWithComponent<EnemyToxicDrone>();

        if (removed > 0)
        {
            Debug.Log($"{LogTag} Removed {removed} single-player/enemy objects from multiplayer scene '{scene.name}'.");
        }
    }

    private static int DestroyObjectsWithComponent<T>() where T : Component
    {
        T[] components = UnityEngine.Object.FindObjectsByType<T>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        int count = 0;
        foreach (T component in components)
        {
            if (component == null || component.gameObject == null)
            {
                continue;
            }

            UnityEngine.Object.Destroy(component.gameObject);
            count++;
        }

        return count;
    }

    private static bool IsMultiplayerScene(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName))
        {
            return false;
        }

        return sceneName.IndexOf("Multiplayer", StringComparison.OrdinalIgnoreCase) >= 0 ||
               sceneName.IndexOf("Multiplay", StringComparison.OrdinalIgnoreCase) >= 0 ||
               sceneName.IndexOf("DedicatedServer", StringComparison.OrdinalIgnoreCase) >= 0 ||
               sceneName.IndexOf("Network", StringComparison.OrdinalIgnoreCase) >= 0;
    }
}
