// Decompiled with JetBrains decompiler
// Type: Vintagestory.Client.NoObf.GuiCompositeSettings
// Assembly: VintagestoryLib, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 3C34B313-80E7-4F34-A3B7-98604168E042
// Assembly location: /Applications/Vintage Story.app/VintagestoryLib.dll

using Cairo;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.MathTools;
using Vintagestory.API.Util;
using Vintagestory.Client.Gui;

#nullable disable
namespace Vintagestory.Client.NoObf;

public class GuiCompositeSettings : GuiComposite
{
  private IGameSettingsHandler handler;
  private bool onMainscreen;
  private GuiComposer composer;
  private string startupLanguage = ClientSettings.Language;
  public bool IsInCreativeMode;
  private ElementBounds gButtonBounds = ElementBounds.Fixed(0.0, 0.0, 0.0, 40.0).WithFixedPadding(0.0, 3.0);
  private ElementBounds mButtonBounds = ElementBounds.Fixed(0.0, 0.0, 0.0, 40.0).WithFixedPadding(0.0, 3.0);
  private ElementBounds aButtonBounds = ElementBounds.Fixed(0.0, 0.0, 0.0, 40.0).WithFixedPadding(0.0, 3.0);
  private ElementBounds cButtonBounds = ElementBounds.Fixed(0.0, 0.0, 0.0, 40.0).WithFixedPadding(0.0, 3.0);
  private ElementBounds sButtonBounds = ElementBounds.Fixed(0.0, 0.0, 0.0, 40.0).WithFixedPadding(0.0, 3.0);
  private ElementBounds iButtonBounds = ElementBounds.Fixed(0.0, 0.0, 0.0, 40.0).WithFixedPadding(0.0, 3.0);
  private ElementBounds dButtonBounds = ElementBounds.Fixed(0.0, 0.0, 0.0, 40.0).WithFixedPadding(0.0, 3.0);
  private ElementBounds backButtonBounds = ElementBounds.Fixed(0.0, 0.0, 0.0, 40.0).WithFixedPadding(0.0, 3.0);
  private List<ConfigItem> mousecontrolItems = new List<ConfigItem>();
  private bool mousecontrolsTabActive;
  private List<ConfigItem> keycontrolItems = new List<ConfigItem>();
  private HotKey keyCombClone;
  private int? clickedItemIndex;
  private HotkeyCapturer hotkeyCapturer = new HotkeyCapturer();
  public string currentSearchText;
  private Dictionary<HotkeyType, int> sortOrder = new Dictionary<HotkeyType, int>()
  {
    {
      HotkeyType.MovementControls,
      0
    },
    {
      HotkeyType.MouseModifiers,
      1
    },
    {
      HotkeyType.CharacterControls,
      2
    },
    {
      HotkeyType.HelpAndOverlays,
      3
    },
    {
      HotkeyType.GUIOrOtherControls,
      4
    },
    {
      HotkeyType.InventoryHotkeys,
      5
    },
    {
      HotkeyType.CreativeOrSpectatorTool,
      6
    },
    {
      HotkeyType.CreativeTool,
      7
    },
    {
      HotkeyType.DevTool,
      8
    },
    {
      HotkeyType.MouseControls,
      9
    }
  };
  private string[] titles = new string[9]
  {
    Lang.Get("Movement controls"),
    Lang.Get("Mouse click modifiers"),
    Lang.Get("Actions"),
    Lang.Get("In-game Help and Overlays"),
    Lang.Get("User interface & More"),
    Lang.Get("Inventory hotkeys"),
    Lang.Get("Creative mode"),
    Lang.Get("Creative mode"),
    Lang.Get("Debug and Macros")
  };

  public bool IsCapturingHotKey => this.hotkeyCapturer.IsCapturing();

  public GuiCompositeSettings(IGameSettingsHandler handler, bool onMainScreen)
  {
    this.handler = handler;
    this.onMainscreen = onMainScreen;
  }

  private GuiComposer ComposerHeader(string dialogName, string currentTab)
  {
    CairoFont font = CairoFont.ButtonText();
    this.updateButtonBounds();
    GuiComposer composer;
    if (this.onMainscreen)
    {
      int width = ScreenManager.Platform.WindowSize.Width;
      int height = ScreenManager.Platform.WindowSize.Height;
      ElementBounds bounds = ElementBounds.Fixed(0.0, 0.0, 950.0, 740.0);
      this.aButtonBounds.ParentBounds = bounds;
      this.gButtonBounds.ParentBounds = bounds;
      this.mButtonBounds.ParentBounds = bounds;
      this.cButtonBounds.ParentBounds = bounds;
      this.sButtonBounds.ParentBounds = bounds;
      this.iButtonBounds.ParentBounds = bounds;
      this.dButtonBounds.ParentBounds = bounds;
      composer = this.handler.dialogBase(dialogName + "main", bounds.fixedWidth, bounds.fixedHeight).BeginChildElements(bounds).AddToggleButton(Lang.Get("setting-graphics-header"), font, new Action<bool>(this.OnGraphicsOptions), this.gButtonBounds, "graphics").AddToggleButton(Lang.Get("setting-mouse-header"), font, new Action<bool>(this.OnMouseOptions), this.mButtonBounds, "mouse").AddToggleButton(Lang.Get("setting-controls-header"), font, new Action<bool>(this.OnControlOptions), this.cButtonBounds, "controls").AddToggleButton(Lang.Get("setting-accessibility-header"), font, new Action<bool>(this.OnAccessibilityOptions), this.aButtonBounds, "accessibility").AddToggleButton(Lang.Get("setting-sound-header"), font, new Action<bool>(this.OnSoundOptions), this.sButtonBounds, "sounds").AddToggleButton(Lang.Get("setting-interface-header"), font, new Action<bool>(this.OnInterfaceOptions), this.iButtonBounds, "interface").AddIf(ClientSettings.DeveloperMode).AddToggleButton(Lang.Get("setting-dev-header"), font, new Action<bool>(this.OnDeveloperOptions), this.dButtonBounds, "developer").EndIf();
    }
    else
    {
      ElementBounds bounds1 = ElementStdBounds.AutosizedMainDialog.WithAlignment(EnumDialogArea.CenterFixed).WithFixedPosition(0.0, 75.0);
      double num = this.backButtonBounds.fixedX + this.backButtonBounds.fixedWidth + 35.0;
      bounds1.horizontalSizing = ElementSizing.Fixed;
      bounds1.fixedWidth = num;
      ElementBounds bounds2 = new ElementBounds().WithSizing(ElementSizing.FitToChildren).WithFixedPadding(GuiStyle.ElementToDialogPadding);
      bounds2.horizontalSizing = ElementSizing.Fixed;
      bounds2.fixedWidth = num - 2.0 * GuiStyle.ElementToDialogPadding;
      this.gButtonBounds.ParentBounds = bounds2;
      this.aButtonBounds.ParentBounds = bounds2;
      this.mButtonBounds.ParentBounds = bounds2;
      this.cButtonBounds.ParentBounds = bounds2;
      this.sButtonBounds.ParentBounds = bounds2;
      this.iButtonBounds.ParentBounds = bounds2;
      this.dButtonBounds.ParentBounds = bounds2;
      this.backButtonBounds.ParentBounds = bounds2;
      composer = this.handler.GuiComposers.Create(dialogName + "ingame", bounds1).AddShadedDialogBG(bounds2, false).AddStaticCustomDraw(bounds2, (DrawDelegateWithBounds) ((ctx, surface, bounds) =>
      {
        ctx.SetSourceRGBA(1.0, 1.0, 1.0, 0.1);
        GuiElement.RoundRectangle(ctx, GuiElement.scaled(5.0) + bounds.bgDrawX, GuiElement.scaled(5.0) + bounds.bgDrawY, bounds.OuterWidth - GuiElement.scaled(10.0), GuiElement.scaled(75.0), 1.0);
        ctx.Fill();
      })).BeginChildElements().AddToggleButton(Lang.Get("setting-graphics-header"), font, new Action<bool>(this.OnGraphicsOptions), this.gButtonBounds, "graphics").AddToggleButton(Lang.Get("setting-mouse-header"), font, new Action<bool>(this.OnMouseOptions), this.mButtonBounds, "mouse").AddToggleButton(Lang.Get("setting-controls-header"), font, new Action<bool>(this.OnControlOptions), this.cButtonBounds, "controls").AddToggleButton(Lang.Get("setting-accessibility-header"), font, new Action<bool>(this.OnAccessibilityOptions), this.aButtonBounds, "accessibility").AddToggleButton(Lang.Get("setting-sound-header"), font, new Action<bool>(this.OnSoundOptions), this.sButtonBounds, "sounds").AddToggleButton(Lang.Get("setting-interface-header"), font, new Action<bool>(this.OnInterfaceOptions), this.iButtonBounds, "interface").AddIf(ClientSettings.DeveloperMode).AddToggleButton(Lang.Get("setting-dev-header"), font, new Action<bool>(this.OnDeveloperOptions), this.dButtonBounds, "developer").EndIf().AddButton(Lang.Get("general-back"), (ActionConsumable) (() =>
      {
        this.clickedItemIndex = new int?();
        this.hotkeyCapturer?.EndCapture(true);
        return this.handler.OnBackPressed();
      }), this.backButtonBounds);
    }
    composer.GetToggleButton("graphics").SetValue(currentTab == "graphics");
    composer.GetToggleButton("mouse").SetValue(currentTab == "mouse");
    composer.GetToggleButton("controls").SetValue(currentTab == "controls");
    composer.GetToggleButton("accessibility").SetValue(currentTab == "accessibility");
    composer.GetToggleButton("sounds").SetValue(currentTab == "sounds");
    composer.GetToggleButton("interface").SetValue(currentTab == "interface");
    composer.GetToggleButton("developer")?.SetValue(currentTab == "developer");
    return composer;
  }

  private void updateButtonBounds()
  {
    CairoFont cairoFont = CairoFont.ButtonText();
    double width1 = cairoFont.GetTextExtents(Lang.Get("setting-graphics-header")).Width / (double) ClientSettings.GUIScale + 15.0;
    double width2 = cairoFont.GetTextExtents(Lang.Get("setting-mouse-header")).Width / (double) ClientSettings.GUIScale + 15.0;
    double width3 = cairoFont.GetTextExtents(Lang.Get("setting-controls-header")).Width / (double) ClientSettings.GUIScale + 15.0;
    double width4 = cairoFont.GetTextExtents(Lang.Get("setting-accessibility-header")).Width / (double) ClientSettings.GUIScale + 15.0;
    double width5 = cairoFont.GetTextExtents(Lang.Get("setting-sound-header")).Width / (double) ClientSettings.GUIScale + 15.0;
    double width6 = cairoFont.GetTextExtents(Lang.Get("setting-interface-header")).Width / (double) ClientSettings.GUIScale + 15.0;
    double width7 = cairoFont.GetTextExtents(Lang.Get("setting-dev-header")).Width / (double) ClientSettings.GUIScale + 15.0;
    double width8 = cairoFont.GetTextExtents(Lang.Get("general-back")).Width / (double) ClientSettings.GUIScale + 15.0;
    this.gButtonBounds.WithFixedWidth(width1);
    this.mButtonBounds.WithFixedWidth(width2).FixedRightOf(this.gButtonBounds, 15.0);
    this.cButtonBounds.WithFixedWidth(width3).FixedRightOf(this.mButtonBounds, 15.0);
    this.aButtonBounds.WithFixedWidth(width4).FixedRightOf(this.cButtonBounds, 15.0);
    this.sButtonBounds.WithFixedWidth(width5).FixedRightOf(this.aButtonBounds, 15.0);
    this.iButtonBounds.WithFixedWidth(width6).FixedRightOf(this.sButtonBounds, 15.0);
    this.dButtonBounds.WithFixedWidth(width7).FixedRightOf(this.iButtonBounds, 15.0);
    this.backButtonBounds.WithFixedWidth(width8).FixedRightOf(ClientSettings.DeveloperMode ? this.dButtonBounds : this.iButtonBounds, 25.0);
  }

  internal bool OpenSettingsMenu()
  {
    this.OnGraphicsOptions(true);
    return true;
  }

  internal void Refresh()
  {
    if (!ClientSettings.DynamicColorGrading || this.composer == null)
      return;
    DefaultShaderUniforms shaderUniforms = ScreenManager.Platform.ShaderUniforms;
    if (shaderUniforms == null)
      return;
    this.composer.GetSlider("sepiaSlider")?.SetValue((int) ((double) shaderUniforms.SepiaLevel * 100.0));
    this.composer.GetSlider("contrastSlider")?.SetValue((int) ((double) shaderUniforms.ExtraContrastLevel * 100.0) + 100);
  }

