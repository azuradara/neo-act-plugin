using System.Text.RegularExpressions;

namespace NeoActPlugin.Test
{
    [TestClass]
    public class LogParseRegex
    {
        [TestMethod]
        public void LogParse_Regex_yourdamage_simple()
        {
            string testLine = "Blazing Palm hit Blackram Landing Soldier for 413 damage.";

            Match m = LogParse.regex_yourdamage.Match(testLine);

            Assert.IsTrue(m.Groups["skill"].Success && m.Groups["skill"].Value == "Blazing Palm");
            Assert.IsTrue(m.Groups["critical"].Success && m.Groups["critical"].Value == "hit");
            Assert.IsTrue(m.Groups["target"].Success && m.Groups["target"].Value == "Blackram Landing Soldier");
            Assert.IsTrue(m.Groups["damage"].Success && m.Groups["damage"].Value == "413");
            Assert.IsFalse(m.Groups["HPDrain"].Success);
            Assert.IsFalse(m.Groups["FocusDrain"].Success);
            Assert.IsFalse(m.Groups["skillremove"].Success);
        }

        [TestMethod]
        public void LogParse_Regex_yourdamage_critical()
        {
            string testLine = "Blazing Palm critically hit Blackram Landing Soldier for 736 damage.";

            Match m = LogParse.regex_yourdamage.Match(testLine);

            Assert.IsTrue(m.Groups["skill"].Success && m.Groups["skill"].Value == "Blazing Palm");
            Assert.IsTrue(m.Groups["critical"].Success && m.Groups["critical"].Value == "critically hit");
            Assert.IsTrue(m.Groups["target"].Success && m.Groups["target"].Value == "Blackram Landing Soldier");
            Assert.IsTrue(m.Groups["damage"].Success && m.Groups["damage"].Value == "736");
            Assert.IsFalse(m.Groups["HPDrain"].Success);
            Assert.IsFalse(m.Groups["FocusDrain"].Success);
            Assert.IsFalse(m.Groups["skillremove"].Success);
        }
        [TestMethod]
        public void LogParse_Regex_yourdamage_drain()
        {
            string testLine = "Dragonwhorl hit Sajifi for 283 damage, draining 283 HP and 1 Focus.";

            Match m = LogParse.regex_yourdamage.Match(testLine);

            Assert.IsTrue(m.Groups["skill"].Success && m.Groups["skill"].Value == "Dragonwhorl");
            Assert.IsTrue(m.Groups["critical"].Success && m.Groups["critical"].Value == "hit");
            Assert.IsTrue(m.Groups["target"].Success && m.Groups["target"].Value == "Sajifi");
            Assert.IsTrue(m.Groups["damage"].Success && m.Groups["damage"].Value == "283");
            Assert.IsTrue(m.Groups["HPDrain"].Success && m.Groups["HPDrain"].Value == "283");
            Assert.IsTrue(m.Groups["FocusDrain"].Success && m.Groups["FocusDrain"].Value == "1");
            Assert.IsFalse(m.Groups["skillremove"].Success);
        }


        [TestMethod]
        public void LogParse_Regex_debuff2_simple()
        {
            string testLine = "Blazing Palm hit Blackram Landing Soldier and inflicted Ember.";

            Match m = LogParse.regex_debuff2.Match(testLine);

            Assert.IsFalse(m.Groups["actor"].Success);
            Assert.IsTrue(m.Groups["skill"].Success && m.Groups["skill"].Value == "Blazing Palm");
            Assert.IsTrue(m.Groups["critical"].Success && m.Groups["critical"].Value == "hit");
            Assert.IsTrue(m.Groups["target"].Success && m.Groups["target"].Value == "Blackram Landing Soldier");
            Assert.IsTrue(m.Groups["debuff"].Success && m.Groups["debuff"].Value == "Ember");
            Assert.IsFalse(m.Groups["debuff2"].Success);
        }

