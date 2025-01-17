﻿using static WikiBrowser.UI.UiConfig;

namespace WikiBrowser.UI.SpecialisedUIElements {
    internal class MainPanel : DragableUiPanel {
        public MainPanel() {
            SetPadding(General.Margin);
            Left.Set(Panel.InitLeft, 0f);
            Top.Set(Panel.InitTop, 0f);
            Width.Set(Panel.Width, 0f);
            Height.Set(Panel.Height, 0f);
            BackgroundColor = General.PanelBgColor;
        }
    }
}