  internal void OnGraphicsOptions(bool on)
  {
    int num = 160 /*0xA0*/;
    ElementBounds elementBounds1 = ElementBounds.Fixed(0.0, 82.0, 225.0, 42.0);
    ElementBounds elementBounds2 = ElementBounds.Fixed(235.0, 85.0, (double) num, 20.0);
    ElementBounds elementBounds3 = ElementBounds.Fixed(470.0, 90.0, 225.0, 42.0);
    ElementBounds elementBounds4 = ElementBounds.Fixed(705.0, 119.0, (double) num, 20.0);
    ElementBounds.Fixed(0.0, 0.0, 30.0, 30.0).WithFixedPadding(10.0, 2.0);
    string[] strArray = new string[26]
    {
      !this.handler.MaxViewDistanceAlarmValue.HasValue ? Lang.Get("setting-hover-viewdist-singleplayer") : Lang.Get("setting-hover-viewdist"),
      Lang.Get("setting-hover-gamma"),
      Lang.Get("setting-hover-sepia"),
      Lang.Get("setting-hover-fov"),
      Lang.Get("setting-hover-guiscale"),
      Lang.Get("setting-hover-maxfps"),
      Lang.Get("setting-hover-resolution"),
      Lang.Get("setting-hover-smoothshadows"),
      Lang.Get("setting-hover-vsync"),
      Lang.Get("setting-hover-fxaa"),
      Lang.Get("setting-hover-bloom"),
      Lang.Get("setting-hover-abloom"),
      Lang.Get("setting-hover-godrays"),
      Lang.Get("setting-hover-particles"),
      Lang.Get("setting-hover-grasswaves"),
      Lang.Get("setting-hover-dynalight"),
      Lang.Get("setting-hover-dynashade"),
      Lang.Get("setting-hover-contrast"),
      Lang.Get("setting-hover-hqanimation"),
      Lang.Get("setting-hover-optimizeram"),
      Lang.Get("setting-hover-occlusionculling"),
      Lang.Get("setting-hover-foamandshinyeffect"),
      Lang.Get("setting-hover-ssao"),
      "setting-hover-radeonhdfix",
      Lang.Get("setting-hover-instancedgrass"),
      Lang.Get("setting-hover-chunkuploadratelimiter")
    };
    string[] array1 = GraphicsPreset.Presets.Select<GraphicsPreset, string>((System.Func<GraphicsPreset, string>) (p => p.PresetId.ToString() ?? "")).ToArray<string>();
    string[] array2 = GraphicsPreset.Presets.Select<GraphicsPreset, string>((System.Func<GraphicsPreset, string>) (p => Lang.Get(p.Langcode) ?? "")).ToArray<string>();
    string displayText = ClientSettings.ShowMoreGfxOptions ? Lang.Get("general-lessoptions") : Lang.Get("general-moreoptions");
    CairoFont font = CairoFont.WhiteSmallishText().Clone().WithWeight(FontWeight.Bold);
    font.Color[3] = 0.6;
    ElementBounds elementBounds5;
    ElementBounds elementBounds6;
    ElementBounds elementBounds7;
    ElementBounds elementBounds8;
    ElementBounds elementBounds9;
    ElementBounds elementBounds10;
    ElementBounds elementBounds11;
    ElementBounds elementBounds12;
    ElementBounds elementBounds13;
    ElementBounds elementBounds14;
    ElementBounds elementBounds15;
    ElementBounds elementBounds16;
    ElementBounds elementBounds17;
    ElementBounds elementBounds18;
    ElementBounds elementBounds19;
    ElementBounds elementBounds20;
    this.composer = this.ComposerHeader("gamesettings-graphics", "graphics").AddRichtext(new RichTextComponentBase[1]
    {
      (RichTextComponentBase) new LinkTextComponent(this.handler.Api, displayText, CairoFont.WhiteDetailText(), new Action<LinkTextComponent>(this.OnMoreOptions))
    }, elementBounds5 = elementBounds3.FlatCopy()).AddStaticText(Lang.Get("setting-column-appear"), font, elementBounds6 = elementBounds5.BelowCopy(fixedDeltaY: 10.0).WithFixedWidth(250.0)).AddStaticText(Lang.Get("setting-name-gamma"), CairoFont.WhiteSmallishText(), elementBounds7 = elementBounds6.BelowCopy(fixedDeltaY: -4.0).WithFixedWidth(200.0)).AddSlider(new ActionConsumable<int>(this.onGammaChanged), elementBounds8 = elementBounds4.BelowCopy(fixedDeltaY: 45.0), "gammaSlider").AddHoverText(strArray[1], CairoFont.WhiteSmallText(), 250, elementBounds7.FlatCopy().WithFixedHeight(25.0)).AddStaticText(Lang.Get("setting-name-dynamiccolorgrading"), CairoFont.WhiteSmallishText(), elementBounds9 = elementBounds7.BelowCopy(fixedDeltaY: -8.0)).AddHoverText(Lang.Get("setting-hover-dynamiccolorgrading"), CairoFont.WhiteSmallText(), 250, elementBounds9.FlatCopy().WithFixedHeight(25.0)).AddSwitch(new Action<bool>(this.onDynamicGradingToggled), elementBounds10 = elementBounds8.BelowCopy(fixedDeltaY: 15.0), "dynamicColorGradingSwitch").AddStaticText(Lang.Get("setting-name-contrast"), CairoFont.WhiteSmallishText(), elementBounds11 = elementBounds9.BelowCopy(fixedDeltaY: 10.0)).AddSlider(new ActionConsumable<int>(this.onContrastChanged), elementBounds12 = elementBounds10.BelowCopy(fixedDeltaY: 21.0).WithFixedSize((double) num, 20.0), "contrastSlider").AddHoverText(strArray[17], CairoFont.WhiteSmallText(), 250, elementBounds11.FlatCopy().WithFixedHeight(25.0)).AddStaticText(Lang.Get("setting-name-sepia"), CairoFont.WhiteSmallishText(), elementBounds13 = elementBounds11.BelowCopy()).AddSlider(new ActionConsumable<int>(this.onSepiaLevelChanged), elementBounds14 = elementBounds12.BelowCopy(fixedDeltaY: 21.0), "sepiaSlider").AddHoverText(strArray[2], CairoFont.WhiteSmallText(), 250, elementBounds13.FlatCopy().WithFixedHeight(25.0)).AddStaticText(Lang.Get("setting-name-abloom"), CairoFont.WhiteSmallishText(), elementBounds15 = elementBounds13.BelowCopy()).AddSlider(new ActionConsumable<int>(this.onAmbientBloomChanged), elementBounds16 = elementBounds14.BelowCopy(fixedDeltaY: 21.0).WithFixedSize((double) num, 20.0), "ambientBloomSlider").AddHoverText(strArray[11], CairoFont.WhiteSmallText(), 250, elementBounds15.FlatCopy().WithFixedHeight(25.0)).AddStaticText(Lang.Get("setting-name-fov"), CairoFont.WhiteSmallishText(), elementBounds17 = elementBounds15.BelowCopy()).AddSlider(new ActionConsumable<int>(this.onVowChanged), elementBounds18 = elementBounds16.BelowCopy(fixedDeltaY: 21.0), "fovSlider").AddHoverText(strArray[3], CairoFont.WhiteSmallText(), 250, elementBounds17.FlatCopy().WithFixedHeight(25.0)).AddStaticText(Lang.Get("setting-name-windowmode"), CairoFont.WhiteSmallishText(), elementBounds19 = elementBounds17.BelowCopy(fixedDeltaY: -2.0)).AddDropDown(new string[4]
    {
      "0",
      "1",
      "2",
      "3"
    }, new string[4]
    {
      Lang.Get("windowmode-normal"),
      Lang.Get("windowmode-fullscreen"),
      Lang.Get("windowmode-maxborderless"),
      Lang.Get("windowmode-fullscreen-ontop")
    }, GuiCompositeSettings.GetWindowModeIndex(), new SelectionChangedDelegate(this.OnWindowModeChanged), elementBounds20 = elementBounds18.BelowCopy(fixedDeltaY: 18.0).WithFixedSize((double) num, 26.0), "windowModeSwitch");
    if (ClientSettings.ShowMoreGfxOptions)
    {
      ElementBounds elementBounds21;
      ElementBounds elementBounds22;
      ElementBounds elementBounds23;
      ElementBounds elementBounds24;
      ElementBounds elementBounds25;
      ElementBounds elementBounds26;
      ElementBounds elementBounds27;
      ElementBounds elementBounds28;
      ElementBounds elementBounds29;
      ElementBounds elementBounds30;
      this.composer.AddStaticText(Lang.Get("setting-name-maxfps"), CairoFont.WhiteSmallishText(), elementBounds21 = elementBounds19.BelowCopy(fixedDeltaY: -3.0).WithFixedHeight(40.0)).AddSlider(new ActionConsumable<int>(this.onMaxFpsChanged), elementBounds22 = elementBounds20.BelowCopy(fixedDeltaY: 15.0).WithFixedSize((double) num, 20.0), "maxFpsSlider").AddHoverText(strArray[5], CairoFont.WhiteSmallText(), 250, elementBounds21.FlatCopy().WithFixedHeight(25.0)).AddStaticText(Lang.Get("setting-name-vsync"), CairoFont.WhiteSmallishText(), elementBounds23 = elementBounds21.BelowCopy(fixedDeltaY: 4.0)).AddHoverText(strArray[8], CairoFont.WhiteSmallText(), 250, elementBounds23.FlatCopy().WithFixedHeight(25.0)).AddDropDown(new string[3]
      {
        "0",
        "1",
        "2"
      }, new string[3]
      {
        Lang.Get("Off"),
        Lang.Get("On"),
        Lang.Get("On + Sleep")
      }, ClientSettings.VsyncMode, new SelectionChangedDelegate(this.onVsyncChanged), elementBounds24 = elementBounds22.BelowCopy(fixedDeltaY: 18.0).WithFixedSize((double) num, 26.0), "vsyncMode").AddStaticText(Lang.Get("setting-name-optimizeram"), CairoFont.WhiteSmallishText(), elementBounds25 = elementBounds23.BelowCopy(fixedDeltaY: 4.0)).AddHoverText(strArray[19], CairoFont.WhiteSmallText(), 250, elementBounds25.FlatCopy().WithFixedHeight(25.0)).AddDropDown(new string[2]
      {
        "1",
        "2"
      }, new string[2]
      {
        Lang.Get("Optimize somewhat"),
        Lang.Get("Aggressively optimize ram")
      }, ClientSettings.OptimizeRamMode - 1, new SelectionChangedDelegate(this.onOptimizeRamChanged), elementBounds26 = elementBounds24.BelowCopy(fixedDeltaY: 18.0).WithFixedSize((double) num, 26.0), "optimizeRamMode").AddStaticText(Lang.Get("setting-name-occlusionculling"), CairoFont.WhiteSmallishText(), elementBounds27 = elementBounds25.BelowCopy(fixedDeltaY: 3.0)).AddHoverText(strArray[20], CairoFont.WhiteSmallText(), 250, elementBounds27.FlatCopy().WithFixedHeight(25.0)).AddSwitch(new Action<bool>(this.onOcclusionCullingChanged), elementBounds28 = elementBounds26.BelowCopy(fixedDeltaY: 17.0), "occlusionCullingSwitch").AddStaticText(Lang.Get("setting-name-lodbiasfar"), CairoFont.WhiteSmallishText(), elementBounds29 = elementBounds27.BelowCopy(fixedDeltaY: 4.0)).AddHoverText(Lang.Get("setting-hover-lodbiasfar"), CairoFont.WhiteSmallText(), 250, elementBounds29.FlatCopy().WithFixedHeight(25.0)).AddSlider(new ActionConsumable<int>(this.onLodbiasFarChanged), elementBounds30 = elementBounds28.BelowCopy(fixedDeltaY: 21.0).WithFixedSize((double) num, 20.0), "lodbiasfarSlider").AddStaticText(Lang.Get("setting-name-windowborder"), CairoFont.WhiteSmallishText(), elementBounds29.BelowCopy(fixedDeltaY: 4.0)).AddDropDown(new string[3]
      {
        "0",
        "1",
        "2"
      }, new string[3]
      {
        Lang.Get("windowborder-resizable"),
        Lang.Get("windowborder-fixed"),
        Lang.Get("windowborder-hidden")
      }, (int) ScreenManager.Platform.WindowBorder, new SelectionChangedDelegate(this.OnWindowBorderChanged), elementBounds30.BelowCopy(fixedDeltaY: 18.0).WithFixedSize((double) num, 26.0), "windowBorder");
    }
    ElementBounds elementBounds31;
    ElementBounds elementBounds32;
    ElementBounds elementBounds33;
    ElementBounds elementBounds34;
    ElementBounds elementBounds35;
    this.composer.AddStaticText(Lang.Get("setting-name-preset"), CairoFont.WhiteSmallishText(), elementBounds31 = elementBounds1.FlatCopy().WithFixedOffset(0.0, 5.0)).AddDropDown(array1, array2, ClientSettings.GraphicsPresetId, new SelectionChangedDelegate(this.onPresetChanged), elementBounds32 = elementBounds2.FlatCopy().WithFixedSize((double) num, 30.0), "graphicsPreset").AddStaticText(Lang.Get("setting-column-graphics"), font, elementBounds33 = elementBounds31.BelowCopy(fixedDeltaY: 15.0)).AddStaticText(Lang.Get("setting-name-viewdist"), CairoFont.WhiteSmallishText(), elementBounds34 = elementBounds33.BelowCopy(fixedDeltaY: -6.0)).AddSlider(new ActionConsumable<int>(this.onViewdistanceChanged), elementBounds35 = elementBounds32.BelowCopy(fixedDeltaY: 68.0).WithFixedSize((double) num, 20.0), "viewDistanceSlider").AddHoverText(strArray[0], CairoFont.WhiteSmallText(), 250, elementBounds34.FlatCopy().WithFixedHeight(25.0));
    ElementBounds elementBounds36;
    if (ClientSettings.ShowMoreGfxOptions)
    {
      ElementBounds elementBounds37;
      ElementBounds elementBounds38;
      ElementBounds elementBounds39;
      ElementBounds elementBounds40;
      ElementBounds elementBounds41;
      ElementBounds elementBounds42;
      ElementBounds elementBounds43;
      ElementBounds elementBounds44;
      ElementBounds elementBounds45;
      ElementBounds elementBounds46;
      ElementBounds elementBounds47;
      ElementBounds elementBounds48;
      ElementBounds elementBounds49;
      ElementBounds elementBounds50;
      ElementBounds elementBounds51;
      ElementBounds elementBounds52;
      ElementBounds elementBounds53;
      ElementBounds elementBounds54;
      ElementBounds elementBounds55;
      ElementBounds elementBounds56;
      ElementBounds elementBounds57;
      ElementBounds elementBounds58;
      ElementBounds elementBounds59;
      this.composer.AddStaticText(Lang.Get("setting-name-smoothshadows"), CairoFont.WhiteSmallishText(), elementBounds37 = elementBounds34.BelowCopy()).AddHoverText(strArray[7], CairoFont.WhiteSmallText(), 250, elementBounds37.FlatCopy().WithFixedHeight(25.0)).AddSwitch(new Action<bool>(this.onSmoothShadowsToggled), elementBounds38 = elementBounds35.BelowCopy(fixedDeltaY: 15.0), "smoothShadowsLever").AddStaticText(Lang.Get("setting-name-fxaa"), CairoFont.WhiteSmallishText(), elementBounds39 = elementBounds37.BelowCopy()).AddHoverText(strArray[9], CairoFont.WhiteSmallText(), 250, elementBounds39.FlatCopy().WithFixedHeight(25.0)).AddSwitch(new Action<bool>(this.onFxaaChanged), elementBounds40 = elementBounds38.BelowCopy(fixedDeltaY: 12.0), "FxaaSwitch").AddStaticText(Lang.Get("setting-name-grasswaves"), CairoFont.WhiteSmallishText(), elementBounds41 = elementBounds39.BelowCopy()).AddHoverText(strArray[14], CairoFont.WhiteSmallText(), 250, elementBounds41.FlatCopy().WithFixedHeight(25.0)).AddSwitch(new Action<bool>(this.onWavingFoliageChanged), elementBounds42 = elementBounds40.BelowCopy(fixedDeltaY: 12.0), "wavingFoliageSwitch").AddStaticText(Lang.Get("setting-name-foamandshinyeffect"), CairoFont.WhiteSmallishText(), elementBounds43 = elementBounds41.BelowCopy()).AddHoverText(strArray[21], CairoFont.WhiteSmallText(), 250, elementBounds43.FlatCopy().WithFixedHeight(25.0)).AddSwitch(new Action<bool>(this.onFoamAndShinyEffectChanged), elementBounds44 = elementBounds42.BelowCopy(fixedDeltaY: 12.0), "liquidFoamEffectSwitch").AddStaticText(Lang.Get("setting-name-bloom"), CairoFont.WhiteSmallishText(), elementBounds45 = elementBounds43.BelowCopy()).AddHoverText(strArray[10], CairoFont.WhiteSmallText(), 250, elementBounds45.FlatCopy().WithFixedHeight(25.0)).AddSwitch(new Action<bool>(this.onBloomChanged), elementBounds46 = elementBounds44.BelowCopy(fixedDeltaY: 12.0), "BloomSwitch").AddStaticText(Lang.Get("setting-name-clouds"), CairoFont.WhiteSmallishText(), elementBounds47 = elementBounds45.BelowCopy().WithFixedHeight(39.0)).AddHoverText(Lang.Get("settings-hover-clouds"), CairoFont.WhiteSmallText(), 250, elementBounds47.FlatCopy().WithFixedHeight(25.0)).AddDropDown(new string[3]
      {
        "0",
        "1",
        "2"
      }, new string[3]
      {
        Lang.Get("settings-clouds-off"),
        Lang.Get("settings-clouds-volumetric"),
        Lang.Get("settings-clouds-classic")
      }, ClientSettings.CloudRenderMode, new SelectionChangedDelegate(this.onCloudsChanged), elementBounds48 = elementBounds46.BelowCopy(fixedDeltaY: 18.0).WithFixedSize((double) num, 26.0), "clouds").AddStaticText(Lang.Get("setting-name-godrays"), CairoFont.WhiteSmallishText(), elementBounds49 = elementBounds47.BelowCopy(fixedDeltaY: 3.0).WithFixedHeight(39.0)).AddSwitch(new Action<bool>(this.onGodRaysToggled), elementBounds50 = elementBounds48.BelowCopy(fixedDeltaY: 15.0).WithFixedSize((double) num, 20.0), "godraySwitch").AddHoverText(strArray[12], CairoFont.WhiteSmallText(), 250, elementBounds49.FlatCopy().WithFixedHeight(25.0)).AddStaticText(Lang.Get("setting-name-ssao"), CairoFont.WhiteSmallishText(), elementBounds51 = elementBounds49.BelowCopy(fixedDeltaY: 4.0).WithFixedHeight(36.0)).AddHoverText(strArray[22], CairoFont.WhiteSmallText(), 250, elementBounds51.FlatCopy().WithFixedHeight(25.0)).AddSlider(new ActionConsumable<int>(this.onSsaoChanged), elementBounds52 = elementBounds50.BelowCopy(fixedDeltaY: 17.0).WithFixedSize((double) num, 20.0), "ssaoSlider").AddStaticText(Lang.Get("setting-name-shadows"), CairoFont.WhiteSmallishText(), elementBounds53 = elementBounds51.BelowCopy(fixedDeltaY: 4.0)).AddSlider(new ActionConsumable<int>(this.onShadowsChanged), elementBounds54 = elementBounds52.BelowCopy(fixedDeltaY: 21.0).WithFixedSize((double) num, 20.0), "shadowsSlider").AddHoverText(strArray[16 /*0x10*/], CairoFont.WhiteSmallText(), 250, elementBounds53.FlatCopy().WithFixedHeight(25.0)).AddStaticText(Lang.Get("setting-name-particles"), CairoFont.WhiteSmallishText(), elementBounds55 = elementBounds53.BelowCopy(fixedDeltaY: 4.0)).AddSlider(new ActionConsumable<int>(this.onParticleLevelChanged), elementBounds56 = elementBounds54.BelowCopy(fixedDeltaY: 21.0).WithFixedSize((double) num, 20.0), "particleSlider").AddHoverText(strArray[13], CairoFont.WhiteSmallText(), 250, elementBounds55.FlatCopy().WithFixedHeight(25.0)).AddStaticText(Lang.Get("setting-name-dynalight"), CairoFont.WhiteSmallishText(), elementBounds57 = elementBounds55.BelowCopy(fixedDeltaY: 4.0)).AddSlider(new ActionConsumable<int>(this.onDynamicLightsChanged), elementBounds58 = elementBounds56.BelowCopy(fixedDeltaY: 21.0).WithFixedSize((double) num, 20.0), "dynamicLightsSlider").AddHoverText(strArray[15], CairoFont.WhiteSmallText(), 250, elementBounds57.FlatCopy().WithFixedHeight(25.0)).AddStaticText(Lang.Get("setting-name-resolution"), CairoFont.WhiteSmallishText(), elementBounds59 = elementBounds57.BelowCopy(fixedDeltaY: 4.0)).AddHoverText(strArray[6], CairoFont.WhiteSmallText(), 250, elementBounds59.FlatCopy().WithFixedHeight(25.0)).AddSlider(new ActionConsumable<int>(this.onResolutionChanged), elementBounds58.BelowCopy(fixedDeltaY: 21.0), "resolutionSlider").AddRichtext(Lang.Get("help-framerateissues"), CairoFont.WhiteDetailText(), elementBounds36 = elementBounds59.BelowCopy(fixedDeltaY: 5.0)).EndChildElements();
    }
    else
    {
      ElementBounds elementBounds60;
      this.composer.AddRichtext(Lang.Get("help-moresettingsavailable", (object) displayText), CairoFont.WhiteDetailText(), elementBounds60 = elementBounds34.BelowCopy(fixedDeltaY: 225.0, fixedDeltaWidth: 440.0)).AddRichtext(Lang.Get("help-framerateissues"), CairoFont.WhiteDetailText(), elementBounds36 = elementBounds60.BelowCopy(fixedDeltaY: 125.0));
    }
    this.composer.GetDropDown("graphicsPreset").listMenu.MaxHeight = 330;
    this.composer.Compose();
    this.handler.LoadComposer(this.composer);
    this.SetGfxValues();
  }

