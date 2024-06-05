// Copyright (c) Scott Doxey. All Rights Reserved. Licensed under the MIT License. See LICENSE in the project root for license information.

#if UNITY_EDITOR
using System;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace CandyCoded.GitStatus
{

    public class GitStatusPanel : EditorWindow
    {

        [MenuItem("Git/Git Status", false, GitMenuItems.PRIORITY - 100)]
        public static void ShowWindow()
        {

            GetWindow(typeof(GitStatusPanel), false, "Git Status", true);

        }

        private void OnGUI()
        {

            if (!GitStatus.isGitRepo)
            {

                if (GUILayout.Button("Initialize git repo"))
                {

                    Task.Run(async () =>
                    {

                        await Git.Init();

                        await GitIgnore.Create(Environment.CurrentDirectory);

                        GitStatus.Update();

                    });

                }

                return;

            }

            GUILayout.Space(5);

            var selectedBranch = Array.IndexOf(GitStatus.branches, GitStatus.branch);

            if (selectedBranch == -1)
            {

                GUILayout.Label($"Branch: {GitStatus.branch}");

            }
            else
            {

                selectedBranch = EditorGUILayout.Popup("Branch:", selectedBranch, GitStatus.branches);

                if (!GitStatus.branches[selectedBranch].Equals(GitStatus.branch))
                {

                    if (GitStatus.changedFiles?.Length > 0)
                    {

                        EditorUtility.DisplayDialog(
                            "Unable to checkout branch",
                            $"Unable to checkout {GitStatus.branches[selectedBranch]} as with {GitStatus.changedFiles?.Length} changes. " +
                            "Commit, discard or stash before checking out a different branch.",
                            "Ok");

                    }
                    else
                    {

                        Task.Run(async () =>
                        {

                            await Git.CheckoutBranch(GitStatus.branches[selectedBranch]);

                        });

                        EditorApplication.ExecuteMenuItem("Assets/Refresh");

                    }

                }

            }

            // Create a custom style that looks like a hyperlink
            var hyperlinkStyle = new GUIStyle(GUI.skin.label)
            {
                hover = { textColor = Color.cyan }
            };
            
            var headingStyle = new GUIStyle(GUI.skin.label)
            {
                normal = { textColor = Color.yellow },
            };

            // Display changed files
            if (GitStatus.changedFiles is { Length: > 0 })
            {
                GUILayout.Label($"Changed Files: {GitStatus.changedFiles?.Length} ", headingStyle);
                if (GitStatus.changedFiles != null)
                    foreach (var file in GitStatus.changedFiles)
                        if (GUILayout.Button(new GUIContent(file), hyperlinkStyle))
                        {
                            if (file.EndsWith(".meta"))
                            {
                                // Get the folder path
                                string folderPath = System.IO.Path.GetDirectoryName(file);
                                // Load the folder
                                UnityEngine.Object folder = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(folderPath);
                                EditorGUIUtility.PingObject(folder);
                            }
                            else
                            {
                                // Load the asset
                                UnityEngine.Object asset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(file);
                                EditorGUIUtility.PingObject(asset);
                            }
                        }

            }

            // Display untracked files
            if (GitStatus.untrackedFiles is { Length: > 0 })
            {
                GUILayout.Label($"Untracked Files: {GitStatus.untrackedFiles?.Length} ", headingStyle);
                if (GitStatus.untrackedFiles != null)
                    foreach (var file in GitStatus.untrackedFiles)
                        if (GUILayout.Button(new GUIContent(file), hyperlinkStyle))
                        {
                            if (file.EndsWith(".meta"))
                            {
                                // Get the folder path
                                string folderPath = System.IO.Path.GetDirectoryName(file);
                                // Load the folder
                                UnityEngine.Object folder = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(folderPath);
                                EditorGUIUtility.PingObject(folder);
                            }
                            else
                            {
                                // Load the asset
                                UnityEngine.Object asset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(file);
                                EditorGUIUtility.PingObject(asset);
                            }
                        }

            }

            if (GUILayout.Button("Refresh")) GitStatus.Update();

            GUILayout.Label($"Last Updated: {GitStatus.lastUpdated}", headingStyle);
        }

        private void Update()
        {

            if (EditorApplication.isPlaying || EditorApplication.isPaused)
            {

                return;

            }

            Repaint();

        }

    }

}

#endif
