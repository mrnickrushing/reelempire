using System;
using System.Collections.Generic;

namespace ReelEmpire.Core
{
    public enum OrderKind { Product, Rarity, Species }

    /// <summary>Someone in town wants something from the harbour: so much of a product, or so many of a kind of fish.</summary>
    [Serializable]
    public sealed class Order
    {
        public int kind;                // OrderKind
        public string harbour;          // where it was placed, and the only place that fills it
        public string who;
        public int goods;               // Product: index into Content.Goods
        public int rarity;              // Rarity: the least rarity that counts
        public string species;          // Species: its id
        public double need, have;
        public int cashSeconds, gems;
        public bool favourite;          // Product: the customer's favourite line (pays more)
    }

    /// <summary>
    /// The order board. Up to three orders stand at a time and a new one comes ten minutes after a place frees up.
    /// Nothing expires and nothing is lost by ignoring one; filling it pays well on top of the ordinary sale.
    /// </summary>
    public sealed partial class Sim
    {
        public const int OrderSlots = 3;
        public const double OrderEvery = 600;
        static readonly string[] Customers =
        {
            "The Heron Inn", "Marta's Chowder Stand", "Lakeside Smokery", "The Old Ferry Café", "The Gull & Anchor", "The school kitchen",
            "Saturday Market", "The lighthouse keeper", "Captain Brannock's table", "The harbour master's office", "Pip's fish cart", "The cannery wharf shop",
        };

        public double OrderIn { get { return Math.Max(0, State.orderAt - RealNow()); } }
        public int OrdersReady { get { int n = 0; foreach (var o in State.orders) if (o.have >= o.need) n++; return n; } }

        /// <summary>Fills the empty places on the board as their time comes (called as the game runs).</summary>
        void TickOrders()
        {
            if (!Unlocked("orders")) return;
            if (State.ordersDone == 0 && State.orders.Count == 0 && State.orderAt <= 0)
            {
                for (int i = 0; i < OrderSlots; i++) State.orders.Add(NewOrder(i));       // the board opens full, one of each kind
                State.orderAt = RealNow() + OrderGap;
                Touch();
                return;
            }
            if (State.orders.Count >= OrderSlotCount) { State.orderAt = RealNow() + OrderGap; return; }
            if (RealNow() < State.orderAt) return;
            State.orders.Add(NewOrder((int)(Roll() * 3)));
            State.orderAt = RealNow() + OrderGap;
            Touch();
        }

        Order NewOrder(int kind)
        {
            var h = H;
            var o = new Order { harbour = h.id, who = Customers[(int)(Roll() * Customers.Length)] };
            // No two orders on the board from the same customer.
            for (int tries = 0; tries < 8 && State.orders.Exists(x => x.who == o.who); tries++) o.who = Customers[(int)(Roll() * Customers.Length)];
            if (kind == (int)OrderKind.Species)
            {
                var pool = Book.In(h.id).FindAll(s => s.Rarity <= Rarity.Uncommon && s.When == Bite.Any && !State.orders.Exists(x => x.species == s.Id));
                if (pool.Count > 0)
                {
                    var sp = pool[(int)(Roll() * pool.Count)];
                    o.kind = (int)OrderKind.Species; o.species = sp.Id;
                    o.need = sp.Rarity == Rarity.Common ? 5 + (int)(Roll() * 6) : 3 + (int)(Roll() * 3);
                    o.cashSeconds = 240; o.gems = sp.Rarity == Rarity.Common ? 2 : 4;
                    return o;
                }
                kind = (int)OrderKind.Rarity;
            }
            if (kind == (int)OrderKind.Rarity)
            {
                o.kind = (int)OrderKind.Rarity;
                bool rare = Roll() < (h.berths.Count >= 5 ? 0.5 : 0.25);
                o.rarity = (int)(rare ? Rarity.Rare : Rarity.Uncommon);
                o.need = rare ? 2 + (int)(Roll() * 3) : 6 + (int)(Roll() * 7);
                o.cashSeconds = rare ? 420 : 240; o.gems = rare ? 6 : 3;
                return o;
            }
            // So much of a product: three to six minutes of the hall or the harbour at its present pace.
            o.kind = (int)OrderKind.Product;
            var owned = new List<int>();
            for (int g = 0; g < Content.Goods.Length; g++) if (OwnsGoods(h, g)) owned.Add(g);
            o.goods = owned.Count > 1 && Roll() < 0.8 ? owned[1 + (int)(Roll() * (owned.Count - 1))] : 0;
            // Customers ask for their favourite line when the hall has it, and pay more for it.
            int fav = FavouriteOf(o.who);
            if (OwnsGoods(h, fav) && Roll() < 0.6) { o.goods = fav; o.favourite = true; }
            double pace = Math.Max(Flow(h, true), Content.DockBase * DefOf(h).Scale * 0.5);
            if (o.goods > 0) pace = Math.Min(pace, StageOut(Content.HallBase, h.hall) * DefOf(h).Scale * Content.Goods[o.goods].Speed * Fx(h, "hall"));
            double seconds = 180 + Roll() * 180;
            o.need = Math.Max(10, Math.Round(pace * seconds));
            o.cashSeconds = (int)(seconds * (1.2 + o.goods * 0.2)); o.gems = 1 + o.goods;
            return o;
        }

