using System.Reflection.Emit;
using System.Runtime.InteropServices;
using HarmonyLib;
using Il2Cpp;
using MelonLoader;
using Unity.IO.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.Bindings;
using static Il2CppSystem.Collections.Hashtable;
using static MelonLoader.MelonLogger;
using System.Collections.Generic;

namespace Utilities
{
    internal class Patches
    {
        [HarmonyPatch(typeof(AbyssManager))]
        public static class AbyssManager_Patch
        {
            [HarmonyPrefix]
            [HarmonyPatch("UpdateStatistics")]
            private static void UpdateStatistics(AbyssManager __instance,Plant plant)
            {
                // Use reflection to modify the read-only property
                var maxBuffCountField = typeof(AbyssManager).GetProperty("MaxBuffCount", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
                if (maxBuffCountField != null && maxBuffCountField.CanWrite)
                {
                    maxBuffCountField.SetValue(__instance, 999999);
                }

                __instance.ultiPlantCount += 999999;
                __instance.superPlantCount += 999999;
                __instance.maxPlantCount += 999999;
                __instance.money += 999999;
                __instance.moreHealth += 3 * (__instance.moreHealth + plant.thePlantMaxHealth);
                __instance.attackSpeed += 10 * __instance.attackSpeed;
                __instance.strikeRate += 10 * (__instance.strikeRate * plant.exchangeSpeed);
                __instance.strikeDmg += 10 * (__instance.strikeDmg + plant.attackDamage);
                __instance.cardLessCost += 999999;
                __instance.cardSpeed += 999999;
                __instance.damage += 999999;
                __instance.extraSun += 999999;
                __instance.gloveSpeed += 999999;
                __instance.hammerSpeed += 999999;
                __instance.recoverCount += 999999;
                __instance.refreshCount += 999999;
                __instance.stealHealth += 999999;
                MelonLogger.Msg("AbyssManager updated!");

            }
        }


        [HarmonyPatch(typeof(CardUI))]
        public static class CardUI_Patch
        {
            [HarmonyPostfix]
            [HarmonyPatch("Update")]
            private static void Update(CardUI __instance)
            {
                if (Utility.GetActive(Utility.UtilityType.NoCooldown))
                {
                    __instance.CD = __instance.fullCD;
                    __instance.isAvailable = true;
                }
            }
        }

        [HarmonyPatch(typeof(Glove))]
        public static class GloveMgr_Patch
        {
            [HarmonyPrefix]
            [HarmonyPatch("CDUpdate")]
            private static void CDUpdate(Glove __instance)
            {
                if (Utility.GetActive(Utility.UtilityType.NoCooldown))
                {
                    float noCD = 0.001f;
                    __instance.fullCD = noCD;
                    __instance.fullCD = 0.001f;
                    __instance.CD = __instance.fullCD;
                    __instance.avaliable = true;
                    GameObject.Find("Glove").GetComponent<Glove>().CD = 999999f;
                }
                __instance.fullCD = 0.001f;
                GameObject.Find("Glove").GetComponent<Glove>().CD = 999999f;
            }
        }



        [HarmonyPatch(typeof(HammerMgr))]
        public static class HammerMgr_Patch
        {
            [HarmonyPrefix]
            [HarmonyPatch("CDUpdate")]
            private static void CDUpdate(HammerMgr __instance)
            {
                if (Utility.GetActive(Utility.UtilityType.NoCooldown))
                {
                    __instance.CD = __instance.fullCD;
                    __instance.avaliable = true;
                }
            }
        }

