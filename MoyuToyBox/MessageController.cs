using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Modding;
using Modding.Common;
using Modding.Blocks;
using Modding.Levels;

namespace MoyuToyBox
{
    public static class Messages
    {
        //For rockets
        public static MessageType cameraRay;
        public static MessageType safetyLength;
        public static MessageType maxLength;
        public static MessageType AimInfo;
        public static MessageType SendCDInfo;

        public static MessageType TorpedoLaunched;
        public static MessageType TorpedoExplode;

        //public static MessageType keyDownMsg;


    }
    public class MessageController : SingleInstance<MessageController>
    {
        public override string Name { get; } = "Message Controller";
        public MessageController()
        {
            Messages.cameraRay = ModNetworking.CreateMessageType(DataType.Vector3, DataType.Vector3,DataType.Boolean);
            Messages.safetyLength = ModNetworking.CreateMessageType(DataType.Single);
            Messages.maxLength = ModNetworking.CreateMessageType(DataType.Single);
            Messages.AimInfo = ModNetworking.CreateMessageType(DataType.Integer,DataType.Boolean,DataType.Vector3);
            Messages.SendCDInfo = ModNetworking.CreateMessageType(DataType.Single);

            Messages.TorpedoLaunched = ModNetworking.CreateMessageType(DataType.Integer, DataType.Integer,DataType.Integer);
            Messages.TorpedoExplode = ModNetworking.CreateMessageType(DataType.Integer, DataType.Integer, DataType.Integer);

            //Messages.keyDownMsg = ModNetworking.CreateMessageType(DataType.Integer, DataType.Boolean);
            
            ModNetworking.Callbacks[Messages.cameraRay] += (Message msg) =>
            {
                TurretController.Instance.ray[msg.Sender.NetworkId].origin = (Vector3)msg.GetData(0);
                TurretController.Instance.ray[msg.Sender.NetworkId].direction = (Vector3)msg.GetData(1);
                TurretController.Instance.camBindA[msg.Sender.NetworkId] = (bool)msg.GetData(2);

            };


            ModNetworking.Callbacks[Messages.safetyLength] += (Message msg) =>
            {
                TurretController.Instance.safetylengthArray[msg.Sender.NetworkId] = (float)msg.GetData(0);
            };

            ModNetworking.Callbacks[Messages.maxLength] += (Message msg) =>
            {
                try
                {
                    TurretController.Instance.maxlengthArray[msg.Sender.NetworkId] = (float)msg.GetData(0);
                }
                catch { }
            };

            ModNetworking.Callbacks[Messages.AimInfo] += (Message msg) =>
            {
                try
                {
                    TurretController.Instance.camBindA[(int)msg.GetData(0)] = (bool)msg.GetData(1);
                    TurretController.Instance.lastaimpos[(int)msg.GetData(0)] = TurretController.Instance.thisaimpos[(int)msg.GetData(0)];
                    TurretController.Instance.thisaimpos[(int)msg.GetData(0)] = (Vector3)msg.GetData(2);
                    TurretController.Instance.rcvPassed[(int)msg.GetData(0)] = 0f;
                }
                catch { }
            };

            ModNetworking.Callbacks[Messages.SendCDInfo] += (Message msg) =>
            {
                try
                {
                    TurretController.Instance.sendCD = (float)msg.GetData(0);
                }
                catch { }
            };
            ModNetworking.Callbacks[Messages.TorpedoLaunched] += (Message msg) =>
            {
                try
                {
                    TorpedoController.Instance.TL[(int)msg.GetData(2)].Add((int)msg.GetData(0), (int)msg.GetData(1));
                }
                catch { }
            };
            ModNetworking.Callbacks[Messages.TorpedoExplode] += (Message msg) =>
            {
                try
                {
                    TorpedoController.Instance.TD[(int)msg.GetData(2)][(int)msg.GetData(0)].Add((int)msg.GetData(1));
                }
                catch { }
            };

            ModNetworking.Callbacks[MissileLauncher.MissileLaunched] += MissileController.MissileLaunchedEvent;
            //ModNetworking.Callbacks[MissileLauncher.MissilePosAndRot] += MissileController.MissileMoveEvent;
            ModNetworking.Callbacks[MissileLauncher.MissileExplode] += MissileController.MissileExplodeEvent;

            ModNetworking.Callbacks[LaserLauncher.SendLength] += LaserController.lengthRcved;

            ModNetworking.Callbacks[BeamRifle.BeamLaunched] += BeamRifleController.BeamLaunchedEvent;
            ModNetworking.Callbacks[BeamRifle.BeamExplode] += BeamRifleController.BeamExplodeEvent;
            ModNetworking.Callbacks[BeamRifle.ShotgunLaunched] += BeamRifleController.ShotgunLaunchedEvent;


            ModNetworking.Callbacks[KeymsgController.SendHeld] += KeymsgController.HeldRcved;
            ModNetworking.Callbacks[KeymsgController.SendPressed] += KeymsgController.PressedRcved;
            /*
            ModNetworking.Callbacks[Messages.MissileExplode] += (Message msg) =>
            {
                try
                {
                    MissileController.Instance.ME.Add((int)msg.GetData(0));
                }
                catch { }
            };
            ModNetworking.Callbacks[Messages.MissileLaunched] += (Message msg) =>
            {
                try
                {
                    MissileController.Instance.MD[(int)msg.GetData(2)][(int)msg.GetData(0)].Add((int)msg.GetData(1));
                }
                catch { }
            };
            ModNetworking.Callbacks[Messages.MissilePosAndRot] += (Message msg) =>
            {
                try
                {
                    MissileController.Instance.MP[(int)msg.GetData(0)].position = (Vector3)msg.GetData(1);
                    MissileController.Instance.MP[(int)msg.GetData(0)].eulerAngles = (Vector3)msg.GetData(2);
                }
                catch { }
            };
            */
        }


    }
}
