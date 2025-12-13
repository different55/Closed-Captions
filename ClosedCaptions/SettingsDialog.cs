using System;
using System.Collections.Generic;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;

namespace ClosedCaptions;

public class SettingsDialog : GuiDialog
{
    private CaptionsConfig Cfg => CaptionsSystem.Config;
    
    public override string ToggleKeyCombinationCode => "captionssettings";
    
    public SettingsDialog(ICoreClientAPI capi) : base(capi)
    {
        SetupDialog();
    }

    private ElementBounds containerBounds;

    private void InitializeDialog()
    {
        double insetWidth = 950.0;
        double insetHeight = 740.0;

        ElementBounds dialogBounds = ElementStdBounds.AutosizedMainDialog.WithAlignment(EnumDialogArea.CenterMiddle);
        ElementBounds insetBounds = ElementBounds.Fixed(0, GuiStyle.TitleBarHeight, insetWidth, insetHeight);
        ElementBounds scrollbarBounds = insetBounds.RightCopy().WithFixedWidth(20);
        
        ElementBounds clipBounds = insetBounds.ForkContainingChild(GuiStyle.HalfPadding, GuiStyle.HalfPadding, GuiStyle.HalfPadding, GuiStyle.HalfPadding);
        ElementBounds containerBounds = insetBounds.ForkContainingChild(GuiStyle.HalfPadding, GuiStyle.HalfPadding, GuiStyle.HalfPadding);
        
        ElementBounds bgBounds = ElementBounds.Fill
            .WithFixedPadding(GuiStyle.ElementToDialogPadding)
            .WithSizing(ElementSizing.FitToChildren)
            .WithChildren(insetBounds, scrollbarBounds);
        
        SingleComposer = capi.Gui.CreateCompo("captionsSettings", dialogBounds)
            .AddShadedDialogBG(bgBounds)
            .AddDialogTitleBar("Captions", OnTitleBarClose)
            .BeginChildElements()
            .AddInset(clipBounds, 3)
            .BeginClip(clipBounds)
            .AddContainer(containerBounds, "scroll-content")
            .EndClip()
            .AddVerticalScrollbar(OnNewScrollbarValue, scrollbarBounds, "scrollbar")
            .EndChildElements();
        
        GuiElementContainer container = SingleComposer.GetContainer("scroll-content");
                
    }
    