  private void onCloudsChanged(string newvalue, bool selected)
  {
    ClientSettings.CloudRenderMode = newvalue.ToInt();
  }

  private void onDynamicGradingToggled(bool on)
  {
    ClientSettings.DynamicColorGrading = on;
    if (!on)
    {
      ScreenManager.Platform.ShaderUniforms.SepiaLevel = ClientSettings.SepiaLevel;
      ScreenManager.Platform.ShaderUniforms.ExtraContrastLevel = ClientSettings.ExtraContrastLevel;
    }
    this.composer.GetSlider("sepiaSlider").SetValue((int) ((double) ScreenManager.Platform.ShaderUniforms.SepiaLevel * 100.0));
    this.composer.GetSlider("sepiaSlider").Enabled = !ClientSettings.DynamicColorGrading;
    this.composer.GetSlider("contrastSlider").SetValue((int) ((double) ScreenManager.Platform.ShaderUniforms.ExtraContrastLevel * 100.0) + 100);
    this.composer.GetSlider("contrastSlider").Enabled = !ClientSettings.DynamicColorGrading;
  }

  private void onVsyncChanged(string newvalue, bool selected)
  {
    ClientSettings.VsyncMode = newvalue.ToInt();
  }

  private void OnMoreOptions(LinkTextComponent comp)
  {
    ClientSettings.ShowMoreGfxOptions = !ClientSettings.ShowMoreGfxOptions;
    this.OnGraphicsOptions(true);
  }

  private void SetGfxValues()
  {
    this.composer.GetSlider("viewDistanceSlider").SetValues(ClientSettings.ViewDistance, 32 /*0x20*/, 1536 /*0x0600*/, 32 /*0x20*/, " blocks");
    this.composer.GetSlider("viewDistanceSlider").OnSliderTooltip = (SliderTooltipDelegate) (value =>
    {
      string str = Lang.Get("createworld-worldheight", (object) value);
      return value <= 512 /*0x0200*/ ? str : $"{str}\n{Lang.Get("vram-warning")}";
    });
    this.composer.GetSlider("viewDistanceSlider").TriggerOnlyOnMouseUp();
    int? distanceAlarmValue = this.handler.MaxViewDistanceAlarmValue;
    if (distanceAlarmValue.HasValue)
    {
      GuiElementSlider slider = this.composer.GetSlider("viewDistanceSlider");
      distanceAlarmValue = this.handler.MaxViewDistanceAlarmValue;
      int num = distanceAlarmValue.Value;
      slider.SetAlarmValue(num);
    }
    if (ClientSettings.ShowMoreGfxOptions)
    {
      this.composer.GetSwitch("smoothShadowsLever").On = ClientSettings.SmoothShadows;
      this.composer.GetSwitch("FxaaSwitch").On = ClientSettings.FXAA;
      this.composer.GetDropDown("optimizeRamMode").SetSelectedIndex(ClientSettings.OptimizeRamMode - 1);
      this.composer.GetSwitch("occlusionCullingSwitch").On = ClientSettings.Occlusionculling;
      this.composer.GetSwitch("wavingFoliageSwitch").On = ClientSettings.WavingFoliage;
      this.composer.GetSwitch("liquidFoamEffectSwitch").On = ClientSettings.LiquidFoamAndShinyEffect;
      this.composer.GetSwitch("BloomSwitch").On = ClientSettings.Bloom;
      this.composer.GetSwitch("godraySwitch").On = ClientSettings.GodRayQuality > 0;
      this.composer.GetDropDown("windowModeSwitch").SetSelectedIndex(ClientSettings.CloudRenderMode);
      this.composer.GetSlider("ambientBloomSlider").SetValues((int) ClientSettings.AmbientBloomLevel, 0, 100, 10, "%");
      this.composer.GetSlider("ambientBloomSlider").TriggerOnlyOnMouseUp();
      this.composer.GetSlider("ssaoSlider").SetValues(ClientSettings.SSAOQuality, 0, 2, 1);
      string[] qualityssao = new string[3]
      {
        Lang.Get("Off"),
        Lang.Get("Medium quality"),
        Lang.Get("High quality")
      };
      this.composer.GetSlider("ssaoSlider").OnSliderTooltip = (SliderTooltipDelegate) (value => qualityssao[value]);
      this.composer.GetSlider("ssaoSlider").ComposeHoverTextElement();
      this.composer.GetSlider("ssaoSlider").TriggerOnlyOnMouseUp();
      this.composer.GetSlider("shadowsSlider").SetValues(ClientSettings.ShadowMapQuality, 0, 4, 1);
      string[] quality2 = new string[5]
      {
        Lang.Get("Off"),
        Lang.Get("Low quality"),
        Lang.Get("Medium quality"),
        Lang.Get("High quality"),
        Lang.Get("Very high quality")
      };
      this.composer.GetSlider("shadowsSlider").OnSliderTooltip = (SliderTooltipDelegate) (value => quality2[value]);
      this.composer.GetSlider("shadowsSlider").ComposeHoverTextElement();
      this.composer.GetSlider("shadowsSlider").TriggerOnlyOnMouseUp();
      this.composer.GetSlider("particleSlider").SetValues(ClientSettings.ParticleLevel, 0, 100, 2, " %");
      this.composer.GetSlider("dynamicLightsSlider").SetValues(ClientSettings.MaxDynamicLights, 0, 100, 1, " " + Lang.Get("units-lightsources"));
      this.composer.GetSlider("dynamicLightsSlider").OnSliderTooltip = (SliderTooltipDelegate) (value => value != 0 ? $"{value.ToString()} {Lang.Get("units-lightsources")}" : Lang.Get("disabled"));
      this.composer.GetSlider("dynamicLightsSlider").TriggerOnlyOnMouseUp();
      this.composer.GetSlider("resolutionSlider").SetValues((int) ((double) ClientSettings.SSAA * 100.0), 25, 100, 25, " %");
      this.composer.GetSlider("resolutionSlider").OnSliderTooltip = (SliderTooltipDelegate) (value =>
      {
        float num = (float) value / 100f;
        return $"{num.ToString()}x ({((int) ((double) num * (double) num * 100.0)).ToString()}%)";
      });
      this.composer.GetSlider("resolutionSlider").TriggerOnlyOnMouseUp();
      this.composer.GetSlider("lodbiasfarSlider").SetValues((int) ((double) ClientSettings.LodBiasFar * 100.0), 35, 100, 1, " %");
      this.composer.GetSlider("lodbiasfarSlider").OnSliderTooltip = (SliderTooltipDelegate) (value =>
      {
        float num = (float) value / 100f;
        return $"{num.ToString()}x ({((int) ((double) num * (double) num * 100.0)).ToString()}%)";
      });
      this.composer.GetSlider("lodbiasfarSlider").TriggerOnlyOnMouseUp();
    }
    this.composer.GetSlider("gammaSlider").Enabled = true;
    this.composer.GetSlider("gammaSlider").OnSliderTooltip = (SliderTooltipDelegate) null;
    this.composer.GetSlider("gammaSlider").ComposeHoverTextElement();
    this.composer.GetSlider("gammaSlider").SetValues((int) Math.Round((double) ClientSettings.GammaLevel * 100.0), 30, 300, 5);
    this.composer.GetSwitch("dynamicColorGradingSwitch").On = ClientSettings.DynamicColorGrading;
    this.composer.GetSlider("sepiaSlider").SetValues((int) ((double) ScreenManager.Platform.ShaderUniforms.SepiaLevel * 100.0), 0, 100, 5);
    this.composer.GetSlider("sepiaSlider").Enabled = !ClientSettings.DynamicColorGrading;
    this.composer.GetSlider("contrastSlider").SetValues((int) ((double) ScreenManager.Platform.ShaderUniforms.ExtraContrastLevel * 100.0) + 100, 100, 200, 10, "%");
    this.composer.GetSlider("contrastSlider").Enabled = !ClientSettings.DynamicColorGrading;
    this.composer.GetSlider("fovSlider").SetValues(ClientSettings.FieldOfView, 20, 150, 1, "°");
    this.composer.GetDropDown("windowModeSwitch").SetSelectedIndex(GuiCompositeSettings.GetWindowModeIndex());
    if (!ClientSettings.ShowMoreGfxOptions)
      return;
    this.composer.GetSlider("maxFpsSlider").SetValues(GameMath.Clamp(ClientSettings.MaxFPS, 15, 241), 15, 241, 1);
    this.composer.GetSlider("maxFpsSlider").OnSliderTooltip = (SliderTooltipDelegate) (value => value != 241 ? value.ToString() : Lang.Get("unlimited"));
    this.composer.GetSlider("maxFpsSlider").ComposeHoverTextElement();
    this.composer.GetDropDown("vsyncMode").SetSelectedIndex(ClientSettings.VsyncMode);
  }

  internal static int GetWindowModeIndex()
  {
    int windowModeIndex = ClientSettings.GameWindowMode;
    if (ClientSettings.GameWindowMode == 2 && ScreenManager.Platform.WindowBorder != EnumWindowBorder.Hidden)
      windowModeIndex = 0;
    return windowModeIndex;
  }

