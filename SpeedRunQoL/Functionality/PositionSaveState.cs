﻿using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using System.Reflection;
using static DebugMod.EnemiesPanel;
using System;
using System.Collections;
using DebugMod;
using UnityEngine.SceneManagement;
using Console = DebugMod.Console;
using Object = UnityEngine.Object;

namespace SpeedRunQoL.Functionality
{
    public static class PositionSaveState
    {
        public static List<EnemyData> AllEnemiesList = new List<EnemyData>(); 
        private static List<EnemyData> LoadEnemiesList = new List<EnemyData>();

        private static Vector3 KnightPos;
        private static Vector3 CamPos;
        private static Vector2 KnightVel;
        
        static PositionSaveState()
        {
            UnityEngine.SceneManagement.SceneManager.activeSceneChanged += DeleteSaveState;
        }
        
        private static void DeleteSaveState(Scene arg0, Scene arg1)
        {
            //if list is empty, the check in LoadPosition will stop loading state
            AllEnemiesList.Clear();
        }

        public static void SavePosition()
        {
            KnightPos = HeroController.instance.gameObject.transform.position;
            KnightVel = HeroController.instance.current_velocity;
            CamPos = GameManager.instance.cameraCtrl.gameObject.transform.position;
            
            AllEnemiesList = CreateAllEnemiesList();
            AllEnemiesList.ForEach(delegate(EnemyData ed) {ed.gameObject.SetActive(false);});
        }
        

        public static void LoadPosition()
        {
            //check for list is empty (ie no saved state) so dont load state
            if (!AllEnemiesList.Any())
            {
                Console.AddLine("Cannot load save state because no state is saved");
                return;
            }

            DestroyOldEnemies();
            LoadEnemiesList = LoadEnemies();
            // Move knight to saved location, change velocity to saved velocity, Move Camera to saved campos, 

            HeroController.instance.gameObject.transform.position = KnightPos;
            HeroController.instance.current_velocity = KnightVel;
            GameManager.instance.cameraCtrl.gameObject.transform.position = CamPos;

        }

        public static List<EnemyData> LoadEnemies()
        {
            List<EnemyData> data = new List<EnemyData>();
            
            foreach (EnemyData tempEd in AllEnemiesList.Where(ed => ed.gameObject != null))
            {
                GameObject gameObject2 = Object.Instantiate(tempEd.gameObject, tempEd.gameObject.transform.position, tempEd.gameObject.transform.rotation);
                
                Component component = gameObject2.GetComponent<tk2dSprite>();
                HealthManager hm = gameObject2.GetComponent<HealthManager>();
                
                int value8 = hm.hp;
                gameObject2.SetActive(true);
                
                data.Add(new EnemyData(value8, hm, component, DebugMod.EnemiesPanel.parent, gameObject2));
            }
            return data;
        }

        public static void DestroyOldEnemies()
        {
            //get all created
            LoadEnemiesList.ForEach(delegate(EnemyData dat)
            {
                if (AllEnemiesList.All(ed => ed.gameObject != dat.gameObject))
                {
                    Object.Destroy(dat.gameObject.gameObject.gameObject.gameObject);
                }

            });

        }

        public static List<EnemyData> CreateAllEnemiesList()
        {
            float boxSize = 250f;
            List<EnemyData> ret = new List<EnemyData>();
            if (HeroController.instance != null && !HeroController.instance.cState.transitioning && GameManager.instance.IsGameplayScene())
            {
                int layerMask = 133120;
                foreach (Collider2D enemy in Physics2D.OverlapBoxAll(HeroController.instance.transform.position, new Vector2(boxSize, boxSize), 1f, layerMask))
                {
                        HealthManager hm = enemy.GetComponent<HealthManager>();

                        if (hm && enemy.gameObject.activeSelf && !DebugMod.EnemiesPanel.Ignore(enemy.gameObject.name))
                        {
                            Component component = enemy.gameObject.GetComponent<tk2dSprite>();
                            if (component == null)
                            {
                                component = null;
                            }

                            int value = hm.hp;
                            ret.Add(new EnemyData(value, hm, component, DebugMod.EnemiesPanel.parent, enemy.gameObject));
                        }
                }
            }
            return ret;
        }
    }
}