        [TestMethod]
        public void LogParse_Regex_debuff2_resisted()
        {
            string testLine = "PLAYERNAME&apos;s Dragonfrost hit Mercenary Enforcer but Chill was resisted.";

            Match m = LogParse.regex_debuff2.Match(testLine);

            Assert.IsTrue(m.Groups["actor"].Success && m.Groups["actor"].Value == "PLAYERNAME");
            Assert.IsTrue(m.Groups["skill"].Success && m.Groups["skill"].Value == "Dragonfrost");
            Assert.IsTrue(m.Groups["critical"].Success && m.Groups["critical"].Value == "hit");
            Assert.IsTrue(m.Groups["target"].Success && m.Groups["target"].Value == "Mercenary Enforcer");
            Assert.IsFalse(m.Groups["debuff"].Success);
            Assert.IsTrue(m.Groups["debuff2"].Success && m.Groups["debuff2"].Value == "Chill");
        }

        [TestMethod]
        public void LogParse_Regex_debuff2_nodamage_inflicted()
        {
            string testLine = "PLAYER NAME&apos;s Leading Palm inflicted Bleed.";

            Match m = LogParse.regex_debuff2.Match(testLine);

            Assert.IsTrue(m.Groups["actor"].Success && m.Groups["actor"].Value == "PLAYER NAME");
            Assert.IsTrue(m.Groups["skill"].Success && m.Groups["skill"].Value == "Leading Palm");
            Assert.IsFalse(m.Groups["critical"].Success);
            Assert.IsFalse(m.Groups["target"].Success);
            Assert.IsTrue(m.Groups["debuff"].Success && m.Groups["debuff"].Value == "Bleed");
            Assert.IsFalse(m.Groups["debuff2"].Success);
        }
        //
        [TestMethod]
        public void LogParse_Regex_debuff_simple()
        {
            string testLine = "Assault Unit Mercenary receives Chill.";

            Match m = LogParse.regex_debuff.Match(testLine);

            Assert.IsTrue(m.Groups["target"].Success && m.Groups["target"].Value == "Assault Unit Mercenary");
            Assert.IsTrue(m.Groups["skill"].Success && m.Groups["skill"].Value == "Chill");
        }


        [TestMethod]
        public void LogParse_Regex_incomingdamage1_simple()
        {
            string testLine = "Assault Unit Mercenary received 1384 damage from PLAYERNAME&apos;s Sundering Sword.";

            Match m = LogParse.regex_incomingdamage1.Match(testLine);

            Assert.IsTrue(m.Groups["target"].Success && m.Groups["target"].Value == "Assault Unit Mercenary");
            Assert.IsTrue(m.Groups["damage"].Success && m.Groups["damage"].Value == "1384");
            Assert.IsFalse(m.Groups["critical"].Success);
            Assert.IsFalse(m.Groups["HPDrain"].Success);
            Assert.IsFalse(m.Groups["FocusDrain"].Success);
            Assert.IsFalse(m.Groups["debuff"].Success);
            Assert.IsFalse(m.Groups["resistdebuff"].Success);
            Assert.IsTrue(m.Groups["actor"].Success && m.Groups["actor"].Value == "PLAYERNAME");
            Assert.IsTrue(m.Groups["skill"].Success && m.Groups["skill"].Value == "Sundering Sword");
        }

        [TestMethod]
        public void LogParse_Regex_incomingdamage1_unknown()
        {
            const string testLine = "Infernal Lord received 777 damage from Bleed.";

            Match m = LogParse.regex_incomingdamage1.Match(testLine);

            Assert.IsTrue(m.Groups["target"].Success && m.Groups["target"].Value == "Infernal Lord");
            Assert.IsTrue(m.Groups["damage"].Success && m.Groups["damage"].Value == "777");
            Assert.IsFalse(m.Groups["critical"].Success);
            Assert.IsFalse(m.Groups["HPDrain"].Success);
            Assert.IsFalse(m.Groups["FocusDrain"].Success);
            Assert.IsFalse(m.Groups["debuff"].Success);
            Assert.IsFalse(m.Groups["resistdebuff"].Success);
            Assert.IsTrue(m.Groups["skill"].Success && m.Groups["skill"].Value == "Bleed");
            Assert.IsFalse(m.Groups["actor"].Success);

        }

