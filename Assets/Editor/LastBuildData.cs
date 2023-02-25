using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "LastBuildData", menuName = "Addressables/LastBuildData", order = 1)]
public class LastBuildData : ScriptableObject
{
    
    
    private Dictionary<string, string> m_Entries = new Dictionary<string, string>();

    public bool HasChanged(string assetPath, out bool isNewAsset, out string oldHash, out string newHash)
    {
        oldHash = "";
        newHash = "";
        
        isNewAsset = !m_Entries.TryGetValue(assetPath, out oldHash);

        if (isNewAsset)
            return true;

        var assetGUID = AssetDatabase.AssetPathToGUID(assetPath);
        newHash = AssetDatabase.GetAssetDependencyHash(assetGUID).ToString();
        return newHash != oldHash;
    }
}