        [HarmonyPatch(typeof(Board))]
        public static class Board_Patch
        {
            [HarmonyPostfix]
            [HarmonyPatch("Update")]
            private static void Update(Board __instance)
            {
                if (Utility.GetActive(Utility.UtilityType.UnliSun))
                    __instance.theSun = 99999;

                if (Utility.GetActive(Utility.UtilityType.UnliCoins))
                    __instance.theMoney = 2147400000;

                __instance.freeCD = Utility.GetActive(Utility.UtilityType.NoCooldown) || Utility.GetActive(Utility.UtilityType.DeveloperMode);

                if (Utility.GetActive(Utility.UtilityType.StopZombieSpawn))
                {
                    __instance.newZombieWaveCountDown = 15f;
                }

                if (Input.GetKeyDown(KeyCode.Quote))
                {
                    Core.isScaredyDream = !Core.isScaredyDream;
                    Board.BoardTag boardTag = Board.Instance.boardTag;
                    boardTag.isScaredyDream = Core.isScaredyDream;
                    Board.Instance.boardTag = boardTag;
                }

                if (Input.GetKeyDown(KeyCode.Backslash))
                {
                    Core.isSeedRain = !Core.isSeedRain;
                    Board.BoardTag boardTag = Board.Instance.boardTag;
                    boardTag.isSeedRain = Core.isSeedRain;
                    boardTag.isNight = Core.isSeedRain;
                    Board.Instance.boardTag = boardTag;
                }
            }
        }

        [HarmonyPatch(typeof(Mouse))]
        public static class Mouse_Patch
        {
            [HarmonyPrefix]
            [HarmonyPatch("TryToSetPlantByCard")]
            private static void TryToSetPlantByCard(Mouse __instance)
            {
                if (Utility.GetActive(Utility.UtilityType.ColumnPlants))
                {
                    for (int i = 0; i < Board.Instance.rowNum; i++)
                    {
                        if (i != __instance.theMouseRow)
                        {
                            CreatePlant.Instance.SetPlant(__instance.theMouseColumn, i, __instance.thePlantTypeOnMouse, null, default(Vector2), false, true);
                        }
                    }
                }
            }
        }

        [HarmonyPatch(typeof(CreatePlant))]
        public static class CreatePlant_Patch
        {
#if X
			[HarmonyPostfix]
			[HarmonyPatch("CheckBox")]
			private static void CheckBox(ref bool __result)
			{
				if (Utility.GetActive(Utility.UtilityType.PlantEverywhere))
				{
					__result = true;
				}
			}
#endif

            [HarmonyPrefix]
            [HarmonyPatch("SetPlant")]
            private static void SetPlant(ref bool isFreeSet)
            {
                if (Utility.GetActive(Utility.UtilityType.PlantEverywhere))
                {
                    isFreeSet = true;
                }
            }


            [HarmonyPrefix]
			[HarmonyPatch("Lim")]
			private static bool Lim(ref bool __result)
			{
				__result = false;
				return false;
			}

			[HarmonyPrefix]
			[HarmonyPatch("LimTravel")]
			private static bool LimTravel(ref bool __result)
			{
				__result = false;
				return false;
			}

        }


        [HarmonyPatch(typeof(InGameUI))]
        public static class InGameUIMgr_Patch
        {
            [HarmonyPostfix]
            [HarmonyPatch("Update")]
            private static void Update(InGameUI __instance)
            {
                if (Utility.GetActive(Utility.UtilityType.UnliSun))
                {
                    __instance.sun.text = "∞";
                }
                if (Utility.GetActive(Utility.UtilityType.DeveloperMode))
                {
                    __instance.sun.text = "∞";
                }
            }
        }

        [HarmonyPatch(typeof(Money))]
        public static class Money_Patch
        {
            [HarmonyPostfix]
            [HarmonyPatch("Update")]
            private static void Update(Money __instance)
            {
                if (Utility.GetActive(Utility.UtilityType.UnliCoins))
                {
                    __instance.textMesh.text = "∞";
                    __instance.beanCount.text = "∞";
                    __instance.beanCount2.text = "∞";
                }
                if (Utility.GetActive(Utility.UtilityType.DeveloperMode))
                {
                    __instance.textMesh.text = "∞";
                    __instance.beanCount.text = "∞";
                    __instance.beanCount2.text = "∞";
                }
            }
        }