    private void SetupDialog()
    {
        // 1. Layout Calculations
        double col1Width = 150; // Labels
        double col2Width = 200; // Controls
        double padding = GuiStyle.ElementToDialogPadding;
        double contentWidth = col1Width + col2Width;
        double visibleHeight = 500; // Fixed visible height for scrolling area

        // 2. Define Bounds
        ElementBounds dialogBounds = ElementStdBounds.AutosizedMainDialog.WithAlignment(EnumDialogArea.CenterMiddle);
        
        ElementBounds bgBounds = ElementBounds.Fill.WithFixedPadding(padding);
        bgBounds.BothSizing = ElementSizing.FitToChildren;

        // Clip and Container Bounds
        // InsetBounds is the area "reserved" for the list in the dialog
        ElementBounds insetBounds = ElementBounds.Fixed(0, GuiStyle.TitleBarHeight, contentWidth + 20, visibleHeight);
        
        // Explicit ClipBounds
        ElementBounds clipBounds = insetBounds.ForkContainingChild(GuiStyle.HalfPadding, GuiStyle.HalfPadding, GuiStyle.HalfPadding, GuiStyle.HalfPadding);
        
        // ContainerBounds
        containerBounds = ElementBounds.Fixed(0, 0, contentWidth, 0); 
        containerBounds.ParentBounds = clipBounds;
        
        // Scrollbar
        ElementBounds scrollbarBounds = insetBounds.RightCopy().WithFixedWidth(20).WithFixedPadding(0, 0);
        
        // Buttons Row (Fixed at bottom)
        ElementBounds buttonRow = insetBounds.BelowCopy(fixedDeltaY: 10).WithFixedHeight(30);

        // 3. Compose
        SingleComposer = capi.Gui.CreateCompo("captionssettings", dialogBounds)
            .AddShadedDialogBG(bgBounds)
            .AddDialogTitleBar("Captions Settings", OnTitleBarClose)
            .BeginChildElements(bgBounds)
                .AddInset(insetBounds, 3)
                .BeginClip(clipBounds)
                    .AddContainer(containerBounds, "scroll-content")
                .EndClip()
                .AddVerticalScrollbar(OnNewScrollbarValue, scrollbarBounds, "scrollbar")
                .AddSmallButton("Cancel", OnCancel, buttonRow.FlatCopy().WithFixedWidth(80).WithAlignment(EnumDialogArea.LeftFixed))
                .AddSmallButton("Save", OnSave, buttonRow.FlatCopy().WithFixedWidth(80).WithAlignment(EnumDialogArea.RightFixed))
            .EndChildElements();

        // 4. Populate Container
        GuiElementContainer container = SingleComposer.GetContainer("scroll-content");
        double currentY = 0;
        
        // --- Options Section ---
        AddSectionHeader(container, "Options", ref currentY);
        AddSwitch(container, "Enabled", "enabled", Cfg.Enabled, ref currentY);
        AddSwitch(container, "Show Symbols", "showSymbols", Cfg.ShowSymbols, ref currentY);
        AddSwitch(container, "Invert Warnings", "invertedWarnings", Cfg.InvertedWarnings, ref currentY);
        AddSlider(container, "Duration", "duration", Cfg.Duration, 0, 10, ref currentY, true);
        AddNumberInput(container, "Max Captions", "maxCaptions", Cfg.MaxCaptions, ref currentY);

        // --- Colors Section ---
        AddSectionHeader(container, "Colors", ref currentY);
        AddSlider(container, "BG Opacity", "backgroundOpacity", (float)Cfg.BackgroundOpacity, 0, 1, ref currentY);
        AddSlider(container, "Text Opacity", "textOpacity", (float)Cfg.TextOpacity, 0, 1, ref currentY);
        // RGB For Warning
        AddRGBInput(container, "Warning Color", "warnR", "warnG", "warnB", Cfg.WarningRed, Cfg.WarningGreen, Cfg.WarningBlue, ref currentY);
        // RGB For Notice
        AddRGBInput(container, "Notice Color", "noticeR", "noticeG", "noticeB", Cfg.NoticeRed, Cfg.NoticeGreen, Cfg.NoticeBlue, ref currentY);

        // --- Typography Section ---
        AddSectionHeader(container, "Typography", ref currentY);
        AddTextInput(container, "Font", "font", Cfg.Font, ref currentY);
        AddNumberInput(container, "Size", "fontSize", (int)Cfg.FontSize, ref currentY);
        AddSwitch(container, "Bold", "fontBold", Cfg.FontBold, ref currentY);

        // --- Appearance Section ---
        AddSectionHeader(container, "Appearance", ref currentY);
        AddNumberInput(container, "Width", "width", Cfg.Width, ref currentY);
        AddNumberInput(container, "Height", "height", Cfg.Height, ref currentY);
        
        // Position Dropdown
        string[] names = Enum.GetNames(typeof(EnumDialogArea));
        int index = Array.IndexOf(names, Cfg.Position.ToString());
        
        AddLabel(container, "Position", ref currentY); // Label on left
        // Dropdown on right
        ElementBounds dropdownBounds = ElementBounds.Fixed(col1Width, currentY - 30, col2Width, 30).WithParent(containerBounds);
        var dd = new GuiElementDropDown(capi, names, names, index, (code, selected) => { }, dropdownBounds, CairoFont.WhiteDetailText(), false);
        _elements["position"] = dd;
        container.Add(dd);
        
        currentY += 35;

        AddNumberInput(container, "Padding", "padding", (int)Cfg.Padding, ref currentY);

        // Finish Composition
        SingleComposer.Compose();
        
        // 5. Setup Scrollbar Heights
        // Calculate total height based on currentY
        SingleComposer.GetScrollbar("scrollbar").SetHeights((float)clipBounds.fixedHeight, (float)currentY);
        
        // Update container bounds to full height just in case
        containerBounds.fixedHeight = currentY;
    }
    