  private void onPresetChanged(string id, bool on)
  {
    GraphicsPreset preset = GraphicsPreset.Presets[int.Parse(id)];
    if (preset.Langcode == "preset-custom")
      return;
    ClientSettings.GraphicsPresetId = preset.PresetId;
    ClientSettings.ViewDistance = preset.ViewDistance;
    ClientSettings.SmoothShadows = preset.SmoothLight;
    ClientSettings.FXAA = preset.FXAA;
    ClientSettings.SSAOQuality = preset.SSAO;
    ClientSettings.WavingFoliage = preset.WavingFoliage;
    ClientSettings.LiquidFoamAndShinyEffect = preset.LiquidFoamEffect;
    ClientSettings.Bloom = preset.Bloom;
    ClientSettings.GodRayQuality = preset.GodRays ? 1 : 0;
    ClientSettings.ShadowMapQuality = preset.ShadowMapQuality;
    ClientSettings.ParticleLevel = preset.ParticleLevel;
    ClientSettings.MaxDynamicLights = preset.DynamicLights;
    ClientSettings.SSAA = preset.Resolution;
    ClientSettings.LodBiasFar = preset.LodBiasFar;
    this.SetGfxValues();
    ScreenManager.Platform.RebuildFrameBuffers();
    this.handler.ReloadShaders();
  }

  private void SetCustomPreset()
  {
    GraphicsPreset graphicsPreset = GraphicsPreset.Presets.Where<GraphicsPreset>((System.Func<GraphicsPreset, bool>) (p => p.Langcode == "preset-custom")).FirstOrDefault<GraphicsPreset>();
    ClientSettings.GraphicsPresetId = graphicsPreset.PresetId;
    this.composer.GetDropDown("graphicsPreset").SetSelectedIndex(graphicsPreset.PresetId);
  }

  private void OnWindowModeChanged(string code, bool selected)
  {
    GuiCompositeSettings.SetWindowMode(code.ToInt());
  }

  internal static void SetWindowMode(int mode)
  {
    switch (mode)
    {
      case 1:
        ScreenManager.Platform.SetWindowAttribute((WindowAttribute) 131078 /*0x020006*/, true);
        ScreenManager.Platform.SetWindowState((WindowState) 3);
        ClientSettings.GameWindowMode = 1;
        break;
      case 2:
        ClientSettings.WindowBorder = 2;
        ScreenManager.Platform.WindowBorder = EnumWindowBorder.Hidden;
        if (ScreenManager.Platform.GetWindowState() == 2)
          ScreenManager.Platform.SetWindowState((WindowState) 0);
        ScreenManager.Platform.SetWindowState((WindowState) 2);
        ClientSettings.GameWindowMode = 2;
        break;
      case 3:
        ScreenManager.Platform.SetWindowAttribute((WindowAttribute) 131078 /*0x020006*/, false);
        ScreenManager.Platform.SetWindowState((WindowState) 3);
        ClientSettings.GameWindowMode = 3;
        break;
      default:
        ScreenManager.Platform.SetWindowAttribute((WindowAttribute) 131078 /*0x020006*/, false);
        ScreenManager.Platform.SetWindowState((WindowState) 0);
        if (ScreenManager.Platform.WindowBorder != EnumWindowBorder.Resizable)
        {
          ScreenManager.Platform.WindowBorder = EnumWindowBorder.Resizable;
          ClientSettings.WindowBorder = 0;
        }
        ClientSettings.GameWindowMode = 0;
        break;
    }
  }

  private void OnWindowBorderChanged(string newval, bool on)
  {
    int result;
    int.TryParse(newval, out result);
    ClientSettings.WindowBorder = result;
    if (ClientSettings.GameWindowMode != 2 || result == 2)
      return;
    ClientSettings.GameWindowMode = 0;
  }

  private void onOptimizeRamChanged(string code, bool selected)
  {
    ClientSettings.OptimizeRamMode = code.ToInt();
  }

  private void onOcclusionCullingChanged(bool on) => ClientSettings.Occlusionculling = on;

  private bool onResolutionChanged(int newval)
  {
    ClientSettings.SSAA = (float) newval / 100f;
    ScreenManager.Platform.RebuildFrameBuffers();
    this.SetCustomPreset();
    return true;
  }

  private bool onLodbiasFarChanged(int newval)
  {
    ClientSettings.LodBiasFar = (float) newval / 100f;
    this.SetCustomPreset();
    return true;
  }

  private bool onDynamicLightsChanged(int value)
  {
    ClientSettings.MaxDynamicLights = value;
    this.handler.ReloadShaders();
    this.SetCustomPreset();
    return true;
  }

  private void onWavingFoliageChanged(bool on)
  {
    ClientSettings.WavingFoliage = on;
    this.handler.ReloadShaders();
    this.SetCustomPreset();
  }

  private void onFoamAndShinyEffectChanged(bool on)
  {
    ClientSettings.LiquidFoamAndShinyEffect = on;
    this.handler.ReloadShaders();
    this.SetCustomPreset();
  }

  private bool onParticleLevelChanged(int level)
  {
    ClientSettings.ParticleLevel = level;
    this.SetCustomPreset();
    return true;
  }

  private bool onMaxFpsChanged(int fps)
  {
    ClientSettings.MaxFPS = fps;
    return true;
  }

  private bool onSepiaLevelChanged(int value)
  {
    ClientSettings.SepiaLevel = (float) value / 100f;
    return true;
  }

  private bool onGammaChanged(int value)
  {
    ClientSettings.GammaLevel = (float) value / 100f;
    return true;
  }

  private void onGodRaysToggled(bool on)
  {
    ClientSettings.GodRayQuality = on ? 1 : 0;
    this.handler.ReloadShaders();
    this.SetCustomPreset();
  }

  private bool onShadowsChanged(int newvalue)
  {
    ClientSettings.ShadowMapQuality = newvalue;
    ScreenManager.Platform.RebuildFrameBuffers();
    this.handler.ReloadShaders();
    this.SetCustomPreset();
    return true;
  }

  private bool onLagspikeReductionChanged(int newvalue)
  {
    ClientSettings.ChunkVerticesUploadRateLimiter = newvalue;
    return true;
  }

  private bool onAmbientBloomChanged(int newvalue)
  {
    ClientSettings.AmbientBloomLevel = (float) newvalue;
    this.handler.ReloadShaders();
    this.SetCustomPreset();
    return true;
  }

  private bool onContrastChanged(int newvalue)
  {
    ClientSettings.ExtraContrastLevel = (float) (newvalue - 100) / 100f;
    this.SetCustomPreset();
    return true;
  }

  private void onBloomChanged(bool on)
  {
    ClientSettings.Bloom = on;
    this.handler.ReloadShaders();
    this.SetCustomPreset();
  }

  private bool onVowChanged(int newvalue)
  {
    ClientSettings.FieldOfView = newvalue;
    return true;
  }

  private bool onGuiScaleChanged(int newsize)
  {
    ClientSettings.GUIScale = (float) newsize / 8f;
    this.updateButtonBounds();
    return true;
  }

  private void onFxaaChanged(bool fxaa)
  {
    ClientSettings.FXAA = fxaa;
    this.handler.ReloadShaders();
    this.SetCustomPreset();
  }

  private bool onSsaoChanged(int ssao)
  {
    ClientSettings.SSAOQuality = ssao;
    if (!this.handler.IsIngame)
    {
      ScreenManager.Platform.RebuildFrameBuffers();
      this.handler.ReloadShaders();
    }
    this.SetCustomPreset();
    return true;
  }

  internal void onSmoothShadowsToggled(bool newstate)
  {
    ClientSettings.SmoothShadows = newstate;
    this.SetCustomPreset();
  }

  internal bool onViewdistanceChanged(int newvalue)
  {
    ClientSettings.ViewDistance = newvalue;
    this.SetCustomPreset();
    return true;
  }

  private void OnMouseOptions(bool on)
  {
    this.mousecontrolsTabActive = true;
    this.LoadMouseCombinations();
    ElementBounds elementBounds1 = ElementBounds.Fixed(0.0, 85.0, 320.0, 42.0);
    ElementBounds elementBounds2 = ElementBounds.Fixed(340.0, 89.0, 200.0, 20.0);
    ElementBounds bounds1 = ElementBounds.Fixed(0.0, 0.0, 900.0 - 2.0 * GuiStyle.ElementToDialogPadding - 35.0, this.onMainscreen ? 140.0 : 114.0);
    ElementBounds bounds2 = bounds1.ForkBoundingParent(5.0, 5.0, 5.0, 5.0);
    ElementBounds bounds3 = bounds1.FlatCopy().WithParent(bounds2);
    ElementBounds elementBounds3;
    ElementBounds elementBounds4;
    ElementBounds elementBounds5;
    ElementBounds elementBounds6;
    ElementBounds elementBounds7;
    ElementBounds elementBounds8;
    ElementBounds elementBounds9;
    ElementBounds elementBounds10;
    ElementBounds elementBounds11;
    ElementBounds elementBounds12;
    ElementBounds refBounds;
    this.composer = this.ComposerHeader("gamesettings-mouse", "mouse").AddStaticText(Lang.Get("setting-name-mousesensivity"), CairoFont.WhiteSmallishText(), elementBounds1.FlatCopy()).AddSlider(new ActionConsumable<int>(this.onMouseSensivityChanged), elementBounds3 = elementBounds2.FlatCopy(), "mouseSensivitySlider").AddStaticText(Lang.Get("setting-name-mousesmoothing"), CairoFont.WhiteSmallishText(), elementBounds4 = elementBounds1.BelowCopy()).AddSlider(new ActionConsumable<int>(this.onMouseSmoothingChanged), elementBounds5 = elementBounds3.BelowCopy(fixedDeltaY: 21.0), "mouseSmoothingSlider").AddStaticText(Lang.Get("setting-name-mousewheelsensivity"), CairoFont.WhiteSmallishText(), elementBounds6 = elementBounds4.BelowCopy()).AddSlider(new ActionConsumable<int>(this.onMouseWheelSensivityChanged), elementBounds7 = elementBounds5.BelowCopy(fixedDeltaY: 21.0), "mouseWheelSensivitySlider").AddStaticText(Lang.Get("setting-name-directmousemode"), CairoFont.WhiteSmallishText(), elementBounds8 = elementBounds6.BelowCopy(fixedDeltaY: 3.0)).AddSwitch(new Action<bool>(this.onMouseModeChanged), elementBounds9 = elementBounds7.BelowCopy(fixedDeltaY: 21.0), "directMouseModeSwitch").AddHoverText(Lang.Get("setting-hover-directmousemode"), CairoFont.WhiteSmallText(), 250, elementBounds8.FlatCopy().WithFixedHeight(25.0)).AddStaticText(Lang.Get("setting-name-invertyaxis"), CairoFont.WhiteSmallishText(), elementBounds10 = elementBounds8.BelowCopy(fixedDeltaY: 3.0)).AddSwitch(new Action<bool>(this.onInvertYAxisChanged), elementBounds11 = elementBounds9.BelowCopy(fixedDeltaY: 21.0), "invertYAxisSwitch").AddStaticText(Lang.Get("setting-name-itemCollectMode"), CairoFont.WhiteSmallishText(), elementBounds12 = elementBounds10.BelowCopy(fixedDeltaY: 2.0)).AddDropDown(new string[2]
    {
      "0",
      "1"
    }, new string[2]
    {
      Lang.Get("Always collect items"),
      Lang.Get("Only collect items when sneaking")
    }, ClientSettings.ItemCollectMode, new SelectionChangedDelegate(this.onCollectionModeChange), elementBounds11.BelowCopy(fixedDeltaY: 12.0).WithFixedWidth(200.0), "itemCollectionMode").AddStaticText(Lang.Get("mousecontrols"), CairoFont.WhiteSmallishText(), refBounds = elementBounds12.BelowCopy(fixedDeltaY: 20.0)).AddHoverText(Lang.Get("hover-mousecontrols"), CairoFont.WhiteSmallText(), 250, refBounds.FlatCopy().WithFixedHeight(60.0)).AddInset(bounds2.FixedUnder(refBounds, -8.0), 3, 0.8f).BeginClip(bounds3).AddConfigList(this.mousecontrolItems, new ConfigItemClickDelegate(this.OnMouseControlItemClick), CairoFont.WhiteSmallText().WithFontSize(18f), bounds1, "configlist").EndClip().AddIf(this.onMainscreen).AddStaticText(Lang.Get("mousecontrols-mainmenuwarning"), CairoFont.WhiteSmallText(), refBounds.BelowCopy(fixedDeltaY: 112.0, fixedDeltaWidth: 500.0)).EndIf().EndChildElements().Compose();
    this.handler.LoadComposer(this.composer);
    this.composer.GetSlider("mouseWheelSensivitySlider").SetValues((int) ((double) ClientSettings.MouseWheelSensivity * 10.0), 1, 100, 1);
    this.composer.GetSlider("mouseWheelSensivitySlider").OnSliderTooltip = (SliderTooltipDelegate) (value => ((float) value / 10f).ToString() + "x");
    this.composer.GetSlider("mouseWheelSensivitySlider").ComposeHoverTextElement();
    this.composer.GetSlider("mouseSensivitySlider").SetValues(ClientSettings.MouseSensivity, 1, 200, 5);
    this.composer.GetSlider("mouseSmoothingSlider").SetValues(100 - ClientSettings.MouseSmoothing, 0, 95, 5);
    this.composer.GetSwitch("directMouseModeSwitch").SetValue(ClientSettings.DirectMouseMode);
    this.composer.GetSwitch("invertYAxisSwitch").SetValue(ClientSettings.InvertMouseYAxis);
  }

  private void OnMouseControlItemClick(int index, int indexNoTitle)
  {
    if (this.clickedItemIndex.HasValue)
      return;
    this.mousecontrolItems[index].Value = "?";
    this.clickedItemIndex = new int?(index);
    int data = (int) this.mousecontrolItems[this.clickedItemIndex.Value].Data;
    this.composer.GetConfigList("configlist").Refresh();
    string keyAtIndex = ScreenManager.hotkeyManager.HotKeys.GetKeyAtIndex(data);
    this.keyCombClone = ScreenManager.hotkeyManager.HotKeys[keyAtIndex].Clone();
    this.hotkeyCapturer.BeginCapture();
    this.keyCombClone.CurrentMapping = this.hotkeyCapturer.CapturingKeyComb;
  }

