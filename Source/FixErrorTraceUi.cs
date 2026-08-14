using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace HSK.KebabTweaks
{
    /// <summary>
    /// Fixes-tab header copy control: custom trace panel tooltip and clipboard on click.
    /// Hover records which trace is active inside the tab scroll view; panel placement uses
    /// Event.current.mousePosition at the end of DrawSettings (outside the scroll
    /// group) — same GUI space as vanilla tips relative to the settings window.
    ///
    /// Hover запоминает активный trace внутри scroll вкладки; позиция панели берёт
    /// Event.current.mousePosition в конце DrawSettings (вне scroll group) —
    /// тот же GUI space, что у vanilla tips относительно окна настроек.
    /// </summary>
    public static class FixErrorTraceUi
    {
        public const float CopyButtonSize = 24f;
        public const float CopyButtonGap = 4f;

        private const float TooltipMaxWidth = 560f;
        private const float TooltipMinWidth = 280f;
        private const float TooltipMaxHeight = 420f;
        private const float TooltipPadding = 8f;
        /// <summary>Same order of magnitude as vanilla ActiveTip mouse offset (~15–18).</summary>
        private const float TooltipOffset = 16f;
        private const float BorderThickness = 1f;
        private const float BoundsInset = 4f;
        /// <summary>
        /// Settings inRect ends above dialog Close/OK; vanilla tips may cover that chrome.
        /// Expanding yMax stops the content bottom from shoving tall panels upward.
        /// </summary>
        private const float DialogChromeBelowAllowance = 72f;

        private static readonly Color PanelFillColor = new Color(0f, 0f, 0f, 0.22f);
        private static readonly Color PanelBorderColor = new Color(0.55f, 0.55f, 0.55f, 0.8f);

        private static string activeTrace;
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

        /// <summary>
        /// Clears hover state at the start of each settings frame.
        ///
        /// Сбрасывает hover в начале каждого кадра настроек.
        /// </summary>
        public static void BeginHoverFrame()
        {
            activeTrace = null;
            hasActiveHover = false;
            hasDrawBoundsGui = false;
        }

        /// <summary>
        /// Draws TexButton.Copy; hover shows a dimmed panel with trace text only.
        ///
        /// Рисует TexButton.Copy; при наведении — затемнённая панель только с текстом trace.
        /// </summary>
        public static void DrawCopyButton(Rect rect, string trace, int tipUniqueId)
        {
            if (trace.NullOrEmpty())
            {
                return;
            }

            if (Widgets.ButtonImage(rect, TexButton.Copy, true))
            {
                GUIUtility.systemCopyBuffer = trace;
                SoundDefOf.Click.PlayOneShotOnCamera();
            }

            if (Mouse.IsOver(rect))
            {
                // Only remember which trace; mouse for placement is read outside the scroll group.
                activeTrace = trace;
                hasActiveHover = true;
            }
        }

        /// <summary>
        /// Draws the hovered trace panel in settings GUI space (call once at end of DrawSettings).
        ///
        /// Рисует панель trace в GUI space настроек (один раз в конце DrawSettings).
        /// </summary>
        public static void DrawHoverPanelIfNeeded()
        {
            if (activeTrace.NullOrEmpty() || !hasActiveHover || Event.current.type != EventType.Repaint)
            {
                return;
            }

            // End of DrawSettings is outside BeginScrollView — mousePosition matches panel draw space
            // (vanilla tips also follow the current GUI mouse, not a scroll-local capture).
            Vector2 mouseGui = Event.current.mousePosition;
            Rect panelRect = CalcPanelRect(activeTrace, mouseGui);
            DrawTraceTooltipPanelContents(activeTrace, panelRect);
        }

        /// <summary>
        /// Width reserved for copy + gap when a trace copy button is shown.
        ///
        /// Ширина под кнопку копирования и зазор, когда trace-кнопка отображается.
        /// </summary>
        public static float CopyButtonReservedWidth =>
            CopyButtonSize + CopyButtonGap;

        private static Rect GetDrawBounds()
        {
            if (!hasDrawBoundsGui)
            {
                return new Rect(0f, 0f, UI.screenWidth, UI.screenHeight);
            }

            Rect bounds = drawBoundsGui;
            // Window GUI origin is above settings body (reset/tabs); allow tip into that chrome.
            bounds.yMin = 0f;
            // inRect.yMax is above Dialog Close — without this, tall tips are shoved upward.
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
        /// Top may leave the settings body (tabs/reset); bottom uses expanded dialog chrome.
        ///
        /// Как ActiveTip: сначала ниже-справа от мыши; у краёв — слева / сверху.
        /// Верх может выходить из тела настроек (вкладки/сброс); низ — расширенный chrome диалога.
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

            // Reject only if the whole panel is above the window GUI top.
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

            // Pull up only when bottom would leave expanded bounds — do not force yMin down into body.
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