        [TestMethod]
        public void LogParse_Regex_incomingdamage1_HPDrain_Crit()
        {
            string testLine = "Deputy Mercenary received 1889 Critical Damage and 963 HP drain from PLAYERNAME&apos;s Poison Breath.";

            Match m = LogParse.regex_incomingdamage1.Match(testLine);

            Assert.IsTrue(m.Groups["target"].Success && m.Groups["target"].Value == "Deputy Mercenary");
            Assert.IsTrue(m.Groups["damage"].Success && m.Groups["damage"].Value == "1889");
            Assert.IsTrue(m.Groups["critical"].Success && m.Groups["critical"].Value == "Critical");
            Assert.IsTrue(m.Groups["HPDrain"].Success && m.Groups["HPDrain"].Value == "963");
            Assert.IsFalse(m.Groups["FocusDrain"].Success);
            Assert.IsFalse(m.Groups["debuff"].Success);
            Assert.IsFalse(m.Groups["resistdebuff"].Success);
            Assert.IsTrue(m.Groups["actor"].Success && m.Groups["actor"].Value == "PLAYERNAME");
            Assert.IsTrue(m.Groups["skill"].Success && m.Groups["skill"].Value == "Poison Breath");

        }
        [TestMethod]
        public void LogParse_Regex_incomingdamage1_FocusDrain()
        {
            string testLine = "Ferocious Attack Bear received 1130 damage and 1 Focus drain from PLAYERNAME&apos;s Blazing Palm.";

            Match m = LogParse.regex_incomingdamage1.Match(testLine);

            Assert.IsTrue(m.Groups["target"].Success && m.Groups["target"].Value == "Ferocious Attack Bear");
            Assert.IsTrue(m.Groups["damage"].Success && m.Groups["damage"].Value == "1130");
            Assert.IsFalse(m.Groups["critical"].Success);
            Assert.IsTrue(m.Groups["FocusDrain"].Success && m.Groups["FocusDrain"].Value == "1");
            Assert.IsFalse(m.Groups["HPDrain"].Success);
            Assert.IsFalse(m.Groups["debuff"].Success);
            Assert.IsFalse(m.Groups["resistdebuff"].Success);
            Assert.IsTrue(m.Groups["actor"].Success && m.Groups["actor"].Value == "PLAYERNAME");
            Assert.IsTrue(m.Groups["skill"].Success && m.Groups["skill"].Value == "Blazing Palm");

        }
        [TestMethod]
        public void LogParse_Regex_incomingdamage1_damage_knockdown()
        {
            string testLine = "Assassin Guard received 287 damage and Knockdown from PLAYERNAME&apos;s Leg Sweep.";

            Match m = LogParse.regex_incomingdamage1.Match(testLine);

            Assert.IsTrue(m.Groups["target"].Success && m.Groups["target"].Value == "Assassin Guard");
            Assert.IsTrue(m.Groups["damage"].Success && m.Groups["damage"].Value == "287");
            Assert.IsFalse(m.Groups["critical"].Success);
            Assert.IsFalse(m.Groups["FocusDrain"].Success);
            Assert.IsFalse(m.Groups["HPDrain"].Success);
            Assert.IsTrue(m.Groups["debuff"].Success && m.Groups["debuff"].Value == "Knockdown");
            Assert.IsFalse(m.Groups["resistdebuff"].Success);
            Assert.IsTrue(m.Groups["actor"].Success && m.Groups["actor"].Value == "PLAYERNAME");
            Assert.IsTrue(m.Groups["skill"].Success && m.Groups["skill"].Value == "Leg Sweep");
        }
        [TestMethod]
        public void LogParse_Regex_incomingdamage1_damage_complex()
        {
            string testLine = "Sac Spider received 346 damage, 36 HP drain, and 1 Focus drain from PLAYERNAME&apos;s Blazing Palm. ";

            Match m = LogParse.regex_incomingdamage1.Match(testLine);

            Assert.IsTrue(m.Groups["target"].Success && m.Groups["target"].Value == "Sac Spider");
            Assert.IsTrue(m.Groups["damage"].Success && m.Groups["damage"].Value == "346");
            Assert.IsFalse(m.Groups["critical"].Success);
            Assert.IsTrue(m.Groups["FocusDrain"].Success && m.Groups["FocusDrain"].Value == "1");
            Assert.IsTrue(m.Groups["HPDrain"].Success && m.Groups["HPDrain"].Value == "36");
            Assert.IsFalse(m.Groups["debuff"].Success);
            Assert.IsFalse(m.Groups["resistdebuff"].Success);
            Assert.IsTrue(m.Groups["actor"].Success && m.Groups["actor"].Value == "PLAYERNAME");
            Assert.IsTrue(m.Groups["skill"].Success && m.Groups["skill"].Value == "Blazing Palm");
        }