    private void OnNewScrollbarValue(float value)
    {
        containerBounds.fixedY = -value;
        containerBounds.CalcWorldBounds();
        // Manually update children bounds to ensure they track the scroll
        GuiElementContainer container = SingleComposer.GetContainer("scroll-content");
        foreach(var elem in container.Elements) {
             elem.Bounds.CalcWorldBounds();
        }
    }

    // Helpers
    private void AddSectionHeader(GuiElementContainer container, string text, ref double y)
    {
        y += 15;
        ElementBounds headerBounds = ElementBounds.Fixed(0, y, 350, 25).WithParent(containerBounds);
        container.Add(new GuiElementStaticText(capi, text, EnumTextOrientation.Left, headerBounds, CairoFont.WhiteSmallText().WithWeight(Cairo.FontWeight.Bold)));
        y += 30;
    }

    private void AddSwitch(GuiElementContainer container, string label, string key, bool value, ref double y)
    {
        ElementBounds left = ElementBounds.Fixed(0, y, 150, 30).WithParent(containerBounds);
        ElementBounds right = ElementBounds.Fixed(150, y, 50, 30).WithParent(containerBounds);
        
        container.Add(new GuiElementStaticText(capi, label, EnumTextOrientation.Left, left, CairoFont.WhiteDetailText()));
        
        var sw = new GuiElementSwitch(capi, OnSwitchToggle, right);
        sw.On = value;
        _elements[key] = sw;
        container.Add(sw);
        
        y += 35;
    }
    
    private void OnSwitchToggle(bool on) { } 

    private void AddSlider(GuiElementContainer container, string label, string key, float value, float min, float max, ref double y, bool displayValue = false)
    {
        ElementBounds left = ElementBounds.Fixed(0, y, 120, 30).WithParent(containerBounds);
        ElementBounds right = ElementBounds.Fixed(120, y, 180, 30).WithParent(containerBounds);
        
        container.Add(new GuiElementStaticText(capi, label, EnumTextOrientation.Left, left, CairoFont.WhiteDetailText()));
        
        var slider = new GuiElementSlider(capi, (val) => true, right);
        _elements[key] = slider;
        container.Add(slider);
        
        y += 35;
    }

    private void AddNumberInput(GuiElementContainer container, string label, string key, float value, ref double y)
    {
        ElementBounds left = ElementBounds.Fixed(0, y, 150, 30).WithParent(containerBounds);
        ElementBounds right = ElementBounds.Fixed(150, y, 100, 30).WithParent(containerBounds);
        
        container.Add(new GuiElementStaticText(capi, label, EnumTextOrientation.Left, left, CairoFont.WhiteDetailText()));
        
        var input = new GuiElementNumberInput(capi, right, null, CairoFont.WhiteDetailText());
        input.SetValue(value);
        _elements[key] = input;
        container.Add(input);
        
        y += 35;
    }

    private void AddTextInput(GuiElementContainer container, string label, string key, string value, ref double y)
    {
        ElementBounds left = ElementBounds.Fixed(0, y, 100, 30).WithParent(containerBounds);
        ElementBounds right = ElementBounds.Fixed(100, y, 200, 30).WithParent(containerBounds);
        
        container.Add(new GuiElementStaticText(capi, label, EnumTextOrientation.Left, left, CairoFont.WhiteDetailText()));
        
        var input = new GuiElementTextInput(capi, right, null, CairoFont.WhiteDetailText());
        input.SetValue(value);
        _elements[key] = input;
        container.Add(input);
        
        y += 35;
    }

    private void AddLabel(GuiElementContainer container, string label, ref double y) {
        ElementBounds left = ElementBounds.Fixed(0, y, 150, 30).WithParent(containerBounds);
        container.Add(new GuiElementStaticText(capi, label, EnumTextOrientation.Left, left, CairoFont.WhiteDetailText()));
        // Do NOT increment Y here, let caller handle it for multi-element rows
    }

