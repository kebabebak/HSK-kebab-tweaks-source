using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace HSK.KebabTweaks
{
    /// <summary>
    /// Header clipboard icons (copy / magnifier): hover panel and clipboard on click.
    /// Hover is recorded in the tab scroll; the panel is placed at the end of DrawSettings
    /// from Event.current.mousePosition (outside the scroll group). Inactive icons use reduced
    /// glyph alpha and have no tooltip.
    ///
    /// Иконки буфера в заголовке (копирование / лупа): панель при наведении и клик в буфер.
    /// Hover запоминается внутри scroll вкладки; панель ставится в конце DrawSettings из
    /// Event.current.mousePosition (вне scroll group). Неактивные иконки — пониженная альфа
    /// глифа, без тултипа.
    /// </summary>
    public static class FixErrorTraceUi
    {
        public const float CopyButtonSize = 24f;
        public const float CopyButtonGap = 4f;

        private static readonly Color IconDisabledTint = new Color(1f, 1f, 1f, 0.35f);

        private static Texture2D cachedSearchIcon;

        private const float TooltipMaxWidth = 560f;
        private const float TooltipMinWidth = 280f;
        private const float TooltipMaxHeight = 420f;
        private const float TooltipPadding = 8f;
        private const float TooltipOffset = 16f;
        private const float BorderThickness = 1f;
        private const float BoundsInset = 4f;
        private const float DialogChromeBelowAllowance = 72f;

        private static readonly Color PanelFillColor = new Color(0f, 0f, 0f, 0.22f);
        private static readonly Color PanelBorderColor = new Color(0.55f, 0.55f, 0.55f, 0.8f);

        private static string activePanelText;
        private static bool hasActiveHover;
        private static Rect drawBoundsGui;
        private static bool hasDrawBoundsGui;

        /// <summary>
        /// Horizontal clip follows settings inRect; vertical expands like vanilla tips
        /// (may cover tabs/reset above and dialog Close below the settings body).
        ///
        /// Горизонтальный clip — settings inRect; по вертикали как vanilla tips
        /// (можно накрывать вкладки/сброс сверху и Close диалога ниже тела настроек).
        /// </summary>
        public static void SetDrawBounds(Rect settingsInRect)
        {
            drawBoundsGui = settingsInRect;
            hasDrawBoundsGui = true;
        }

        /// <summary>
        /// Kept for call sites in scroll content; clip is settings bounds, not the tab scroll rect.
        ///
        /// Оставлен для вызовов из scroll; clip — bounds настроек, не rect вкладки.
        /// </summary>
        public static void SetScrollContext(Rect tabContentRect, Vector2 tabScrollPosition)
        {
        }

        public static void BeginHoverFrame()
        {
            activePanelText = null;
            hasActiveHover = false;
            hasDrawBoundsGui = false;
        }

        /// <summary>
        /// Magnifier used on patch and fix headers. TexButton.Search when present, else the
        /// vanilla search atlas, else Info.
        ///
        /// Лупа в заголовках патчей и фиксов. TexButton.Search если есть, иначе атлас поиска,
        /// иначе Info.
        /// </summary>
        public static Texture2D SearchIcon
        {
            get
            {
                if (cachedSearchIcon != null)
                {
                    return cachedSearchIcon;
                }

                cachedSearchIcon = AccessToolsSearchIcon();
                if (cachedSearchIcon == null)
                {
                    cachedSearchIcon = ContentFinder<Texture2D>.Get("UI/Buttons/Search", reportFailure: false);
                }

                if (cachedSearchIcon == null)
                {
                    cachedSearchIcon = TexButton.Info;
                }

                return cachedSearchIcon;
            }
        }

        /// <summary>
        /// Draws a header clipboard icon. Empty payload or clickDisabled: same glyph at reduced
        /// alpha, no click, no hover panel.
        ///
        /// Рисует иконку буфера в заголовке. Пустой текст или clickDisabled: тот же глиф с
        /// пониженной альфой, без клика и без панели.
        /// </summary>
        public static void DrawHeaderIconButton(Rect rect, Texture2D icon, string clipboardText,
            bool clickDisabled = false)
        {
            bool active = !clickDisabled && !clipboardText.NullOrEmpty() && icon != null;
            if (!active)
            {
                DrawDisabledHeaderIcon(rect, icon);
                SwallowMouseOn(rect);
                return;
            }

            if (Widgets.ButtonImage(rect, icon, true))
            {
                GUIUtility.systemCopyBuffer = clipboardText;
                SoundDefOf.Click.PlayOneShotOnCamera();
            }

            if (Mouse.IsOver(rect))
            {
                activePanelText = clipboardText;
                hasActiveHover = true;
            }
        }

        /// <summary>
        /// Draws the hovered clipboard panel in settings GUI space (call once at end of DrawSettings).
        ///
        /// Рисует панель буфера в GUI space настроек (один раз в конце DrawSettings).
        /// </summary>
        public static void DrawHoverPanelIfNeeded()
        {
            if (activePanelText.NullOrEmpty() || !hasActiveHover || Event.current.type != EventType.Repaint)
            {
                return;
            }

            Vector2 mouseGui = Event.current.mousePosition;
            Rect panelRect = CalcPanelRect(activePanelText, mouseGui);
            DrawTraceTooltipPanelContents(activePanelText, panelRect);
        }

        public static float CopyButtonReservedWidth =>
            CopyButtonSize + CopyButtonGap;

        public static float HeaderIconsReservedWidth(int iconCount)
        {
            if (iconCount <= 0)
            {
                return 0f;
            }

            return iconCount * CopyButtonReservedWidth;
        }

        private static Texture2D AccessToolsSearchIcon()
        {
            var field = HarmonyLib.AccessTools.Field(typeof(TexButton), "Search");
            if (field == null)
            {
                return null;
            }

            return field.GetValue(null) as Texture2D;
        }

        private static void DrawDisabledHeaderIcon(Rect rect, Texture2D icon)
        {
            if (icon == null || Event.current.type != EventType.Repaint)
            {
                return;
            }

            Color previous = GUI.color;
            GUI.color = IconDisabledTint;
            GUI.DrawTexture(rect, icon);
            GUI.color = previous;
        }

        private static void SwallowMouseOn(Rect rect)
        {
            if (!Mouse.IsOver(rect))
            {
                return;
            }

            Event current = Event.current;
            if (current.type == EventType.MouseDown || current.type == EventType.MouseUp ||
                current.type == EventType.MouseDrag)
            {
                current.Use();
            }
        }

        private static Rect GetDrawBounds()
        {
            if (!hasDrawBoundsGui)
            {
                return new Rect(0f, 0f, UI.screenWidth, UI.screenHeight);
            }

            Rect bounds = drawBoundsGui;
            bounds.yMin = 0f;
            bounds.yMax += DialogChromeBelowAllowance;
            return bounds;
        }

        private static Rect CalcPanelRect(string trace, Vector2 mouseGui)
        {
            GameFont previousFont = Text.Font;
            TextAnchor previousAnchor = Text.Anchor;
            try
            {
                Text.Font = GameFont.Tiny;
                Text.Anchor = TextAnchor.UpperLeft;

                Rect bounds = GetDrawBounds();
                float maxWidth = Mathf.Min(
                    TooltipMaxWidth,
                    Mathf.Max(TooltipMinWidth, bounds.width - BoundsInset * 2f));

                float innerWidth = maxWidth - TooltipPadding * 2f - BorderThickness * 2f;
                float textHeight = Text.CalcHeight(trace, innerWidth);
                float innerHeight = Mathf.Min(textHeight, TooltipMaxHeight - TooltipPadding * 2f);
                float panelWidth = maxWidth;
                float panelHeight = innerHeight + TooltipPadding * 2f + BorderThickness * 2f;

                if (TryFindPanelRect(panelWidth, panelHeight, mouseGui, bounds, out Rect found))
                {
                    return found;
                }

                return ClampPanelToBounds(
                    new Rect(mouseGui.x + TooltipOffset, mouseGui.y + TooltipOffset, panelWidth, panelHeight),
                    panelWidth,
                    panelHeight,
                    bounds);
            }
            finally
            {
                Text.Font = previousFont;
                Text.Anchor = previousAnchor;
            }
        }

        /// <summary>
        /// Vanilla ActiveTip-style placement: prefer below-right of mouse; flip left/above at edges.
        ///
        /// Как ActiveTip: сначала ниже-справа от мыши; у краёв — слева / сверху.
        /// </summary>
        private static bool TryFindPanelRect(
            float panelWidth,
            float panelHeight,
            Vector2 mouse,
            Rect bounds,
            out Rect panelRect)
        {
            Vector2 belowRight = new Vector2(TooltipOffset, TooltipOffset);
            Vector2 belowLeft = new Vector2(-panelWidth - TooltipOffset, TooltipOffset);
            Vector2 aboveRight = new Vector2(TooltipOffset, -panelHeight - TooltipOffset);
            Vector2 aboveLeft = new Vector2(-panelWidth - TooltipOffset, -panelHeight - TooltipOffset);

            bool nearRight = mouse.x + panelWidth + TooltipOffset > bounds.xMax - BoundsInset;
            bool nearBottom = mouse.y + panelHeight + TooltipOffset > bounds.yMax - BoundsInset;

            Vector2[] offsets;
            if (nearRight && nearBottom)
            {
                offsets = new[] { aboveLeft, belowLeft, aboveRight, belowRight };
            }
            else if (nearRight)
            {
                offsets = new[] { belowLeft, aboveLeft, belowRight, aboveRight };
            }
            else if (nearBottom)
            {
                offsets = new[] { aboveRight, aboveLeft, belowRight, belowLeft };
            }
            else
            {
                offsets = new[] { belowRight, belowLeft, aboveRight, aboveLeft };
            }

            for (int i = 0; i < offsets.Length; i++)
            {
                Rect candidate = new Rect(mouse.x + offsets[i].x, mouse.y + offsets[i].y, panelWidth, panelHeight);
                if (!FitsInBounds(candidate, bounds))
                {
                    continue;
                }

                panelRect = candidate;
                return true;
            }

            panelRect = default;
            return false;
        }

        /// <summary>
        /// Strict on X and bottom; top of settings body is soft (overflow into window chrome OK).
        ///
        /// Жёстко по X и низу; верх тела настроек мягкий (выход в chrome окна допустим).
        /// </summary>
        private static bool FitsInBounds(Rect candidate, Rect bounds)
        {
            if (candidate.xMin < bounds.x + BoundsInset
                || candidate.xMax > bounds.xMax - BoundsInset
                || candidate.yMax > bounds.yMax - BoundsInset)
            {
                return false;
            }

            if (candidate.yMax < BoundsInset)
            {
                return false;
            }

            return true;
        }

        private static Rect ClampPanelToBounds(Rect panelRect, float panelWidth, float panelHeight, Rect bounds)
        {
            panelRect.x = Mathf.Clamp(
                panelRect.x,
                bounds.x + BoundsInset,
                Mathf.Max(bounds.x + BoundsInset, bounds.xMax - panelWidth - BoundsInset));

            float maxY = bounds.yMax - panelHeight - BoundsInset;
            if (panelRect.y > maxY)
            {
                panelRect.y = maxY;
            }

            return panelRect;
        }

        private static void DrawTraceTooltipPanelContents(string trace, Rect panelRect)
        {
            GameFont previousFont = Text.Font;
            TextAnchor previousAnchor = Text.Anchor;
            try
            {
                Text.Font = GameFont.Tiny;
                Text.Anchor = TextAnchor.UpperLeft;

                float innerWidth = panelRect.width - TooltipPadding * 2f - BorderThickness * 2f;
                float innerHeight = panelRect.height - TooltipPadding * 2f - BorderThickness * 2f;

                DrawGrayBorderPanel(panelRect);

                Rect textViewRect = new Rect(
                    panelRect.x + BorderThickness + TooltipPadding,
                    panelRect.y + BorderThickness + TooltipPadding,
                    innerWidth,
                    innerHeight);

                Widgets.Label(textViewRect, trace);
            }
            finally
            {
                Text.Font = previousFont;
                Text.Anchor = previousAnchor;
            }
        }

        /// <summary>
        /// Dimmed fill with gray border on all sides (no rainbow top).
        ///
        /// Затемнённая заливка и серая рамка со всех сторон (без радужного верха).
        /// </summary>
        private static void DrawGrayBorderPanel(Rect panel)
        {
            if (Event.current.type != EventType.Repaint)
            {
                return;
            }

            Color previousColor = GUI.color;
            GUI.color = PanelFillColor;
            GUI.DrawTexture(panel, BaseContent.WhiteTex);

            GUI.color = PanelBorderColor;
            Texture2D tex = BaseContent.WhiteTex;
            GUI.DrawTexture(new Rect(panel.x, panel.y, panel.width, BorderThickness), tex);
            GUI.DrawTexture(new Rect(panel.x, panel.yMax - BorderThickness, panel.width, BorderThickness), tex);
            float sideHeight = Mathf.Max(0f, panel.height - BorderThickness * 2f);
            GUI.DrawTexture(new Rect(panel.x, panel.y + BorderThickness, BorderThickness, sideHeight), tex);
            GUI.DrawTexture(new Rect(panel.xMax - BorderThickness, panel.y + BorderThickness, BorderThickness, sideHeight), tex);
            GUI.color = previousColor;
        }
    }
}