        [TestMethod]
        public void LogParse_Regex_incomingdamage1_resist()
        {
            string testLine = "Cobalt Widow received 1543 damage from PLAYERNAME&apos;s Strike but resisted Daze.";

            Match m = LogParse.regex_incomingdamage1.Match(testLine);

            Assert.IsTrue(m.Groups["target"].Success && m.Groups["target"].Value == "Cobalt Widow");
            Assert.IsTrue(m.Groups["damage"].Success && m.Groups["damage"].Value == "1543");
            Assert.IsFalse(m.Groups["critical"].Success);
            Assert.IsFalse(m.Groups["HPDrain"].Success);
            Assert.IsFalse(m.Groups["FocusDrain"].Success);
            Assert.IsFalse(m.Groups["debuff"].Success);
            Assert.IsTrue(m.Groups["resistdebuff"].Success && m.Groups["resistdebuff"].Value == "Daze");
            Assert.IsTrue(m.Groups["actor"].Success && m.Groups["actor"].Value == "PLAYERNAME");
            Assert.IsTrue(m.Groups["skill"].Success && m.Groups["skill"].Value == "Strike");

        }
        [TestMethod]
        public void LogParse_Regex_incomingdamage1_resist2()
        {
            string testLine = "Scarlet Widow received 1943 Critical Damage from PLAYERNAME&apos;s Strike, but resisted Daze effect.";

            Match m = LogParse.regex_incomingdamage1.Match(testLine);

            Assert.IsTrue(m.Groups["target"].Success && m.Groups["target"].Value == "Scarlet Widow");
            Assert.IsTrue(m.Groups["damage"].Success && m.Groups["damage"].Value == "1943");
            Assert.IsTrue(m.Groups["critical"].Success && m.Groups["critical"].Value == "Critical");
            Assert.IsFalse(m.Groups["HPDrain"].Success);
            Assert.IsFalse(m.Groups["FocusDrain"].Success);
            Assert.IsFalse(m.Groups["debuff"].Success);
            Assert.IsTrue(m.Groups["resistdebuff"].Success && m.Groups["resistdebuff"].Value == "Daze");
            Assert.IsTrue(m.Groups["actor"].Success && m.Groups["actor"].Value == "PLAYERNAME");
            Assert.IsTrue(m.Groups["skill"].Success && m.Groups["skill"].Value == "Strike");

        }

        [TestMethod]
        public void LogParse_Regex_incomingdamage2_Blocked()
        {
            string testLine = "Blocked Mercenary Enforcer&apos;s Shuriken but received 247 damage.";

            Match m = LogParse.regex_incomingdamage2.Match(testLine);

            Assert.IsFalse(m.Groups["target"].Success);
            Assert.IsTrue(m.Groups["actor"].Success && m.Groups["actor"].Value == "Mercenary Enforcer");
            Assert.IsTrue(m.Groups["skill"].Success && m.Groups["skill"].Value == "Shuriken");
            Assert.IsTrue(m.Groups["damage"].Success && m.Groups["damage"].Value == "247");
            Assert.IsFalse(m.Groups["HPDrain"].Success);
            Assert.IsFalse(m.Groups["debuff"].Success);
            Assert.IsFalse(m.Groups["HPDrain"].Success);
        }

