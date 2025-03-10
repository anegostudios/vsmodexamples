using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent;

namespace VSTutorial.EntityBehaviors
{
    internal class EntityBehaviorTotalPlayTime : EntityBehavior
    {
        /// <summary>
        /// The listener ID for the gameTickListener. You need to store this to properly dispose of the listener when the mob is despawned.
        /// </summary>
        private long listenerId;

        /// <summary>
        /// This allows us to calculate the in-game time since the last function call.
        /// </summary>
        private double lastTotalHours;

        /// <summary>
        /// This is our stored value. Using a getter and setter allows us to directly set/get the value to/from the WatchedAttributes.
        /// Watched Attributes are synced from the server to the client. In other words...
        ///     * TotalTimePlayedFor can be read on both server and client.
        ///     * TotalTimePlayedFor can only be written to from the server.
        ///     * As soon as TotalTimePlayedFor is written to and the path is marked dirty, the data is saved to disk and all clients are sent this data.
        /// </summary>
        /// <remarks>
        /// Some notes on how watched attributes are used:
        /// Each entity has its own attributes that are stored on the server, and they are stored like a tree.
        /// In this case, we are accessing another branch/tree attribute called "vstutorial".
        /// Inside vstutorial, we get or set a double called "timeplayed".
        /// When set, we need to mark the nearest tree as dirty. This tells the game that the data needs to be saved and sent to the clients.
        /// </remarks>
        public double TotalTimePlayedFor
        {
            get
            {
                return entity.WatchedAttributes.GetTreeAttribute("vstutorial").GetDouble("timeplayed");
            }
            set
            {
                entity.WatchedAttributes.GetTreeAttribute("vstutorial").SetDouble("timeplayed", value);
                entity.WatchedAttributes.MarkPathDirty("vstutorial");
            }
        }

        public EntityBehaviorTotalPlayTime(Entity entity)
            : base(entity)
        {}

        /// <summary>
        /// Initialize is called when the entity comes into existence. This can be through world generation, natural spawning, loading a world, and more.
        /// This is where you setup any initialization data and register game tick listeners.
        /// </summary>
        public override void Initialize(EntityProperties properties, JsonObject typeAttributes)
        {
            //The first important thing is to actually ensure that our timeplayer attribute exists!
            ITreeAttribute treeAttribute = entity.WatchedAttributes.GetTreeAttribute("vstutorial");
            if (treeAttribute == null)
            {
                //If it doesn't exist, create the tree...
                entity.WatchedAttributes.SetAttribute("vstutorial", treeAttribute = new TreeAttribute());
                //... then set a default value for our total time played for.
                TotalTimePlayedFor = 0;
            }

            //Now, register the game tick listener. This was detailed in tutorial 5, block entities, but this time we're actually storing the listener ID.
            //In this particular tick listener, we're going to call it every 3000ms (3 seconds).
            listenerId = entity.World.RegisterGameTickListener(SlowTick, 3000);

            //We also get the current time, in hours, since the world start.
            lastTotalHours = entity.World.Calendar.TotalHours;
        }

        /// <summary>
        /// This is the function that is called by the game tick listener every 3 seconds.
        /// </summary>
        private void SlowTick(float dt)
        {
            //We only want this logic to occur on the server.
            if (entity.Api.Side != EnumAppSide.Client) 
            {
                //Get the number of hours since the function was last called.
                double deltaHours = entity.World.Calendar.TotalHours - lastTotalHours;
                //And then add it on to our value.
                TotalTimePlayedFor += deltaHours;
                //And lastly, we need to reset our total hours amount.
                lastTotalHours = entity.World.Calendar.TotalHours;
            }
        }

        /// <summary>
        /// When the entity despawns, we want to remove the game tick listener using the ID.
        /// </summary>
        public override void OnEntityDespawn(EntityDespawnData despawn)
        {
            base.OnEntityDespawn(despawn);
            entity.World.UnregisterGameTickListener(listenerId);
        }

        /// <summary>
        /// Entity Behaviors require a property name. This should usually be the name you used to register the entity behavior with.
        /// </summary>
        public override string PropertyName()
        {
            return "vstutorial.totalplaytime";
        }

    }
}
