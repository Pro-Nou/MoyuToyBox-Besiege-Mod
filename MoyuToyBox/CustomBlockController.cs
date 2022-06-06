using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Modding;
using Modding.Blocks;

using System.Collections;
namespace MoyuToyBox
{
    class CustomBlockController:SingleInstance<CustomBlockController>
    {
        
        public override string Name { get; } = "Custom Block Controller";

        internal PlayerMachineInfo PMI;
        
        private void Awake()
        {
            
            //加载配置
            //Events.OnMachineLoaded += LoadConfiguration;
            Events.OnMachineLoaded += (pmi) => { PMI = pmi; };
            ////储存配置
            //Events.OnMachineSave += SaveConfiguration;
            //添加零件初始化事件委托
            Events.OnBlockInit += AddSliders;
            
        }
        private void AddSliders(Block block)
        {

            BlockBehaviour blockbehaviour = block.BuildingBlock.InternalObject;
            AddSliders(blockbehaviour);
        }
        private void AddSliders(BlockBehaviour block)
        {
            /*if (block.gameObject.GetComponent(typeof(CommomStript)) == null)

                block.gameObject.AddComponent(typeof(CommomStript));*/
            /*
            if (block.BlockID == (int)BlockType.Cannon)
            {
                if (StatMaster.isMP)
                    return;
                if (block.gameObject.GetComponent(typeof(CannonStript)) == null)

                    block.gameObject.AddComponent(typeof(CannonStript));
            }
            */
            if (block.BlockID == (int)BlockType.SpinningBlock)
            {
                if (block.gameObject.GetComponent(typeof(SpinningBlockStript)) == null)

                    block.gameObject.AddComponent(typeof(SpinningBlockStript));
            }
            /*
            if (block.BlockID == (int)BlockType.SteeringBlock)
            {
                if (block.gameObject.GetComponent(typeof(SteeringBlockStript)) == null)

                    block.gameObject.AddComponent(typeof(SteeringBlockStript));
            }
            */
            /*
            if (dic_EnhancementBlock.ContainsKey(block.BlockID))
            {
                var EB = dic_EnhancementBlock[block.BlockID];

                if (block.GetComponent(EB) == null)
                {
                    block.gameObject.AddComponent(typeof(MSlider));
                }
            }
            */
        }

        
    }
}
