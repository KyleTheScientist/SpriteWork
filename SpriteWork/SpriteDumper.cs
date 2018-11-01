using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;
using UnityEngine;
using MonoMod.RuntimeDetour;

namespace SpriteWork
{
    class SpriteDumper
    {
        public static List<tk2dSpriteCollectionData> dumpedCollections;
        public static string dumpDirectory = Path.Combine(ETGMod.ResourcesDirectory, "dump");
        public readonly static Dictionary<string, Texture2D> TextureMap = new Dictionary<string, Texture2D>();

        public static void Init()
        {
            dumpedCollections = new List<tk2dSpriteCollectionData>();
            /*
            Hook hook = new Hook(
                typeof(tk2dSpriteCollectionData).GetMethod("Init", BindingFlags.NonPublic | BindingFlags.Instance),
                typeof(SpriteDumper).GetMethod("Dump")
            );
            ;
            */

            Hook hook = new Hook(
               typeof(tk2dBaseSprite).GetMethod("InitInstance", BindingFlags.NonPublic | BindingFlags.Instance),
               typeof(SpriteDumper).GetMethod("BaseDump")
           );
        }

        public static void BaseDump(Action<tk2dBaseSprite> orig, tk2dBaseSprite self)
        {
            orig(self);
            if (dumpedCollections.Contains(self.Collection)) return;
            
            DumpSpriteCollection(self.Collection);
            dumpedCollections.Add(self.Collection);

        }

            public static void Dump(Action<tk2dSpriteCollectionData> orig, tk2dSpriteCollectionData self)
        {
            orig(self);
            if (dumpedCollections.Contains(self)) return;
            /*
            DumpSpriteCollection(self);
            dumpedCollections.Add(self);
            */

            string collectionDir = Path.Combine(dumpDirectory, self.name);
            Directory.CreateDirectory(collectionDir);

            foreach (tk2dSpriteDefinition def in self.spriteDefinitions)
            {
                string filePath = Path.Combine(collectionDir, def.name + ".png");
                Texture2D savedTexture = def.material.mainTexture as Texture2D;
                Texture2D copy = savedTexture.GetRW();
                try
                {
                    File.WriteAllBytes(filePath, copy.EncodeToPNG());
                }
                catch (Exception e)
                {
                    ETGModConsole.Log(e.Message);
                }
                dumpedCollections.Add(self);
                return;
            }

        }

