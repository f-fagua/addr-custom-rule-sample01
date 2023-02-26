using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.Build.Pipeline;
using UnityEditor.Build.Pipeline.Interfaces;
using UnityEngine;

[CreateAssetMenu(fileName = "LastBuildData", menuName = "Addressables/LastBuildData", order = 1)]
public class LastBuildData : ScriptableObject, ISerializationCallbackReceiver
{
    private const string kLastDataFileName = "LastBuildData.asset";
    public static string DefaultAssetPath => Path.Combine("Assets", "AddressableAssetsData", "LastBuildData", kLastDataFileName );

    [SerializeField]
    private List<string> m_EntryPaths;
    
    [SerializeField]
    private List<string> m_EntryHashes;

    private Dictionary<string, string> m_Entries = new Dictionary<string, string>();
    
    public void SetData(List<string> entryPaths, List<string> entryHashes)
    {
        m_EntryPaths = entryPaths;
        m_EntryHashes = entryHashes;
        RebuildDictionary();
    }
    
    public bool HasChanged(string assetPath, out bool isNewAsset, out string oldHash, out string newHash)
    {
        oldHash = "";
        newHash = "";
        
        isNewAsset = !m_Entries.TryGetValue(assetPath, out oldHash);

        if (isNewAsset)
            return true;

        newHash = AssetDatabase.GetAssetDependencyHash(assetPath).ToString();
        return newHash != oldHash;
    }

    public static void CreateLastBuildDataAsset()
    {
        LastBuildData defaultAsset = ScriptableObject.CreateInstance<LastBuildData>();
        AssetDatabase.CreateAsset(defaultAsset, DefaultAssetPath);
    }

    public void OnBeforeSerialize()
    {
        m_EntryPaths.Clear();
        m_EntryHashes.Clear();
        
        foreach (var kvp in m_Entries)
        {
            m_EntryPaths.Add(kvp.Key);
            m_EntryHashes.Add(kvp.Value);
        }
    }

    public void OnAfterDeserialize()
    {
        RebuildDictionary();
    }

    private void RebuildDictionary()
    {
        m_Entries = new Dictionary<string, string>();

        for (int i = 0; i < m_EntryPaths.Count; i++)
        {
            m_Entries.Add(m_EntryPaths[i], m_EntryHashes[i]);
        }
    }
}

[InitializeOnLoad]
class RegisterBuildCallbacks    {
    static RegisterBuildCallbacks()
    {
        ContentPipeline.BuildCallbacks.PostScriptsCallbacks -= OnBuildCompleted;
        ContentPipeline.BuildCallbacks.PostScriptsCallbacks += OnBuildCompleted;
    }

    private static ReturnCode OnBuildCompleted(IBuildParameters buildParameters, IBuildResults buildResults)
    {
        Debug.Log("OnBuildCompleted");

        if (!File.Exists(LastBuildData.DefaultAssetPath))
            LastBuildData.CreateLastBuildDataAsset();

        var lastBuildData = AssetDatabase.LoadAssetAtPath<LastBuildData>(LastBuildData.DefaultAssetPath);

        var entryPaths = new List<string>();
        var entryHashes = new List<string>();

        foreach (var group in AddressableAssetSettingsDefaultObject.Settings.groups)
        {
            foreach (var entry in group.entries)
            {
                entryPaths.Add(entry.AssetPath);
                entryHashes.Add(AssetDatabase.GetAssetDependencyHash(entry.AssetPath).ToString());
            }
        }

        lastBuildData.SetData(entryPaths, entryHashes);
        
        AssetDatabase.SaveAssets();
        
        AssetDatabase.ForceReserializeAssets(new[] {LastBuildData.DefaultAssetPath});
        
        return ReturnCode.Success;
    }
}    
