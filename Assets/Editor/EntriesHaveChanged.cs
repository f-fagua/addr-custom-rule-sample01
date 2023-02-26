#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.AddressableAssets.Build;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
using UnityEditor.Build.Pipeline;
using UnityEngine;

public class EntriesHaveChanged : UnityEditor.AddressableAssets.Build.AnalyzeRules.AnalyzeRule
{
    private LastBuildData m_LastBuildData;
    
    public override bool CanFix
    {
        get => false;
        set { }
    }

    public override string ruleName => "Check which assets have changed";

    public override List<AnalyzeResult> RefreshAnalysis(AddressableAssetSettings settings)
    {
        m_LastBuildData = AssetDatabase.LoadAssetAtPath<LastBuildData>(LastBuildData.DefaultAssetPath);
        
        List<AnalyzeResult> results = new List<AnalyzeResult>();
        foreach (var group in settings.groups)
        {
            if (!group.HasSchema<BundledAssetGroupSchema>())
                continue;
            
            foreach (var e in group.entries)
            {
                var hasChanged = m_LastBuildData.HasChanged(e.AssetPath, out var isNewAsset, out var oldHash, out var newHash);
                
                if (isNewAsset)
                    results.Add(new AnalyzeResult{resultName = "New Assets" + kDelimiter + group.Name + kDelimiter + e.address, severity = MessageType.Warning});
                else if(hasChanged)
                    results.Add(new AnalyzeResult{resultName = "Changed Assets" + kDelimiter + group.Name + kDelimiter + e.address + $" from {oldHash} to {newHash}", severity = MessageType.Error});
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