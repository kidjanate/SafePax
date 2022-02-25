using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Security.Cryptography;
using UnityEngine.Networking;
using System.Text;
using System.IO;
using System;
using kidjanate;

namespace kidjanate
{
    public class SafePax
    {
        public static string KEY = "4MlhsaPvlqHNnhe5jmKlrw=="; //Default
        public static string API_URL = "http://localhost:14302/"; //Default
        public static byte[] IV = new byte[16];

        public static IEnumerator getPax(string fileName, Action<byte[]> onComplete, Action<float> onProgress, Action<string> onError)
        {
            UnityWebRequest www = UnityWebRequest.Get(API_URL + "assets/"+fileName);
            var req = www.SendWebRequest();
            while (!req.isDone)
            {
                onProgress.Invoke(req.progress);
                yield return null;
            }

            if (www.isNetworkError)
            {
                onError.Invoke(www.error);
            }
            else
            {
                onComplete.Invoke(www.downloadHandler.data);
            }
        }

        public static IEnumerator getManifest(Action<AssetManifests> onComplete, Action<float> onProgress, Action<string> onError)
        {
            UnityWebRequest www = UnityWebRequest.Get(API_URL + "manifest");
            var req = www.SendWebRequest();
            while (!req.isDone)
            {
                onProgress.Invoke(req.progress);
                yield return null;
            }

            if (www.isNetworkError)
            {
                onError.Invoke(www.error);
            }
            else
            {
                string json = www.downloadHandler.text;
                AssetManifests result = JsonUtility.FromJson<AssetManifests>(json);
                onComplete.Invoke(result);
            }
        }
        
        public static void LoadAssetFromByteAsync(byte[] e, Action<AssetBundle> finished, string key = null)
        {
            if (key == null)
                key = KEY;
            byte[] decrypted = Decrypt(e, new byte[16]);
            AssetBundleCreateRequest req = AssetBundle.LoadFromMemoryAsync(decrypted);
            req.completed += (AsyncOperation obj) => {
                finished.Invoke(req.assetBundle);
            };
        }

        public static AssetBundle LoadAssetFromByte(byte[] e, string key = null)
        {
            byte[] decrypted = Decrypt(e, new byte[16]);
            return AssetBundle.LoadFromMemory(decrypted);
        }

        public static string RandKey()
        {
            using (var aes = Aes.Create())
            {
                aes.KeySize = 128;
                aes.BlockSize = 128;
                aes.Padding = PaddingMode.Zeros;
                aes.IV = IV;
                aes.GenerateKey();
                
                return Convert.ToBase64String(aes.Key);
            }
        }

        public static byte[] Encrypt(byte[] data, byte[] iv, byte[] key=null)
        {
            if (key == null)
                key = Encoding.UTF8.GetBytes(KEY);
            using (var aes = Aes.Create())
            {
                aes.KeySize = 128;
                aes.BlockSize = 128;
                aes.Padding = PaddingMode.Zeros;

                aes.Key = key;
                aes.IV = iv;

                using (var encryptor = aes.CreateEncryptor(aes.Key, aes.IV))
                {
                    return PerformCryptography(data, encryptor);
                }
            }
        }

        public static byte[] Decrypt(byte[] data, byte[] iv, byte[] key = null)
        {
            if (key == null)
                key = Encoding.UTF8.GetBytes(KEY);
            using (var aes = Aes.Create())
            {
                aes.KeySize = 128;
                aes.BlockSize = 128;
                aes.Padding = PaddingMode.Zeros;

                aes.Key = key;
                aes.IV = iv;

                using (var decryptor = aes.CreateDecryptor(aes.Key, aes.IV))
                {
                    return PerformCryptography(data, decryptor);
                }
            }
        }

        public static byte[] PerformCryptography(byte[] data, ICryptoTransform cryptoTransform)
        {
            using (var ms = new MemoryStream())
            using (var cryptoStream = new CryptoStream(ms, cryptoTransform, CryptoStreamMode.Write))
            {
                cryptoStream.Write(data, 0, data.Length);
                cryptoStream.FlushFinalBlock();

                return ms.ToArray();
            }
        }

        public static string GetMD5(byte[] e)
        {
            using(var md5 = MD5.Create())
            {
                return Convert.ToBase64String(md5.ComputeHash(e));
            }
        }
    }

    [System.Serializable]
    public class AssetManifests
    {
        public List<manifest> manifests = new List<manifest>();
    }

    [System.Serializable]
    public class manifest
    {
        public string name;
        public string key;
        public string hash;
    }
}