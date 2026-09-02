using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Game.EditorTools
{
    /// <summary>
    /// Re-registers and re-selects a phone-portrait Game View size on every
    /// Editor load. The Standalone build target's Game View size list has no
    /// phone preset by default and was observed reverting to "4K UHD" across
    /// Editor restarts, so this uses Unity's only entry point for managing
    /// that list — the internal GameViewSizes API — via reflection.
    /// </summary>
    [InitializeOnLoad]
    internal static class PhoneGameViewSize
    {
        private const string SizeLabel = "Phone Portrait";
        private const int Width = 1080;
        private const int Height = 1920;

        static PhoneGameViewSize()
        {
            EditorApplication.delayCall += Apply;
        }

        private static void Apply()
        {
            try
            {
                var group = CurrentGroup();
                var index = FindSizeIndex(group, SizeLabel);
                if (index < 0) index = AddSize(group);
                SelectOnGameView(index);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"PhoneGameViewSize: couldn't set the Game View resolution ({e.Message}).");
            }
        }

        private static object CurrentGroup()
        {
            var sizesType = EditorAssembly().GetType("UnityEditor.GameViewSizes");
            var instance = SingletonInstance(sizesType);
            var groupType = sizesType.GetProperty("currentGroupType").GetValue(instance, null);
            return sizesType.GetMethod("GetGroup").Invoke(instance, new[] { groupType });
        }

        private static object SingletonInstance(Type sizesType)
        {
            var singletonType = typeof(ScriptableSingleton<>).MakeGenericType(sizesType);
            return singletonType.GetProperty("instance").GetValue(null, null);
        }

        private static int FindSizeIndex(object group, string label)
        {
            var texts = (string[])group.GetType().GetMethod("GetDisplayTexts").Invoke(group, null);
            for (var i = 0; i < texts.Length; i++)
                if (texts[i].StartsWith(label, StringComparison.Ordinal))
                    return i;
            return -1;
        }

        private static int AddSize(object group)
        {
            var groupType = group.GetType();
            groupType.GetMethod("AddCustomSize").Invoke(group, new[] { NewFixedResolutionSize() });
            return (int)groupType.GetMethod("GetTotalCount").Invoke(group, null) - 1;
        }

        private static object NewFixedResolutionSize()
        {
            var sizeType = EditorAssembly().GetType("UnityEditor.GameViewSize");
            var typeEnum = EditorAssembly().GetType("UnityEditor.GameViewSizeType");
            var fixedResolution = Enum.Parse(typeEnum, "FixedResolution");
            var ctor = sizeType.GetConstructor(new[] { typeEnum, typeof(int), typeof(int), typeof(string) });
            return ctor.Invoke(new object[] { fixedResolution, Width, Height, SizeLabel });
        }

        private static void SelectOnGameView(int index)
        {
            var gameViewType = EditorAssembly().GetType("UnityEditor.GameView");
            var window = EditorWindow.GetWindow(gameViewType, false, null, false);
            SelectedSizeIndexProperty(gameViewType).SetValue(window, index, null);
            window.Repaint();
        }

        private static PropertyInfo SelectedSizeIndexProperty(Type gameViewType)
        {
            const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            return gameViewType.GetProperty("selectedSizeIndex", flags);
        }

        private static Assembly EditorAssembly() => typeof(Editor).Assembly;
    }
}
