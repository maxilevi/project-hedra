/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 09/07/2016
 * Time: 03:25 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Hedra.Engine.BiomeSystem;
using Hedra.Engine.Generation;
using Hedra.Engine.Management;
using Hedra.Engine.Networking;
using Hedra.Engine.PhysicsSystem;
using Hedra.Engine.Player;
using Hedra.Engine.Rendering;
using Hedra.Engine.Scenes;
using Hedra.Engine.Sound;
using OpenTK;

namespace Hedra.Engine.QuestSystem.Objectives
{
    /// <summary>
    ///     Description of Objective.
    /// </summary>
    public abstract class Objective
    {
        public static Vector3 DefaultPosition = new Vector3(
            BiomePool.WorldWidth * .5f + GameSettings.SpawnPoint.X,
            0,
            BiomePool.WorldHeight * .5f + GameSettings.SpawnPoint.Y);

        public static Vector3[] ObjectivePositions;

        public static string[] LoadData;
        public List<Objective> AvailableOuts = new List<Objective>();
        public List<Objective> AvailablePartials = new List<Objective>();
        /// <summary>
        /// The height added to theirs.
        /// </summary>
        public int CenterHeight = 25;
        /// <summary>
        /// Max Height the objective plane can have.
        /// </summary>
        public int CenterMaxHeight = 24;
        /// <summary>
        /// The area affected by the objective.
        /// </summary>
        public int CenterRadius = 512;
        /// <summary>
        /// Radius where trees cant grow around the objective
        /// </summary>
        public int NoTreesRadius = 64;
        public uint IconId;
        public bool IsLost = false;
        public int NoEnviromentRadius = 0;

        public Vector3 ObjectivePosition, Position;
        public Objective OutObjective;
        protected List<CollisionShape> Shapes;
        protected bool Disposed;
        protected VertexData Model;

        public abstract string Description { get; }

        public bool IsPartial { get; set; }
        public Objective PartialParent;

        public virtual Vector3 IconPosition => ObjectivePosition;

        public string ObjectiveDirection
        {
            get
            {
                Vector3 diff = ObjectivePosition - SceneManager.Game.LPlayer.Position;
                string leftRight = diff.X < 0 ? " West" : " East";
                string upDown = diff.Z < 0 ? "South" : "North";

                return upDown + leftRight;
            }
        }

        public abstract uint QuestLogIcon { get; }

        public abstract bool ShouldDisplay { get; }

        public void SetQuestParams()
        {
            if (this.IsPartial)
                ObjectivePosition = World.QuestManager.PassedObjectives[World.QuestManager.PassedObjectives.Count - 1].ObjectivePosition;
            else
                ObjectivePosition = this.NextObjectivePosition();
            World.QuestManager.ObjectivePosition = ObjectivePosition;
            LocalPlayer.Instance.MessageDispatcher.ShowMessage("[T] To open the quest log", 3f, Color.White);
        }

        static Objective()
        {
            ObjectivePositions = new Vector3[6];
            ObjectivePositions[0] = new Vector3(BiomePool.WorldWidth * .5f, 0, BiomePool.WorldHeight * .5f);
            ObjectivePositions[1] = new Vector3(BiomePool.WorldWidth, 0, BiomePool.WorldHeight);
            ObjectivePositions[2] = new Vector3(BiomePool.WorldWidth, 0, BiomePool.WorldHeight * .5f);
            ObjectivePositions[3] = new Vector3(BiomePool.WorldWidth, 0, BiomePool.WorldHeight * 1.25f);
            ObjectivePositions[4] = new Vector3(BiomePool.WorldWidth * .5f, 0, BiomePool.WorldHeight * 1.25f);
            ObjectivePositions[5] = new Vector3(BiomePool.WorldWidth * .25f, 0, BiomePool.WorldHeight * 1.0f);

            for (var i = 0; i < ObjectivePositions.Length; i++)
            {
                ObjectivePositions[i] += GameSettings.SpawnPoint.ToVector3();
            }
        }

        public Vector3 NextObjectivePosition()
        {
            if (World.QuestManager.PassedObjectives.Count == 0)
                return DefaultPosition;

            return NextObjectivePosition(World.QuestManager
                .PassedObjectives[World.QuestManager.PassedObjectives.Count - 1].ObjectivePosition);
        }

        public static Vector3 NextObjectivePosition(Vector3 Position)
        {
            for (int i = 0; i < ObjectivePositions.Length; i++)
            {
                if (Position == ObjectivePositions[i] && i < ObjectivePositions.Length-1)
                    return ObjectivePositions[i + 1];
            }
            return ObjectivePositions[0];
        }

