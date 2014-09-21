using System;
using MineProxy.Packets;
using MineProxy.Data;
using System.Collections.Generic;

namespace MineProxy.Plugins
{
    /// <summary>
    /// Helper to generate packets
    /// </summary>
    public class Scoreboard
    {
        public readonly string Board;
        public string Title;
        public List<Item> Items = new List<Item>();
        public ScreenPosition Position;

        public Scoreboard(string board, string title, ScreenPosition pos)
        {
            this.Board = board;
            if (title.Length < 32)
                this.Title = title;
            else
                this.Title = title.Substring(0, 32);
            this.Position = pos;
        }

        public void Add(string name, int score)
        {
            Items.Add(new Item(name, score));
        }

        public List<PacketFromServer> CreateFill()
        {
            var p = new List<PacketFromServer>();

            //Create
            var sb = new ScoreboardObjective();
            sb.Board = Board;
            sb.Mode = ScoreAction.Create;
            sb.Value = Title;
            sb.Type = "integer";
            p.Add(sb);

            //Fill
            foreach (var i in Items)
            {
                var u = new ScoreboardUpdate();
                u.Board = Board;
                u.Name = i.Name;
                u.Value = i.Score;
                p.Add(u);
            }

            //Show
            p.Add(new ScoreboardShow(Board, Position));
            return p;
        }

        public ScoreboardObjective Remove()
        {
            var sb = new ScoreboardObjective();
            sb.Board = Board;
            sb.Mode = ScoreAction.Remove;
            return sb;
        }

        public class Item
        {
            public string Name { get; set; }
            public int Score { get; set; }

            public Item(string name, int value)
            {
                this.Name = name;
                this.Score = value;                
            }
        }
    }
}

