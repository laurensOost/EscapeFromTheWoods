using System;
using System.Collections.Generic;

namespace EscapeFromTheWoods
{
    public class MonkeyManager
    {
        private DBwriter _dbWriter;
        private List<Monkey> _monkeys;

        public MonkeyManager(DBwriter dbWriter)
        {
            _dbWriter = dbWriter;
            _monkeys = new List<Monkey>();
        }

        public void PlaceMonkey(int woodID, string monkeyName, int monkeyID, List<Tree> trees, Random r)
        {
            int treeNr;
            do
            {
                treeNr = r.Next(0, trees.Count);
            }
            while (trees[treeNr].hasMonkey);
            Monkey m = new Monkey(monkeyID, monkeyName, trees[treeNr]);
            _monkeys.Add(m);
            trees[treeNr].hasMonkey = true;
            Tree placedTree = trees[treeNr];
            _dbWriter.WriteLogRecord(new DBLogRecord
            {
                woodID = woodID,
                monkeyID = monkeyID,
                message = $"{monkeyName} placed in tree {placedTree.treeID} at location ({placedTree.x},{placedTree.y})"
            });
        }

        public List<Monkey> GetMonkeys()
        {
            return _monkeys;
        }
    }
}