        [TestMethod]
        public void LogParse_Regex_incomingdamage2_targetdebuff()
        {
            string testLine = "PLAYERNAME blocked Mercenary Enforcer&apos;s Shadow Slash but received Shadow.";

            Match m = LogParse.regex_incomingdamage2.Match(testLine);

            Assert.IsTrue(m.Groups["target"].Success && m.Groups["target"].Value == "PLAYERNAME");
            Assert.IsTrue(m.Groups["actor"].Success && m.Groups["actor"].Value == "Mercenary Enforcer");
            Assert.IsTrue(m.Groups["skill"].Success && m.Groups["skill"].Value == "Shadow Slash");
            Assert.IsFalse(m.Groups["damage"].Success);
            Assert.IsFalse(m.Groups["HPDrain"].Success);
            Assert.IsTrue(m.Groups["debuff"].Success && m.Groups["debuff"].Value == "Shadow");
        }

        [TestMethod]
        public void LogParse_Regex_incomingdamage2_targetdebuff_knockback()
        {
            string testLine = "Blocked Sajifi&apos;s Elite Beastmaster&apos;s Searing Strike but received 326 damage and Knockback.";

            Match m = LogParse.regex_incomingdamage2.Match(testLine);

            Assert.IsFalse(m.Groups["target"].Success);
            Assert.IsTrue(m.Groups["actor"].Success && m.Groups["actor"].Value == "Sajifi&apos;s Elite Beastmaster");
            Assert.IsTrue(m.Groups["skill"].Success && m.Groups["skill"].Value == "Searing Strike");
            Assert.IsTrue(m.Groups["damage"].Success && m.Groups["damage"].Value == "326");
            Assert.IsFalse(m.Groups["HPDrain"].Success);
            Assert.IsTrue(m.Groups["debuff"].Success && m.Groups["debuff"].Value == "Knockback");
        }
        [TestMethod]
        public void LogParse_Regex_incomingdamage2_counter()
        {
            string testLine = "PLAYER NAME countered Cobalt Widow&apos;s Frost Fury but received 216 damage.";

            Match m = LogParse.regex_incomingdamage2.Match(testLine);

            Assert.IsTrue(m.Groups["target"].Success && m.Groups["target"].Value == "PLAYER NAME");
            Assert.IsTrue(m.Groups["actor"].Success && m.Groups["actor"].Value == "Cobalt Widow");
            Assert.IsTrue(m.Groups["skill"].Success && m.Groups["skill"].Value == "Frost Fury");
            Assert.IsTrue(m.Groups["damage"].Success && m.Groups["damage"].Value == "216");
            Assert.IsFalse(m.Groups["HPDrain"].Success);
            Assert.IsFalse(m.Groups["debuff"].Success);
        }
        [TestMethod]
        public void LogParse_Regex_incomingdamage2_receiving()
        {
            string testLine = "Engineer Corps Defense Captain partially blocked Breeze receiving 155 damage and 31 HP drain.";

            Match m = LogParse.regex_incomingdamage2.Match(testLine);

            Assert.IsTrue(m.Groups["target"].Success && m.Groups["target"].Value == "Engineer Corps Defense Captain");
            Assert.IsFalse(m.Groups["actor"].Success);
            Assert.IsTrue(m.Groups["skill"].Success && m.Groups["skill"].Value == "Breeze");
            Assert.IsTrue(m.Groups["damage"].Success && m.Groups["damage"].Value == "155");
            Assert.IsTrue(m.Groups["HPDrain"].Success && m.Groups["HPDrain"].Value == "31");
            Assert.IsFalse(m.Groups["debuff"].Success);
        }
        //
        [TestMethod]
        public void LogParse_Regex_incomingdamage3_damage_knockback()
        {
            string testLine = "Sochon Gamyung&apos;s Fury inflicted 3563 damage and Knockback.";

            Match m = LogParse.regex_incomingdamage3.Match(testLine);

            Assert.IsTrue(m.Groups["actor"].Success && m.Groups["actor"].Value == "Sochon Gamyung");
            Assert.IsTrue(m.Groups["skill"].Success && m.Groups["skill"].Value == "Fury");
            Assert.IsTrue(m.Groups["damage"].Success && m.Groups["damage"].Value == "3563");
            Assert.IsTrue(m.Groups["debuff"].Success && m.Groups["debuff"].Value == "Knockback");
            Assert.IsFalse(m.Groups["target"].Success);

            // todo: add test case for target
        }
        [TestMethod]
        public void LogParse_Regex_incomingdamage3_damage_target()
        {
            string testLine = "Blazing Palm&apos;s Lasting Effects inflicted 109 damage to Sochon Gamyung.";

            Match m = LogParse.regex_incomingdamage3.Match(testLine);

            Assert.IsTrue(m.Groups["actor"].Success && m.Groups["actor"].Value == "Blazing Palm");
            Assert.IsTrue(m.Groups["skill"].Success && m.Groups["skill"].Value == "Lasting Effects");
            Assert.IsTrue(m.Groups["damage"].Success && m.Groups["damage"].Value == "109");
            Assert.IsFalse(m.Groups["debuff"].Success);
            Assert.IsTrue(m.Groups["target"].Success && m.Groups["target"].Value == "Sochon Gamyung");

            // todo: add test case for target
        }
        //        //[15:20:48.110]  

