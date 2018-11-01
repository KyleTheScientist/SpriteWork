using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using UnityEngine;
using MonoMod.RuntimeDetour;

namespace SpriteWork
{
    class SpriteReplacer
    {
        static List<string> collectionsToReplace;
        static List<string> spriteSheetsToReplace;
        static List<tk2dSpriteCollectionData> replacedCollections;

        public static void Init()
        {
            collectionsToReplace = ResourceExtractor.GetResourceFolders();
            spriteSheetsToReplace = ResourceExtractor.GetCollectionFiles();
            foreach (string s in spriteSheetsToReplace)
            {
                ETGModConsole.Log(s);
            } 
            replacedCollections = new List<tk2dSpriteCollectionData>();

            Hook hook = new Hook(typeof(tk2dSpriteCollectionData).GetMethod("Init", BindingFlags.NonPublic | BindingFlags.Instance), typeof(SpriteReplacer).GetMethod("Replace"));
        }

        public static void Replace(Action<tk2dSpriteCollectionData> orig, tk2dSpriteCollectionData self)
        {
            orig(self);


            if (replacedCollections.Contains(self)) return;

            bool found = false;
            bool spriteSheet = false;
            foreach (string collectionName in collectionsToReplace)
            {
                if (self.name.Equals(collectionName))
                    found = true;
            }

            foreach (string sheetName in spriteSheetsToReplace)
            {
                if (self.name.Equals(sheetName))
                {
                    found = true;
                    spriteSheet = true;
                }
            }

            if (!found) return;

            ETGModConsole.Log("Replacing collection: " + self.name);
            if (spriteSheet)
            {
                ReplaceWithSpriteSheet(self);
            }
            else
            {
                ReplaceWithTextures(self);
            }

            replacedCollections.Add(self);
        }

        public static void ReplaceWithSpriteSheet(tk2dSpriteCollectionData data)
        {
            Texture2D spritesheet = ResourceExtractor.GetTextureFromFolder(data.name, "");
            //Texture2D[] textarray = new Texture2D[] { spritesheet};
            foreach(Material m in data.materialInsts)
            {
                m.mainTexture = spritesheet;
            }
        }

        public static void ReplaceWithTextures(tk2dSpriteCollectionData data)
        {
            List<Texture2D> replacements = ResourceExtractor.GetTexturesFromFolder(data.name);
            foreach (Texture2D texture in replacements)
            {
                tk2dSpriteDefinition def = GetDefinition(data, texture.name);
                if (def != null)
                {
                    def.ReplaceTexture(texture);
                }
                else
                {
                    ETGModConsole.Log("<color=#FF0000FF>" + texture.name + " not found. </color>");
                }

            }
        }

        public static tk2dSpriteDefinition GetDefinition(tk2dSpriteCollectionData data, string name)
        {
            foreach (tk2dSpriteDefinition d in data.spriteDefinitions)
            {
                if (d.name.Equals(name))
                {
                    return d;
                }
            }
            return null;
        }
    }
}
