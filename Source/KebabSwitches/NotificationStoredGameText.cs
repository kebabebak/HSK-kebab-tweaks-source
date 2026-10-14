using System;
using System.Collections.Generic;
using System.Text;

namespace HSK.KebabTweaks.KebabSwitches
{
    /// <summary>
    /// Game sentences kept in the mod for search when they are not the checkbox label:
    /// letter bodies, the 1.7 long-range lump quest, food-poisoning causes, surgery failure.
    ///
    /// Игровые фразы, которые мод хранит для поиска, когда это не подпись чекбокса:
    /// текст писем, квест далёких залежей в 1.7, причины пищевого отравления, провал операции.
    /// </summary>
    internal static class NotificationStoredGameText
    {
        public static void Append(string id, bool russian, StringBuilder sb, HashSet<string> seen)
        {
            if (id == null)
            {
                return;
            }

            if (id == "RoofCollapsed")
            {
                AppendLine(sb, seen, russian
                    ? "Из-за отсутствия опор рухнула крыша."
                    : "A roof has collapsed because it was too far from any support.");
            }
            else if (id == "FoundPreciousLump")
            {
                if (russian)
                {
                    AppendLine(sb, seen, "При помощи сканера дальнего действия");
                    AppendLine(sb, seen, "обнаруживает неподалёку залежи");
                    AppendLine(sb, seen, "Найдены далёкие залежи");
                }
                else
                {
                    AppendLine(sb, seen, "Using your long-range mineral scanner");
                    AppendLine(sb, seen, "has found a lump");
                    AppendLine(sb, seen, "Distant resource scanned");
                }
            }
            else if (id == "DeepScannerFoundLump")
            {
                AppendLine(sb, seen, russian
                    ? "при помощи глубинного сканера"
                    : "ground-penetrating scanner");
                AppendLine(sb, seen, russian
                    ? "Найдены подземные залежи"
                    : "Scanned underground");
            }
            else if (id == "FoodPoisoning")
            {
                AppendFoodCauses(russian, sb, seen);
            }
            else if (id == "SurgeryFailed")
            {
                if (russian)
                {
                    AppendLine(sb, seen, "Операция над");
                    AppendLine(sb, seen, "Операция провалилась");
                }
                else
                {
                    AppendLine(sb, seen, "Surgery failed on");
                }
            }
            else if (id == "FilledTheMap")
            {
                AppendLine(sb, seen, "You have filled the map.");
            }
            else if (id == "FishCaughtInTrap")
            {
                AppendLine(sb, seen, "Fish Caught in Trap");
            }
            else if (id == "EspInjured")
            {
                AppendLine(sb, seen, "{0} is injured and fleeing!");
            }
            else if (id == "GateEntered")
            {
                AppendLine(sb, seen, russian ? "Логово биошматов" : "Fleshbeast lair");
                AppendLine(sb, seen, russian
                    ? "Хтонические врата привели вас в огромную систему пещер, кишащую биошматами."
                    : "discovered a massive cavern system infested with fleshbeasts");
            }
            else if (id == "OdysseyTurretDisabled")
            {
                AppendLine(sb, seen, russian ? "Турель отключена" : "Turret disabled");
                AppendLine(sb, seen, russian
                    ? "использовал древний охранный терминал для отключения турели"
                    : "used the ancient security terminal to disable a turret");
            }
            else if (id == "OdysseyBlastDoorOpened")
            {
                AppendLine(sb, seen, russian ? "Взрывозащитная дверь открыта" : "Blast door opened");
                AppendLine(sb, seen, russian
                    ? "для открытия взрывозащитной двери"
                    : "used the ancient security terminal to open a blast door");
            }
            else if (id == "SkipAbduction")
            {
                AppendLine(sb, seen, russian ? "Похищение телепортацией" : "Skip abduction");
            }
            else if (id == "FleshbeastsEmerging")
            {
                AppendLine(sb, seen, russian ? "Появление биошматов" : "Fleshbeasts emerging");
                AppendLine(sb, seen, russian
                    ? "Биошматы вырываются из-под земли."
                    : "Fleshbeasts are clawing their way out of the ground.");
            }
            else if (id == "HunterLacksRangedWeapon")
            {
                AppendLine(sb, seen, russian
                    ? "У охотника нет подходящего оружия"
                    : "Hunter lacks suitable weapon");
            }
        }

        /// <summary>
        /// Labels that are def fields, not keyed translations. The English catalog already matches
        /// the English label; these Russian labels must be matched too.
        ///
        /// Подписи из полей def, не из keyed-перевода. Английский каталог уже ловит английскую
        /// подпись; эти русские подписи тоже нужно сопоставлять.
        /// </summary>
        public static void AppendDefLetterLabels(string id, Action<string> addExact)
        {
            if (addExact == null || id == null)
            {
                return;
            }

            if (id == "GateEntered")
            {
                addExact("Логово биошматов");
            }
            else if (id == "OdysseyTurretDisabled")
            {
                addExact("Турель отключена");
            }
            else if (id == "OdysseyBlastDoorOpened")
            {
                addExact("Взрывозащитная дверь открыта");
            }
            else if (id == "SkipAbduction")
            {
                addExact("Похищение телепортацией");
            }
            else if (id == "FleshbeastsEmerging")
            {
                addExact("Появление биошматов");
            }
        }

        private static void AppendFoodCauses(bool russian, StringBuilder sb, HashSet<string> seen)
        {
            if (russian)
            {
                AppendLine(sb, seen, "неопытный повар");
                AppendLine(sb, seen, "грязь на кухне");
                AppendLine(sb, seen, "испорченная еда");
                AppendLine(sb, seen, "опасная пища");
            }
            else
            {
                AppendLine(sb, seen, "incompetent cook");
                AppendLine(sb, seen, "dirty cooking area");
                AppendLine(sb, seen, "rotten food");
                AppendLine(sb, seen, "dangerous food type");
            }
        }

        private static void AppendLine(StringBuilder sb, HashSet<string> seen, string text)
        {
            if (text == null || !seen.Add(text))
            {
                return;
            }

            if (sb.Length > 0)
            {
                sb.Append('\n');
            }

            sb.Append(text);
        }
    }
}