        [TestMethod]
        public void LogParse_Regex_incomingdamage3_nodamage()
        {
            string testLine = "Sochon Gamyung&apos;s Stone Shield inflicted Stun.";

            Match m = LogParse.regex_incomingdamage3.Match(testLine);

            Assert.IsTrue(m.Groups["actor"].Success && m.Groups["actor"].Value == "Sochon Gamyung");
            Assert.IsTrue(m.Groups["skill"].Success && m.Groups["skill"].Value == "Stone Shield");
            Assert.IsFalse(m.Groups["damage"].Success);
            Assert.IsTrue(m.Groups["debuff"].Success && m.Groups["debuff"].Value == "Stun");
            Assert.IsFalse(m.Groups["target"].Success);
        }
        //
        [TestMethod]
        public void LogParse_Regex_yourdamage_complex()
        {
            string testLine = "Frost Fury critically hit Silver Deva for 986 damage, draining 591 HP and 1 Focus.";

            Match m = LogParse.regex_yourdamage.Match(testLine);

            Assert.IsTrue(m.Groups["skill"].Success && m.Groups["skill"].Value == "Frost Fury");
            Assert.IsTrue(m.Groups["critical"].Success && m.Groups["critical"].Value == "critically hit");
            Assert.IsTrue(m.Groups["target"].Success && m.Groups["target"].Value == "Silver Deva");
            Assert.IsTrue(m.Groups["damage"].Success && m.Groups["damage"].Value == "986");
            Assert.IsTrue(m.Groups["HPDrain"].Success && m.Groups["HPDrain"].Value == "591");
            Assert.IsTrue(m.Groups["FocusDrain"].Success && m.Groups["FocusDrain"].Value == "1");
            Assert.IsFalse(m.Groups["skillremove"].Success);

            // todo: add test case for skillremove
        }

        [TestMethod]
        public void LogParse_Regex_yourdamage_damage_hpdrain()
        {
            string testLine = "Frost Fury hit Junghado for 545 damage and drained 163 HP.";

            Match m = LogParse.regex_yourdamage.Match(testLine);

            Assert.IsTrue(m.Groups["skill"].Success && m.Groups["skill"].Value == "Frost Fury");
            Assert.IsTrue(m.Groups["critical"].Success && m.Groups["critical"].Value == "hit");
            Assert.IsTrue(m.Groups["target"].Success && m.Groups["target"].Value == "Junghado");
            Assert.IsTrue(m.Groups["damage"].Success && m.Groups["damage"].Value == "545");
            Assert.IsTrue(m.Groups["HPDrain"].Success && m.Groups["HPDrain"].Value == "163");
            Assert.IsFalse(m.Groups["FocusDrain"].Success);
            Assert.IsFalse(m.Groups["skillremove"].Success);
        }

