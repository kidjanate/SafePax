using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.IO;
using kidjanate;
using System;
using System.Text;

namespace kidjanate
{
    public class SafePaxBuilder
    {
        [MenuItem("SafePax/Build Bundles/Windows")]
        public static void buildBundlesWindows()
        {
            string targetDir = "Assets/AssetBundles/Built";
            if (!Directory.Exists(targetDir))
            {
                Directory.CreateDirectory(targetDir);
            }
            EditorUtility.DisplayProgressBar("SafePax","Building bundles.",0f);
            BuildPipeline.BuildAssetBundles(targetDir,
                                            BuildAssetBundleOptions.None,
                                            BuildTarget.StandaloneWindows);
            EditorUtility.ClearProgressBar();

            AssetManifests manifests = new AssetManifests();
            foreach (string e in Directory.GetFiles(targetDir))
            {
                if (Path.GetExtension(Path.Combine(targetDir, Path.GetFileName(e))) == "")
                {
                    string key = SafePax.RandKey();

                    string fileName = Path.GetFileName(e);
                    string filePath = Path.Combine(targetDir, fileName);
                    EditorUtility.DisplayProgressBar("SafePax", "Encrypting : " + fileName, 0f);

                    byte[] normalByte = File.ReadAllBytes(filePath);
                    
                    byte[] encrypted = SafePax.Encrypt(normalByte, SafePax.IV, Encoding.UTF8.GetBytes(key));
                    manifests.manifests.Add(new manifest
                    {
                        name = fileName + ".pax",
                        hash = SafePax.GetMD5(encrypted),
                        key = key
                    });


                    File.WriteAllBytes(Path.Combine(targetDir, Path.GetFileNameWithoutExtension(e)) + ".pax", encrypted);
                    Debug.Log(Path.GetFileName(e) + ".pax");
                    File.Delete(Path.Combine(targetDir, Path.GetFileName(e)));

                    File.WriteAllText(Path.Combine(targetDir, "manifest.json"), JsonUtility.ToJson(manifests, true));

                    EditorUtility.ClearProgressBar();
                }
                
            }
            
            Debug.Log("SafePax : Build Success!");
        }

        
    }

}