  private void LoadMouseCombinations()
  {
    int num1 = -1;
    int count = this.mousecontrolItems.Count;
    int? clickedItemIndex = this.clickedItemIndex;
    int valueOrDefault = clickedItemIndex.GetValueOrDefault();
    if (count >= valueOrDefault & clickedItemIndex.HasValue)
      num1 = (int) this.mousecontrolItems[this.clickedItemIndex.Value].Data;
    this.mousecontrolItems.Clear();
    int num2 = 0;
    List<ConfigItem>[] configItemListArray = new List<ConfigItem>[this.sortOrder.Count];
    for (int index = 0; index < configItemListArray.Length; ++index)
      configItemListArray[index] = new List<ConfigItem>();
    this.mousecontrolItems.Add(new ConfigItem()
    {
      Type = EnumItemType.Title,
      Key = Lang.Get("mouseactions")
    });
    foreach (KeyValuePair<string, HotKey> hotKey in ScreenManager.hotkeyManager.HotKeys)
    {
      HotKey keyCombClone = hotKey.Value;
      if (this.clickedItemIndex.HasValue && num2 == num1)
        keyCombClone = this.keyCombClone;
      string text = "?";
      if (keyCombClone.CurrentMapping != null)
        text = keyCombClone.CurrentMapping.ToString();
      ConfigItem configItem = new ConfigItem()
      {
        Code = hotKey.Key,
        Key = keyCombClone.Name,
        Value = text,
        Data = (object) num2
      };
      int index = this.mousecontrolItems.FindIndex((Predicate<ConfigItem>) (configitem => configitem.Value == text));
      if (index != -1)
      {
        configItem.error = true;
        this.mousecontrolItems[index].error = true;
      }
      configItemListArray[this.sortOrder[keyCombClone.KeyCombinationType]].Add(configItem);
      ++num2;
    }
    for (int index = 9; index < configItemListArray.Length; ++index)
      this.mousecontrolItems.AddRange((IEnumerable<ConfigItem>) configItemListArray[index]);
  }

  private void OnControlOptions(bool on)
  {
    this.mousecontrolsTabActive = false;
    this.LoadKeyCombinations();
    ElementBounds bounds1 = ElementBounds.Fixed(0.0, 0.0, 900.0 - 2.0 * GuiStyle.ElementToDialogPadding - 35.0, 400.0);
    ElementBounds elementBounds1 = bounds1.ForkBoundingParent(5.0, 5.0, 5.0, 5.0);
    ElementBounds bounds2 = bounds1.FlatCopy().WithParent(elementBounds1);
    ElementBounds elementBounds2 = ElementStdBounds.VerticalScrollbar(elementBounds1);
    ElementBounds elementBounds3 = ElementBounds.Fixed(0.0, 41.0, 360.0, 42.0);
    ElementBounds elementBounds4 = ElementBounds.Fixed(490.0, 38.0, 200.0, 20.0);
    ElementBounds elementBounds5;
    ElementBounds elementBounds6;
    ElementBounds refBounds;
    this.composer = this.ComposerHeader("gamesettings-controls", "controls").AddStaticText(Lang.Get("setting-name-noseparatectrlkeys"), CairoFont.WhiteSmallishText(), elementBounds5 = elementBounds3.BelowCopy(fixedDeltaY: 10.0, fixedDeltaWidth: 120.0)).AddSwitch(new Action<bool>(this.onSeparateCtrl), elementBounds4.BelowCopy(fixedDeltaY: 32.0), "separateCtrl").AddHoverText(Lang.Get("setting-hover-noseparatectrlkeys"), CairoFont.WhiteSmallText(), 250, elementBounds5.FlatCopy().WithFixedHeight(25.0)).AddStaticText(Lang.Get("keycontrols"), CairoFont.WhiteSmallishText(), elementBounds6 = elementBounds5.BelowCopy(fixedDeltaY: 5.0, fixedDeltaWidth: -120.0)).AddTextInput(refBounds = elementBounds6.BelowCopy(fixedDeltaY: 5.0), (Action<string>) (text =>
    {
      if (this.currentSearchText == text)
        return;
      this.currentSearchText = text;
      this.ReLoadKeyCombinations();
    }), key: "searchField").AddVerticalScrollbar(new Action<float>(this.OnNewScrollbarValue), elementBounds2.FixedUnder(refBounds, 10.0), "scrollbar").AddInset(elementBounds1.FixedUnder(refBounds, 10.0), 3, 0.8f).BeginClip(bounds2).AddConfigList(this.keycontrolItems, new ConfigItemClickDelegate(this.OnKeyControlItemClick), CairoFont.WhiteSmallText().WithFontSize(18f), bounds1, "configlist").EndClip().AddButton(Lang.Get("setting-name-setdefault"), new ActionConsumable(this.OnResetControls), ElementStdBounds.MenuButton(0.0f, EnumDialogArea.LeftFixed).FixedUnder(elementBounds1, 10.0).WithFixedPadding(10.0, 2.0)).AddIf(this.handler.IsIngame).AddButton(Lang.Get("setting-name-macroeditor"), new ActionConsumable(this.OnMacroEditor), ElementStdBounds.MenuButton(0.0f, EnumDialogArea.RightFixed).FixedUnder(elementBounds1, 10.0).WithFixedPadding(10.0, 2.0)).EndIf().EndChildElements().Compose();
    this.handler.LoadComposer(this.composer);
    this.composer.GetSwitch("separateCtrl").SetValue(!ClientSettings.SeparateCtrl);
    this.composer.GetTextInput("searchField").SetPlaceHolderText(Lang.Get("Search..."));
    this.composer.GetTextInput("searchField").SetValue("");
    GuiElementConfigList configList = this.composer.GetConfigList("configlist");
    configList.errorFont = configList.stdFont.Clone();
    configList.errorFont.Color = GuiStyle.ErrorTextColor;
    configList.Bounds.CalcWorldBounds();
    bounds2.CalcWorldBounds();
    this.ReLoadKeyCombinations();
    this.composer.GetScrollbar("scrollbar").SetHeights((float) bounds2.fixedHeight, (float) configList.innerBounds.fixedHeight);
  }

  private bool OnMacroEditor()
  {
    this.handler.OnMacroEditor();
    return true;
  }

  private void onCollectionModeChange(string code, bool selected)
  {
    ClientSettings.ItemCollectMode = code.ToInt();
  }

  private void onMouseModeChanged(bool on)
  {
    ClientSettings.DirectMouseMode = on;
    ScreenManager.Platform.SetDirectMouseMode(on);
  }

  private void onInvertYAxisChanged(bool on) => ClientSettings.InvertMouseYAxis = on;

  private void onSeparateCtrl(bool on)
  {
    ClientSettings.SeparateCtrl = !on;
    if (on)
    {
      HotKey hotKey1 = ScreenManager.hotkeyManager.HotKeys["shift"];
      hotKey1.CurrentMapping = ScreenManager.hotkeyManager.HotKeys["sneak"].CurrentMapping;
      ClientSettings.Inst.SetKeyMapping("shift", hotKey1.CurrentMapping);
      HotKey hotKey2 = ScreenManager.hotkeyManager.HotKeys["ctrl"];
      hotKey2.CurrentMapping = ScreenManager.hotkeyManager.HotKeys["sprint"].CurrentMapping;
      ClientSettings.Inst.SetKeyMapping("ctrl", hotKey2.CurrentMapping);
    }
    else
    {
      HotKey hotKey3 = ScreenManager.hotkeyManager.HotKeys["shift"];
      hotKey3.CurrentMapping = new KeyCombination()
      {
        KeyCode = 1
      };
      ClientSettings.Inst.SetKeyMapping("shift", hotKey3.CurrentMapping);
      HotKey hotKey4 = ScreenManager.hotkeyManager.HotKeys["ctrl"];
      hotKey4.CurrentMapping = new KeyCombination()
      {
        KeyCode = 3
      };
      ClientSettings.Inst.SetKeyMapping("ctrl", hotKey4.CurrentMapping);
    }
    this.OnControlOptions(true);
  }

  private bool onMouseWheelSensivityChanged(int val)
  {
    ClientSettings.MouseWheelSensivity = (float) val / 10f;
    return true;
  }

  private void ReLoadKeyCombinations()
  {
    if (this.mousecontrolsTabActive)
      this.LoadMouseCombinations();
    else
      this.LoadKeyCombinations();
    GuiElementConfigList configList = this.composer.GetConfigList("configlist");
    if (configList == null)
      return;
    configList.Refresh();
    this.composer.GetScrollbar("scrollbar")?.SetNewTotalHeight((float) configList.innerBounds.OuterHeight);
    this.composer.GetScrollbar("scrollbar")?.TriggerChanged();
  }

  private void LoadKeyCombinations()
  {
    int num1 = -1;
    int count = this.keycontrolItems.Count;
    int? clickedItemIndex = this.clickedItemIndex;
    int valueOrDefault = clickedItemIndex.GetValueOrDefault();
    if (count >= valueOrDefault & clickedItemIndex.HasValue)
      num1 = (int) this.keycontrolItems[this.clickedItemIndex.Value].Data;
    this.keycontrolItems.Clear();
    int num2 = 0;
    List<ConfigItem>[] configItemListArray = new List<ConfigItem>[this.sortOrder.Count];
    for (int index = 0; index < configItemListArray.Length; ++index)
      configItemListArray[index] = new List<ConfigItem>();
    foreach (KeyValuePair<string, HotKey> hotKey in ScreenManager.hotkeyManager.HotKeys)
    {
      HotKey keyCombClone = hotKey.Value;
      if (this.clickedItemIndex.HasValue && num2 == num1)
        keyCombClone = this.keyCombClone;
      string text = "?";
      if (keyCombClone.CurrentMapping != null)
        text = keyCombClone.CurrentMapping.ToString();
      ConfigItem configItem = new ConfigItem()
      {
        Code = hotKey.Key,
        Key = keyCombClone.Name,
        Value = text,
        Data = (object) num2
      };
      int index = this.keycontrolItems.FindIndex((Predicate<ConfigItem>) (configitem => configitem.Value == text));
      if (index != -1)
      {
        configItem.error = true;
        this.keycontrolItems[index].error = true;
      }
      configItemListArray[this.sortOrder[keyCombClone.KeyCombinationType]].Add(configItem);
      ++num2;
    }
    for (int index = 0; index < configItemListArray.Length; ++index)
    {
      List<ConfigItem> source = new List<ConfigItem>();
      string currentSearchText = this.currentSearchText;
      string lowerInvariant = currentSearchText != null ? currentSearchText.ToSearchFriendly().ToLowerInvariant() : (string) null;
      bool flag = !string.IsNullOrEmpty(lowerInvariant);
      if ((index != 1 || ClientSettings.SeparateCtrl) && index != 9)
      {
        if (flag)
        {
          foreach (ConfigItem configItem in configItemListArray[index])
          {
            if (configItem.Key.ToSearchFriendly().ToLowerInvariant().Contains(lowerInvariant))
              source.Add(configItem);
          }
          if (source != null && !source.Any<ConfigItem>())
            continue;
        }
        if (index != 7)
          this.keycontrolItems.Add(new ConfigItem()
          {
            Type = EnumItemType.Title,
            Key = this.titles[index]
          });
        this.keycontrolItems.AddRange(flag ? (IEnumerable<ConfigItem>) source : (IEnumerable<ConfigItem>) configItemListArray[index]);
      }
    }
  }

  private void OnKeyControlItemClick(int index, int indexNoTitle)
  {
    if (this.clickedItemIndex.HasValue)
      return;
    this.keycontrolItems[index].Value = "?";
    this.clickedItemIndex = new int?(index);
    int data = (int) this.keycontrolItems[this.clickedItemIndex.Value].Data;
    this.composer.GetConfigList("configlist").Refresh();
    this.composer.GetScrollbar("scrollbar")?.TriggerChanged();
    string keyAtIndex = ScreenManager.hotkeyManager.HotKeys.GetKeyAtIndex(data);
    this.keyCombClone = ScreenManager.hotkeyManager.HotKeys[keyAtIndex].Clone();
    this.hotkeyCapturer.BeginCapture();
    this.keyCombClone.CurrentMapping = this.hotkeyCapturer.CapturingKeyComb;
  }

  public bool ShouldCaptureAllInputs() => this.hotkeyCapturer.IsCapturing();

  public void OnKeyDown(KeyEvent eventArgs)
  {
    if (!this.hotkeyCapturer.OnKeyDown(eventArgs))
      return;
    if (!this.hotkeyCapturer.IsCapturing())
    {
      this.clickedItemIndex = new int?();
      this.keyCombClone = (HotKey) null;
    }
    this.ReLoadKeyCombinations();
  }

  public void OnKeyUp(KeyEvent eventArgs)
  {
    this.hotkeyCapturer.OnKeyUp(eventArgs, new Action(this.CompletedCapture));
  }

  public void OnMouseDown(MouseEvent eventArgs)
  {
    if (!this.hotkeyCapturer.OnMouseDown(eventArgs))
      return;
    if (!this.hotkeyCapturer.IsCapturing())
    {
      this.clickedItemIndex = new int?();
      this.keyCombClone = (HotKey) null;
    }
    this.ReLoadKeyCombinations();
  }

  public void OnMouseUp(MouseEvent eventArgs)
  {
    this.hotkeyCapturer.OnMouseUp(eventArgs, new Action(this.CompletedCapture));
  }

  private void CompletedCapture()
  {
    int index = this.mousecontrolsTabActive ? (int) this.mousecontrolItems[this.clickedItemIndex.Value].Data : (int) this.keycontrolItems[this.clickedItemIndex.Value].Data;
    string keyAtIndex = ScreenManager.hotkeyManager.HotKeys.GetKeyAtIndex(index);
    if (!this.hotkeyCapturer.WasCancelled)
    {
      this.keyCombClone.CurrentMapping = this.hotkeyCapturer.CapturedKeyComb;
      ScreenManager.hotkeyManager.HotKeys[keyAtIndex] = this.keyCombClone;
      ClientSettings.Inst.SetKeyMapping(keyAtIndex, this.keyCombClone.CurrentMapping);
      if (keyAtIndex == "sneak" && !ClientSettings.SeparateCtrl)
      {
        ScreenManager.hotkeyManager.HotKeys["shift"].CurrentMapping = this.keyCombClone.CurrentMapping;
        this.ShiftOrCtrlChanged();
      }
      if (keyAtIndex == "sprint" && !ClientSettings.SeparateCtrl)
      {
        ScreenManager.hotkeyManager.HotKeys["ctrl"].CurrentMapping = this.keyCombClone.CurrentMapping;
        this.ShiftOrCtrlChanged();
      }
      if (keyAtIndex == "shift" || keyAtIndex == "ctrl" || keyAtIndex == "primarymouse" || keyAtIndex == "secondarymouse" || keyAtIndex == "toolmodeselect")
        this.ShiftOrCtrlChanged();
    }
    this.clickedItemIndex = new int?();
    this.keyCombClone = (HotKey) null;
    this.ReLoadKeyCombinations();
  }