        [TestMethod]
        public void LogParse_Regex_evade_simple()
        {
            string testLine = "PLAYER NAME evaded Scorching Arrow of the Hills.";

            Match m = LogParse.regex_evade.Match(testLine);

            Assert.IsTrue(m.Groups["target"].Success && m.Groups["target"].Value == "PLAYER NAME");
            Assert.IsTrue(m.Groups["skill"].Success && m.Groups["skill"].Value == "Scorching Arrow of the Hills");
        }
        [TestMethod]
        public void LogParse_Regex_defeat_skill()
        {
            string testLine = "Chuluun the Strong was defeated by PLAYER NAME&apos;s Sunder.";

            Match m = LogParse.regex_defeat.Match(testLine);

            Assert.IsTrue(m.Groups["target"].Success && m.Groups["target"].Value == "Chuluun the Strong");
            Assert.IsTrue(m.Groups["actor"].Success && m.Groups["actor"].Value == "PLAYER NAME");
            Assert.IsTrue(m.Groups["skill"].Success && m.Groups["skill"].Value == "Sunder");
        }

        [TestMethod]
        public void LogParse_Regex_defeat_neardeath()
        {
            string testLine = "You were rendered near death by Stalker Jiangshi&apos;s Furious Flurry.";

            Match m = LogParse.regex_defeat.Match(testLine);

            Assert.IsTrue(m.Groups["target"].Success && m.Groups["target"].Value == "You");
            Assert.IsTrue(m.Groups["actor"].Success && m.Groups["actor"].Value == "Stalker Jiangshi");
            Assert.IsTrue(m.Groups["skill"].Success && m.Groups["skill"].Value == "Furious Flurry");
        }
        [TestMethod]
        public void LogParse_Regex_defeat_killed()
        {
            string testLine = "You were killed by Ape King Ogong&apos;s Roar.";

            Match m = LogParse.regex_defeat.Match(testLine);

            Assert.IsTrue(m.Groups["target"].Success && m.Groups["target"].Value == "You");
            Assert.IsTrue(m.Groups["actor"].Success && m.Groups["actor"].Value == "Ape King Ogong");
            Assert.IsTrue(m.Groups["skill"].Success && m.Groups["skill"].Value == "Roar");
        }

        [TestMethod]
        public void LogParse_Regex_heal_simple()
        {
            string testLine = "PLAYERNAME recovered 813 HP with Doom &apos;n&apos; Bloom.";

            Match m = LogParse.regex_heal.Match(testLine);

            Assert.IsTrue(m.Groups["target"].Success && m.Groups["target"].Value == "PLAYERNAME");
            Assert.IsTrue(m.Groups["HPAmount"].Success && m.Groups["HPAmount"].Value == "813");
            Assert.IsFalse(m.Groups["FocusAmount"].Success);
            Assert.IsTrue(m.Groups["skill"].Success && m.Groups["skill"].Value == "Doom &apos;n&apos; Bloom");
        }

        [TestMethod]
        public void LogParse_Regex_heal_you()
        {
            string testLine = "Recovered 1125 HP from Doom &apos;n&apos; Bloom.";

            Match m = LogParse.regex_heal.Match(testLine);

            Assert.IsFalse(m.Groups["target"].Success);
            Assert.IsTrue(m.Groups["HPAmount"].Success && m.Groups["HPAmount"].Value == "1125");
            Assert.IsFalse(m.Groups["FocusAmount"].Success);
            Assert.IsTrue(m.Groups["skill"].Success && m.Groups["skill"].Value == "Doom &apos;n&apos; Bloom");
        }

        [TestMethod]
        public void LogParse_Regex_heal_focus()
        {
            string testLine = "PLAYER NAME recovered 2 Focus from Grab.";

            Match m = LogParse.regex_heal.Match(testLine);

            Assert.IsTrue(m.Groups["target"].Success && m.Groups["target"].Value == "PLAYER NAME");
            Assert.IsFalse(m.Groups["HPAmount"].Success);
            Assert.IsTrue(m.Groups["FocusAmount"].Success && m.Groups["FocusAmount"].Value == "2");
            Assert.IsTrue(m.Groups["skill"].Success && m.Groups["skill"].Value == "Grab");
        }

        [TestMethod]
        public void LogParse_Regex_buff_simple()
        {
            string testLine = "Critical Bonus is now active.";

            Match m = LogParse.regex_buff.Match(testLine);

            Assert.IsTrue(m.Groups["skill"].Success && m.Groups["skill"].Value == "Critical Bonus");
        }
    }
}