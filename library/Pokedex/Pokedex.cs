﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PkmnFoundations.Data;
using PkmnFoundations.Structures;
using PkmnFoundations.Support;

namespace PkmnFoundations.Pokedex
{
    public class Pokedex
    {
        public Pokedex(Database db, bool lazy)
        {
            if (lazy) throw new NotImplementedException();

            GetAllData(db);
            BuildAdditionalIndexes();
            PrefetchRelations();
        }

        private void GetAllData(Database db)
        {
            m_species = db.PokedexGetAllSpecies(this).ToDictionary(s => s.NationalDex, s => s);
            m_families = db.PokedexGetAllFamilies(this).ToDictionary(f => f.ID, f => f);
            m_forms = db.PokedexGetAllForms(this).ToDictionary(f => f.ID, f => f);
            m_items = db.PokedexGetAllItems(this).ToDictionary(i => i.ID, i => i);
            m_moves = db.PokedexGetAllMoves(this).ToDictionary(m => m.ID, m => m);
            m_types = db.PokedexGetAllTypes(this).ToDictionary(t => t.ID, t => t);
            m_abilities = db.PokedexGetAllAbilities(this).ToDictionary(a => a.Value, a => a);

            List<FormStats> form_stats = db.PokedexGetAllFormStats(this);
            form_stats.Sort(delegate(FormStats f, FormStats other) 
            { 
                if (f.FormID != other.FormID) return f.FormID.CompareTo(other.FormID); 
                return f.MinGeneration.CompareTo(other.MinGeneration); 
            });

            Dictionary<int, SortedDictionary<Generations, FormStats>> resultFormStats = new Dictionary<int,SortedDictionary<Generations,FormStats>>();
            SortedDictionary<Generations, FormStats> currFormStats = null;
            int currFormId = 0;

            foreach (FormStats f in form_stats)
            {
                if (currFormStats == null || currFormId != f.FormID)
                {
                    if (currFormStats != null) resultFormStats.Add(currFormId, currFormStats);
                    currFormStats = new SortedDictionary<Generations, FormStats>();
                }
                currFormStats.Add(f.MinGeneration, f);
            }
            if (currFormStats != null) resultFormStats.Add(currFormId, currFormStats);
            m_form_stats = resultFormStats;
        }

        private void BuildAdditionalIndexes()
        {

        }

        private void PrefetchRelations()
        {
            // xxx: clean this up
            // todo: reflect these classes to decide whether or not prefetching
            // is even needed
            foreach (var k in m_species)
                k.Value.PrefetchRelations();
            foreach (var k in m_families)
                k.Value.PrefetchRelations();
            foreach (var k in m_forms)
                k.Value.PrefetchRelations();
            foreach (var k in m_items)
                k.Value.PrefetchRelations();
            foreach (var k in m_moves)
                k.Value.PrefetchRelations();
            foreach (var k in m_types)
                k.Value.PrefetchRelations();
            foreach (var k in m_abilities)
                k.Value.PrefetchRelations();

            foreach (var k in m_form_stats)
            {
                foreach (var j in k.Value)
                    j.Value.PrefetchRelations();
            }
        }

        private Dictionary<int, Species> m_species;
        private Dictionary<int, Family> m_families;
        private Dictionary<int, Form> m_forms;
        private Dictionary<int, SortedDictionary<Generations, FormStats>> m_form_stats;
        //private Dictionary<int, Evolution> m_evolutions;

        private Dictionary<int, Item> m_items;
        private Dictionary<int, Move> m_moves;
        private Dictionary<int, PkmnFoundations.Pokedex.Type> m_types;
        private Dictionary<int, Ability> m_abilities;

        public Species Species(int national_dex)
        {
            return m_species[national_dex];
        }

        public Family Families(int id)
        {
            return m_families[id];
        }

        public Form Forms(int id)
        {
            return m_forms[id];
        }

        public SortedDictionary<Generations, FormStats> FormStats(int form_id)
        {
            return m_form_stats[form_id];
        }

        public Item Items(int id)
        {
            return m_items[id];
        }

        public Move Moves(int value)
        {
            return m_moves[value];
        }

        public PkmnFoundations.Pokedex.Type Types(int id)
        {
            return m_types[id];
        }

        public Ability Abilities(int value)
        {
            return m_abilities[value];
        }
    }
}