  private void ShiftOrCtrlChanged()
  {
    if (!(this.handler.Api is ClientCoreAPI api))
      return;
    api.eventapi.TriggerHotkeysChanged();
  }

  private void OnNewScrollbarValue(float value)
  {
    ElementBounds innerBounds = this.composer.GetConfigList("configlist").innerBounds;
    innerBounds.fixedY = 5.0 - (double) value;
    innerBounds.CalcWorldBounds();
  }

  private bool onMouseSmoothingChanged(int value)
  {
    ClientSettings.MouseSmoothing = 100 - value;
    return true;
  }

  private bool onMouseSensivityChanged(int value)
  {
    ClientSettings.MouseSensivity = value;
    return true;
  }

  private bool OnResetControls()
  {
    this.composer = this.ComposerHeader("gamesettings-confirmreset", "controls").AddStaticText(Lang.Get("Please Confirm"), CairoFont.WhiteSmallishText(), ElementStdBounds.Rowed(1.5f, 0.0, EnumDialogArea.LeftFixed).WithFixedWidth(600.0)).AddStaticText(Lang.Get("Really reset key controls to default settings?"), CairoFont.WhiteSmallishText(), ElementStdBounds.Rowed(2f, 0.0, EnumDialogArea.LeftFixed).WithFixedSize(600.0, 100.0)).AddButton(Lang.Get("Cancel"), new ActionConsumable(this.OnCancelReset), ElementStdBounds.Rowed(3.7f, 0.0, EnumDialogArea.LeftFixed).WithFixedPadding(10.0, 2.0)).AddButton(Lang.Get("Confirm"), new ActionConsumable(this.OnConfirmReset), ElementStdBounds.Rowed(3.7f, 0.0, EnumDialogArea.RightFixed).WithFixedPadding(10.0, 2.0)).EndChildElements().Compose();
    this.handler.LoadComposer(this.composer);
    return true;
  }

  private bool OnConfirmReset()
  {
    ClientSettings.KeyMapping.Clear();
    ScreenManager.hotkeyManager.ResetKeyMapping();
    this.OnControlOptions(true);
    return true;
  }

  private bool OnCancelReset()
  {
    this.OnControlOptions(true);
    return true;
  }

  private void OnAccessibilityOptions(bool on)
  {
    ElementBounds elementBounds1 = ElementBounds.Fixed(0.0, 85.0, 450.0, 42.0);
    ElementBounds elementBounds2 = ElementBounds.Fixed(470.0, 138.0, 200.0, 20.0);
    ElementBounds elementBounds3;
    ElementBounds elementBounds4;
    ElementBounds elementBounds5;
    ElementBounds elementBounds6;
    ElementBounds elementBounds7;
    ElementBounds elementBounds8;
    ElementBounds elementBounds9;
    ElementBounds elementBounds10;
    ElementBounds elementBounds11;
    ElementBounds elementBounds12;
    this.composer = this.ComposerHeader("gamesettings-accessibility", "accessibility").AddStaticText(Lang.Get("setting-accessibility-notes"), CairoFont.WhiteSmallText(), elementBounds1.FlatCopy().WithFixedWidth(800.0)).AddStaticText(Lang.Get("setting-name-togglesprint"), CairoFont.WhiteSmallishText(), elementBounds3 = elementBounds1.BelowCopy(fixedDeltaY: 12.0).WithFixedWidth(360.0)).AddHoverText(Lang.Get("setting-hover-togglesprint"), CairoFont.WhiteSmallText(), 250, elementBounds3.FlatCopy().WithFixedHeight(25.0)).AddSwitch(new Action<bool>(this.onToggleSprint), elementBounds2.FlatCopy(), "toggleSprint").AddStaticText(Lang.Get("setting-name-bobblehead"), CairoFont.WhiteSmallishText(), elementBounds4 = elementBounds3.BelowCopy(fixedDeltaY: 2.0)).AddHoverText(Lang.Get("setting-hover-bobblehead"), CairoFont.WhiteSmallText(), 250, elementBounds4.FlatCopy().WithFixedHeight(25.0)).AddSwitch(new Action<bool>(this.onViewBobbingChanged), elementBounds5 = elementBounds2.BelowCopy(fixedDeltaY: 20.0), "viewBobbingSwitch").AddStaticText(Lang.Get("setting-name-camerashake"), CairoFont.WhiteSmallishText(), elementBounds6 = elementBounds4.BelowCopy(fixedDeltaY: 2.0)).AddSlider(new ActionConsumable<int>(this.onCameraShakeChanged), elementBounds7 = elementBounds5.BelowCopy(fixedDeltaY: 18.0).WithFixedSize(200.0, 25.0), "cameraShakeSlider").AddHoverText(Lang.Get("setting-hover-camerashake"), CairoFont.WhiteSmallText(), 250, elementBounds6.FlatCopy().WithFixedHeight(25.0)).AddStaticText(Lang.Get("setting-name-wireframethickness"), CairoFont.WhiteSmallishText(), elementBounds8 = elementBounds6.BelowCopy(fixedDeltaY: 2.0)).AddSlider(new ActionConsumable<int>(this.onWireframeThicknessChanged), elementBounds9 = elementBounds7.BelowCopy(fixedDeltaY: 19.0).WithFixedSize(200.0, 25.0), "wireframethicknessSlider").AddHoverText(Lang.Get("setting-hover-wireframethickness"), CairoFont.WhiteSmallText(), 250, elementBounds8.FlatCopy().WithFixedHeight(25.0)).AddStaticText(Lang.Get("setting-name-wireframecolors"), CairoFont.WhiteSmallishText(), elementBounds10 = elementBounds8.BelowCopy(fixedDeltaY: 2.0)).AddDropDown(new string[3]
    {
      "Preset1",
      "Preset2",
      "Preset3"
    }, new string[3]
    {
      Lang.Get("Preset 1"),
      Lang.Get("Preset 2"),
      Lang.Get("Preset 3")
    }, ClientSettings.guiColorsPreset - 1, new SelectionChangedDelegate(this.onWireframeColorsChanged), elementBounds11 = elementBounds9.BelowCopy(fixedDeltaY: 19.0).WithFixedSize(100.0, 25.0), "wireframecolorsDropdown").AddHoverText(Lang.Get("setting-hover-wireframecolors"), CairoFont.WhiteSmallText(), 250, elementBounds10.FlatCopy().WithFixedHeight(25.0)).AddStaticText(Lang.Get("setting-name-instabilityWavingStrength"), CairoFont.WhiteSmallishText(), elementBounds12 = elementBounds10.BelowCopy(fixedDeltaY: 2.0)).AddSlider(new ActionConsumable<int>(this.onInstabilityStrengthChanged), elementBounds11.BelowCopy(fixedDeltaY: 19.0).WithFixedSize(200.0, 25.0), "instabilityWavingStrengthSlider").AddHoverText(Lang.Get("setting-hover-instabilityWavingStrength"), CairoFont.WhiteSmallText(), 250, elementBounds12.FlatCopy().WithFixedHeight(25.0)).AddRichtext(Lang.Get("help-accessibility"), CairoFont.WhiteDetailText(), elementBounds12.BelowCopy(fixedDeltaY: 23.0)).EndChildElements().Compose();
    this.composer.GetSwitch("viewBobbingSwitch").On = ClientSettings.ViewBobbing;
    this.composer.GetSwitch("toggleSprint").SetValue(ClientSettings.ToggleSprint);
    this.composer.GetSlider("cameraShakeSlider").SetValues((int) ((double) ClientSettings.CameraShakeStrength * 100.0), 0, 100, 1, " %");
    this.composer.GetSlider("wireframethicknessSlider").SetValues((int) ((double) ClientSettings.Wireframethickness * 2.0), 1, 16 /*0x10*/, 1, "x");
    this.composer.GetSlider("wireframethicknessSlider").OnSliderTooltip = (SliderTooltipDelegate) (value => ((float) value / 2f).ToString() + "x");
    this.composer.GetSlider("wireframethicknessSlider").ComposeHoverTextElement();
    this.composer.GetSlider("instabilityWavingStrengthSlider").SetValues((int) ((double) ClientSettings.InstabilityWavingStrength * 100.0), 0, 150, 1, " %");
    this.handler.LoadComposer(this.composer);
  }

  private bool onInstabilityStrengthChanged(int value)
  {
    ClientSettings.InstabilityWavingStrength = (float) value / 100f;
    return true;
  }

  private bool onWireframeThicknessChanged(int value)
  {
    ClientSettings.Wireframethickness = (float) value / 2f;
    return true;
  }

  private void onWireframeColorsChanged(string code, bool selected)
  {
    ClientSettings.guiColorsPreset = (int) code[code.Length - 1] - 48 /*0x30*/;
    this.handler.Api.ColorPreset?.OnUpdateSetting();
  }

  private bool onCameraShakeChanged(int value)
  {
    ClientSettings.CameraShakeStrength = (float) value / 100f;
    return true;
  }

  private void onViewBobbingChanged(bool val) => ClientSettings.ViewBobbing = val;

  private void onToggleSprint(bool on) => ClientSettings.ToggleSprint = on;

  internal void OnSoundOptions(bool on)
  {
    ElementBounds elementBounds1 = ElementBounds.Fixed(0.0, 87.0, 320.0, 40.0);
    ElementBounds elementBounds2 = ElementBounds.Fixed(340.0, 89.0, 330.0, 20.0);
    string[] strArray = new string[1].Append<string>(ScreenManager.Platform.AvailableAudioDevices.ToArray<string>());
    string[] names = new string[1]{ "Default" }.Append<string>(ScreenManager.Platform.AvailableAudioDevices.ToArray<string>());
    ElementBounds elementBounds3;
    ElementBounds elementBounds4;
    ElementBounds elementBounds5;
    ElementBounds elementBounds6;
    ElementBounds elementBounds7;
    ElementBounds elementBounds8;
    ElementBounds elementBounds9;
    ElementBounds elementBounds10;
    ElementBounds elementBounds11;
    ElementBounds elementBounds12;
    ElementBounds elementBounds13;
    ElementBounds elementBounds14;
    ElementBounds elementBounds15;
    ElementBounds elementBounds16;
    ElementBounds elementBounds17;
    ElementBounds elementBounds18;
    this.composer = this.ComposerHeader("gamesettings-soundoptions", "sounds").AddStaticText(Lang.Get("setting-name-mastersoundlevel"), CairoFont.WhiteSmallishText(), elementBounds3 = elementBounds1.FlatCopy()).AddSlider(new ActionConsumable<int>(this.onMasterSoundLevelChanged), elementBounds4 = elementBounds2.FlatCopy(), "mastersoundLevel").AddStaticText(Lang.Get("setting-name-soundlevel"), CairoFont.WhiteSmallishText(), elementBounds5 = elementBounds3.BelowCopy(fixedDeltaY: 25.0)).AddSlider(new ActionConsumable<int>(this.onSoundLevelChanged), elementBounds6 = elementBounds4.BelowCopy(fixedDeltaY: 46.0), "soundLevel").AddStaticText(Lang.Get("setting-name-entitysoundlevel"), CairoFont.WhiteSmallishText(), elementBounds7 = elementBounds5.BelowCopy()).AddSlider(new ActionConsumable<int>(this.onEntitySoundLevelChanged), elementBounds8 = elementBounds6.BelowCopy(fixedDeltaY: 21.0), "entitySoundLevel").AddStaticText(Lang.Get("setting-name-ambientsoundlevel"), CairoFont.WhiteSmallishText(), elementBounds9 = elementBounds7.BelowCopy()).AddSlider(new ActionConsumable<int>(this.onAmbientSoundLevelChanged), elementBounds10 = elementBounds8.BelowCopy(fixedDeltaY: 21.0), "ambientSoundLevel").AddStaticText(Lang.Get("setting-name-weathersoundlevel"), CairoFont.WhiteSmallishText(), elementBounds11 = elementBounds9.BelowCopy()).AddSlider(new ActionConsumable<int>(this.onWeatherSoundLevelChanged), elementBounds12 = elementBounds10.BelowCopy(fixedDeltaY: 21.0), "weatherSoundLevel").AddStaticText(Lang.Get("setting-name-musiclevel"), CairoFont.WhiteSmallishText(), elementBounds13 = elementBounds11.BelowCopy(fixedDeltaY: 22.0)).AddSlider(new ActionConsumable<int>(this.onMusicLevelChanged), elementBounds14 = elementBounds12.BelowCopy(fixedDeltaY: 41.0), "musicLevel").AddStaticText(Lang.Get("setting-name-musicfrequency"), CairoFont.WhiteSmallishText(), elementBounds15 = elementBounds13.BelowCopy()).AddSlider(new ActionConsumable<int>(this.onMusicFrequencyChanged), elementBounds16 = elementBounds14.BelowCopy(fixedDeltaY: 21.0), "musicFrequency").AddStaticText(Lang.Get("setting-name-hrtfmode"), CairoFont.WhiteSmallishText(), elementBounds17 = elementBounds15.BelowCopy(fixedDeltaY: 26.0)).AddHoverText(Lang.Get("setting-hover-hrtfmode"), CairoFont.WhiteSmallText(), 250, elementBounds17.FlatCopy().WithFixedHeight(30.0)).AddSwitch(new Action<bool>(this.onHRTFMode), elementBounds18 = elementBounds16.BelowCopy(fixedDeltaY: 34.0), "hrtfmode").AddStaticText(Lang.Get("setting-name-audiooutputdevice"), CairoFont.WhiteSmallishText(), elementBounds17.BelowCopy(fixedDeltaY: 5.0)).AddDropDown(strArray, names, 0, new SelectionChangedDelegate(this.onAudioDeviceChanged), elementBounds18.BelowCopy(fixedDeltaY: 16.0).WithFixedSize(300.0, 30.0), "audiooutputdevice").EndChildElements().Compose();
    this.handler.LoadComposer(this.composer);
    this.composer.GetSlider("mastersoundLevel").SetValues(ClientSettings.MasterSoundLevel, 0, 100, 1, "%");
    this.composer.GetSlider("soundLevel").SetValues(ClientSettings.SoundLevel, 0, 100, 1, "%");
    this.composer.GetSlider("entitySoundLevel").SetValues(ClientSettings.EntitySoundLevel, 0, 100, 1, "%");
    this.composer.GetSlider("ambientSoundLevel").SetValues(ClientSettings.AmbientSoundLevel, 0, 100, 1, "%");
    this.composer.GetSlider("weatherSoundLevel").SetValues(ClientSettings.WeatherSoundLevel, 0, 100, 1, "%");
    this.composer.GetSlider("musicLevel").SetValues(ClientSettings.MusicLevel, 0, 100, 1, "%");
    string[] frequencies = new string[4]
    {
      Lang.Get("setting-musicfrequency-low"),
      Lang.Get("setting-musicfrequency-medium"),
      Lang.Get("setting-musicfrequency-often"),
      Lang.Get("setting-musicfrequency-veryoften")
    };
    this.composer.GetSlider("musicFrequency").OnSliderTooltip = (SliderTooltipDelegate) (value => frequencies[value] ?? "");
    this.composer.GetSlider("musicFrequency").SetValues(ClientSettings.MusicFrequency, 0, 3, 1);
    this.composer.GetSwitch("hrtfmode").SetValue(ClientSettings.UseHRTFAudio);
    this.composer.GetDropDown("audiooutputdevice").SetSelectedIndex(Math.Max(0, strArray.IndexOf<string>(ClientSettings.AudioDevice)));
  }