        public static void DumpSpriteCollection(tk2dSpriteCollectionData sprites)
        {
            string path = "DUMPsprites/" + sprites.spriteCollectionName;
            string diskPath = null;

            diskPath = Path.Combine(ETGMod.ResourcesDirectory, path.Replace('/', Path.DirectorySeparatorChar).Replace('\\', Path.DirectorySeparatorChar) + ".png");
            if (File.Exists(diskPath))
            {
                return;
            }

            Texture2D texRWOrig = null;
            Texture2D texRW = null;
            Color[] texRWData = null;
            for (int i = 0; i < sprites.spriteDefinitions.Length; i++)
            {
                tk2dSpriteDefinition frame = sprites.spriteDefinitions[i];
                Texture2D texOrig = frame.material.mainTexture as Texture2D;
                if (texOrig == null || !frame.Valid || (frame.materialInst != null && TextureMap.ContainsValue((Texture2D)frame.materialInst.mainTexture)))
                {
                    continue;
                }
                string pathFull = path + "/" + frame.name;
                // Console.WriteLine("Frame " + i + ": " + frame.name + " (" + pathFull + ")");

                /*
                for (int ii = 0; ii < frame.uvs.Length; ii++) {
                    Console.WriteLine("UV " + ii + ": " + frame.uvs[ii].x + ", " + frame.uvs[ii].y);
                }

                /**/
                /*
                Console.WriteLine("P0 " + frame.position0.x + ", " + frame.position0.y);
                Console.WriteLine("P1 " + frame.position1.x + ", " + frame.position1.y);
                Console.WriteLine("P2 " + frame.position2.x + ", " + frame.position2.y);
                Console.WriteLine("P3 " + frame.position3.x + ", " + frame.position3.y);
                /**/

                if (texRWOrig != texOrig)
                {
                    texRWOrig = texOrig;
                    texRW = texOrig.GetRW();
                    texRWData = texRW.GetPixels();
                    diskPath = Path.Combine(ETGMod.ResourcesDirectory, path.Replace('/', Path.DirectorySeparatorChar).Replace('\\', Path.DirectorySeparatorChar) + ".png");

                    Directory.GetParent(diskPath).Create();
                    File.WriteAllBytes(diskPath, texRW.EncodeToPNG());
                }

                Texture2D texRegion;

                double x1UV = 1D;
                double y1UV = 1D;
                double x2UV = 0D;
                double y2UV = 0D;
                for (int ii = 0; ii < frame.uvs.Length; ii++)
                {
                    if (frame.uvs[ii].x < x1UV) x1UV = frame.uvs[ii].x;
                    if (frame.uvs[ii].y < y1UV) y1UV = frame.uvs[ii].y;
                    if (x2UV < frame.uvs[ii].x) x2UV = frame.uvs[ii].x;
                    if (y2UV < frame.uvs[ii].y) y2UV = frame.uvs[ii].y;
                }

                int x1 = (int)Math.Floor(x1UV * texOrig.width);
                int y1 = (int)Math.Floor(y1UV * texOrig.height);
                int x2 = (int)Math.Ceiling(x2UV * texOrig.width);
                int y2 = (int)Math.Ceiling(y2UV * texOrig.height);
                int w = x2 - x1;
                int h = y2 - y1;

                if (
                    frame.uvs[0].x == x1UV && frame.uvs[0].y == y1UV &&
                    frame.uvs[1].x == x2UV && frame.uvs[1].y == y1UV &&
                    frame.uvs[2].x == x1UV && frame.uvs[2].y == y2UV &&
                    frame.uvs[3].x == x2UV && frame.uvs[3].y == y2UV
                )
                {
                    // original
                    texRegion = new Texture2D(w, h);
                    texRegion.SetPixels(texRW.GetPixels(x1, y1, w, h));
                }
                else
                {
                    // flipped
                    if (frame.uvs[0].x == frame.uvs[1].x)
                    {
                        int t = h;
                        h = w;
                        w = t;
                    }
                    texRegion = new Texture2D(w, h);

                    // Flipping using GPU / GL / Quads / UV doesn't work (returns blank texture for some reason).
                    // RIP performance.

                    double fxX = frame.uvs[1].x - frame.uvs[0].x;
                    double fyX = frame.uvs[2].x - frame.uvs[0].x;
                    double fxY = frame.uvs[1].y - frame.uvs[0].y;
                    double fyY = frame.uvs[2].y - frame.uvs[0].y;

                    double wO = texOrig.width * (frame.uvs[3].x - frame.uvs[0].x);
                    double hO = texOrig.height * (frame.uvs[3].y - frame.uvs[0].y);

                    double e = 0.001D;
                    double fxX0w = fxX < e ? 0D : wO;
                    double fyX0w = fyX < e ? 0D : wO;
                    double fxY0h = fxY < e ? 0D : hO;
                    double fyY0h = fyY < e ? 0D : hO;

                    for (int y = 0; y < h; y++)
                    {
                        double fy = y / (double)h;
                        for (int x = 0; x < w; x++)
                        {
                            double fx = x / (double)w;

                            double fxUV0w = fx * fxX0w + fy * fyX0w;
                            double fyUV0h = fx * fxY0h + fy * fyY0h;

                            double p =
                                Math.Round(frame.uvs[0].y * texOrig.height + fyUV0h) * texOrig.width +
                                Math.Round(frame.uvs[0].x * texOrig.width + fxUV0w);

                            texRegion.SetPixel(x, y, texRWData[(int)p]);

                        }
                    }

                }

                diskPath = Path.Combine(ETGMod.ResourcesDirectory, pathFull.Replace('/', Path.DirectorySeparatorChar).Replace('\\', Path.DirectorySeparatorChar) + ".png");
                if (!File.Exists(diskPath))
                {
                    Directory.GetParent(diskPath).Create();
                    File.WriteAllBytes(diskPath, texRegion.EncodeToPNG());
                }

            }
        }
    }
}
