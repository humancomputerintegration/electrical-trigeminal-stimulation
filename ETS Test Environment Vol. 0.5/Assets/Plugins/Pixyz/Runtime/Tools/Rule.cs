using System;
using System.Collections.Generic;
using UnityEngine;

namespace Pixyz.Tools {

    public interface IRenamable {
        string name { get; set; }
    }

    /// <summary>
    /// A RuleEngine Rule
    /// </summary>
    [Serializable]
    public sealed class Rule : ISerializationCallbackReceiver, IRenamable
    {

        [SerializeField]
        public bool isCollapsed = false;

        [SerializeField]
        public bool isEnabled = true;

        [SerializeField]
        public string name = "New Rule";

        [SerializeField]
        private List<RuleBlock> blocks = new List<RuleBlock>();

        [NonSerialized]
        public RuleSet rules;

        public RuleBlock getBlock(int i) => blocks[i];

        public void setBlock(int i, RuleBlock block) => blocks[i] = block;

        public int getBlockIndex(RuleBlock block) => blocks.IndexOf(block);

        public void removeBlockAt(int index) => blocks.Remove(blocks[index]);

        public void removeBlock(RuleBlock block) => blocks.Remove(block);

        public void appendBlock(RuleBlock block) => blocks.Add(block);

        public void insertBlock(RuleBlock block, int index) => blocks.Insert(index, block);

        public int blocksCount => blocks.Count;

        public void OnBeforeSerialize()
        {

        }

        public void OnAfterDeserialize()
        {
            foreach (var block in blocks)
            {
                block.rule = this;
            }
        }

        string IRenamable.name
        {
            get { return name; }
            set { name = value; }
            }
        }
    }