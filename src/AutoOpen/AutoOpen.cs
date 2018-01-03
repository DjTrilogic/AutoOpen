﻿using System;
using System.Collections.Generic;
using System.Windows.Forms;
using AutoOpen.Utils;
using PoeHUD.Framework.Helpers;
using PoeHUD.Models;
using PoeHUD.Plugins;
using PoeHUD.Poe.Components;
using PoeHUD.Poe.RemoteMemoryObjects;
using SharpDX;
using SharpDX.Direct3D9;

namespace AutoOpen
{
    internal class AutoOpen : BaseSettingsPlugin<Settings>
    {
        private IngameState ingameState;

        public AutoOpen()
        {
            PluginName = "AutoOpen";
        }

        public override void Initialise()
        {
            ingameState = GameController.Game.IngameState;
            base.Initialise();
        }

        public override void Render()
        {
            if (!Settings.Enable)
                return;

            if (Settings.doors) openDoor();
            if (Settings.switches) openSwitch();
        }

        public override void EntityAdded(EntityWrapper entityWrapper)
        {
            base.EntityAdded(entityWrapper);
        }

        public override void EntityRemoved(EntityWrapper entityWrapper)
        {
            base.EntityRemoved(entityWrapper);
        }

        public override void OnClose()
        {
            base.OnClose();
        }

        List<string> doorBlacklist = new List<string>
        {
            "LabyrinthAirlockDoor",
            "HiddenDoor_Short",
            "HiddenDoor",
            "LabyrinthIzaroArenaDoor",
            "Door_Closed",
            "Door_Open",
            "Door_Toggle_Closed",
            "Door_Toggle_Open",
            "SilverDoor",
            "GoldenDoor"
        };

        private void openDoor()
        {
            var entities = GameController.Entities;
            var camera = ingameState.Camera;
            var playerPos = GameController.Player.Pos;
            var prevMousePosition = Mouse.GetCursorPosition();


            foreach (EntityWrapper entity in entities)
            {
                if (entity.HasComponent<TriggerableBlockage>() && entity.Path.ToLower().Contains("door"))
                {
                    var entityPos = entity.Pos;
                    var entityScreenPos = camera.WorldToScreen(entityPos.Translate(0, 0, 0), entity);
                    var entityDistanceToPlayer = Math.Sqrt(Math.Pow(playerPos.X - entityPos.X, 2) + Math.Pow(playerPos.Y - entityPos.Y, 2));
                    bool isClosed = entity.GetComponent<TriggerableBlockage>().IsClosed;

                    string s = isClosed ? "closed" : "opened";
                    Color c = isClosed ? Color.Red : Color.Green;

                    Graphics.DrawText(s, 16, entityScreenPos, c, FontDrawFlags.Center);

                    if (Control.MouseButtons == MouseButtons.Left)
                    {
                        if (entityDistanceToPlayer <= Settings.doorDistance && isClosed)
                        {
                            open(entityScreenPos, prevMousePosition);
                        }
                    }
                }
            }
        }

        private void openSwitch()
        {
            var entities = GameController.Entities;
            var camera = ingameState.Camera;
            var playerPos = GameController.Player.Pos;
            var prevMousePosition = Mouse.GetCursorPosition();

            foreach (EntityWrapper entity in entities)
            {
                if (entity.HasComponent<Transitionable>() && entity.HasComponent<Targetable>() && !entity.HasComponent<TriggerableBlockage>() && entity.Path.ToLower().Contains("switch"))
                {
                    var entityPos = entity.Pos;
                    var entityScreenPos = camera.WorldToScreen(entityPos.Translate(0, 0, 0), entity);
                    bool isTargeted = entity.GetComponent<Targetable>().isTargeted;

                    var switchState = entity.InternalEntity.GetComponent<Transitionable>().switchState;
                    bool switched = switchState != 1;
                    var entityDistanceToPlayer = Math.Sqrt(Math.Pow(playerPos.X - entityPos.X, 2) + Math.Pow(playerPos.Y - entityPos.Y, 2));

                    int count = 1;

                    string s = isTargeted ? "targeted" : "not targeted";
                    Color c = isTargeted ? Color.Green : Color.Red;

                    Graphics.DrawText(s, 20, entityScreenPos.Translate(0, count * 16), c, FontDrawFlags.Center);
                    count++;

                    string s2 = switched ? "switched" : "not switched";
                    Color c2 = switched ? Color.Green : Color.Red;

                    Graphics.DrawText(s2 + ":" + switchState, 20, entityScreenPos.Translate(0, count * 16), c2, FontDrawFlags.Center);
                    count++;

                    if (Control.MouseButtons == MouseButtons.Left)
                    {
                        if (entityDistanceToPlayer <= Settings.doorDistance && !switched)
                        {
                            open(entityScreenPos, prevMousePosition);
                        }
                    }
                }
            }
        }

        private void open(Vector2 entityScreenPos, Vector2 prevMousePosition)
        {
            Mouse.moveMouse(entityScreenPos);
            Mouse.LeftUp(1);
            Mouse.LeftDown(1);
            Mouse.LeftUp(1);
            Mouse.moveMouse(prevMousePosition);
            Mouse.LeftDown(1);
        }
    }
}