        void OrderCatch(HarbourState h, SpeciesDef sp)
        {
            foreach (var o in State.orders)
            {
                if (o.harbour != h.id || o.have >= o.need) continue;
                if (o.kind == (int)OrderKind.Species && o.species == sp.Id || o.kind == (int)OrderKind.Rarity && (int)sp.Rarity >= o.rarity) o.have = Math.Min(o.need, o.have + 1);
            }
        }

        void OrderSold(HarbourState h, int goods, double units)
        {
            if (units <= 0) return;
            foreach (var o in State.orders)
                if (o.harbour == h.id && o.kind == (int)OrderKind.Product && o.goods == goods && o.have < o.need) { o.have = Math.Min(o.need, o.have + units); return; }
        }

        /// <summary>What an order asks for, in a line.</summary>
        public string OrderText(Order o)
        {
            switch ((OrderKind)o.kind)
            {
                case OrderKind.Species: { var sp = Book.Get(o.species); return Format.Money(o.need) + " " + (sp != null ? sp.Name : "fish"); }
                case OrderKind.Rarity: return Format.Money(o.need) + " " + ((Rarity)o.rarity == Rarity.Rare ? "rare fish or better" : "uncommon fish or better");
                default: return Format.Money(o.need) + " of " + Content.Goods[o.goods].Name.ToLowerInvariant() + " fish";
            }
        }

        /// <summary>How to go about it: the line to run, or the bait that helps.</summary>
        public string OrderHint(Order o)
        {
            switch ((OrderKind)o.kind)
            {
                case OrderKind.Species: return "Land them at any dock here. Worms or Groundbait bring the most.";
                case OrderKind.Rarity: return (Rarity)o.rarity == Rarity.Rare ? "Spinners and Chum bring rare fish far more often." : "Spinners help; Groundbait brings plenty of these too.";
                default: return o.goods == 0 ? "Sold fresh: whatever the hall doesn't work counts." : "Run the " + Content.Goods[o.goods].Line + " in the hall while it fills.";
            }
        }

        public double OrderCash(Order o) { var h = State.harbours.Find(x => x.id == o.harbour) ?? H; return Math.Max(40 * DefOf(h).Scale, o.cashSeconds * RewardRate(h)) * (Researched("haggle") ? 1.25 : 1) * OrderBonus(o); }

        /// <summary>Hands a finished order over. Returns the cash paid (0 if it isn't finished).</summary>
        public double ClaimOrder(int index)
        {
            if (index < 0 || index >= State.orders.Count) return 0;
            var o = State.orders[index];
            if (o.have < o.need) return 0;
            var h = State.harbours.Find(x => x.id == o.harbour) ?? H;
            double cash = OrderCash(o);
            Pay(h, cash); State.gems += o.gems + (TieLevel(o.who) >= 3 ? 1 : 0);
            TieNews = Deliver(o.who) ?? TieNews;
            State.notes += o.kind == (int)OrderKind.Rarity && o.rarity >= (int)Rarity.Rare ? 2 : 1;      // what the chart room learns from the town
            if (Unlocked("wear")) AddWrenches(1);       // the town pays in kind too
            State.orders.RemoveAt(index); State.ordersDone++;
            AddSeasonPoints(15);
            if (State.orders.Count == OrderSlotCount - 1) State.orderAt = RealNow() + OrderGap;
            Touch();
            return cash;
        }

        /// <summary>Turns an order down. Its place fills again in the usual time: there is nothing to gain by rerolling.</summary>
        public bool DismissOrder(int index)
        {
            if (index < 0 || index >= State.orders.Count) return false;
            State.orders.RemoveAt(index);
            State.orderAt = RealNow() + OrderGap;
            Touch();
            return true;
        }
    }
}
