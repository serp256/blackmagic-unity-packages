using System.IO;

using UnityEngine;

using System.Text.RegularExpressions;
using System;
using System.Linq;

using System.Collections.Generic;
using UnityEditor.Android;

public class GradlePostProcessor: IPostGenerateGradleAndroidProject
    {



        void ReplaceOrAdd(Dictionary<string, string> dictionary, string key, string value)
        {
            if (dictionary.ContainsKey(key))
            {
                dictionary[key] = value;
            }
            else
            {
                dictionary.Add(key, value);
            }
        }
        public void OnPostGenerateGradleAndroidProject(string path)
        {
            Debug.Log($"PostGradle: {path}");
            var gradleFilepath = Path.Combine(path, "..", "launcher", "build.gradle");
            if (!File.Exists(gradleFilepath))
            {
                Debug.LogError($"File '{gradleFilepath}' is not found");
                
                return;
            }
            var gradleContent = File.ReadAllText(gradleFilepath);
            var matches = Regex.Matches(gradleContent, @"manifestPlaceholders\s*=\s*\[(.*?)\]", RegexOptions.Singleline);
            if (matches.Count == 0)
            {
                Debug.LogError($"manifestPlaceholders definition is not found in '{gradleFilepath}'");
                return;
            }
            
            var match = matches[0];
            var manifestPlaceholders = 
                match.Groups[1].Value
                    .Split(new[] {','}, StringSplitOptions.RemoveEmptyEntries)
                    .Select(x => x.Trim())
                    .Where(x => !string.IsNullOrEmpty(x))
                    .Select(x => x.Split(new[] {':'}, StringSplitOptions.RemoveEmptyEntries))
                    .ToDictionary(x => x[0].Trim(), x => x[1].Trim());
            var settings = VKIDSettings.Instance;
            //VKIDClientID: "52722655",
            //   VKIDClientSecret: "zeQKWssdPcoAThnk0edu",
            //   VKIDRedirectHost: "vk.com", // Обычно vk.com.
            //   VKIDRedirectScheme: "vk52722655", // Строго в формате vk{ID приложения}.
            ReplaceOrAdd(manifestPlaceholders, $"VKIDClientID", $"\"settings.VKIDClientID\"");
            ReplaceOrAdd(manifestPlaceholders, $"VKIDClientSecret", "\"settings.VKIDClientSecret\"");
            ReplaceOrAdd(manifestPlaceholders, $"VKIDRedirectHost", "\"vk.com\"");
            ReplaceOrAdd(manifestPlaceholders, $"VKIDRedirectScheme", $"\"vk{settings.VKIDClientID}\"");
            
            
            var manifestPlaceholdersString = string.Join(",", manifestPlaceholders.Select(kvp => $"{kvp.Key}:{kvp.Value}"));
            var newGradleContent = gradleContent.Replace(match.Value, $"manifestPlaceholders = [{manifestPlaceholdersString}]");
            File.WriteAllText(gradleFilepath, newGradleContent);

        }

        public int callbackOrder => 120;
    }
