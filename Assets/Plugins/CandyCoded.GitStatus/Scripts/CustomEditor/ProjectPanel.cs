// Copyright (c) Scott Doxey. All Rights Reserved. Licensed under the MIT License. See LICENSE in the project root for license information.

#if UNITY_EDITOR
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace CandyCoded.GitStatus
{

    [InitializeOnLoad]
    public class ProjectPanel : AssetPostprocessor
    {

        private const float ICON_SCALE = 0.25f;

        private const float ICON_PADDING = 2;

        static ProjectPanel()
        {

            EditorApplication.projectWindowItemOnGUI -= ProjectWindowItemOnGui;
            EditorApplication.projectWindowItemOnGUI += ProjectWindowItemOnGui;

        }

        private static void ProjectWindowItemOnGui(string guid, Rect selectionRect)
        {

            // Check if GitStatusPanel is open
            var windows = Resources.FindObjectsOfTypeAll<GitStatusPanel>();
            if (windows == null || windows.Length == 0)
            {

                return;

            }
            
            if (!Event.current.type.Equals(EventType.Repaint)) return;

            if (string.IsNullOrEmpty(guid)) return;

            var path = NormalizePath(AssetDatabase.GUIDToAssetPath(guid));

            var iconSize = Mathf.Max(selectionRect.height * ICON_SCALE, EditorGUIUtility.singleLineHeight);

            var rect = new Rect(
                selectionRect.xMax - iconSize - ICON_PADDING,
                selectionRect.y + ICON_PADDING,
                iconSize - ICON_PADDING,
                iconSize - ICON_PADDING);

            if (GitStatus.changedFiles.Contains(path))
                GUI.DrawTexture(rect, GitIcons.Changed, ScaleMode.ScaleToFit);
            else if (GitStatus.untrackedFiles.Contains(path))
                GUI.DrawTexture(rect, GitIcons.Untracked, ScaleMode.ScaleToFit);
            else if (IsInChangedFolders(path)) GUI.DrawTexture(rect, GitIcons.Changed, ScaleMode.ScaleToFit);
            else if (IsInUntrackedFolders(path)) GUI.DrawTexture(rect, GitIcons.Untracked, ScaleMode.ScaleToFit);
        }

            }

        private static bool IsInChangedFolders(string path)
        {
            return GitStatus.changedFolders.Any(folder => path.Equals(folder) && (path.Length == folder.Length || path[folder.Length] == '/'));
        }
        
        private static bool IsInUntrackedFolders(string path)
        {
            return GitStatus.untrackedFolders.Any(folder => path.Equals(folder) && (path.Length == folder.Length || path[folder.Length] == '/'));
        }

        private static string NormalizePath(string path)
        {
            return path.Replace('\\', '/');
        }
    }

}
#endif
