﻿using Harmony;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Verse;

namespace AlienRace
{
    public sealed class AlienPartGenerator
    {
        public List<string> aliencrowntypes = new List<string>() { "Average_Normal" };

        public List<BodyType> alienbodytypes = new List<BodyType>();

        public bool UseGenderedHeads = true;

        public ColorGenerator alienskincolorgen;
        public ColorGenerator alienskinsecondcolorgen;
        public ColorGenerator alienhaircolorgen;

        public Vector2 CustomDrawSize = Vector2.one;
        public Vector2 CustomPortraitDrawSize = Vector2.one;

        static Dictionary<Vector2, GraphicMeshSet[]> meshPools = new Dictionary<Vector2, GraphicMeshSet[]>();

        public GraphicMeshSet bodySet;
        public GraphicMeshSet headSet;
        public GraphicMeshSet hairSetAverage;
        public GraphicMeshSet hairSetNarrow;
        public Mesh tailMesh;
        public Mesh tailMeshFlipped;

        public GraphicMeshSet bodyPortraitSet;
        public GraphicMeshSet headPortraitSet;
        public GraphicMeshSet hairPortraitSetAverage;
        public GraphicMeshSet hairPortraitSetNarrow;
        public Mesh tailPortraitMesh;
        public Mesh tailPortraitMeshFlipped;

        public BodyPartDef tailBodyPart;
        public bool UseSkinColorForTail = true;

        static MethodInfo meshInfo = AccessTools.Method(AccessTools.TypeByName("MeshMakerPlanes"), "NewPlaneMesh", new Type[] { typeof(Vector2), typeof(bool), typeof(bool), typeof(bool) });

        public string RandomAlienHead(string userpath, Gender gender) => userpath + (UseGenderedHeads ? gender.ToString() + "_" : "") + aliencrowntypes[Rand.Range(0, aliencrowntypes.Count)];

        public static Graphic GetNakedGraphic(BodyType bodyType, Shader shader, Color skinColor, Color skinColorSecond, string userpath) => GraphicDatabase.Get<Graphic_Multi>(userpath + "Naked_" + bodyType.ToString(), shader, Vector2.one, skinColor, skinColorSecond);

        public Color SkinColor(Pawn alien, bool first = true)
        {
            AlienComp alienComp = alien.TryGetComp<AlienComp>();
            if (alienComp.skinColor == Color.clear)
            {
                alienComp.skinColor = (alienskincolorgen != null ? alienskincolorgen.NewRandomizedColor() : PawnSkinColors.GetSkinColor(alien.story.melanin));
                alienComp.skinColorSecond = (alienskinsecondcolorgen != null ? alienskinsecondcolorgen.NewRandomizedColor() : alienComp.skinColor);
            }
            return first ? alienComp.skinColor : alienComp.skinColorSecond;
        }

        public AlienPartGenerator()
        {
            LongEventHandler.QueueLongEvent(() =>
                { 
                    
                    {
                        if (!meshPools.Keys.Any(v => v.Equals(CustomDrawSize)))
                        {
                            meshPools.Add(CustomDrawSize, new GraphicMeshSet[]
                                {
                                new GraphicMeshSet(1.5f * CustomDrawSize.x, 1.5f * CustomDrawSize.y), // bodySet
                                new GraphicMeshSet(1.5f * CustomDrawSize.x, 1.5f * CustomDrawSize.y), // headSet
                                new GraphicMeshSet(1.5f * CustomDrawSize.x, 1.5f * CustomDrawSize.y), // hairSetAverage
                                new GraphicMeshSet(1.3f * CustomDrawSize.x, 1.5f * CustomDrawSize.y), // hairSetNarrow

                                });
                        }

                        GraphicMeshSet[] meshSet = meshPools[meshPools.Keys.First(v => v.Equals(CustomDrawSize))];

                        bodySet = meshSet[0];
                        headSet = meshSet[1];
                        hairSetAverage = meshSet[2];
                        hairSetNarrow = meshSet[3];
                        tailMesh = (Mesh)meshInfo.Invoke(null, new object[] {CustomDrawSize, false, false, false });
                        tailMeshFlipped = (Mesh)meshInfo.Invoke(null, new object[] { CustomDrawSize, true, false, false });
                    }
                    {
                        if (!meshPools.Keys.Any(v => v.Equals(CustomPortraitDrawSize)))
                        {
                            meshPools.Add(CustomPortraitDrawSize, new GraphicMeshSet[]
                                {
                                new GraphicMeshSet(1.5f * CustomPortraitDrawSize.x, 1.5f * CustomPortraitDrawSize.y), // bodySet
                                new GraphicMeshSet(1.5f * CustomPortraitDrawSize.x, 1.5f * CustomPortraitDrawSize.y), // headSet
                                new GraphicMeshSet(1.5f * CustomPortraitDrawSize.x, 1.5f * CustomPortraitDrawSize.y), // hairSetAverage
                                new GraphicMeshSet(1.3f * CustomPortraitDrawSize.x, 1.5f * CustomPortraitDrawSize.y), // hairSetNarrow
                                });
                        }

                        GraphicMeshSet[] meshSet = meshPools[meshPools.Keys.First(v => v.Equals(CustomPortraitDrawSize))];

                        bodyPortraitSet = meshSet[0];
                        headPortraitSet = meshSet[1];
                        hairPortraitSetAverage = meshSet[2];
                        hairPortraitSetNarrow = meshSet[3];
                        tailPortraitMesh = (Mesh)meshInfo.Invoke(null, new object[] { CustomPortraitDrawSize, false, false, false });
                        tailPortraitMeshFlipped = (Mesh)meshInfo.Invoke(null, new object[] { CustomPortraitDrawSize, true, false, false });
                    }
                }, "meshSetAlien", false, null);
        }

        public bool CanDrawTail(Pawn pawn) => RestUtility.CurrentBed(pawn) == null && !pawn.Downed && !pawn.Dead && (tailBodyPart == null ||
                pawn.health.hediffSet.GetNotMissingParts().Any(bpr => bpr.def == tailBodyPart));

        public class AlienComp : ThingComp
        {
            public bool fixGenderPostSpawn;
            public Color skinColor;
            public Color skinColorSecond;
            public Graphic Tail;

            public override void PostExposeData()
            {
                base.PostExposeData();
                Scribe_Values.LookValue(ref fixGenderPostSpawn, "fixAlienGenderPostSpawn", false);
                Scribe_Values.LookValue(ref skinColor, "skinColorAlien");
                Scribe_Values.LookValue(ref skinColorSecond, "skinColorSecondAlien");
            }
        }
    }
}