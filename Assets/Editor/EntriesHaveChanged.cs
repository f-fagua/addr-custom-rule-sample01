#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.AddressableAssets.Build;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
using UnityEngine;

public class EntriesHaveChanged : UnityEditor.AddressableAssets.Build.AnalyzeRules.AnalyzeRule
{
    private const string kLastDataFileName = "LastBuildData.asset";
    
    private LastBuildData m_LastBuildData;
    
    public override bool CanFix
    {
        get { return false;}
        set { }
    }

    public override string ruleName
    {
        get { return "Check which assets have changed"; }
    }

    private string LastBuildDataPath => Path.Combine("Assets", "AddressableAssetsData", "LastBuildData", kLastDataFileName );

    public override List<AnalyzeResult> RefreshAnalysis(AddressableAssetSettings settings)
    {
        m_LastBuildData = AssetDatabase.LoadAssetAtPath<LastBuildData>(LastBuildDataPath);
        
        List<AnalyzeResult> results = new List<AnalyzeResult>();
        foreach (var group in settings.groups)
        {
            if (!group.HasSchema<BundledAssetGroupSchema>())
                continue;
            
            foreach (var e in group.entries)
            {
                var hasChanged = m_LastBuildData.HasChanged(e.AssetPath, out var isNewAsset, out var oldHash, out var newHash);
                
                if (isNewAsset)
                    results.Add(new AnalyzeResult{resultName = "[New Asset] " + group.Name + kDelimiter + e.address, severity = MessageType.Info});
                else if(hasChanged)
                    results.Add(new AnalyzeResult{resultName = group.Name + kDelimiter + e.address + $" old hash {oldHash}, new hash {newHash}", severity = MessageType.Error});
            }
        }
        
        if (results.Count == 0)
            results.Add(new AnalyzeResult{resultName = ruleName + " - No issues found."});

        return results;
    }
}

[InitializeOnLoad]
class RegisterEntriesHaveChanged
{
    static RegisterEntriesHaveChanged()
    {
        AnalyzeSystem.RegisterNewRule<EntriesHaveChanged>();
    }
}
#endif