        [HarmonyPatch(typeof(Plant))]
        public static class Plant_Patch
        {
            [HarmonyPrefix]
            [HarmonyPatch("TakeDamage")]
            private static void TakeDamage(ref int damage)
            {
                if (Utility.GetActive(Utility.UtilityType.InvulPlants))
                {
                    damage = 0;
                }
            }

            [HarmonyPostfix]
            [HarmonyPatch("Update")]
            private static void Update(Plant __instance)
            {
                if (Input.GetKeyDown(KeyCode.KeypadPlus))
                {
                    __instance.Die(0);
                }
            }

            //[HarmonyPrefix]
            //[HarmonyPatch("Crashed")]
            //private static bool  Crashed(Plant plant)
            //{
            //    if (Utility.GetActive(Utility.UtilityType.InvulPlants))
            //    {
            //        //PlantType plantType = plant.thePlantType;
            //        plant.isCrashed = false;

            //    }
            //    return false;
            //}
        }

        [HarmonyPatch(typeof(Zombie))]
        public static class Zombie_Patch
        {
            [HarmonyPrefix]
            [HarmonyPatch("TakeDamage")]
            private static void TakeDamage(ref int theDamage)
            {
                if (Utility.GetActive(Utility.UtilityType.InvulZombies))
                {
                    theDamage = 0;
                }
                if (Utility.GetActive(Utility.UtilityType.DoubleDamage))
                {
                    theDamage *= 2;
                }
                if (Utility.GetActive(Utility.UtilityType.SuperDamage))
                {
                    theDamage *= 100;
                }
            }
        }

        [HarmonyPatch(typeof(GameAPP))]
        public static class GameAPP_Patch
        {
            [HarmonyPrefix]
            [HarmonyPatch("Update")]
            private static void Update(GameAPP __instance)
            {
                GameAPP.developerMode = Utility.GetActive(Utility.UtilityType.DeveloperMode);
                Generate();
            }

            private static void Generate()
            {
                if (Input.GetKeyDown(KeyCode.Keypad0))
                {
                    Utility.SpawnItem("Board/Award/TrophyPrefab");
                }

                if (Input.GetKeyDown(KeyCode.Keypad1))
                {
                    Utility.SpawnItem("Items/fertilize/Ferilize");
                }

                if (Input.GetKeyDown(KeyCode.Keypad2))
                {
                    Utility.SpawnItem("Items/Bucket");
                }

                if (Input.GetKeyDown(KeyCode.Keypad3))
                {
                    Utility.SpawnItem("Items/Helmet");
                }

                if (Input.GetKeyDown(KeyCode.Keypad4))
                {
                    Utility.SpawnItem("Items/JackBox");
                }

                if (Input.GetKeyDown(KeyCode.Keypad5))
                {
                    Utility.SpawnItem("Items/Pickaxe");
                }

                if (Input.GetKeyDown(KeyCode.Keypad6))
                {
                    Utility.SpawnItem("Items/Machine");
                }

                // 
                if (Input.GetKeyDown(KeyCode.Alpha5))
                {
                    for (int i = 0; i < 15; i++)
                    {
                        Utility.SpawnItem("Items/SuperMachine");
                    }
                }

                if (Input.GetKeyDown(KeyCode.Keypad8))
                {
                    //Board.Instance.CreateUltimateMateorite();
                    int successfulCreations = 0;
                    for (int i = 0; i < 5; i++)
                    {
                        try
                        {
                            Board.Instance.CreateUltimateMateorite();
                            successfulCreations++;
                        }
                        catch (Exception ex)
                        {
                            Debug.LogError($"Failed to create meteorite {i + 1}: {ex.Message}");
                        }
                    }

                    Debug.Log($"Created {successfulCreations} out of 5 Ultimate Meteorites");
                }


                if (Input.GetKeyDown(KeyCode.Keypad8))
                {
                    foreach (Zombie zombie in Board.Instance.zombieArray)
                    {
                        if (zombie != null)
                        {
                            zombie.SetMindControl();
                        }
                    }
                }

                if (Input.GetKeyDown(KeyCode.Keypad9))
                {
                    foreach (Zombie zombie in Board.Instance.zombieArray)
                    {
                        if (zombie != null && !zombie.isMindControlled)
                        {
                            zombie.Die(1);
                        }
                    }
                }
            }
        }