    private void AddRGBInput(GuiElementContainer container, string label, string rKey, string gKey, string bKey, double r, double g, double b, ref double y)
    {
         AddLabel(container, label, ref y);
         
         ElementBounds rBounds = ElementBounds.Fixed(150, y, 50, 30).WithParent(containerBounds);
         ElementBounds gBounds = ElementBounds.Fixed(210, y, 50, 30).WithParent(containerBounds);
         ElementBounds bBounds = ElementBounds.Fixed(270, y, 50, 30).WithParent(containerBounds);

         var rIn = new GuiElementNumberInput(capi, rBounds, null, CairoFont.WhiteDetailText());
         rIn.SetValue((float)r);
         _elements[rKey] = rIn;
         container.Add(rIn);
         
         var gIn = new GuiElementNumberInput(capi, gBounds, null, CairoFont.WhiteDetailText());
         gIn.SetValue((float)g);
         _elements[gKey] = gIn;
         container.Add(gIn);
         
         var bIn = new GuiElementNumberInput(capi, bBounds, null, CairoFont.WhiteDetailText());
         bIn.SetValue((float)b);
         _elements[bKey] = bIn;
         container.Add(bIn);
         
         y += 35;
    }
    
    // Helper to get typed element
    private T Get<T>(string key) where T : GuiElement
    {
        if (_elements.TryGetValue(key, out var e) && e is T typed) return typed;
        return null;
    }

    private bool OnSave()
    {
        // Harvest values
        Cfg.Enabled = Get<GuiElementSwitch>("enabled").On;
        Cfg.ShowSymbols = Get<GuiElementSwitch>("showSymbols").On;
        Cfg.InvertedWarnings = Get<GuiElementSwitch>("invertedWarnings").On;
        
        // Sliders need normalization
        int durationVal = Get<GuiElementSlider>("duration").GetValue(); 
        Cfg.Duration = (durationVal / 100f) * 10f; 
        
        Cfg.MaxCaptions = (int)Get<GuiElementNumberInput>("maxCaptions").GetValue();

        // Opacity
        Cfg.BackgroundOpacity = Get<GuiElementSlider>("backgroundOpacity").GetValue() / 100.0;
        Cfg.TextOpacity = Get<GuiElementSlider>("textOpacity").GetValue() / 100.0;
        
        // RGB
        Cfg.WarningRed = Get<GuiElementNumberInput>("warnR").GetValue();
        Cfg.WarningGreen = Get<GuiElementNumberInput>("warnG").GetValue();
        Cfg.WarningBlue = Get<GuiElementNumberInput>("warnB").GetValue();
        
        Cfg.NoticeRed = Get<GuiElementNumberInput>("noticeR").GetValue();
        Cfg.NoticeGreen = Get<GuiElementNumberInput>("noticeG").GetValue();
        Cfg.NoticeBlue = Get<GuiElementNumberInput>("noticeB").GetValue();
        
        // Font
        Cfg.Font = Get<GuiElementTextInput>("font").GetText();
        Cfg.FontSize = Get<GuiElementNumberInput>("fontSize").GetValue();
        Cfg.FontBold = Get<GuiElementSwitch>("fontBold").On;
        
        // Appearance
        Cfg.Width = (int)Get<GuiElementNumberInput>("width").GetValue();
        Cfg.Height = (int)Get<GuiElementNumberInput>("height").GetValue();
        Cfg.Padding = Get<GuiElementNumberInput>("padding").GetValue();
        
        // Position Dropdown
        string pos = Get<GuiElementDropDown>("position").SelectedValue;
        if (Enum.TryParse(pos, out EnumDialogArea area))
        {
            Cfg.Position = area;
        }

        // Save
        try {
             capi.StoreModConfig(Cfg, "captions.json");
             CaptionsSystem.Reload();
        } catch(Exception e) {
            capi.Logger.Error("Failed to save captions config: " + e);
        }
        
        TryClose();
        return true;
    }

    private bool OnCancel()
    {
        TryClose();
        return true;
    }
    
    public override void OnGuiOpened() {
        base.OnGuiOpened();
        // Set slider values
        Get<GuiElementSlider>("duration")?.SetValues((int)((Cfg.Duration / 10.0) * 100), 0, 100, 1);
        Get<GuiElementSlider>("backgroundOpacity")?.SetValues((int)(Cfg.BackgroundOpacity * 100), 0, 100, 1);
        Get<GuiElementSlider>("textOpacity")?.SetValues((int)(Cfg.TextOpacity * 100), 0, 100, 1);
    }
    
    private void OnTitleBarClose()
    {
        TryClose();
    }
}