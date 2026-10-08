#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// Editor tool: menambah menu "SmartCampus" di bar atas Unity.
/// WAJIB ditaruh di folder:  Assets/Editor/
/// (butuh SmartCampusOnboarding.cs ada di folder Assets)
/// </summary>
public static class SmartCampusOnboardingMenu
{
    const string ScenePath = "Assets/Scenes/WelcomeScreen.unity";

    [MenuItem("SmartCampus/1. Buat Scene Welcome Baru")]
    static void CreateNewScene()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

        var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
        AddOnboardingObject();

        System.IO.Directory.CreateDirectory("Assets/Scenes");
        EditorSceneManager.SaveScene(scene, ScenePath);

        // daftarkan ke Build Settings (jika belum ada)
        var list = new System.Collections.Generic.List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
        bool exists = false;
        foreach (var s in list) if (s.path == ScenePath) exists = true;
        if (!exists)
        {
            list.Add(new EditorBuildSettingsScene(ScenePath, true));
            EditorBuildSettings.scenes = list.ToArray();
        }

        AssetDatabase.Refresh();
        Debug.Log("[SmartCampus] Scene dibuat: " + ScenePath + " - tekan Play untuk melihat hasilnya.");
    }

    [MenuItem("SmartCampus/2. Tambah Welcome Screen ke Scene Ini")]
    static void AddToCurrentScene()
    {
        AddOnboardingObject();
    }

    static void AddOnboardingObject()
    {
#if UNITY_2023_1_OR_NEWER
        var existing = Object.FindFirstObjectByType<SmartCampusOnboarding>();
#else
        var existing = Object.FindObjectOfType<SmartCampusOnboarding>();
#endif
        if (existing != null)
        {
            Selection.activeGameObject = existing.gameObject;
            Debug.LogWarning("[SmartCampus] Welcome Screen sudah ada di scene ini.");
            return;
        }

        var go = new GameObject("SmartCampusOnboarding");
        Undo.RegisterCreatedObjectUndo(go, "Tambah SmartCampus Onboarding");
        go.AddComponent<SmartCampusOnboarding>();
        Selection.activeGameObject = go;
        EditorSceneManager.MarkSceneDirty(go.scene);
    }
}
#endif