        [HarmonyPatch(typeof(Plant))]
        public static class Uncrashable
        {
            [HarmonyPrefix]
            [HarmonyPatch("Crashes")]
            public static bool CrashedPatch([Optional] Zombie zombie, Plant plant, int type = 0, int soundID = 0)
            {
                return false;
            }
        }

        [HarmonyPatch(typeof(Plant), "Update")]
        public static class PlantPatchD
        {
            [HarmonyPostfix]
            public static void Postfix(Plant __instance)
            {
                if (Utility.GetActive(Utility.UtilityType.TestUpgrade))
                {
                    if (__instance.theLevel != 3)
                    {
                        __instance.Upgrade(3, true);
                        __instance.isCrashed = false;
                    }
                }
            }
        }


#if PlantHealth
        [HarmonyPatch(typeof(Plant))]
        public static class PlantLimHealth
        {
            [HarmonyPrefix]
            [HarmonyPatch("LimHealth")]
            private static void LimHealth(Plant __instance)
            {
                if (Utility.GetActive(Utility.UtilityType.InvulPlants))
                {
                    int num = __instance.thePlantMaxHealth;
                    if (__instance.thePlantHealth > num)
                    {
                       __instance.thePlantHealth = __instance.thePlantHealth;
                    }
                }
            }
        }
#endif


        [HarmonyPatch(typeof(GameLose))]
        public static class GameLose_Patch
        {
            [HarmonyPrefix]
            [HarmonyPatch("OnTriggerEnter2D")]
            private static bool OnTriggerEnter2D()
            {
                return !Utility.GetActive(Utility.UtilityType.StopGameOver);
            }
        }
#if SSG
        [HarmonyPatch(typeof(SuperSnowGatling))]
        public static class SSG_Patch
        {
            [HarmonyPostfix]
            [HarmonyPatch("GetBulletType")]
            private static void GetBulletType_Postfix(ref int __result)
            {
                __result = 23;
            }

            [HarmonyPrefix]
            [HarmonyPatch("TakeDamage")]
            private static void TakeDamage( SuperSnowGatling __instance,Plant plant, Shooter shooter)
            {
                if (plant != null && shooter != null)
                {
                    plant.attackDamage = 999;
                    plant.regeneration = true;
                    __instance.timer = 0;
                    shooter.dreamTime = 0;
                    plant.attributeCountdown = 0;
                    plant.brightness = 999f;
                    plant.currentLightLevel = 99;
                    plant.keepShooting = true;
                    plant.theOriginSpeed = 1f;
                    plant.thePlantAttackCountDown = 0.001f;
                    plant.thePlantAttackInterval = 0.001f;
                    plant.thePlantHealth = 999999;
                    plant.thePlantMaxHealth = 999999;
                    plant.thePlantSpeed = 1f;
                    //__instance.TakeDamage(1, damageType);
                    __instance.GetComponent<Plant>().exchangeSpeed = 10;
                    __instance.GetComponent<Plant>().attackDamage = 999999;
                    __instance.GetComponent<Plant>().thePlantMaxHealth = 999999;
                    __instance.GetComponent<GloveMgr>().CD = 999999;
                    __instance.GetComponent<Plant>().regeneration = true;
                    __instance.GetComponent<Plant>().thePlantAttackCountDown = 0.001f;
                    __instance.GetComponent<Plant>().thePlantAttackInterval = 0.001f;
                    __instance.GetComponent<SuperSnowGatling>().attributeCountdown = 0.001f;

                    MelonLogger.Msg("SuperSnowGatling applied!");
                }
                else
                {
                    MelonLogger.Msg("Plant or Shooter not found!");
                }
            }
        }
#endif
    }


}
