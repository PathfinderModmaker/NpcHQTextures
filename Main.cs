﻿using UnityModManagerNet;
using System;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker;
using UnityEngine;
using System.IO;
using Harmony12;
using System.Collections.Generic;
using System.Reflection.Emit;
using Kingmaker.View;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Customization;
using Kingmaker.ResourceLinks;

namespace NpcHQTextures
{
#if DEBUG
    [EnableReloading]
#endif
    public class Main
    {

        public static Dictionary<string, Vector2Int> texOnDiskInfoList;



        public static string hqTexPath = Path.Combine(UnityModManager.modsPath, "NpcHQTextures","HQTex");
            //Path.Combine(Directory.GetParent(Application.dataPath).FullName, "Mods","NpcHQTextures","HQTex");


        public static Dictionary<string, string> customPrefabUnits;
        public static Dictionary<UnitViewLink, UnitCustomizationVariation> allVariations;



        public static bool notRandom = false;
        public static void Init()
        {








            //Main.DebugLog($"Prefabs / BlueprintUnits: {i.ToString()}/{blueprintUnits.Count().ToString()}");



            Main.texOnDiskInfoList = new Dictionary<string, Vector2Int>();

            string hqDir = Main.hqTexPath;

            List<string> texturePathList = Directory.GetFiles(hqDir, "*.png", SearchOption.AllDirectories).Where(d => !d.Contains(@"/-/") && !d.Contains(@"\-\") && !d.Contains(@"014x")).ToList();

          //  Main.DebugLog(texturePathList.Count().ToString());



            texturePathList.ForEach(x => Main.texOnDiskInfoList.Add(x, ImageHeader.GetDimensions(x)));

            Main.customPrefabUnits = new Dictionary<string, string>();
            Main.allVariations = new Dictionary<UnitViewLink, UnitCustomizationVariation>();


            List<UnitCustomizationPreset> presets = ResourcesLibrary.GetBlueprints<UnitCustomizationPreset>().ToList();


            foreach (UnitCustomizationPreset preset in presets)
            {

                foreach (BlueprintUnit blueprintUnit in preset.Units)
                {

                    if (!Main.customPrefabUnits.ContainsKey(blueprintUnit.name))
                    {
                        Main.customPrefabUnits.Add(blueprintUnit.name, preset.AssetGuid);
                        // Main.DebugLog(blueprintUnit.name);
                    }


                }

                
                foreach (UnitVariations uvs in preset.UnitVariations)
                {
                    foreach (UnitCustomizationVariation ucv in uvs.Variations)
                    {
                        if (!allVariations.ContainsKey(ucv.Prefab))
                        {
                            allVariations.Add(ucv.Prefab, ucv);
                 

                        }
                        else
                        {
                            Main.DebugLog("NOOOO!!!!!!!");
                        }
                    }
                }
                

            }

            
            List<BlueprintUnit> blueprintUnits = ResourcesLibrary.GetBlueprints<BlueprintUnit>().ToList();

            int i = 0;
            foreach (BlueprintUnit blueprintUnit1 in blueprintUnits)
            {


                if (blueprintUnit1.CustomizationPreset != null && !allVariations.ContainsKey(blueprintUnit1.Prefab))
                {
                    UnitEntityView unitEntityView = ResourcesLibrary.TryGetResource<UnitEntityView>(blueprintUnit1.Prefab.AssetId, false);

                    if (unitEntityView != null && unitEntityView.GetComponentsInChildren<SkinnedMeshRenderer>().Count() > 0)
                    {

                        foreach (SkinnedMeshRenderer smr in unitEntityView.GetComponentsInChildren<SkinnedMeshRenderer>())
                        {

                            if (smr.material != null && !string.IsNullOrEmpty(smr.material?.mainTexture?.name))
                            {
                                if (smr.material.mainTexture.name.StartsWith("0"))
                                {
                                    Main.DebugLog("total: "+smr.material.mainTexture.name);

                                   // saveTex(ensureUniqueFileName(Path.Combine(Main.hqTexPath, "-", blueprintUnit1.name +" - "+smr.material.mainTexture.name+".png")), smr.material.mainTexture as Texture2D);
                                }

                            }
                        }
                    }
                }

            }

            
        }

            /*
        public static string ensureUniqueFileName(string path)
        {
            if (File.Exists(path))
            {
                int ix = 0;
                string fileNamePathStub = Path.Combine(Directory.GetParent(path).FullName, Path.GetFileNameWithoutExtension(path) + "_");
                string fileNamePath = null;
                do
                {
                    ix++;
                    fileNamePath = String.Format("{0}{1}{2}", fileNamePathStub, ix, ".png");
                } while (File.Exists(fileNamePath));
                return fileNamePath;
            }
            return path;
        }
        private static void saveTex(string fullpath, Texture2D texture)
        {
        

            if (texture != null)
            {

                byte[] bytes = duplicateTexture(texture).EncodeToPNG();

                //   ((line = reader.ReadLine()) != null)



                File.WriteAllBytes(fullpath, bytes);
                Main.DebugLog("Disk, The file was created successfully at " + fullpath);
            }
            else
            {
                Main.DebugLog("Disk, not created: " + fullpath);
            }
        }


        private static Texture2D duplicateTexture(Texture2D source)
        {


            try
            {
                RenderTexture renderTex = RenderTexture.GetTemporary(
                            source.width,
                            source.height,
                            0,
                            RenderTextureFormat.Default,
                            RenderTextureReadWrite.Linear);

                Graphics.Blit(source, renderTex);
                RenderTexture previous = RenderTexture.active;
                RenderTexture.active = renderTex;

                Texture2D readableText = new Texture2D(source.width, source.height);
                readableText.ReadPixels(new Rect(0, 0, renderTex.width, renderTex.height), 0, 0);
                readableText.Apply();
                RenderTexture.active = previous;
                RenderTexture.ReleaseTemporary(renderTex);
                return readableText;
            }
            catch (Exception e)
            {

                Main.DebugLog("dupliactor error: " + e);
            }
            return null;
        }
        */
        public static string OrigTexName = "";

        public static Texture2D ReadableText;

        public static UnitCustomizationVariation preset;

        public static UnitEntityView unitEntityViewTexReplacer(UnitEntityView unitEntityView, string texfullpath, string origtexname)
        {


            if(texfullpath == "x" || origtexname == "x")
            {
                return unitEntityView;
            };

            //  Main.DebugLog("a");

            if (unitEntityView != null && unitEntityView.GetComponentsInChildren<SkinnedMeshRenderer>().Count() > 0)
            {
                Main.DebugLog("unitEntityViewTexReplacer: unitEntityView has SkinnedMeshRenderer");


                    if (string.IsNullOrEmpty(origtexname))
                {
                    foreach (SkinnedMeshRenderer smr in unitEntityView.GetComponentsInChildren<SkinnedMeshRenderer>())
                    {

                        if (smr.material != null && !string.IsNullOrEmpty(smr.material?.mainTexture?.name))
                        {
                            //Main.DebugLog("c");

                             if (Main.texOnDiskInfoList == null || Main.texOnDiskInfoList.Count() == 0)
                             {
                                 Main.Init();
                             }

                            Main.DebugLog("unitEntityViewTexReplacer smr: " + smr.material?.mainTexture?.name);

                            if (Main.texOnDiskInfoList.Keys.Any(key => Path.GetFileNameWithoutExtension(key).Equals(smr.material.mainTexture.name) ? true : key.Contains(smr.material.mainTexture.name)))
                            {




                                if (string.IsNullOrEmpty(texfullpath))
                                {

                                    bool found = false;
                                    foreach (string path in Main.texOnDiskInfoList.Keys)
                                    {
                                        if (Path.GetFileNameWithoutExtension(path).Equals(smr.material.mainTexture.name))
                                        {
                                            texfullpath = path;
                                            found = true;
                                            break;
                                        }
                                    }
                                    if (!found)
                                    {
                                        foreach (string path in Main.texOnDiskInfoList.Keys)
                                        {
                                            if (Path.GetFileNameWithoutExtension(path).Contains(smr.material.mainTexture.name))
                                            {
                                                texfullpath = path;
                                                break;
                                            }

                                        }
                                    }


                                    //texfullpath = Main.texOnDiskInfoList.Keys.ToList().Find(key => Path.GetFileNameWithoutExtension(key).Equals(smr.material?.mainTexture?.name) ? Path.GetFileNameWithoutExtension(key).Equals(smr.material?.mainTexture?.name) : key.Contains(smr.material.mainTexture.name));

                                }

                                origtexname = smr.material.mainTexture.name;


                                Main.DebugLog("unitEntityViewTexReplacer smr: " + texfullpath);
                                //Main.DebugLog("unitEntityViewTexReplacer smr: " + origtexname);

                                break;
                            }
                        }
                    }
                }
                
               else  if (string.IsNullOrEmpty(texfullpath))
                {
                    bool found = false;
                    foreach (string path in Main.texOnDiskInfoList.Keys)
                    {
                        if (Path.GetFileNameWithoutExtension(path).Equals(origtexname))
                        {
                            texfullpath = path;
                            found = true;
                            break;
                        }
                    }
                    if (!found)
                    {
                        foreach (string path in Main.texOnDiskInfoList.Keys)
                        {
                            if (Path.GetFileNameWithoutExtension(path).Contains(origtexname))
                            {
                                texfullpath = path;
                                break;
                            }

                        }
                    }

                    Main.DebugLog("unitEntityViewTexReplacer smr 2: " + texfullpath);
                    Main.DebugLog("unitEntityViewTexReplacer smr 2: " + origtexname);

                }

                string tname = "noname";

                string anyInMesh = "false";

                Main.DebugLog("a");



                if (!string.IsNullOrEmpty(texfullpath) && !string.IsNullOrEmpty(origtexname))
                {
                 //   Main.OrigTexName = origtexname;

                      Main.DebugLog("b");

                    try
                    {

                        byte[] array = File.ReadAllBytes(texfullpath);



                        Vector2Int v2 = Main.texOnDiskInfoList[texfullpath];


                        Texture2D texture2D = new Texture2D(v2.x, v2.y, TextureFormat.ARGB32, false);

                        texture2D.filterMode = FilterMode.Point;


                        texture2D.anisoLevel = 9;
                        ImageConversion.LoadImage(texture2D, array);


                        RenderTexture renderTex = RenderTexture.GetTemporary(
                                             texture2D.width,
                                             texture2D.height,
                                             32,
                                             RenderTextureFormat.ARGB32,
                                             RenderTextureReadWrite.sRGB);

                        renderTex.antiAliasing = 8;
                        renderTex.anisoLevel = 9;
                        renderTex.filterMode = FilterMode.Trilinear;
                        Graphics.Blit(texture2D, renderTex);
                        RenderTexture previous = RenderTexture.active;
                        RenderTexture.active = renderTex;

                        Texture2D readableText = new Texture2D(texture2D.width, texture2D.height);
                        readableText.ReadPixels(new Rect(0, 0, renderTex.width, renderTex.height), 0, 0);
                        readableText.Apply();
                        RenderTexture.active = previous;
                        RenderTexture.ReleaseTemporary(renderTex);


                        if (texture2D != null)
                        {
                           // Main.ReadableText = readableText;
                            Main.DebugLog("c");

                            //   if (unitEntityView.CharacterAvatar != null)
                            //   {

                            //       Main.DebugLog("charav!");
                            //   }
                            //    else { Main.DebugLog("NO charav!"); }


                            if (unitEntityView.GetComponentsInChildren<SkinnedMeshRenderer>().Any(x => (tname = x.material?.mainTexture?.name) == origtexname))
                            {
                                // string tname2 = "stillno";
                                anyInMesh = "true";


                                foreach (SkinnedMeshRenderer smr in unitEntityView.GetComponentsInChildren<SkinnedMeshRenderer>())
                                {
                                    if (smr.material?.mainTexture?.name == origtexname)
                                    {

                                        smr.material.mainTexture = readableText;
                                        smr.material.mainTexture.name = origtexname;
                                    }

                                }

                                Main.DebugLog("wtf: (" + origtexname + " - " + tname + " - " + anyInMesh + ") ");
                                Main.DebugLog(texfullpath);
                                
                                //   unitEntityView.GetComponentsInChildren<SkinnedMeshRenderer>().First(x => (tname = x.material.mainTexture.name) == origtexname).material.mainTexture = readableText;
                                // Main.DebugLog(tname2);
                            }
                            else
                            {
                                Main.DebugLog("wtf: (" + origtexname + " - " + tname + " - " + anyInMesh + ") ");
                            }


                        }
                    }
                    catch (Exception x) { Main.DebugLog("Caught: (" + origtexname + " - " + tname + " - " + anyInMesh + ") " + x.ToString()); }
                }

            }
            else
            {
                Main.DebugLog("unitEntityView texreplacer has NO SkinnedMeshRenderer");

            }



            return unitEntityView;
        }

        public static Tuple<string, string> randomPool(BlueprintUnit blueprintUnit, UnitEntityView unitEntityView)
        {

            

            string presetName = "";
            UnitCustomizationPreset preset = null;


            Main.DebugLog("1");

            
            switch (blueprintUnit.name)
           {

                
                case "BanditAdmirersShortie":
                    return Tuple.Create(Path.Combine(Main.hqTexPath, "RandomPool", "Bandits_CustomizationPreset", "3", "015_Halfling_Male_Diffuse_Atlas.png"), "015_Halfling_Male_Diffuse_Atlas");
                case "BaronationGuest":
                    return Tuple.Create(Path.Combine(Main.hqTexPath, "RandomPool", "Noble_CustomizationPreset", "1", "002_Human_Male_Diffuse_Atlas.png"), "002_Human_Male_Diffuse_Atlas");
                case "BaronationGuest_6":
                    return Tuple.Create(Path.Combine(Main.hqTexPath, "RandomPool", "Noble_CustomizationPreset", "4", "016_Halfling_Female_Diffuse_Atlas.png"), "016_Halfling_Female_Diffuse_Atlas");
                case "BossGodEmperor":
                    return Tuple.Create(Path.Combine(Main.hqTexPath, "RandomPool", "BanditsCaster_CustomizationPreset", "1", "009_Gnome_Male_Diffuse_Atlas.png"), "009_Gnome_Male_Diffuse_Atlas");
                case "C51 - LinziQ3_Firebrand":
                    return Tuple.Create(Path.Combine(Main.hqTexPath, "RandomPool", "Bandits_CustomizationPreset 1", "4", "013_Human_Female_Diffuse_Atlas.png"), "013_Human_Female_Diffuse_Atlas");
                case "CR1_SpecialGnomeBanditFighterMelee_ThornRiverCamp":
                    return Tuple.Create(Path.Combine(Main.hqTexPath, "RandomPool", "Bandits_CustomizationPreset 1", "2", "027_Gnome_Male_Diffuse_Atlas.png"), "027_Gnome_Male_Diffuse_Atlas");
                case "DunswardVillageTrader":
                    return Tuple.Create(Path.Combine(Main.hqTexPath, "RandomPool", "Commoner_CustomizationPreset", "1", "004_Human_Male_Diffuse_Atlas.png"), "004_Human_Male_Diffuse_Atlas");
                case "GlenebonVillageTrader":
                    return Tuple.Create(Path.Combine(Main.hqTexPath, "RandomPool", "Commoner_CustomizationPreset", "1", "009_Human_Male_Diffuse_Atlas.png"), "009_Human_Male_Diffuse_Atlas");
                case "Priest_Ghost":
                    return Tuple.Create(Path.Combine(Main.hqTexPath, "RandomPool", "Commoner_CustomizationPreset", "1", "008_Human_Male_Diffuse_Atlas.png"), "008_Human_Male_Diffuse_Atlas");
                case "SilverstepVillageTrader":
                    return Tuple.Create(Path.Combine(Main.hqTexPath, "RandomPool", "Commoner_CustomizationPreset", "1", "011_Human_Female_Diffuse_Atlas.png"), "011_Human_Female_Diffuse_Atlas");
                case "StartingTrader":
                    return Tuple.Create(Path.Combine(Main.hqTexPath, "RandomPool", "Commoner_CustomizationPreset", "1", "002_Human_Male_Diffuse_Atlas.png"), "002_Human_Male_Diffuse_Atlas");
                case "MivonGuest":
                    return Tuple.Create(Path.Combine(Main.hqTexPath, "RandomPool", "Noble_CustomizationPreset", "3", "001_Human_Male_Diffuse_Atlas.png"), "001_Human_Male_Diffuse_Atlas");
                case "MivonGuest 1":
                    return Tuple.Create(Path.Combine(Main.hqTexPath, "RandomPool", "Noble_CustomizationPreset", "2", "001_Human_Male_Diffuse_Atlas.png"), "001_Human_Male_Diffuse_Atlas");
                case "PitaxTown_Trader 1":
                    return Tuple.Create(Path.Combine(Main.hqTexPath, "RandomPool", "Noble_CustomizationPreset", "4", "015_Halfling_Male_Diffuse_Atlas.png"), "015_Halfling_Male_Diffuse_Atlas");
                case "Poisoner":
                    return Tuple.Create(Path.Combine(Main.hqTexPath, "RandomPool", "Bandits_CustomizationPreset 1", "5", "035_Halfling_Male_Diffuse_Atlas.png"), "035_Halfling_Male_Diffuse_Atlas");


                case "BaronationGuest_2":
                    //004_Human_Female_Diffuse_Atlas (square shapes leather wizard)
                    return Tuple.Create("x", "x");
                case "BaronationGuest_5":
                    //035_HalfOrc_Male_Diffuse_Atlas (red-blue stripes "merchant")
                    return Tuple.Create("x", "x");
                case "BaronationGuest_7":
                    //015_Human_Female_Diffuse_Atlas (yellow robe)
                    return Tuple.Create("x", "x");
                case "BaronationGuest_9":
                    //007_Human_Male_Diffuse_Atlas (red-blue stripes "merchant")
                    return Tuple.Create("x", "x");
                case "BaronationGuest_11":
                    //013_HalfOrc_Male_Diffuse_Atlas (square shapes leather wizard)
                    return Tuple.Create("x", "x");
                case "BossFallenPriest":
                    //011_Human_Female_Diffuse_Atlas (plate armor wizard)
                    return Tuple.Create("x", "x");
                case "BossSlaver":
                    //003_Human_Male_Diffuse_Atlas (red robe wizard)
                    return Tuple.Create("x", "x");
                case "KamelandsVillageTrader":
                    //009_Gnome_Male_Diffuse_Atlas (green shirt commoner)
                    return Tuple.Create("x", "x");
                case "OutskirtsVillageTrader":
                    //011_Dwarf_Male_Diffuse_Atlas (white shirt commoner)
                    return Tuple.Create("x", "x");
                case "PitaxTown_Trader 2":
                    //043_Aasimar_Male_Diffuse_Atlas (gold blue stripes noble)
                    return Tuple.Create("x", "x");
                case "PitaxTown_Trader 3":
                    //019_Dwarf_Male_Diffuse_Atlas (red robe wizard)
                    return Tuple.Create("x", "x");
                case "SouthNarlmarchesVillageTrader":
                    //006_Dwarf_Female_Diffuse_Atlas (square shapes leather wizard)
                    return Tuple.Create("x", "x");





            }

            Main.DebugLog("2");

            if (blueprintUnit.CustomizationPreset != null)
            {
                presetName = blueprintUnit.CustomizationPreset.name;

                preset = blueprintUnit.CustomizationPreset;


                Main.DebugLog($"orig blueprintUnit.CustomizationPreset.name: {presetName}");
            }
            else if (Main.customPrefabUnits.ContainsKey(blueprintUnit.name))
            {

                preset = ResourcesLibrary.TryGetBlueprint<UnitCustomizationPreset>(Main.customPrefabUnits[blueprintUnit.name]);

                presetName = preset.name;

                Main.DebugLog($"customprefab only in a preset: {presetName}");
            }
            else
            {

                if (Main.allVariations == null || Main.allVariations.Count() == 0)
                {
                    Main.Init();
                }

                //if (Main.notRandom)
                if (!Main.allVariations.Keys.ToList().Any(x => x.Load() == unitEntityView) && !Main.allVariations.ContainsKey(blueprintUnit.Prefab))
                {
                    Main.DebugLog("not random in any lowest level prefab even");
                    return Tuple.Create("", "");

                }



            }


            if (Main.allVariations == null || Main.allVariations.Count() == 0)
            {
                Main.Init();
            }


            Main.DebugLog("3");


            Main.DebugLog("randomPool() -  " + blueprintUnit.CharacterName + " - " + blueprintUnit.name + " - " + blueprintUnit.AssetGuid);

            string path2 = Path.Combine(Main.hqTexPath, "RandomPool");

            //Main.DebugLog($"path2: {path2}");

            // string texfullpath = "";







            BlueprintUnit unit = blueprintUnit;



         //   BlueprintUnit protoType = blueprintUnit.PrototypeLink as BlueprintUnit;



            string fileName = "";

            /*     if (string.IsNullOrEmpty(customPrefabGuid))
                 {


                     //customPrefabGuid = protoType.Prefab.AssetId;
                     customPrefabGuid = unit.Prefab.AssetId;
                 }*/
            //   Main.DebugLog($"customPrefabGuid: {customPrefabGuid}");


            /*  if (unitEntityView == null)
              {
                  unitEntityView = ResourcesLibrary.TryGetResource<UnitEntityView>(protoType.Prefab.AssetId, false);
              }*/


            //unitEntityView = ResourcesLibrary.TryGetResource<UnitEntityView>(unitEntityView.EntityData.Descriptor.Blueprint.Prefab.AssetId, false);



            if (unitEntityView != null && unitEntityView.GetComponentsInChildren<SkinnedMeshRenderer>().Count() > 0)
            {

                foreach (SkinnedMeshRenderer smr in unitEntityView.GetComponentsInChildren<SkinnedMeshRenderer>())

                {

                    if (smr.material != null && !string.IsNullOrEmpty(smr.material?.mainTexture?.name))
                    {

                        //  Main.DebugLog($"smr.material.mainTexture.name: {smr.material.mainTexture.name}");
                        // Main.DebugLog($"texOnDiskInfoList.Count(): {Main.texOnDiskInfoList.Count().ToString()}");
                        if (Main.texOnDiskInfoList == null || Main.texOnDiskInfoList.Count() == 0)
                          {
                              Main.Init();
                          }

                        bool found = false;
                        foreach (string path in Main.texOnDiskInfoList.Keys)
                        {
                            if (Path.GetFileNameWithoutExtension(path).Equals(smr.material.mainTexture.name))
                            {

                                fileName = smr.material.mainTexture.name;
                                Main.DebugLog($"smr/fileName: {fileName}");
                                found = true;
                                break;
                            }
                        }
                        if (!found)
                        {
                            foreach (string path in Main.texOnDiskInfoList.Keys)
                            {
                                if (Path.GetFileNameWithoutExtension(path).Contains(smr.material.mainTexture.name))
                                {

                                    fileName = smr.material.mainTexture.name;
                                    Main.DebugLog($"smr/fileName: {fileName}");

                                    break;
                                }

                            }
                        }
                        if (found) { break; }

                    }
                }
            }

            Main.DebugLog("4");

            int pf = 0;
            bool stop = false;
            if (preset != null)
            {
                foreach (UnitVariations uvs in preset.UnitVariations)
                {

                    pf++;

                    foreach (UnitCustomizationVariation ucv in uvs.Variations)
                    {


                        // || ucv.Prefab.Load().Blueprint.AssetGuid == unitEntityView.Blueprint.AssetGuid)
                        if (/*ucv.Prefab == Main.preset?.Prefab ||*/ ucv.Prefab.Load() == unitEntityView || ucv.Prefab == blueprintUnit.Prefab)
                        {
                            Main.DebugLog($"preset.name: {preset.name}");
                            Main.DebugLog($"UnitVariations index: {pf}");
                            Main.DebugLog(ucv.Prefab.Load().name);
                            stop = true;
                            break;

                        }
                    }
                    if (stop) { break; }

                }
            }
            Main.DebugLog("5");

            if (!stop)
            {

                List<UnitCustomizationPreset> presets = ResourcesLibrary.GetBlueprints<UnitCustomizationPreset>().ToList();

                foreach (UnitCustomizationPreset preset2 in presets.Where(x => !x.name.Equals(preset?.name)))
                {

                    //unitCustomizationVariation = preset.SelectVariation(unit, null);

                    // if (unitCustomizationVariation != null)
                    // {
                    //   break;
                    // }
                    pf = 0;
                    foreach (UnitVariations uvs in preset2.UnitVariations)
                    {
                        pf++;
                        foreach (UnitCustomizationVariation ucv in uvs.Variations)
                        {

                            if (/*ucv.Prefab == Main.preset?.Prefab || */ucv.Prefab.Load() == unitEntityView || ucv.Prefab == blueprintUnit.Prefab)
                            {
                                //newpreset = preset2;
                                presetName = preset2.name;
                                Main.DebugLog($"preset2.name: {preset2.name}");
                                Main.DebugLog($"UnitVariations index: {pf}");
                                Main.DebugLog(ucv.Prefab.Load().name);

                                /*if (Main.allVariations.ContainsKey(unit.Prefab))
                                {
                                    //ucv.Prefab == blueprintUnit.Prefab
                                    Main.preset = Main.allVariations[unit.Prefab];
                                    //                        Main.DebugLog(Main.preset.Prefab.AssetId);


                                }*/

/*
                                switch (pf)
                                {

                                    case 1:
                                        if(preset.name == "Bandits_CustomizationPreset" && preset2.name == "Bandits_CustomizationPreset 1")
                                        {
                                            pf = 1;
                                        }
                                        else if (preset2.name == "Bandits_CustomizationPreset 1") { pf = 4; };
                                        break;
                                    case 2:
                                        if (preset2.name == "Bandits_CustomizationPreset 1") { pf = 4; };
                                        break;
                                    case 3:
                                        if (preset2.name == "Bandits_CustomizationPreset 1") { pf = 4; };
                                        break;
                                    case 4:
                                        if (preset2.name == "Bandits_CustomizationPreset 1") { pf = 4; };
                                        break;
                                    case 5:
                                        if (preset2.name == "Bandits_CustomizationPreset 1") { pf = 5; };
                                        break;
                                    case 6:
                                        if (preset2.name == "Bandits_CustomizationPreset 1") { pf = 6; };
                                        break;
                                }*/
                                stop = true;
                                break;

                            }
                        }
                        if (stop) { break; }

                    }
                    if (stop) { break; }
                }



            }

            Main.DebugLog("6");

            if (!stop)
            {

                Main.DebugLog("not random");
                return Tuple.Create("", "");

            }

            string fileFullPath = "";

            //DirectoryInfo variationdir = new DirectoryInfo(Path.Combine(presetName, pf.ToString()));




            List<string> files = Directory.GetFiles(Path.Combine(path2, presetName, pf.ToString())).ToList();

            //   Main.DebugLog($"files in {Path.Combine(path2, presetName, pf.ToString())}: {files.Count().ToString()}");

            Main.DebugLog("7");

            foreach (string path in files)
            {

                //  Main.DebugLog(path);
                // Main.DebugLog(fileName);

                if (path.Contains(fileName))
                {

                    if (Main.texOnDiskInfoList.Keys.Any(key => key.Equals(path)))
                    {
                        fileFullPath = Main.texOnDiskInfoList.Keys.First(key => key.Equals(path));
                    }

                    Main.DebugLog($"fileFullPath: {fileFullPath}");
                    //return fileFullPath;
                    return Tuple.Create(fileFullPath, fileName);
                }

            }
            fileFullPath = "";

            Main.DebugLog("8");

            //string dirInPreset = Path.Combine(presetName, pf.ToString(), fileName);

            //string fileFullPath = Path.Combine(path2, dirInPreset + ".png");

            return Tuple.Create(fileFullPath, fileName);
        }

        public static UnitEntityView unitEntityViewTexReplacer_old(UnitEntityView unitEntityView, string texfullpath)
        {
            

            string origtexname = "";
      

            if (unitEntityView != null && unitEntityView.GetComponentsInChildren<SkinnedMeshRenderer>().Count() > 0)
            {

                foreach (SkinnedMeshRenderer smr in unitEntityView.GetComponentsInChildren<SkinnedMeshRenderer>())

                {

                    if (smr.material != null && !string.IsNullOrEmpty(smr.material?.mainTexture?.name))
                    {

                        if (Main.texOnDiskInfoList == null || Main.texOnDiskInfoList.Count() == 0)
                        {
                            Main.Init();
                        }


                        if (string.IsNullOrEmpty(texfullpath))
                        {

                            bool found = false;
                            foreach (string path in Main.texOnDiskInfoList.Keys)
                            {
                                if (Path.GetFileNameWithoutExtension(path).Equals(smr.material.mainTexture.name))
                                {
                                    texfullpath = path;
                                    origtexname = smr.material.mainTexture.name;
                                    found = true;
                                    break;
                                }
                            }
                            if (!found)
                            {
                                foreach (string path in Main.texOnDiskInfoList.Keys)
                                {
                                    if (Path.GetFileNameWithoutExtension(path).Contains(smr.material.mainTexture.name))
                                    {
                                        texfullpath = path;
                                        origtexname = smr.material.mainTexture.name;
                                        found = true;
                                        break;
                                    }

                                }
                            }
                            if(found) { break; }
                        }

                    }
                }

                string tname = "noname";

                string anyInMesh = "false";

                if (!string.IsNullOrEmpty(texfullpath) && !string.IsNullOrEmpty(origtexname))
                {

                    try
                    {
                        byte[] array = File.ReadAllBytes(texfullpath);



                        Vector2Int v2 = Main.texOnDiskInfoList[texfullpath];


                        Texture2D texture2D = new Texture2D(v2.x, v2.y, TextureFormat.ARGB32, false);

                        texture2D.filterMode = FilterMode.Point;


                        texture2D.anisoLevel = 9;
                        ImageConversion.LoadImage(texture2D, array);


                        RenderTexture renderTex = RenderTexture.GetTemporary(
                                             texture2D.width,
                                             texture2D.height,
                                             32,
                                             RenderTextureFormat.ARGB32,
                                             RenderTextureReadWrite.sRGB);

                        renderTex.antiAliasing = 8;
                        renderTex.anisoLevel = 9;
                        renderTex.filterMode = FilterMode.Trilinear;
                        Graphics.Blit(texture2D, renderTex);
                        RenderTexture previous = RenderTexture.active;
                        RenderTexture.active = renderTex;

                        Texture2D readableText = new Texture2D(texture2D.width, texture2D.height);
                        readableText.ReadPixels(new Rect(0, 0, renderTex.width, renderTex.height), 0, 0);
                        readableText.Apply();
                        RenderTexture.active = previous;
                        RenderTexture.ReleaseTemporary(renderTex);


                        if (texture2D != null)
                        {


                            

                                if (unitEntityView.GetComponentsInChildren<SkinnedMeshRenderer>().Any(x => (tname = x.material?.mainTexture?.name) == origtexname))
                            {
                               // string tname2 = "stillno";
                                anyInMesh = "true";

                                
                                foreach (SkinnedMeshRenderer smr in unitEntityView.GetComponentsInChildren<SkinnedMeshRenderer>())
                                {
                                    if(smr.material?.mainTexture?.name == origtexname)
                                    {
                                        smr.material.mainTexture = readableText;
                                    }

                                }
                                unitEntityView.GetComponentsInChildren<SkinnedMeshRenderer>().First(x => (tname = x.material.mainTexture.name) == origtexname).material.mainTexture = readableText;

                                //
                                // Main.DebugLog(tname2);
                            }

                        }
                    }
                    catch (Exception x) { Main.DebugLog("Caught: (" + origtexname + " - " + tname + " - " + anyInMesh+") " + x.ToString()); }
                }

            }



            return unitEntityView;
        }
        public static string randomPool_old(BlueprintUnit blueprintUnit, string customPrefabGuid)
        {

            string path2 = Path.Combine(Main.hqTexPath,"RandomPool");

            // string texfullpath = "";

        
            string presetName = "";
            if (blueprintUnit.CustomizationPreset != null)
            {
                presetName = blueprintUnit.CustomizationPreset.name;
            }
            else
            {
                return "";
            }

  


            UnitCustomizationPreset preset = blueprintUnit.CustomizationPreset;

          
            BlueprintUnit unit = blueprintUnit;



            BlueprintUnit protoType = blueprintUnit.PrototypeLink as BlueprintUnit;


            int pf = 0;
            string fileName = "";

            if (string.IsNullOrEmpty(customPrefabGuid))
            {
       

                customPrefabGuid = protoType.Prefab.AssetId;
            }

   
            UnitEntityView unitEntityView = ResourcesLibrary.TryGetResource<UnitEntityView>(customPrefabGuid, false);

            if (unitEntityView != null && unitEntityView.GetComponentsInChildren<SkinnedMeshRenderer>().Count() > 0)
            {

                foreach (SkinnedMeshRenderer smr in unitEntityView.GetComponentsInChildren<SkinnedMeshRenderer>())

                {

                    if (smr.material != null && !string.IsNullOrEmpty(smr.material?.mainTexture?.name))
                    {

                        if (Main.texOnDiskInfoList == null || Main.texOnDiskInfoList.Count() == 0)
                        {
                            Main.Init();
                        }

                        bool found = false;
                        foreach (string path in Main.texOnDiskInfoList.Keys)
                        {
                            if (Path.GetFileNameWithoutExtension(path).Equals(smr.material.mainTexture.name))
                            {

                                fileName = smr.material.mainTexture.name;
                                Main.DebugLog($"smr/fileName: {fileName}");
                                found = true;
                                break;
                            }
                        }
                        if (!found)
                        {
                            foreach (string path in Main.texOnDiskInfoList.Keys)
                            {
                                if (Path.GetFileNameWithoutExtension(path).Contains(smr.material.mainTexture.name))
                                {

                                    fileName = smr.material.mainTexture.name;
                                    Main.DebugLog($"smr/fileName: {fileName}");

                                    break;
                                }

                            }
                        }
                        if (found) { break; }
                    }
                }
            }

 

            if (preset.Units.Contains(protoType) || preset.Units.Contains(unit))
            {

                foreach (UnitVariations uvs in preset.UnitVariations)
                {
                    pf++;
                    if (uvs.Units.Contains(protoType) || uvs.Units.Contains(unit))
                    {

                        if (preset.AssetGuid == "349505290e20b4441b3c25bf9c58bf3d")
                        {
                            break;
                        }
                        else if (preset.AssetGuid == "a8351a67ec06e9244a318a488ca15151")
                        {
                            bool flag33 = false;
                            foreach (UnitCustomizationVariation ucv in uvs.Variations)
                            {

                                if (ucv.Prefab.Load().name.Contains(fileName))
                                {
                                    flag33 = true;
                                    break;
                                }
                            }
                            if (!flag33)
                            {
                                preset = ResourcesLibrary.TryGetBlueprint<UnitCustomizationPreset>("349505290e20b4441b3c25bf9c58bf3d");
                                presetName = preset.name;
                            }


                            break;
                        }
                        break;
                    }

                }
            }
            else if (preset.AssetGuid == "a8351a67ec06e9244a318a488ca15151")
            {
                preset = ResourcesLibrary.TryGetBlueprint<UnitCustomizationPreset>("349505290e20b4441b3c25bf9c58bf3d");

                presetName = preset.name;

                pf = 1;
                bool flag = false;

           /*     if (unit.PrototypeLink.name == "CR0_BanditWarriorMeleeLevel1")
                {
                    pf = 1;
                    flag = true;

                }
                if (unit.PrototypeLink.name == "CR0_BanditWarriorRangedLevel1")
                {
                    pf = 1;
                    flag = true;

                }
                if (unit.PrototypeLink.name == "CR0_BanditWarriorRangedLevel2")
                {
                    pf = 1;
                    flag = true;

                }
                if (unit.PrototypeLink.name == "CR1_BanditWarriorRangedLevel3")
                {
                    pf = 1;
                    flag = true;

                }
                if (unit.PrototypeLink.name == "CR2_BanditWarriorRangedLevel4")
                {
                    pf = 1;
                    flag = true;

                }
                */



               int unitCountInVariation = preset.UnitVariations.Find(x => x.Units.ToList().Find(y => y.name == unit.PrototypeLink.name) ).Units.Count();

                if (unitCountInVariation == 6 && unit.PrototypeLink.name.Contains("Barbarian"))
                {
                    unitCountInVariation = 7;
                }
                switch (unitCountInVariation)
                {

                    case 5:
                        pf = 1;
                        flag = true;
                        break;
                    case 16:
                        pf = 3;
                        flag = true;
                        break;
                    case 6:
                        break;
                    case 25:
                        pf = 4;
                        flag = true;
                        break;
                    case 12:
                        break;
                    case 7:
                        break;


                }
/*
                if (unit.PrototypeLink.name == "CR2_BanditRogueRangedLevel3")
                {
                    pf = 4;
                    flag = true;

                }

                if (unit.PrototypeLink.name == "CR3_BanditRogueRangedLevel4")
                {
                    pf = 4;
                    flag = true;

                }
                if (unit.PrototypeLink.name == "CR3_BanditRogueMeleeLevel4")
                {
                    pf = 4;
                    flag = true;

                }
                if (unit.PrototypeLink.name == "CR4_BanditRogueRangedLevel5")
                {
                    pf = 4;
                    flag = true;

                }
                if (unit.PrototypeLink.name == "CR4_BanditRogueMeleeLevel5")
                {
                    pf = 4;
                    flag = true;

                }
                if (unit.PrototypeLink.name == "CR5_BanditRogueRangedLevel6")
                {
                    pf = 4;
                    flag = true;

                }
                if (unit.PrototypeLink.name == "CR5_BanditRogueMeleeLevel6")
                {
                    pf = 4;
                    flag = true;

                }
                if (unit.PrototypeLink.name == "CR6_BanditRogueRangedLevel7")
                {
                    pf = 4;
                    flag = true;

                }
                if (unit.PrototypeLink.name == "CR6_BanditRogueMeleeLevel7")
                {
                    pf = 4;
                    flag = true;

                }
                if (unit.PrototypeLink.name == "CR7_BanditAlchemistLevel8")
                {
                    //preset2 = "Bandit_Alchemist";
                    pf = 4;
                    flag = true;

                }



                if (unit.PrototypeLink.name == "CR2_BanditWarriorMeleeLevel4")
                {
                    pf = 3;
                    flag = true;

                }

                if (unit.PrototypeLink.name == "CR3_BanditFighterMeleeLevel4")
                {
                    pf = 3;
                    flag = true;
                }
                if (unit.PrototypeLink.name == "CR3_BanditFighterRangedLevel4")
                {
                    pf = 3;
                    flag = true;
                }
                if (unit.PrototypeLink.name == "CR4_BanditFighterMeleeLevel5")
                {
                    pf = 3;
                    flag = true;
                }
                if (unit.PrototypeLink.name == "CR4_BanditFighterRangedLevel5")
                {
                    pf = 3;
                    flag = true;
                }
                if (unit.PrototypeLink.name == "CR5_BanditFighterMeleeLevel6")
                {
                    pf = 3;
                    flag = true;
                }
                if (unit.PrototypeLink.name == "CR5_BanditFighterRangedLevel6")
                {
                    pf = 3;
                    flag = true;
                }
                if (unit.PrototypeLink.name == "CR6_BanditFighterMeleeLevel7")
                {
                    pf = 3;
                    flag = true;
                }
                if (unit.PrototypeLink.name == "CR6_BanditFighterRangedLevel7")
                {
                    pf = 3;
                    flag = true;
                }
                if (unit.PrototypeLink.name == "CR7_BanditFighterMeleeLevel8")
                {
                    pf = 3;
                    flag = true;
                }
                if (unit.PrototypeLink.name == "CR7_BanditFighterRangedLevel8")
                {
                    pf = 3;
                    flag = true;
                }*/
                if (!flag) { Main.DebugLog("add " + unit.PrototypeLink.name + " to bandit0"); }

            }
            else if (preset.AssetGuid == "cc09d457355ef3346ae7103a4d40a718")
            {

                if (unit.PrototypeLink.name == "CR2_BanditNecromancerLevel3")
                {
                    pf = 2;
                }
                if (unit.PrototypeLink.name == "CR6_BanditBardLevel7")
                {
                    pf = 1;
                }

                /*"CR1_BanditConjurerLevel2",
        "CR1_BanditNecromancerLevel2",
        "CR1_BanditTransmuterLevel2",
        "CR1_BanditIllusionistLevel*/


            }
            else if (preset.AssetGuid == "9000d6ca929859848903e2721c1a635c")
            {
                pf = 1;
            }
            else if (preset.AssetGuid == "3272ad72ab8720c4eaa778e56ecdd771")
            {
                if (unit.PrototypeLink.name == "HuntingParty_VandalFighterMelee" || unit.PrototypeLink.name == "Vandal_Fighter")
                {
                    pf = 2;

                }
            }
            else
            {
                Main.DebugLog("variation is not in preset");
            }


            string fileFullPath = "";

            //DirectoryInfo variationdir = new DirectoryInfo(Path.Combine(presetName, pf.ToString()));


            List<string> files = Directory.GetFiles(Path.Combine(path2, presetName, pf.ToString())).ToList();

            foreach (string path in files)
            {
                


                if (path.Contains(fileName))
                {

                    if (Main.texOnDiskInfoList.Keys.Any(key => key.Equals(path)))
                    {
                        fileFullPath = Main.texOnDiskInfoList.Keys.First(key => key.Equals(path));
                    }

           
                    return fileFullPath;
                }
                
            }
            fileFullPath = "";

            //string dirInPreset = Path.Combine(presetName, pf.ToString(), fileName);

            //string fileFullPath = Path.Combine(path2, dirInPreset + ".png");

            return fileFullPath;

        }
        static bool Load(UnityModManager.ModEntry modEntry)
        {


            try
            {
                Main.logger = modEntry.Logger;
                Main.modEnabled = modEntry.Active;
 
              
                modEntry.OnToggle = new Func<UnityModManager.ModEntry, bool, bool>(Main.OnToggle);
              //  modEntry.OnGUI = new Action<UnityModManager.ModEntry>(Main.OnGUI);


                Main.harmonyInstance = HarmonyInstance.Create(modEntry.Info.Id);
                modEntry.OnUnload = new Func<UnityModManager.ModEntry, bool>(Main.Unload);

            }
            catch (Exception ex)
            {
                DebugError(ex);
                throw ex;
            }

            if (!Main.ApplyPatch(typeof(EntityCreationController_SpawnUnit0_Patch), "EntityCreationController_SpawnUnit0_Patch"))
            {
                DebugLog("Failed to patch EntityCreationController_SpawnUnit0_Patch");
            }

            if (!Main.ApplyPatch(typeof(EntityCreationController_SpawnUnit_Patch), "EntityCreationController_SpawnUnit_Patch_Patch"))
            {
                DebugLog("Failed to patch EntityCreationController_SpawnUnit");
            }
             if (!Main.ApplyPatch(typeof(UnitEntityData_CreateView_Patch), "UnitEntityData_CreateView_Patch"))
                 {
                     DebugLog("Failed to patch UnitEntityData_CreateView");
                 }

            if (!Main.ApplyPatch(typeof(LibraryScriptableObject_LoadDictionary_Patch), "Run once startup hook"))
            {
                DebugLog("Failed to patch LibraryScriptableObject_LoadDictionary");
            }


            


            return true;
        }
        private static void OnGUI(UnityModManager.ModEntry modEntry)
        {
            GUILayoutOption[] noExpandwith = new GUILayoutOption[]
             {
                GUILayout.ExpandWidth(false)
             };




            if (Main.okPatches.Count > 0)
            {
                GUILayout.BeginVertical(Array.Empty<GUILayoutOption>());

                GUILayout.Label("<b>OK: Some patches apply:</b>", noExpandwith);

                foreach (string str in Main.okPatches)
                {
                    GUILayout.Label("  • <b>" + str + "</b>", noExpandwith);
                }
                GUILayout.EndVertical();
            }
            if (Main.failedPatches.Count > 0)
            {
                GUILayout.BeginVertical(Array.Empty<GUILayoutOption>());
                GUILayout.Label("<b>Error: Some patches failed to apply. These features may not work:</b>", noExpandwith);
                foreach (string str2 in Main.failedPatches)
                {
                    GUILayout.Label("  • <b>" + str2 + "</b>", noExpandwith);
                }
                GUILayout.EndVertical();
            }
            if (Main.okLoading.Count > 0)
            {
                GUILayout.BeginVertical(Array.Empty<GUILayoutOption>());
                GUILayout.Label("<b>OK: Some assets loaded:</b>", noExpandwith);
                foreach (string str3 in Main.okLoading)
                {
                    GUILayout.Label("  • <b>" + str3 + "</b>", noExpandwith);
                }
                GUILayout.EndVertical();
            }
            if (Main.failedLoading.Count > 0)
            {
                GUILayout.BeginVertical(Array.Empty<GUILayoutOption>());
                GUILayout.Label("<b>Error: Some assets failed to load. Saves using these features won't work:</b>", noExpandwith);
                foreach (string str4 in Main.failedLoading)
                {
                    GUILayout.Label("  • <b>" + str4 + "</b>", noExpandwith);
                }
                GUILayout.EndVertical();
            }
        } 
        internal static bool ApplyPatch(Type type, string featureName)
        {
            bool result;
            try
            {
                if (Main.typesPatched.ContainsKey(type))
                {
                    result = Main.typesPatched[type];
                }
                else
                {
                    List<HarmonyMethod> harmonyMethods = type.GetHarmonyMethods();
                    if (harmonyMethods == null || harmonyMethods.Count<HarmonyMethod>() == 0)
                    {

                        DebugLog("Failed to apply patch " + featureName + ": could not find Harmony attributes.");
                        Main.failedPatches.Add(featureName);
                        Main.typesPatched.Add(type, false);
                        result = false;
                    }
                    else if (new PatchProcessor(Main.harmonyInstance, type, HarmonyMethod.Merge(harmonyMethods)).Patch().FirstOrDefault<DynamicMethod>() == null)
                    {
                        DebugLog("Failed to apply patch " + featureName + ": no dynamic method generated");

                        Main.failedPatches.Add(featureName);
                        Main.typesPatched.Add(type, false);
                        result = false;
                    }
                    else
                    {
                        Main.okPatches.Add(featureName);
                        Main.typesPatched.Add(type, true);
                        result = true;
                    }
                }
            }
            catch (Exception arg)
            {
                DebugLog("Failed to apply patch " + featureName + ": " + arg + ", type: " + type);
                Main.failedPatches.Add(featureName);
                Main.typesPatched.Add(type, false);
                result = false;
            }
            return result;
        }
        internal static void SafeLoad(Action load, string name)
        {
            try
            {
                load();
                Main.okLoading.Add(name);
            }
            catch (Exception e)
            {
                Main.okLoading.Remove(name);
                Main.failedLoading.Add(name);
                DebugError(e);
            }
        }


        private static bool OnToggle(UnityModManager.ModEntry modEntry, bool value)
        {
            Main.modEnabled = value;
           return true;
        }

        // Token: 0x06000026 RID: 38 RVA: 0x000021B3 File Offset: 0x000003B3
        private static bool Unload(UnityModManager.ModEntry modEntry)
        {
              if (Main.okPatches.Count > 0)
            {
                /*if (Main.spawner != null)
                {
                    Main.spawner.Stop();
                    Main.spawner.StopAllCoroutines();
                }*/
            harmonyInstance.UnpatchAll(modEntry.Info.Id);

                //HarmonyInstance.Create(modEntry.Info.Id).UnpatchAll(null);
                return true;
            }
            else { UnityModManager.Logger.Log("couldn't find patches to unload!"); return true; }
        }





        

        public static void DebugLog(string msg)
        {
            #if DEBUG
            if (logger != null) logger.Log(msg);
            #endif
        }
        public static void DebugError(Exception ex)
        {
            if (logger != null) logger.Log(ex.ToString() + "\n" + ex.StackTrace);
        }


        public static UnityModManagerNet.UnityModManager.ModEntry.ModLogger logger;
        public static bool modEnabled;
        private static HarmonyInstance harmonyInstance;
        private static readonly Dictionary<Type, bool> typesPatched = new Dictionary<Type, bool>();
        private static readonly List<string> failedPatches = new List<string>();
        private static readonly List<string> okPatches = new List<string>();
        private static readonly List<string> okLoading = new List<string>();
        private static readonly List<string> failedLoading = new List<string>();

        [Harmony12.HarmonyPatch(typeof(LibraryScriptableObject), "LoadDictionary")]
        public static class LibraryScriptableObject_LoadDictionary_Patch
        {
            static void Postfix(LibraryScriptableObject __instance)
            {

                Main.SafeLoad(new Action(Main.Init), "Example1 of Startup Asset Load Here");
            }
        }




    }
}