        public static bool IsNearObjectives(Vector3 Position)
        {
            return IsNearObjectives(Position, 960);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="Position">The position to test again in Xz</param>
        /// <param name="Radius">The tolerance radius</param>
        /// <returns></returns>
        public static bool IsNearObjectives(Vector3 Position, float Radius)
        {
            for (var i = 0; i < ObjectivePositions.Length; i++)
            {
                if ((ObjectivePositions[i] - Position).LengthSquared < Radius * Radius)
                    return true;
            }
            return false;
        }

        public void RunCoroutine()
        {
            CenterMaxHeight += (int) (new Random(World.Seed + 234).NextFloat() * 15 - 7.5f);
            if (Model != null && Constants.CHARACTER_CHOOSED)
                if (World.GetChunkAt(ObjectivePosition) != null && World.GetChunkAt(ObjectivePosition).Initialized)
                {
                    World.GetChunkAt(ObjectivePosition).RemoveStaticElement(Model);

                    for (int i = Shapes.Count - 1; i > -1; i--)
                        World.GetChunkAt(ObjectivePosition).RemoveCollisionShape(Shapes[i]);
                }
            CoroutineManager.StartCoroutine(this.ChunkCoroutine);
        }

        public string Serialize()
        {
            string data = string.Empty;
            foreach (Objective t in World.QuestManager.PassedObjectives)
            {
                data += t.GetType() + "|";
            }
            return data;
        }

        public static void Unserialize(string Data)
        {
            if (Data == string.Empty) return;
            Objective.LoadData = Data.Split('|');

        }


        private IEnumerator ChunkCoroutine()
        {
            Chunk underChunk = World.GetChunkAt(ObjectivePosition);
            while (underChunk == null || !underChunk.BuildedWithStructures)
            {
                if (Disposed) yield break;

                underChunk = World.GetChunkAt(ObjectivePosition);
                yield return null;
            }

            this.Setup(underChunk);
        }

        public void NextObjective()
        {
            var partialRng = new Random(World.Seed + 63453 + World.QuestManager.PassedObjectives.Count - 1);
            this.SetPartialObjectives();
            bool hasPartialChildren = AvailablePartials.Count > 0;
            for (int i = 0; i < AvailablePartials.Count; i++)
            {
                AvailablePartials[i].IsPartial = true;
                AvailablePartials[i].PartialParent = this;
            }

            this.Dispose();
            World.QuestManager.PassedObjectives.Add(this);
            if (!hasPartialChildren)
            {
                this.SetOutObjectives();
            }
            Random rng;
            if (!hasPartialChildren)
            {
                rng = new Random(World.Seed + 343523 + World.QuestManager.PassedObjectives.Count - 1);
                OutObjective = AvailableOuts[rng.Next(0, AvailableOuts.Count)];
            }
            else
            {
                rng = new Random(World.Seed + 6436785 + World.QuestManager.PassedObjectives.Count - 1);
                OutObjective = AvailablePartials[rng.Next(0, AvailablePartials.Count)];
            }

            if (OutObjective != null)
            {
                if (World.QuestManager.WasQuestTypeDone(OutObjective.GetType()) && !OutObjective.IsPartial ||
                    World.QuestManager.PassedObjectives.Count - 1 >= World.QuestManager.ChainLength && !OutObjective.IsPartial)
                {
                    World.QuestManager.EndRun();
                    return;
                }

                SoundManager.PlaySoundInPlayersLocation(SoundType.NotificationSound);
                LocalPlayer.Instance.MessageDispatcher.ShowNotification("OBJECTIVE COMPLETED", Color.White, 5f, false);
                World.QuestManager.SetQuest(OutObjective);

                if (NetworkManager.IsConnected)
                    NetworkManager.SendQuestCompleted(this.GetType().ToString());
            }
        }

        /// <summary>
        /// Method called to set all the possible next objectives an objective can have
        /// </summary>
        public virtual void SetOutObjectives(){}

        /// <summary>
        /// Method called to set all the possible intermediate objectives an objective can have. This means objectives within an objective.
        /// </summary>
        public virtual void SetPartialObjectives() { }

        /// <summary>
        /// Method called when the chunk under the objective position is generated. Useful for placing structures.
        /// </summary>
        /// <param name="UnderChunk"></param>
        public abstract void Setup(Chunk UnderChunk);

        /// <summary>
        /// Method called when an objective is made current.
        /// </summary>
        /// <param name="World"></param>
        public virtual void Recreate()
        {
            this.SetQuestParams();
            this.RunCoroutine();
        }

        public abstract void Dispose();
    }
}