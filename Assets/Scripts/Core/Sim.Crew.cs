using System;
using System.Collections.Generic;

namespace ReelEmpire.Core
{
    public sealed class CrewDef
    {
        public string Id, Name, Role, Face;
        public Rarity Rarity;       // Common, Rare, Epic or Legendary (Uncommon isn't used for crew)
        public Stage Stage;
    }

    [Serializable]
    public sealed class CrewState { public string id; public int level = 1; public int copies; }

    /// <summary>
    /// The crew: twelve people who come as cards from the crew chest (free every four hours, or for gems). One can
    /// stand duty at each stage and raises what it does; more copies of a card raise its level. The odds are printed
    /// on the chest, and the tenth chest without an Epic or better is sure to hold one.
    /// </summary>
    public sealed partial class Sim
    {
        public static readonly CrewDef[] Crew =
        {
            new CrewDef { Id = "finn", Name = "Finn Tolliver", Role = "Deckhand", Rarity = Rarity.Common, Stage = Stage.Berth, Face = "character-male-a" },
            new CrewDef { Id = "odile", Name = "Odile Marsh", Role = "Net mender", Rarity = Rarity.Rare, Stage = Stage.Berth, Face = "character-female-a" },
            new CrewDef { Id = "brannock", Name = "Captain Brannock", Role = "Old salt", Rarity = Rarity.Legendary, Stage = Stage.Berth, Face = "character-male-f" },
            new CrewDef { Id = "pip", Name = "Pip Reyes", Role = "Barrow runner", Rarity = Rarity.Common, Stage = Stage.Dock, Face = "character-male-b" },
            new CrewDef { Id = "greta", Name = "Greta Lund", Role = "Head porter", Rarity = Rarity.Rare, Stage = Stage.Dock, Face = "character-female-c" },
            new CrewDef { Id = "samson", Name = "Samson Okoye", Role = "Crane hand", Rarity = Rarity.Epic, Stage = Stage.Dock, Face = "character-male-d" },
            new CrewDef { Id = "nell", Name = "Nell Hartwell", Role = "Fish cleaner", Rarity = Rarity.Common, Stage = Stage.Hall, Face = "character-female-d" },
            new CrewDef { Id = "ivo", Name = "Ivo Castellan", Role = "Smokehouse keeper", Rarity = Rarity.Rare, Stage = Stage.Hall, Face = "character-male-c" },
            new CrewDef { Id = "marisol", Name = "Marisol Vane", Role = "Master filleter", Rarity = Rarity.Epic, Stage = Stage.Hall, Face = "character-female-e" },
            new CrewDef { Id = "dot", Name = "Dot Kimura", Role = "Stall keeper", Rarity = Rarity.Common, Stage = Stage.Market, Face = "character-female-f" },
            new CrewDef { Id = "rufus", Name = "Rufus Bell", Role = "Truck driver", Rarity = Rarity.Rare, Stage = Stage.Market, Face = "character-male-e" },
            new CrewDef { Id = "augusta", Name = "Augusta Pryce", Role = "Auctioneer", Rarity = Rarity.Epic, Stage = Stage.Market, Face = "character-male-g" },
        };

        public const double ChestEvery = 4 * 3600;
        public const int ChestGems = 40, ChestPity = 10, CrewMaxLevel = 10;
        /// <summary>Out of 100: Common, Rare, Epic, Legendary. Shown on the chest.</summary>
        public static readonly int[] ChestOdds = { 70, 22, 7, 1 };
        static readonly Rarity[] ChestRarities = { Rarity.Common, Rarity.Rare, Rarity.Epic, Rarity.Legendary };

        public static CrewDef CrewById(string id) { return Array.Find(Crew, c => c.Id == id); }
        public CrewState Owned(string id) { return State.crew.Find(c => c.id == id); }

        /// <summary>How much a card adds to its stage at a level: 10% / 20% / 40% / 80% by rarity, a quarter of that more each level.</summary>
        public static double CrewBonus(CrewDef d, int level)
        {
            double b = d.Rarity == Rarity.Common ? 0.10 : d.Rarity == Rarity.Rare ? 0.20 : d.Rarity == Rarity.Epic ? 0.40 : 0.80;
            return b * (1 + 0.25 * (level - 1));
        }

        /// <summary>Copies a card needs to go from this level to the next.</summary>
        public static int CopiesFor(int level) { return level * 2; }

        public CrewDef OnDuty(Stage stage) { string id = State.duty[(int)stage]; return string.IsNullOrEmpty(id) ? null : CrewById(id); }

        double DutyMult(Stage stage)
        {
            var d = OnDuty(stage); if (d == null) return 1;
            var o = Owned(d.Id); return o == null ? 1 : 1 + CrewBonus(d, o.level);
        }

        /// <summary>Puts an owned card on duty at its own stage (null: nobody).</summary>
        public bool Assign(Stage stage, string id)
        {
            if (id != null) { var d = CrewById(id); if (d == null || d.Stage != stage || Owned(id) == null) return false; }
            State.duty[(int)stage] = id ?? "";
            Touch();
            return true;
        }

        public double ChestIn { get { return Math.Max(0, State.chestAt - RealNow()); } }
        public bool ChestFree { get { return ChestIn <= 0; } }

        /// <summary>Opens a crew chest: the free one when it is ready, else for gems. Returns the card, or null.</summary>
        public CrewDef OpenChest(bool withGems = false)
        {
            if (withGems) { if (State.gems < ChestGems) return null; State.gems -= ChestGems; }
            else { if (!ChestFree) return null; State.chestAt = RealNow() + ChestEvery; }
            State.chestsOpened++; State.chestsSinceEpic++;
            int roll = (int)(Roll() * 100), r = 0, acc = 0;
            for (; r < ChestOdds.Length - 1; r++) { acc += ChestOdds[r]; if (roll < acc) break; }
            if (State.chestsSinceEpic >= ChestPity && r < 2) r = 2;       // the sure Epic
            if (r >= 2) State.chestsSinceEpic = 0;
            var pool = Array.FindAll(Crew, c => c.Rarity == ChestRarities[r]);
            var card = pool[(int)(Roll() * pool.Length)];
            var own = Owned(card.Id);
            if (own == null)
            {
                State.crew.Add(new CrewState { id = card.Id });
                if (OnDuty(card.Stage) == null) State.duty[(int)card.Stage] = card.Id;       // a first hand goes straight to work
            }
            else
            {
                own.copies++;
                while (own.level < CrewMaxLevel && own.copies >= CopiesFor(own.level)) { own.copies -= CopiesFor(own.level); own.level++; }
            }
            Touch();
            return card;
        }
    }
}
