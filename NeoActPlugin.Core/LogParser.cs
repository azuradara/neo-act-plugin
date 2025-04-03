using NeoActPlugin.Common;
using System;
using System.Text.RegularExpressions;

namespace NeoActPlugin.Core
{
    public static class LogParser
    {
        public static Regex regex_incomingdamage1 = new Regex(@"(?<target>.+?)?( received|Received) (?<damage>\d+(,\d+)*) ((?<critical>Critical) )?damage((,)?( and)? (?<HPDrain>\d+(,\d+)*) HP drain)?((,)?( and)? (?<FocusDrain>\d+) Focus drain)?((,)?( and)? (?<debuff>.+?))? from ((?<actor>.+?)&apos;s )?(?<skill>.+?)((,)?( but)? resisted (?<resistdebuff>.+?)( effect)?)?\.", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        public static Regex regex_incomingdamage2 = new Regex(@"((?<target>.+?) )?(Blocked|blocked|partially blocked|countered)( (?<actor>.+)&apos;s)? (?<skill>.+?) (but received|receiving)( (?<damage>\d+(,\d+)*) damage)?(( and)? (?<HPDrain>\d+(,\d+)*) HP drain)?( and?)?( (?<debuff>.+?))?\.", RegexOptions.Compiled);
        public static Regex regex_incomingdamage3 = new Regex(@"(?<actor>.+?)&apos;s (?<skill>.+?) inflicted( (?<damage>\d+(,\d+)*) damage)?( and)?( (?<debuff>.+?))*?( to (?<target>.+?))?\.", RegexOptions.Compiled);
        public static Regex regex_yourdamage = new Regex(@"(?<skill>.+?)\s+(?<critical>(critically hit)|(hit))\s+(?<target>.+?)\s+for\s+(?<damage>\d+(,\d+)*)\s+damage(((, draining| and drained)\s+((?<HPDrain>\d+(,\d+)*)\s+HP)?(\s+and\s+)?((?<FocusDrain>\d+)\s+Focus)?))?(,\s+removing\s+(?<skillremove>.+?))?\.", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        public static Regex regex_debuff2 = new Regex(@"((?<actor>.+?)&apos;s )?(?<skill>.+?)( (?<critical>(critically hit)|(hit)) (?<target>.+?))? ((and )?inflicted (?<debuff>.+?))?(but (?<debuff2>.+?) was resisted)?\.", RegexOptions.Compiled);
        public static Regex regex_evade = new Regex(@"(?<target>.+?) evaded (?<skill>.+?)\.", RegexOptions.Compiled);
        public static Regex regex_defeat = new Regex(@"(?<target>.+?) (was|were) (defeated|rendered near death|rendered Near Death|rendered Near death|killed) by ((?<actor>.+?)&apos;s )?(?<skill>.+?)\.", RegexOptions.Compiled);
        public static Regex regex_debuff = new Regex(@"(?<target>.+?) (receives|resisted) (?<skill>.+?)\.", RegexOptions.Compiled);
        public static Regex regex_heal = new Regex(@"(?<target>.+?)?( recovered|Recovered) ((?<HPAmount>\d+(,\d+)*) HP)?((?<FocusAmount>\d+) Focus)? (with|from) (?<skill>.+?)\.");
        public static Regex regex_buff = new Regex(@"(?<skill>.+?) is now active\.", RegexOptions.Compiled);
        public static Regex regex_resist = new Regex(@"((?<actor>.+?)&apos;s )?(?<skill>.+?)(hit)\s+(?<target>.+?)\s(but was resisted|but  was resisted)", RegexOptions.Compiled);
        public static Regex regex_resist2 = new Regex(@"^Resisted\s+Daze", RegexOptions.Compiled);

        private static IACTWrapper _ACT = null;

        public static void Initialize(IACTWrapper ACT)
        {
            _ACT = ACT;
        }

        public static DateTime ParseLogDateTime(string message)
        {
            DateTime ret = DateTime.MinValue;

            if (_ACT == null)
                throw new ApplicationException("ACT Wrapper not initialized.");

            try
            {
                if (message == null) return ret;

                if (message.Contains("|"))
                {
                    int pipeIndex = message.IndexOf('|');
                    string timestampPart = message.Substring(0, pipeIndex);
                    if (!DateTime.TryParse(timestampPart, out ret))
                    {

                        PluginMain.WriteLog(LogLevel.Error, "Failed to parse timestamp");
                        return DateTime.MinValue;
                    }
                }
            }
            catch (Exception ex)
            {
                PluginMain.WriteLog(LogLevel.Error, "Error [ParseLogDateTime] " + ex.ToString().Replace(Environment.NewLine, " "));
            }
            return ret;
        }

        public static void BeforeLogLineRead(bool isImport, Advanced_Combat_Tracker.LogLineEventArgs logInfo)
        {
            string logLine = logInfo.logLine;

            if (_ACT == null)
                throw new ApplicationException("ACT Wrapper not initialized.");

            try
            {
                DateTime timestamp = ParseLogDateTime(logLine);
                if (logLine.Contains("|"))
                {
                    int pipeIndex = logLine.IndexOf('|');
                    logLine = logLine.Substring(pipeIndex + 1);
                }

                logInfo.logLine = string.Format("[{0:HH:mm:ss.fff}] {1}", timestamp, logLine);

                Match m;

                m = regex_yourdamage.Match(logLine);
                if (m.Success)
                {
                    string actor = "You";
                    string target = m.Groups["target"].Success ? DecodeString(m.Groups["target"].Value) : "";
                    string damage = (m.Groups["damage"].Value ?? "").Replace(",", "");
                    string hpdrain = (m.Groups["HPDrain"].Value ?? "").Replace(",", "");

                    if (_ACT.SetEncounter(timestamp, actor, target))
                    {
                        _ACT.AddCombatAction(
                            (int)Advanced_Combat_Tracker.SwingTypeEnum.NonMelee,
                            m.Groups["critical"].Value == "critically hit",
                            "",
                            actor,
                            DecodeString(m.Groups["skill"].Value),
                            new Advanced_Combat_Tracker.Dnum(int.Parse(damage)),
                            timestamp,
                            _ACT.GlobalTimeSorter,
                            target,
                            "");

                        if (m.Groups["HPDrain"].Success)
                        {
                            _ACT.AddCombatAction(
                                (int)Advanced_Combat_Tracker.SwingTypeEnum.Healing,
                                false,
                                "Drain",
                                actor,
                                DecodeString(m.Groups["skill"].Value),
                                new Advanced_Combat_Tracker.Dnum(int.Parse(hpdrain)),
                                timestamp,
                                _ACT.GlobalTimeSorter,
                                actor,
                                "");
                        }

                    }

                    return;
                }

                m = regex_incomingdamage1.Match(logLine);
                if (!m.Success)
                    m = regex_incomingdamage2.Match(logLine);
                if (!m.Success)
                    m = regex_incomingdamage3.Match(logLine);
                if (m.Success)
                {
                    string target = m.Groups["target"].Success ? DecodeString(m.Groups["target"].Value) : "";
                    if (target == "Unknown")
                        target = "_Unknown";
                    string actor = m.Groups["actor"].Success ? DecodeString(m.Groups["actor"].Value) : "";
                    if (actor == "Unknown")
                        actor = "_Unknown";
                    string skill = m.Groups["skill"].Success ? DecodeString(m.Groups["skill"].Value) : "";
                    string damage = (m.Groups["damage"].Value ?? "").Replace(",", "");
                    string hpdrain = (m.Groups["HPDrain"].Value ?? "").Replace(",", "");

                    // if skillname is blank, the skillname and actor may be transposed
                    if (string.IsNullOrWhiteSpace(skill))
                    {
                        if (!string.IsNullOrWhiteSpace(actor))
                        {
                            // "Received 1373 damage from Rising Blaze&apos;s ."
                            skill = actor;
                        }
                    }

                    // Fix for "Received 1373 damage from Balefire&apos;s Bleed
                    string[] invalidSkills = {
                        "Hellfire",
                        "Venom",
                        "Lasting Effects",
                        "Bleed",
                        "Poison",
                        "Venom Swarm",
                        "Flame Breath&apos;s Bleed",
                        "Flame Breath&apos;s Lasting Effects",
                        "Explosive Rage&apos;s Venom",
                        "Explosive Rage&apos;s Poison",
                    };

                    if (!string.IsNullOrWhiteSpace(actor) && Array.Exists(invalidSkills, e => e == skill))
                    {
                        // using the actor rather than the skill allows users to
                        // recognize their skills by checking Unknown's skill breakdown.
                        // the damage lost here should be negligible in the grand scheme of things

                        skill = actor;
                        actor = "Unknown";
                    }

                    if (string.IsNullOrWhiteSpace(target))
                        target = "You";

                    if (string.IsNullOrWhiteSpace(actor))
                        actor = "Unknown";

                    // todo: in the future, if damage is missing, still parse the buff portion
                    if (!m.Groups["damage"].Success)
                        return;
                    if (_ACT.SetEncounter(timestamp, actor, target))
                    {
                        _ACT.AddCombatAction(
                            (int)Advanced_Combat_Tracker.SwingTypeEnum.NonMelee,
                            m.Groups["critical"].Value == "Critical",
                            "",
                            actor,
                            skill,
                            new Advanced_Combat_Tracker.Dnum(int.Parse(damage)),
                            timestamp,
                            _ACT.GlobalTimeSorter,
                            target,
                            "");

                        if (m.Groups["HPDrain"].Success)
                        {
                            _ACT.AddCombatAction(
                                (int)Advanced_Combat_Tracker.SwingTypeEnum.Healing,
                                false,
                                "Drain",
                                actor,
                                skill,
                                new Advanced_Combat_Tracker.Dnum(int.Parse(hpdrain)),
                                timestamp,
                                _ACT.GlobalTimeSorter,
                                actor,
                                "");
                        }
                    }

                    return;
                }

                m = regex_heal.Match(logLine);
                if (m.Success)
                {
                    string target = m.Groups["target"].Success ? DecodeString(m.Groups["target"].Value) : "";
                    if (string.IsNullOrWhiteSpace(target))
                        target = "You";
                    string actor = "Unknown";

                    // do not process if there is no HP amount.
                    if (!m.Groups["HPAmount"].Success)
                        return;

                    string hpamount = (m.Groups["HPAmount"].Value ?? "").Replace(",", "");

                    if (_ACT.SetEncounter(timestamp, actor, target))
                    {
                        _ACT.AddCombatAction(
                            (int)Advanced_Combat_Tracker.SwingTypeEnum.Healing,
                            false,
                            "",
                            actor,
                            DecodeString(m.Groups["skill"].Value),
                            new Advanced_Combat_Tracker.Dnum(int.Parse(hpamount)),
                            timestamp,
                            _ACT.GlobalTimeSorter,
                            target,
                            "");

                    }
                    return;
                }


                m = regex_debuff2.Match(logLine);
                if (m.Success)
                {
                    // todo: add debuff support
                    return;
                }

                m = regex_debuff.Match(logLine);
                if (m.Success)
                {
                    // todo: add debuff support
                    return;
                }

                m = regex_buff.Match(logLine);
                if (m.Success)
                {
                    // todo: add buff support
                    return;
                }

                m = regex_evade.Match(logLine);
                if (m.Success)
                {
                    // todo: add evade support
                    return;
                }

                m = regex_resist.Match(logLine);
                if (!m.Success)
                    m = regex_resist2.Match(logLine);
                if (m.Success)
                {
                    // todo: add resist support
                    return;
                }

                m = regex_defeat.Match(logLine);
                if (m.Success)
                {
                    // leaving this out for now
                    // causing too many wrong actors to appear

                    /*
                    string target = m.Groups["target"].Success ? DecodeString(m.Groups["target"].Value) : "";
                    if (target == "Unknown")
                        target = "_Unknown";
                    string actor = m.Groups["actor"].Success ? DecodeString(m.Groups["actor"].Value) : "";
                    if (target == "Unknown")
                        actor = "_Unknown";
                    if (string.IsNullOrWhiteSpace(actor))
                        actor = "Unknown";

                    if (_ACT.SetEncounter(timestamp, actor, target))
                    {
                        _ACT.AddCombatAction(
                            (int)Advanced_Combat_Tracker.SwingTypeEnum.NonMelee,
                            false,
                            "",
                            actor,
                            DecodeString(m.Groups["skill"].Value),
                            Advanced_Combat_Tracker.Dnum.Death,
                            timestamp,
                            _ACT.GlobalTimeSorter,
                            target,
                            "");
                    }
                    */

                    return;
                }
            }
            catch (Exception ex)
            {
                string exception = ex.ToString().Replace(Environment.NewLine, " ");
                if (ex.InnerException != null)
                    exception += " " + ex.InnerException.ToString().Replace(Environment.NewLine, " ");

                PluginMain.WriteLog(LogLevel.Error, "Error [LogParse.BeforeLogLineRead] " + exception + " " + logInfo.logLine);
            }

            // For debugging
            if (!string.IsNullOrWhiteSpace(logLine))
                PluginMain.WriteLog(LogLevel.Warning, "Unhandled Line: " + logInfo.logLine);
        }

        private static string DecodeString(string data)
        {
            string ret = data.Replace("&apos;", "'")
                .Replace("&amp;", "&");

            return ret;
        }
    }
}
