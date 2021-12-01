using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Maple.Utilities
{
    public static class Utilities
    {
        /// <summary>Creates a item inheriting from ScriptableObject in the selected folder or root Assets folder if one isn't selected.</summary>
        /// <typeparam name="T">The type of the object being created in the project directory.</typeparam>
        /// <param name="item">The object being created in the project directory.</param>
        public static void CreateAssetFromItem<T>(T item) where T : ScriptableObject
        {
#if UNITY_EDITOR
            // Figure out selection and location info
            UnityEngine.Object currentSelection = UnityEditor.Selection.activeObject;
            string location = UnityEditor.AssetDatabase.GetAssetPath(currentSelection);

            // Define a better location if current selection location sucked
            if (location == "")
            {
                // Default if no item is selected or something went wrong
                location = "Assets";
            }
            else if (Path.GetExtension(location) != "")
            {
                // Cut the file extension off the end off of the creation location
                location = location.Replace(Path.GetFileName(location), "");
            }

            // Create an on disk asset from the item at location
            UnityEditor.AssetDatabase.CreateAsset(item, UnityEditor.AssetDatabase.GenerateUniqueAssetPath($"{location}/{item.name}.asset"));
            UnityEditor.AssetDatabase.SaveAssets();
#endif
        }

        public static void AddSubAssetToAsset(ScriptableObject asset, ScriptableObject subasset)
        {
#if UNITY_EDITOR
            UnityEditor.AssetDatabase.AddObjectToAsset(subasset, asset);
#endif
        }

        public static void RemoveSubAssetFromAsset(ScriptableObject subasset)
        {
#if UNITY_EDITOR
            UnityEditor.AssetDatabase.RemoveObjectFromAsset(subasset);
#endif
        }

        public static List<T> FindAssetsOfType<T>() where T : ScriptableObject
        {
            List<T> assets = new List<T>();

#if UNITY_EDITOR
            // Find the guids of all assets with matching type
            string[] guids = UnityEditor.AssetDatabase.FindAssets(string.Format("t:{0}", typeof(T)));

            for (int i = 0; i < guids.Length; i++)
            {
                // Get asset location
                string assetPath = UnityEditor.AssetDatabase.GUIDToAssetPath(guids[i]);
                
                // Attemp to load asset to check if it exists
                T asset = UnityEditor.AssetDatabase.LoadAssetAtPath<T>(assetPath);

                // If the asset exists add it to the list
                if (asset != null)
                    assets.Add(asset);
            }
#endif

            return assets;
        }
    }
}