// Copyright (c) Scott Doxey. All Rights Reserved. Licensed under the MIT License. See LICENSE in the project root for license information.

#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEditor;

namespace CandyCoded.GitStatus
{

    [InitializeOnLoad]
    public static class GitStatus
    {

        public static bool isGitRepo;

        public static string status = "";

        public static string branch = "HEAD";

        public static string[] branches = { };

        public static string[] changedFiles = { };

        public static string[] untrackedFiles = { };

        public static HashSet<string> changedFolders = new();
        
        public static HashSet<string> untrackedFolders = new();

        public static DateTime lastUpdated = DateTime.Now;

        static GitStatus()
        {

            FileWatcher.UpdateEvent -= Update;
            FileWatcher.UpdateEvent += Update;

            Update();

        }

        public static void Update()
        {

            Task.Run(UpdateAsync);

        }

        public static async void UpdateAsync()
        {

            try
            {

                status = await Git.Status();

                isGitRepo = true;

            }
            catch (Exception error)
            {

                status = error.Message;

                isGitRepo = false;

            }

            branch = await Git.Branch();
            branches = await Git.Branches();
            changedFiles = await Git.ChangedFiles();
            untrackedFiles = await Git.UntrackedFiles();

            UpdateChangedFolders();
            UpdateUntrackedFolders();

            lastUpdated = DateTime.Now;
        }

        private static void UpdateChangedFolders()
        {
            changedFolders.Clear();

            if (changedFiles == null) return;
            foreach (var file in changedFiles)
                AddFoldersForChangedFile(file);
        }
        
        private static void UpdateUntrackedFolders()
        {
            untrackedFolders.Clear();

            if (untrackedFiles == null) return;
            foreach (var file in untrackedFiles)
                AddFoldersForUntrackedFile(file);
        }

        private static void AddFoldersForChangedFile(string filePath)
        {
            var directory = Path.GetDirectoryName(filePath);
            while (!string.IsNullOrEmpty(directory) && directory != "Assets")
            {
                changedFolders.Add(NormalizePath(directory));
                directory = Path.GetDirectoryName(directory);
            }
        }
        
        private static void AddFoldersForUntrackedFile(string filePath)
        {
            var directory = Path.GetDirectoryName(filePath);
            while (!string.IsNullOrEmpty(directory) && directory != "Assets")
            {
                untrackedFolders.Add(NormalizePath(directory));
                directory = Path.GetDirectoryName(directory);
            }
        }

        private static string NormalizePath(string path)
        {
            return path.Replace('\\', '/');
        }
    }
}
#endif