  private void onAudioDeviceChanged(string code, bool selected)
  {
    ClientSettings.AudioDevice = code;
  }

  private bool onMusicFrequencyChanged(int val)
  {
    ClientSettings.MusicFrequency = val;
    return true;
  }

  private bool onMasterSoundLevelChanged(int soundLevel)
  {
    ClientSettings.MasterSoundLevel = soundLevel;
    return true;
  }

  private bool onSoundLevelChanged(int soundLevel)
  {
    ClientSettings.SoundLevel = soundLevel;
    return true;
  }

  private bool onEntitySoundLevelChanged(int soundLevel)
  {
    ClientSettings.EntitySoundLevel = soundLevel;
    return true;
  }

  private bool onAmbientSoundLevelChanged(int soundLevel)
  {
    ClientSettings.AmbientSoundLevel = soundLevel;
    return true;
  }

  private bool onWeatherSoundLevelChanged(int soundLevel)
  {
    ClientSettings.WeatherSoundLevel = soundLevel;
    return true;
  }

  private bool onMusicLevelChanged(int musicLevel)
  {
    ClientSettings.MusicLevel = musicLevel;
    return true;
  }

  private void onHRTFMode(bool val) => ClientSettings.UseHRTFAudio = val;

  public static void getLanguages(out string[] languageCodes, out string[] languageNames)
  {
    GuiCompositeSettings.LanguageConfig[] languageConfigArray = ScreenManager.Platform.AssetManager.Get<GuiCompositeSettings.LanguageConfig[]>(new AssetLocation("lang/languages.json"));
    languageCodes = new string[languageConfigArray.Length];
    languageNames = new string[languageConfigArray.Length];
    for (int index = 0; index < languageConfigArray.Length; ++index)
    {
      languageCodes[index] = languageConfigArray[index].Code;
      languageNames[index] = $"{languageConfigArray[index].Name} / {languageConfigArray[index].Englishname}";
    }
  }

  internal void OnInterfaceOptions(bool on)
  {
    ElementBounds bounds1 = ElementBounds.Fixed(0.0, 85.0, 475.0, 42.0);
    ElementBounds bounds2 = ElementBounds.Fixed(495.0, 89.0, 200.0, 20.0);
    int windowBorder = (int) ScreenManager.Platform.WindowBorder;
    string language = ClientSettings.Language;
    string[] languageCodes;
    string[] languageNames;
    GuiCompositeSettings.getLanguages(out languageCodes, out languageNames);
    int selectedIndex = languageCodes.IndexOf<string>(language);
    ElementBounds elementBounds1;
    ElementBounds elementBounds2;
    ElementBounds elementBounds3;
    ElementBounds elementBounds4;
    ElementBounds elementBounds5;
    ElementBounds elementBounds6;
    ElementBounds elementBounds7;
    ElementBounds elementBounds8;
    ElementBounds elementBounds9;
    ElementBounds elementBounds10;
    ElementBounds elementBounds11;
    ElementBounds elementBounds12;
    this.composer = this.ComposerHeader("gamesettings-interfaceoptions", "interface").AddStaticText(Lang.Get("setting-name-guiscale"), CairoFont.WhiteSmallishText(), bounds1).AddHoverText(Lang.Get("setting-hover-guiscale"), CairoFont.WhiteSmallText(), 250, bounds1.FlatCopy().WithFixedHeight(25.0)).AddSlider(new ActionConsumable<int>(this.onGuiScaleChanged), bounds2, "guiScaleSlider").AddStaticText(Lang.Get("setting-name-language"), CairoFont.WhiteSmallishText(), elementBounds1 = bounds1.BelowCopy(fixedDeltaY: 2.0)).AddHoverText(Lang.Get("setting-hover-language"), CairoFont.WhiteSmallText(), 250, elementBounds1.FlatCopy().WithFixedHeight(25.0)).AddDropDown(languageCodes, languageNames, selectedIndex, new SelectionChangedDelegate(this.onLanguageChanged), elementBounds2 = bounds2.BelowCopy(fixedDeltaY: 17.0).WithFixedSize(330.0, 30.0)).AddStaticText(Lang.Get("setting-name-autochat"), CairoFont.WhiteSmallishText(), elementBounds3 = elementBounds1.BelowCopy(fixedDeltaY: 1.0)).AddHoverText(Lang.Get("setting-hover-autochat"), CairoFont.WhiteSmallText(), 250, elementBounds3.FlatCopy().WithFixedHeight(25.0)).AddSwitch(new Action<bool>(this.onAutoChatChanged), elementBounds4 = elementBounds2.BelowCopy(fixedDeltaY: 15.0), "autoChatSwitch").AddStaticText(Lang.Get("setting-name-autochat-selected"), CairoFont.WhiteSmallishText(), elementBounds5 = elementBounds3.BelowCopy(fixedDeltaY: 1.0)).AddHoverText(Lang.Get("setting-hover-autochat-selected"), CairoFont.WhiteSmallText(), 250, elementBounds5.FlatCopy().WithFixedHeight(25.0)).AddSwitch(new Action<bool>(this.onAutoChatOpenSelectedChanged), elementBounds6 = elementBounds4.BelowCopy(fixedDeltaY: 15.0), "autoChatOpenSelectedSwitch").AddStaticText(Lang.Get("setting-name-blockinfohud") + this.HotkeyReminder("blockinfohud"), CairoFont.WhiteSmallishText(), elementBounds7 = elementBounds5.BelowCopy(fixedDeltaY: 2.0)).AddHoverText(Lang.Get("setting-hover-blockinfohud"), CairoFont.WhiteSmallText(), 250, elementBounds7.FlatCopy().WithFixedHeight(25.0)).AddSwitch(new Action<bool>(this.onBlockInfoHudChanged), elementBounds8 = elementBounds6.BelowCopy(fixedDeltaY: 14.0), "blockinfohudSwitch").AddStaticText(Lang.Get("setting-name-blockinteractioninfohud") + this.HotkeyReminder("blockinteractionhelp"), CairoFont.WhiteSmallishText(), elementBounds9 = elementBounds7.BelowCopy(fixedDeltaY: 2.0)).AddHoverText(Lang.Get("setting-hover-blockinteractioninfohud"), CairoFont.WhiteSmallText(), 250, elementBounds9.FlatCopy().WithFixedHeight(25.0)).AddSwitch(new Action<bool>(this.onBlockInteractionInfoHudChanged), elementBounds10 = elementBounds8.BelowCopy(fixedDeltaY: 14.0), "blockinteractioninfohudSwitch").AddStaticText(Lang.Get("setting-name-coordinatehud") + this.HotkeyReminder("coordinateshud"), CairoFont.WhiteSmallishText(), elementBounds11 = elementBounds9.BelowCopy(fixedDeltaY: 2.0)).AddHoverText(Lang.Get("setting-hover-coordinatehud"), CairoFont.WhiteSmallText(), 250, elementBounds11.FlatCopy().WithFixedHeight(25.0)).AddSwitch(new Action<bool>(this.onCoordinateHudChanged), elementBounds12 = elementBounds10.BelowCopy(fixedDeltaY: 14.0), "coordinatehudSwitch");
    if (this.composer.Api is MainMenuAPI || this.composer.Api.World.Config.GetBool("allowMap", true))
      this.composer = this.composer.AddStaticText(Lang.Get("setting-name-minimaphud") + this.HotkeyReminder("worldmaphud"), CairoFont.WhiteSmallishText(), elementBounds11 = elementBounds11.BelowCopy(fixedDeltaY: 2.0)).AddHoverText(Lang.Get("setting-hover-minimaphud"), CairoFont.WhiteSmallText(), 250, elementBounds11.FlatCopy().WithFixedHeight(25.0)).AddSwitch(new Action<bool>(this.onMinimapHudChanged), elementBounds12 = elementBounds12.BelowCopy(fixedDeltaY: 14.0), "minimaphudSwitch");
    ElementBounds elementBounds13;
    ElementBounds elementBounds14;
    ElementBounds elementBounds15;
    ElementBounds elementBounds16;
    ElementBounds elementBounds17;
    ElementBounds elementBounds18;
    ElementBounds elementBounds19;
    ElementBounds elementBounds20;
    ElementBounds elementBounds21;
    ElementBounds bounds3;
    this.composer = this.composer.AddStaticText(Lang.Get("setting-name-immersivemousemode"), CairoFont.WhiteSmallishText(), elementBounds13 = elementBounds11.BelowCopy(fixedDeltaY: 4.0)).AddHoverText(Lang.Get("setting-hover-immersivemousemode"), CairoFont.WhiteSmallText(), 250, elementBounds13.FlatCopy().WithFixedHeight(25.0)).AddSwitch(new Action<bool>(this.onImmersiveMouseModeChanged), elementBounds14 = elementBounds12.BelowCopy(fixedDeltaY: 17.0), "immersiveMouseModeSwitch").AddStaticText(Lang.Get("setting-name-immersivefpmode"), CairoFont.WhiteSmallishText(), elementBounds15 = elementBounds13.BelowCopy(fixedDeltaY: 5.0)).AddHoverText(Lang.Get("setting-hover-immersivefpmode"), CairoFont.WhiteSmallText(), 250, elementBounds15.FlatCopy().WithFixedHeight(25.0)).AddSwitch(new Action<bool>(this.onImmersiveFpModeChanged), elementBounds16 = elementBounds14.BelowCopy(fixedDeltaY: 17.0), "immersiveFpModeSwitch").AddStaticText(Lang.Get("setting-name-fpmodeyoffset"), CairoFont.WhiteSmallishText(), elementBounds17 = elementBounds15.BelowCopy(fixedDeltaY: 5.0)).AddHoverText(Lang.Get("setting-hover-fpmodeyoffset"), CairoFont.WhiteSmallText(), 250, elementBounds17.FlatCopy().WithFixedHeight(25.0)).AddSlider(new ActionConsumable<int>(this.onFpModeYOffsetChanged), elementBounds18 = elementBounds16.BelowCopy(fixedDeltaY: 19.0).WithFixedSize(150.0, 20.0), "fpmodeYOffsetSlider").AddStaticText(Lang.Get("setting-name-fpmodefov"), CairoFont.WhiteSmallishText(), elementBounds19 = elementBounds17.BelowCopy(fixedDeltaY: 5.0)).AddHoverText(Lang.Get("setting-hover-fpmodefov"), CairoFont.WhiteSmallText(), 250, elementBounds19.FlatCopy().WithFixedHeight(25.0)).AddSlider(new ActionConsumable<int>(this.onFpModeFoVChanged), elementBounds20 = elementBounds18.BelowCopy(fixedDeltaY: 28.0).WithFixedSize(150.0, 20.0), "fpmodefovSlider").AddStaticText(Lang.Get("setting-name-developermode"), CairoFont.WhiteSmallishText(), elementBounds21 = elementBounds19.BelowCopy(fixedDeltaY: 4.0)).AddHoverText(Lang.Get("setting-hover-developermode"), CairoFont.WhiteSmallText(), 250, bounds3 = elementBounds20.BelowCopy(fixedDeltaY: 20.0).WithFixedHeight(25.0)).AddSwitch(new Action<bool>(this.onDeveloperModeChanged), bounds3, "developerSwitch").AddRichtext(this.startupLanguage != "en" ? Lang.Get("setting-notice-lang-communitycreated") : "", CairoFont.WhiteSmallishText(), elementBounds21.BelowCopy().WithFixedMargin(0.0, 25.0).WithFixedSize(880.0, 110.0), "restartText").EndChildElements().Compose();
    this.handler.LoadComposer(this.composer);
    if (ScreenManager.Platform.ScreenSize.Width > 3000)
      this.composer.GetSlider("guiScaleSlider").SetValues((int) (8.0 * (double) ClientSettings.GUIScale), 4, 24, 1);
    else
      this.composer.GetSlider("guiScaleSlider").SetValues((int) (8.0 * (double) ClientSettings.GUIScale), 4, 16 /*0x10*/, 1);
    this.composer.GetSlider("guiScaleSlider").TriggerOnlyOnMouseUp();
    this.composer.GetSlider("fpmodeYOffsetSlider").SetValues((int) ((double) ClientSettings.FpHandsYOffset * 100.0), -100, 10, 1);
    this.composer.GetSlider("fpmodefovSlider").SetValues(ClientSettings.FpHandsFoV, 70, 90, 1, "°");
    this.composer.GetSwitch("immersiveMouseModeSwitch").SetValue(ClientSettings.ImmersiveMouseMode);
    this.composer.GetSwitch("immersiveFpModeSwitch").SetValue(ClientSettings.ImmersiveFpMode);
    this.composer.GetSwitch("autoChatSwitch").SetValue(ClientSettings.AutoChat);
    this.composer.GetSwitch("autoChatOpenSelectedSwitch").SetValue(ClientSettings.AutoChatOpenSelected);
    this.composer.GetSwitch("blockinfohudSwitch").SetValue(ClientSettings.ShowBlockInfoHud);
    this.composer.GetSwitch("blockinteractioninfohudSwitch").SetValue(ClientSettings.ShowBlockInteractionHelp);
    this.composer.GetSwitch("coordinatehudSwitch").SetValue(ClientSettings.ShowCoordinateHud);
    this.composer.GetSwitch("minimaphudSwitch")?.SetValue(this.composer.Api.Settings.Bool["showMinimapHud"]);
    this.composer.GetSwitch("developerSwitch").SetValue(ClientSettings.DeveloperMode);
  }

  private bool onFpModeYOffsetChanged(int pos)
  {
    ClientSettings.FpHandsYOffset = (float) pos / 100f;
    return true;
  }

  private bool onFpModeFoVChanged(int pos)
  {
    ClientSettings.FpHandsFoV = pos;
    return true;
  }

  private string HotkeyReminder(string key)
  {
    HotKey hotKey;
    return !ScreenManager.hotkeyManager.HotKeys.TryGetValue(key, out hotKey) || hotKey.CurrentMapping == null ? "" : $" ({hotKey.CurrentMapping?.ToString()})";
  }

