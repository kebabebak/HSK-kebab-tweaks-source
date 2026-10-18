using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using UnityEngine;
using Verse;

namespace HSK.KebabTweaks
{
    /// <summary>
    /// Problem: ArchitectCategoryTab.DoInfoBox calls GenText.TruncateHeight on every UI frame and does not pass a cache.
    /// TruncateHeight drops one character at a time and measures each step with Text.CalcHeight, so a selected building
    /// with a long description drops the frame rate for as long as it stays selected. CalcHeight uses the current font,
    /// anchor, and word wrap.
    ///
    /// Fix: Store the vanilla result for that text, width, height, and font style. The prefix returns false only when
    /// a stored result exists, because a postfix cannot skip the per-character loop. A miss returns true so the original
    /// runs, the text stays vanilla (including "..." when nothing fits), and a caller dictionary is still filled.
    /// On a hit, a caller dictionary receives the stored string only when that text is not already in it.
    /// The hooks stay installed and do nothing while the checkbox is off.
    ///
    /// Проблема: ArchitectCategoryTab.DoInfoBox вызывает GenText.TruncateHeight на каждом кадре интерфейса и не передаёт кэш.
    /// TruncateHeight убирает по одному символу и измеряет каждый шаг через Text.CalcHeight, поэтому выбранная постройка
    /// с длинным описанием снижает частоту кадров, пока она выбрана. CalcHeight использует текущий шрифт, выравнивание
    /// и перенос слов.
    ///
    /// Исправление: Сохраняется ванильный результат для этого текста, ширины, высоты и стиля шрифта. Prefix возвращает
    /// false только когда сохранённый результат уже есть, потому что postfix не может пропустить посимвольный цикл.
    /// Промах возвращает true, исходный метод выполняется, текст остаётся ванильным (включая "...", когда не помещается
    /// ничего), и словарь вызывающего по-прежнему заполняется. При попадании словарь вызывающего получает сохранённую
    /// строку только если этого текста в нём ещё нет. Хуки остаются установленными и ничего не делают, пока чекбокс выключен.
    /// </summary>
    public static class ArchitectInfoBoxFpsFixFeatures
    {
        private static readonly Dictionary<MeasureKey, string> results = new Dictionary<MeasureKey, string>();

        /// <summary>
        /// Installs both TruncateHeight overloads. The hooks stay in place when the checkbox is off.
        ///
        /// Ставит обе перегрузки TruncateHeight. Хуки остаются, когда чекбокс выключен.
        /// </summary>
        public static void Apply(Harmony harmony)
        {
            MethodInfo stringMethod = AccessTools.Method(typeof(GenText), "TruncateHeight", new[]
            {
                typeof(string),
                typeof(float),
                typeof(float),
                typeof(Dictionary<string, string>)
            });
            MethodInfo taggedMethod = AccessTools.Method(typeof(GenText), "TruncateHeight", new[]
            {
                typeof(TaggedString),
                typeof(float),
                typeof(float),
                typeof(Dictionary<string, TaggedString>)
            });
            if (stringMethod == null || taggedMethod == null)
            {
                Log.Warning("[ArchitectInfoBoxFpsFixPatch] TruncateHeight was not found; patch skipped.");
                return;
            }

            harmony.Patch(
                stringMethod,
                prefix: new HarmonyMethod(typeof(ArchitectInfoBoxFpsFixFeatures), nameof(TruncateHeight_String)),
                postfix: new HarmonyMethod(typeof(ArchitectInfoBoxFpsFixFeatures), nameof(TruncateHeight_String_Postfix)));
            harmony.Patch(
                taggedMethod,
                prefix: new HarmonyMethod(typeof(ArchitectInfoBoxFpsFixFeatures), nameof(TruncateHeight_Tagged)),
                postfix: new HarmonyMethod(typeof(ArchitectInfoBoxFpsFixFeatures), nameof(TruncateHeight_Tagged_Postfix)));
            Log.Message("[ArchitectInfoBoxFpsFixPatch] Patches applied.");
        }

