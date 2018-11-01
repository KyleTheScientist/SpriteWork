using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SpriteWork
{
    public class SpriteWorkMod : ETGModule
    {
        public override void Start()
        {
            //SpriteReplacer.Init();
            SpriteDumper.Init();
            ETGModConsole.Log("Sprite Replacement Initialized");

        }

        public override void Exit()
        {
        }

        public override void Init()
        {
        }
    }
}