  private void onMinimapHudChanged(bool on)
  {
    this.composer.Api.Settings.Bool["showMinimapHud"] = on;
  }

  private void onCoordinateHudChanged(bool on) => ClientSettings.ShowCoordinateHud = on;

  private void onBlockInteractionInfoHudChanged(bool on)
  {
    ClientSettings.ShowBlockInteractionHelp = on;
  }

  private void onBlockInfoHudChanged(bool on) => ClientSettings.ShowBlockInfoHud = on;

  private void onImmersiveMouseModeChanged(bool on) => ClientSettings.ImmersiveMouseMode = on;

  private void onImmersiveFpModeChanged(bool on) => ClientSettings.ImmersiveFpMode = on;

  private void onAutoChatChanged(bool on) => ClientSettings.AutoChat = on;

  private void onAutoChatOpenSelectedChanged(bool on) => ClientSettings.AutoChatOpenSelected = on;

  private void onLanguageChanged(string lang, bool on)
  {
    bool flag = false;
    if (lang != ClientSettings.Language)
    {
      if (lang != "en")
        this.composer.GetRichtext("restartText").SetNewText($"{Lang.GetL(lang, "setting-notice-restart")} {Lang.GetL(lang, "setting-notice-lang-communitycreated")}", CairoFont.WhiteSmallishText());
      else
        this.composer.GetRichtext("restartText").SetNewText(Lang.GetL(lang, "setting-notice-restart"), CairoFont.WhiteSmallishText());
      flag = true;
    }
    if (lang == this.startupLanguage)
      this.composer.GetRichtext("restartText").SetNewText(lang != "en" ? Lang.Get("setting-notice-lang-communitycreated") : "", CairoFont.WhiteSmallishText());
    ClientSettings.Language = lang;
    if (lang.StartsWithOrdinal("zh-") || lang == "ar" || lang == "ja" || lang == "ko" || lang == "th")
    {
      if (RuntimeEnv.OS != OS.Windows)
      {
        if (lang != this.startupLanguage && ClientSettings.DefaultFontName == "sans-serif")
        {
          ClientSettings.DecorativeFontName = "sans-serif";
          this.composer.GetRichtext("restartText").SetNewText($"{Lang.GetL(this.startupLanguage, "setting-notice-restart")} {Lang.GetL(this.startupLanguage, "setting-notice-lang-communitycreated")}\n{Lang.GetL(this.startupLanguage, "setting-notice-lang-nonwindowsfonts")}", CairoFont.WhiteSmallishText());
        }
      }
      else
      {
        switch (lang)
        {
          case "ko":
            this.SetupLocalizedFonts(lang, "Malgun Gothic", "Malgun Gothic");
            flag = true;
            break;
          case "th":
            this.SetupLocalizedFonts(lang, "Leelawadee UI Semilight", "Leelawadee UI");
            flag = true;
            break;
          case "ja":
            this.SetupLocalizedFonts(lang, "meiryo", "meiryo");
            flag = true;
            break;
          case "zh-cn":
            this.SetupLocalizedFonts(lang, "Microsoft YaHei Light", "Microsoft YaHei");
            flag = true;
            break;
          case "zh-tw":
            this.SetupLocalizedFonts(lang, "Microsoft JhengHei UI Light", "Microsoft JhengHei UI");
            flag = true;
            break;
          default:
            ClientSettings.DecorativeFontName = "sans-serif";
            flag = true;
            break;
        }
      }
    }
    else
    {
      if (ClientSettings.DefaultFontName == "meiryo" || ClientSettings.DefaultFontName == "Malgun Gothic" || ClientSettings.DefaultFontName == "Leelawadee UI Semilight" || ClientSettings.DefaultFontName == "Microsoft YaHei Light" || ClientSettings.DefaultFontName == "Microsoft JhengHei UI Light")
      {
        ClientSettings.DefaultFontName = "sans-serif";
        flag = true;
      }
      if (ClientSettings.DefaultFontName == "sans-serif")
      {
        ClientSettings.DecorativeFontName = "Lora";
        flag = true;
      }
    }
    if (!flag)
      return;
    ClientSettings.Inst.Save(true);
  }

  private void SetupLocalizedFonts(string lang, string baseFont, string decorativeFont)
  {
    ClientSettings.DefaultFontName = baseFont;
    ClientSettings.DecorativeFontName = decorativeFont;
    string vtmlCode = lang != this.startupLanguage ? $"{Lang.GetL(lang, "setting-notice-restart")} {Lang.GetL(lang, "setting-notice-lang-communitycreated")}" : Lang.GetL(lang, "setting-notice-lang-communitycreated");
    if (lang != this.startupLanguage)
      this.composer.GetRichtext("restartText").SetNewText(vtmlCode, CairoFont.WhiteSmallishText(baseFont));
    else
      this.composer.GetRichtext("restartText").SetNewText(vtmlCode, CairoFont.WhiteSmallishText(baseFont));
  }

  private void OnDeveloperOptions(bool on)
  {
    ElementBounds elementBounds1 = ElementBounds.Fixed(0.0, 42.0, 425.0, 42.0);
    ElementBounds elementBounds2 = ElementBounds.Fixed(450.0, 45.0, 200.0, 20.0);
    string[] strArray = new string[8]
    {
      Lang.Get("setting-hover-errorreporter"),
      Lang.Get("setting-hover-extdebuginfo"),
      Lang.Get("setting-hover-opengldebug"),
      Lang.Get("setting-hover-openglerrorchecking"),
      Lang.Get("setting-hover-debugtexturedispose"),
      Lang.Get("setting-hover-debugvaodispose"),
      Lang.Get("setting-hover-debugsounddispose"),
      Lang.Get("setting-hover-fasterstartup")
    };
    ElementBounds elementBounds3;
    ElementBounds elementBounds4;
    ElementBounds elementBounds5;
    ElementBounds elementBounds6;
    ElementBounds elementBounds7;
    ElementBounds elementBounds8;
    ElementBounds elementBounds9;
    ElementBounds elementBounds10;
    ElementBounds elementBounds11;
    ElementBounds elementBounds12;
    ElementBounds elementBounds13;
    ElementBounds elementBounds14;
    ElementBounds elementBounds15;
    ElementBounds elementBounds16;
    ElementBounds elementBounds17;
    this.composer = this.ComposerHeader("gamesettings-developeroptions", "developer").AddStaticText(Lang.Get("setting-name-errorreporter"), CairoFont.WhiteSmallishText(), elementBounds3 = elementBounds1.BelowCopy()).AddHoverText(strArray[0], CairoFont.WhiteSmallText(), 250, elementBounds3.FlatCopy().WithFixedHeight(25.0)).AddSwitch(new Action<bool>(this.onErrorReporterChanged), elementBounds4 = elementBounds2.BelowCopy(fixedDeltaY: 16.0), "errorReporterSwitch").AddStaticText(Lang.Get("setting-name-extdebuginfo"), CairoFont.WhiteSmallishText(), elementBounds5 = elementBounds3.BelowCopy(fixedDeltaY: 4.0)).AddHoverText(strArray[1], CairoFont.WhiteSmallText(), 250, elementBounds5.FlatCopy().WithFixedHeight(25.0)).AddSwitch(new Action<bool>(this.onExtDebugInfoChanged), elementBounds6 = elementBounds4.BelowCopy(fixedDeltaY: 16.0), "extDbgInfoSwitch").AddStaticText(Lang.Get("setting-name-opengldebug"), CairoFont.WhiteSmallishText(), elementBounds7 = elementBounds5.BelowCopy(fixedDeltaY: 4.0)).AddHoverText(strArray[2], CairoFont.WhiteSmallText(), 250, elementBounds7.FlatCopy().WithFixedHeight(25.0)).AddSwitch(new Action<bool>(this.onOpenGLDebugChanged), elementBounds8 = elementBounds6.BelowCopy(fixedDeltaY: 16.0), "openglDebugSwitch").AddStaticText(Lang.Get("setting-name-openglerrorchecking"), CairoFont.WhiteSmallishText(), elementBounds9 = elementBounds7.BelowCopy(fixedDeltaY: 4.0)).AddHoverText(strArray[3], CairoFont.WhiteSmallText(), 250, elementBounds9.FlatCopy().WithFixedHeight(25.0)).AddSwitch(new Action<bool>(this.onOpenGLErrorCheckingChanged), elementBounds10 = elementBounds8.BelowCopy(fixedDeltaY: 16.0), "openglErrorCheckingSwitch").AddStaticText(Lang.Get("setting-name-debugtexturedispose"), CairoFont.WhiteSmallishText(), elementBounds11 = elementBounds9.BelowCopy(fixedDeltaY: 4.0)).AddHoverText(strArray[4], CairoFont.WhiteSmallText(), 250, elementBounds11.FlatCopy().WithFixedHeight(25.0)).AddSwitch(new Action<bool>(this.onDebugTextureDisposeChanged), elementBounds12 = elementBounds10.BelowCopy(fixedDeltaY: 16.0), "debugTextureDisposeSwitch").AddStaticText(Lang.Get("setting-name-debugvaodispose"), CairoFont.WhiteSmallishText(), elementBounds13 = elementBounds11.BelowCopy(fixedDeltaY: 4.0)).AddHoverText(strArray[5], CairoFont.WhiteSmallText(), 250, elementBounds13.FlatCopy().WithFixedHeight(25.0)).AddSwitch(new Action<bool>(this.onDebugVaoDisposeChanged), elementBounds14 = elementBounds12.BelowCopy(fixedDeltaY: 16.0), "debugVaoDisposeSwitch").AddStaticText(Lang.Get("setting-name-debugsounddispose"), CairoFont.WhiteSmallishText(), elementBounds15 = elementBounds13.BelowCopy(fixedDeltaY: 4.0)).AddHoverText(strArray[6], CairoFont.WhiteSmallText(), 250, elementBounds15.FlatCopy().WithFixedHeight(25.0)).AddSwitch(new Action<bool>(this.onDebugSoundDisposeChanged), elementBounds16 = elementBounds14.BelowCopy(fixedDeltaY: 16.0), "debugSoundDisposeSwitch").AddStaticText(Lang.Get("setting-name-fasterstartup"), CairoFont.WhiteSmallishText(), elementBounds17 = elementBounds15.BelowCopy(fixedDeltaY: 4.0)).AddHoverText(strArray[7], CairoFont.WhiteSmallText(), 250, elementBounds17.FlatCopy().WithFixedHeight(25.0)).AddSwitch(new Action<bool>(this.onFasterStartupChanged), elementBounds16.BelowCopy(fixedDeltaY: 16.0), "fasterStartupSwitch").EndChildElements().Compose();
    this.handler.LoadComposer(this.composer);
    this.composer.GetSwitch("errorReporterSwitch").SetValue(ClientSettings.StartupErrorDialog);
    this.composer.GetSwitch("extDbgInfoSwitch").SetValue(ClientSettings.ExtendedDebugInfo);
    this.composer.GetSwitch("openglDebugSwitch").SetValue(ClientSettings.GlDebugMode);
    this.composer.GetSwitch("openglErrorCheckingSwitch").SetValue(ClientSettings.GlErrorChecking);
    this.composer.GetSwitch("debugTextureDisposeSwitch").SetValue(RuntimeEnv.DebugTextureDispose);
    this.composer.GetSwitch("debugVaoDisposeSwitch").SetValue(RuntimeEnv.DebugVAODispose);
    this.composer.GetSwitch("debugSoundDisposeSwitch").SetValue(RuntimeEnv.DebugSoundDispose);
    this.composer.GetSwitch("fasterStartupSwitch").SetValue(ClientSettings.OffThreadMipMapCreation);
  }

  private void onErrorReporterChanged(bool on) => ClientSettings.StartupErrorDialog = on;

  private void onDebugSoundDisposeChanged(bool on) => RuntimeEnv.DebugSoundDispose = on;

  private void onDebugVaoDisposeChanged(bool on) => RuntimeEnv.DebugVAODispose = on;

  private void onDebugTextureDisposeChanged(bool on) => RuntimeEnv.DebugTextureDispose = on;

  private void onOpenGLDebugChanged(bool on)
  {
    ClientSettings.GlDebugMode = on;
    ScreenManager.Platform.GlDebugMode = on;
  }

  private void onOpenGLErrorCheckingChanged(bool on)
  {
    ClientSettings.GlErrorChecking = on;
    ScreenManager.Platform.GlErrorChecking = on;
  }

  private void onExtDebugInfoChanged(bool on) => ClientSettings.ExtendedDebugInfo = on;

  private void onFasterStartupChanged(bool on) => ClientSettings.OffThreadMipMapCreation = on;

  private void onDeveloperModeChanged(bool on)
  {
    if (!on)
    {
      ClientSettings.DeveloperMode = on;
      ClientSettings.StartupErrorDialog = false;
      ClientSettings.ExtendedDebugInfo = false;
      ClientSettings.GlDebugMode = false;
      ClientSettings.GlErrorChecking = false;
      RuntimeEnv.DebugTextureDispose = false;
      RuntimeEnv.DebugVAODispose = false;
      RuntimeEnv.DebugSoundDispose = false;
      this.OnInterfaceOptions(true);
    }
    else
    {
      this.composer = this.ComposerHeader("gamesettings-confirmdevelopermode", "developer").AddStaticText(Lang.Get("Please Confirm"), CairoFont.WhiteSmallishText(), ElementStdBounds.Rowed(1.5f, 0.0, EnumDialogArea.LeftFixed).WithFixedWidth(600.0)).AddStaticText(Lang.Get("confirmEnableDevMode"), CairoFont.WhiteSmallishText(), ElementStdBounds.Rowed(2f, 0.0, EnumDialogArea.LeftFixed).WithFixedSize(600.0, 100.0)).AddButton(Lang.Get("Cancel"), new ActionConsumable(this.OnCancelDevMode), ElementStdBounds.Rowed(3.7f, 0.0, EnumDialogArea.LeftFixed).WithFixedPadding(10.0, 2.0)).AddButton(Lang.Get("Confirm"), new ActionConsumable(this.OnConfirmDevMode), ElementStdBounds.Rowed(3.7f, 0.0, EnumDialogArea.RightFixed).WithFixedPadding(10.0, 2.0)).EndChildElements().Compose();
      this.handler.LoadComposer(this.composer);
    }
  }

  private bool OnCancelDevMode()
  {
    this.OnInterfaceOptions(true);
    return true;
  }

  private bool OnConfirmDevMode()
  {
    ClientSettings.DeveloperMode = true;
    this.OnDeveloperOptions(true);
    return true;
  }

  public class LanguageConfig
  {
    public string Code;
    public string Englishname;
    public string Name;
  }
}