        public static bool TruncateHeight_String(
            string str,
            float width,
            float height,
            Dictionary<string, string> cache,
            ref string __result)
        {
            if (!KebabTweaksSettings.EnableArchitectInfoBoxFpsFix)
                return true;
            if (!TryGetStored(str, width, height, out string stored))
                return true;

            __result = stored;
            Remember(cache, str, stored);
            return false;
        }

        public static void TruncateHeight_String_Postfix(string str, float width, float height, string __result)
        {
            if (!KebabTweaksSettings.EnableArchitectInfoBoxFpsFix)
                return;

            Store(str, width, height, __result);
        }

        public static bool TruncateHeight_Tagged(
            TaggedString str,
            float width,
            float height,
            Dictionary<string, TaggedString> cache,
            ref TaggedString __result)
        {
            if (!KebabTweaksSettings.EnableArchitectInfoBoxFpsFix)
                return true;
            if (!TryGetStored(str.RawText, width, height, out string stored))
                return true;

            __result = stored;
            Remember(cache, str.RawText, stored);
            return false;
        }

        public static void TruncateHeight_Tagged_Postfix(TaggedString str, float width, float height, TaggedString __result)
        {
            if (!KebabTweaksSettings.EnableArchitectInfoBoxFpsFix)
                return;

            Store(str.RawText, width, height, __result.RawText);
        }

        /// <summary>
        /// True when this text, width, height, and font style were already measured.
        ///
        /// Истина, если этот текст, ширина, высота и стиль шрифта уже измерялись.
        /// </summary>
        private static bool TryGetStored(string text, float width, float height, out string stored)
        {
            stored = null;
            if (text == null)
                return false;

            return results.TryGetValue(new MeasureKey(text, width, height), out stored);
        }

        /// <summary>
        /// Saves the original result the first time this text and font style are measured.
        ///
        /// Сохраняет результат исходного метода, когда этот текст и стиль шрифта измеряются впервые.
        /// </summary>
        private static void Store(string text, float width, float height, string value)
        {
            if (text == null || value == null)
                return;

            MeasureKey key = new MeasureKey(text, width, height);
            if (!results.ContainsKey(key))
                results.Add(key, value);
        }

        private static void Remember(Dictionary<string, string> cache, string text, string stored)
        {
            if (cache != null && text != null && !cache.ContainsKey(text))
                cache.Add(text, stored);
        }

        private static void Remember(Dictionary<string, TaggedString> cache, string text, string stored)
        {
            if (cache != null && text != null && !cache.ContainsKey(text))
                cache.Add(text, stored);
        }

        private readonly struct MeasureKey : IEquatable<MeasureKey>
        {
            private readonly string text;
            private readonly float width;
            private readonly float height;
            private readonly GameFont font;
            private readonly TextAnchor anchor;
            private readonly bool wordWrap;
            private readonly int hash;

            public MeasureKey(string text, float width, float height)
            {
                this.text = text;
                this.width = width;
                this.height = height;
                font = Text.Font;
                anchor = Text.Anchor;
                wordWrap = Text.WordWrap;
                unchecked
                {
                    int value = text != null ? text.GetHashCode() : 0;
                    value = (value * 397) ^ width.GetHashCode();
                    value = (value * 397) ^ height.GetHashCode();
                    value = (value * 397) ^ (int)font;
                    value = (value * 397) ^ (int)anchor;
                    value = (value * 397) ^ (wordWrap ? 1 : 0);
                    hash = value;
                }
            }

            public bool Equals(MeasureKey other)
            {
                return width == other.width
                    && height == other.height
                    && font == other.font
                    && anchor == other.anchor
                    && wordWrap == other.wordWrap
                    && text == other.text;
            }

            public override bool Equals(object obj)
            {
                return obj is MeasureKey other && Equals(other);
            }

            public override int GetHashCode()
            {
                return hash;
            }
        }
    }
}
