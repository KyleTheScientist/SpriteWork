using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using SGUI;
using UnityEngine;
using System.Reflection;
using System.Diagnostics;

namespace SpriteWork
{
    class ResourceExtractor
    {
        public static string spritesDirectory = Path.Combine(ETGMod.ResourcesDirectory, "sprites");



        public static List<Texture2D> GetTexturesFromFolder(string folder)
        {
            string collectionPath = Path.Combine(spritesDirectory, folder);
            if (!Directory.Exists(collectionPath))
                return null;


            List<Texture2D> textures = new List<Texture2D>();
            foreach (string filePath in Directory.GetFiles(collectionPath))
            {
                Texture2D texture = BytesToTexture(File.ReadAllBytes(filePath), Path.GetFileName(filePath).Replace(".png", ""));
                textures.Add(texture);
            }
            return textures;
        }

        public static Texture2D GetTextureFromFolder(string fileName, string folder)
        {
            string collectionPath = Path.Combine(spritesDirectory, folder);
            string filePath = Path.Combine(collectionPath, fileName + ".png");
            if(!File.Exists(filePath))
            {
                ETGModConsole.Log("<color=#FF0000FF>" + filePath + " not found. </color>");
                return null;
            }
            Texture2D texture = BytesToTexture(File.ReadAllBytes(filePath), fileName);
            return texture;
        }

        public static List<string> GetCollectionFiles()
        {
            List<string> collectionNames = new List<string>();
            foreach (string filePath in Directory.GetFiles(spritesDirectory))
            {
                if (filePath.EndsWith(".png"))
                {
                    collectionNames.Add(Path.GetFileName(filePath).Replace(".png", ""));
                }
            }
            return collectionNames;
        }

        public static Texture2D BytesToTexture(byte[] bytes, string resourceName)
        {
            Texture2D texture = new Texture2D(1, 1, TextureFormat.RGBA32, false);
            ImageConversion.LoadImage(texture, bytes);
            texture.filterMode = FilterMode.Point;
            texture.name = resourceName;
            return texture;
        }

        public static List<String> GetResourceFolders()
        {
            List<String> dirs = new List<String>();
            string spritesDirectory = Path.Combine(ETGMod.ResourcesDirectory, "sprites");

            if (Directory.Exists(spritesDirectory))
            {
                foreach (String directory in Directory.GetDirectories(spritesDirectory))
                {
                    dirs.Add(Path.GetFileName(directory));
                }
            }
            return dirs;
        }

        public static byte[] ExtractEmbeddedResource(String filename)
        {
            Assembly a = Assembly.GetCallingAssembly();
            using (Stream resFilestream = a.GetManifestResourceStream(filename))
            {
                if (resFilestream == null)
                {
                    ETGModConsole.Log("File not found");
                }
                byte[] ba = new byte[resFilestream.Length];
                resFilestream.Read(ba, 0, ba.Length);
                return ba;
            }
        }

        public static Texture2D GetEmbeddedResourceAsTexture(string resourceName)
        {
            byte[] sprite = ExtractEmbeddedResource("SpriteWork.Resources." + resourceName);
            Texture2D texture = new Texture2D(1, 1, TextureFormat.RGBA32, false);
            ImageConversion.LoadImage(texture, sprite);
            texture.filterMode = FilterMode.Point;
            texture.name = resourceName;
            return texture;
        }

        public static string[] GetResourceIDs()
        {
            Assembly asm = System.Reflection.Assembly.GetCallingAssembly();
            ETGModConsole.Log("Assmebly null: " + (asm == null));
            string[] names = asm.GetManifestResourceNames();
            if (names == null)
            {
                ETGModConsole.Log("No resources found.");
                return null;
            }
            ETGModConsole.Log(names.Length.ToString() + " resources found.");
            return names;
        }

    }
}
