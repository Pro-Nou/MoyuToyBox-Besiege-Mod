using System;
using Modding;
using UnityEngine;

namespace MoyuToyBox
{
    public class MakeAudioSourceFixedPitch : MonoBehaviour
    {
        private AudioSource FixedAS;
        private void Start()
        {
            FixedAS = base.GetComponent<AudioSource>();
        }
        private void Update()
        {
            FixedAS.pitch = Time.timeScale;
        }
    }
	class moyuToyBox : ModEntryPoint
	{
        public static GameObject mod;
        public override void OnLoad()
		{

            mod = new GameObject("Moyu Toy Box");
            UnityEngine.Object.DontDestroyOnLoad(mod);
            //mod.AddComponent<CustomBlockController>();
            mod.AddComponent<TurretController>();
            mod.AddComponent<TorpedoController>();
            mod.AddComponent<MissileController>();
            mod.AddComponent<LaserController>();
            mod.AddComponent<BeamRifleController>();
            mod.AddComponent<KeymsgController>();
            MessageController.Instance.transform.SetParent(mod.transform);
            
            // Called when the mod is loaded.
        }
    }
}
