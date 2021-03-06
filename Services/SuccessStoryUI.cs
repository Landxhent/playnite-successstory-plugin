﻿using Newtonsoft.Json;
using Playnite.SDK;
using Playnite.SDK.Models;
using CommonPluginsShared;
using SuccessStory.Models;
using SuccessStory.Views;
using SuccessStory.Views.Interface;
using SuccessStory.Views.InterfaceFS;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Threading;

namespace SuccessStory.Services
{
    public class SuccessStoryUI : PlayniteUiHelper
    {
        private SuccessStoryDatabase PluginDatabase = SuccessStory.PluginDatabase;
        private readonly SuccessStory _Plugin;

        public override string _PluginUserDataPath { get; set; } = string.Empty;

        public override bool IsFirstLoad { get; set; } = true;


        public override string BtActionBarName { get; set; } = string.Empty;
        public override FrameworkElement PART_BtActionBar { get; set; }

        public override string SpDescriptionName { get; set; } = string.Empty;
        public override FrameworkElement PART_SpDescription { get; set; }


        public override string SpInfoBarFSName { get; set; } = string.Empty;
        public override FrameworkElement PART_SpInfoBarFS { get; set; }

        public override string BtActionBarFSName { get; set; } = string.Empty;
        public override FrameworkElement PART_BtActionBarFS { get; set; }


        public override List<CustomElement> ListCustomElements { get; set; } = new List<CustomElement>();


        public SuccessStoryUI(SuccessStory Plugin, IPlayniteAPI PlayniteApi, string PluginUserDataPath) : base(PlayniteApi, PluginUserDataPath)
        {
            _Plugin = Plugin;
            _PluginUserDataPath = PluginUserDataPath;

            BtActionBarName = "PART_ScButton";
            SpDescriptionName = "PART_ScDescriptionIntegration";

            SpInfoBarFSName = "PART_ScSpInfoBar";
        }


        #region BtHeader
        public void AddBtHeader()
        {
            if (_PlayniteApi.ApplicationInfo.Mode == ApplicationMode.Desktop)
            {
                if (PluginDatabase.PluginSettings.EnableIntegrationButtonHeader)
                {
                    logger.Info("SuccessStory - Add Header button");
                    Button btHeader = new SuccessStoryButtonHeader(TransformIcon.Get("SuccessStory"));
                    btHeader.SetValue(TextBlock.FontWeightProperty, FontWeights.Bold);
                    btHeader.Click += OnBtHeaderClick;
                    ui.AddButtonInWindowsHeader(btHeader);
                }
            }
        }

        public void OnBtHeaderClick(object sender, RoutedEventArgs e)
        {
#if DEBUG
            logger.Debug($"SuccessStory [Ignored] - OnBtHeaderClick()");
#endif
            PluginDatabase.IsViewOpen = true;
            var ViewExtension = new SuccessView(_Plugin, _PlayniteApi, _Plugin.GetPluginUserDataPath());
            Window windowExtension = PlayniteUiHelper.CreateExtensionWindow(_PlayniteApi, resources.GetString("LOCSuccessStory"), ViewExtension);
            windowExtension.ShowDialog();
            PluginDatabase.IsViewOpen = false;

            /*
            SuccessView ViewExtension = null;
            if (settings.EnableRetroAchievementsView && PlayniteTools.IsGameEmulated(PlayniteApi, GameSelected))
            {
                ViewExtension = new SuccessView(this, settings, PlayniteApi, this.GetPluginUserDataPath(), true);
            }
            else
            {
                ViewExtension = new SuccessView(this, settings, PlayniteApi, this.GetPluginUserDataPath(), false);
            }
            Window windowExtension = PlayniteUiHelper.CreateExtensionWindow(PlayniteApi, resources.GetString("LOCSuccessStory"), ViewExtension);
            windowExtension.ShowDialog();
            */
        }
        #endregion


        public override void Initial()
        {
           
        }

        public override DispatcherOperation AddElements()
        {
            if (_PlayniteApi.ApplicationInfo.Mode == ApplicationMode.Desktop)
            {
                if (IsFirstLoad)
                {
#if DEBUG
                    logger.Debug($"SuccessStory [Ignored] - IsFirstLoad");
#endif
                    Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Loaded, new ThreadStart(delegate
                    {
                        System.Threading.SpinWait.SpinUntil(() => IntegrationUI.SearchElementByName("PART_HtmlDescription") != null, 5000);
                    })).Wait();
                    IsFirstLoad = false;
                }

                return Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new ThreadStart(delegate
                {
                    CheckTypeView();

                    if (PluginDatabase.PluginSettings.EnableIntegrationButton)
                    {
#if DEBUG
                        logger.Debug($"SuccessStory [Ignored] - AddBtActionBar()");
#endif
                        AddBtActionBar();
                    }

                    if (PluginDatabase.PluginSettings.EnableIntegrationInDescription)
                    {
#if DEBUG
                        logger.Debug($"SuccessStory [Ignored] - AddSpDescription()");
#endif
                        AddSpDescription();
                    }

                    if (PluginDatabase.PluginSettings.EnableIntegrationInCustomTheme)
                    {
#if DEBUG
                        logger.Debug($"SuccessStory [Ignored] - AddCustomElements()");
#endif
                        AddCustomElements();
                    }
                }));
            }

            return null;
        }

        public override void RefreshElements(Game GameSelected, bool force = false)
        {
#if DEBUG
            logger.Debug($"SuccessStory [Ignored] - RefreshElements({GameSelected.Name})");
#endif

            CancellationTokenSource tokenSource = new CancellationTokenSource();
            CancellationToken ct = tokenSource.Token;

            Task TaskRefresh = Task.Run(() =>
            {
#if DEBUG
                string IsCanceld = string.Empty;

                logger.Debug($"SuccessStory [Ignored] - TaskRefresh() - Start");
                Stopwatch stopwatch = new Stopwatch();
                TimeSpan ts;
                stopwatch.Start();
#endif
                try
                {
                    Initial();

                    // Reset resources
                    List<ResourcesList> resourcesLists = new List<ResourcesList>();
                    resourcesLists.Add(new ResourcesList { Key = "Sc_HasData", Value = false });
                    resourcesLists.Add(new ResourcesList { Key = "Sc_Is100Percent", Value = false });
                    resourcesLists.Add(new ResourcesList { Key = "Sc_Total", Value = 0 });
                    resourcesLists.Add(new ResourcesList { Key = "Sc_TotalDouble", Value = (double)0 });
                    resourcesLists.Add(new ResourcesList { Key = "Sc_TotalString", Value = "0" });
                    resourcesLists.Add(new ResourcesList { Key = "Sc_Unlocked", Value = 0 });
                    resourcesLists.Add(new ResourcesList { Key = "Sc_UnlockedDouble", Value = (double)0 });
                    resourcesLists.Add(new ResourcesList { Key = "Sc_UnlockedString", Value = "0" });
                    resourcesLists.Add(new ResourcesList { Key = "Sc_Locked", Value = 0 });
                    resourcesLists.Add(new ResourcesList { Key = "Sc_LockedDouble", Value = (double)0 });
                    resourcesLists.Add(new ResourcesList { Key = "Sc_LockedString", Value = "0" });
                    resourcesLists.Add(new ResourcesList { Key = "Sc_PercentageUnlocked", Value = 0 });
                    resourcesLists.Add(new ResourcesList { Key = "Sc_PercentageUnlockedDouble", Value = (double)0 });
                    resourcesLists.Add(new ResourcesList { Key = "Sc_PercentageUnlockedString", Value = "0" });

                    resourcesLists.Add(new ResourcesList { Key = "Sc_EnableIntegrationInCustomTheme", Value = PluginDatabase.PluginSettings.EnableIntegrationInCustomTheme });
                    resourcesLists.Add(new ResourcesList { Key = "Sc_IntegrationShowGraphic", Value = PluginDatabase.PluginSettings.IntegrationShowGraphic });
                    resourcesLists.Add(new ResourcesList { Key = "Sc_IntegrationShowAchievements", Value = PluginDatabase.PluginSettings.IntegrationShowAchievements });
                    resourcesLists.Add(new ResourcesList { Key = "Sc_IntegrationShowProgressBar", Value = PluginDatabase.PluginSettings.IntegrationShowProgressBar });
                    resourcesLists.Add(new ResourcesList { Key = "Sc_IntegrationShowAchievementsCompactLocked", Value = PluginDatabase.PluginSettings.IntegrationShowAchievementsCompactLocked });
                    resourcesLists.Add(new ResourcesList { Key = "Sc_IntegrationShowAchievementsCompactUnlocked", Value = PluginDatabase.PluginSettings.IntegrationShowAchievementsCompactUnlocked });
                    ui.AddResources(resourcesLists);


                    // Load data
                    if (!PluginDatabase.IsLoaded)
                    {
                        return;
                    }
                    GameAchievements successStories = PluginDatabase.Get(GameSelected);

                    if (successStories.HasData)
                    {
                        resourcesLists = new List<ResourcesList>();
                        resourcesLists.Add(new ResourcesList { Key = "Sc_HasData", Value = true });
                        resourcesLists.Add(new ResourcesList { Key = "Sc_Is100Percent", Value = successStories.Is100Percent });
                        resourcesLists.Add(new ResourcesList { Key = "Sc_Total", Value = successStories.Total });
                        resourcesLists.Add(new ResourcesList { Key = "Sc_TotalDouble", Value = double.Parse(successStories.Total.ToString()) });
                        resourcesLists.Add(new ResourcesList { Key = "Sc_TotalString", Value = successStories.Total.ToString() });
                        resourcesLists.Add(new ResourcesList { Key = "Sc_Unlocked", Value = successStories.Unlocked });
                        resourcesLists.Add(new ResourcesList { Key = "Sc_UnlockedDouble", Value = double.Parse(successStories.Unlocked.ToString()) });
                        resourcesLists.Add(new ResourcesList { Key = "Sc_UnlockedString", Value = successStories.Unlocked.ToString() });
                        resourcesLists.Add(new ResourcesList { Key = "Sc_Locked", Value = successStories.Locked });
                        resourcesLists.Add(new ResourcesList { Key = "Sc_LockedDouble", Value = double.Parse(successStories.Locked.ToString()) });
                        resourcesLists.Add(new ResourcesList { Key = "Sc_LockedString", Value = successStories.Locked.ToString() });
                        resourcesLists.Add(new ResourcesList { Key = "Sc_PercentageUnlocked", Value = successStories.Progression });
                        resourcesLists.Add(new ResourcesList { Key = "Sc_PercentageUnlockedDouble", Value = (double)successStories.Progression });
                        resourcesLists.Add(new ResourcesList { Key = "Sc_PercentageUnlockedString", Value = successStories.Progression.ToString() });
                    }

                    // If not cancel, show
                    if (!ct.IsCancellationRequested && GameSelected.Id == SuccessStoryDatabase.GameSelected.Id)
                    {
                        ui.AddResources(resourcesLists);

                        if (_PlayniteApi.ApplicationInfo.Mode == ApplicationMode.Desktop)
                        {
                            PluginDatabase.SetCurrent(successStories);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Common.LogError(ex, "SuccessStory", $"Error on TaskRefreshBtActionBar()");
                }
#if DEBUG
                stopwatch.Stop();
                ts = stopwatch.Elapsed;
                logger.Debug($"SuccessStory [Ignored] - TaskRefresh(){IsCanceld} - End - {String.Format("{0:00}:{1:00}.{2:00}", ts.Minutes, ts.Seconds, ts.Milliseconds / 10)}");
#endif
            }, ct);

            taskHelper.Add(TaskRefresh, tokenSource);
        }


        #region BtActionBar
        public override void InitialBtActionBar()
        {

        }

        public override void AddBtActionBar()
        {
            if (PART_BtActionBar != null)
            {
#if DEBUG
                logger.Debug($"SuccessStory [Ignored] - PART_BtActionBar allready insert");
#endif
                return;
            }

            FrameworkElement BtActionBar;

            if (PluginDatabase.PluginSettings.EnableIntegrationInDescriptionWithToggle)
            {
                if (PluginDatabase.PluginSettings.EnableIntegrationButtonDetails)
                {
                    BtActionBar = new SuccessStoryToggleButtonDetails();
                }
                else
                {
                    BtActionBar = new SuccessStoryToggleButton();
                }

                ((ToggleButton)BtActionBar).Click += OnBtActionBarToggleButtonClick;
            }
            else
            {
                if (PluginDatabase.PluginSettings.EnableIntegrationButtonDetails)
                {
                    BtActionBar = new SuccessStoryButtonDetails();
                }
                else
                {
                    BtActionBar = new SuccessStoryButton(PluginDatabase.PluginSettings.EnableIntegrationInDescriptionOnlyIcon);
                }

                ((Button)BtActionBar).Click += OnBtActionBarClick;
            }

            if (!PluginDatabase.PluginSettings.EnableIntegrationInDescriptionOnlyIcon)
            {
                BtActionBar.MinWidth = 150;
            }

            BtActionBar.Name = BtActionBarName;

            try
            {
                ui.AddButtonInGameSelectedActionBarButtonOrToggleButton(BtActionBar);
                PART_BtActionBar = IntegrationUI.SearchElementByName(BtActionBarName);
            }
            catch (Exception ex)
            {
                Common.LogError(ex, "SuccessStory", "Error on AddBtActionBar()");
            }
        }

        public override void RefreshBtActionBar()
        {

        }


        public void OnBtActionBarClick(object sender, RoutedEventArgs e)
        {
#if DEBUG
            logger.Debug($"SuccessStory [Ignored] - OnBtActionBarClick()");
#endif
            PluginDatabase.IsViewOpen = true;
            dynamic ViewExtension = null;
            if (PluginDatabase.PluginSettings.EnableOneGameView)
            {
                ViewExtension = new SuccessStoryOneGameView(SuccessStoryDatabase.GameSelected);
            }
            else
            {
                if (PluginDatabase.PluginSettings.EnableRetroAchievementsView && PlayniteTools.IsGameEmulated(_PlayniteApi, SuccessStoryDatabase.GameSelected))
                {
                    ViewExtension = new SuccessView(_Plugin, _PlayniteApi, _Plugin.GetPluginUserDataPath(), true, SuccessStoryDatabase.GameSelected);
                }
                else
                {
                    ViewExtension = new SuccessView(_Plugin, _PlayniteApi, _Plugin.GetPluginUserDataPath(), false, SuccessStoryDatabase.GameSelected);
                }
            }
            Window windowExtension = PlayniteUiHelper.CreateExtensionWindow(_PlayniteApi, resources.GetString("LOCSuccessStory"), ViewExtension);
            windowExtension.ShowDialog();
            PluginDatabase.IsViewOpen = false;
        }

        public void OnCustomThemeButtonClick(object sender, RoutedEventArgs e)
        {
            if (PluginDatabase.PluginSettings.EnableIntegrationInCustomTheme)
            {
                string ButtonName = string.Empty;
                try
                {
                    ButtonName = ((Button)sender).Name;
                    if (ButtonName == "PART_ScCustomButton")
                    {
#if DEBUG
                        logger.Debug($"SuccessStory [Ignored] - OnCustomThemeButtonClick()");
#endif
                        OnBtActionBarClick(sender, e);
                    }
                }
                catch (Exception ex)
                {
                    Common.LogError(ex, "SuccessStory", "OnCustomThemeButtonClick() error");
                }
            }
        }
        #endregion


        #region SpDescription
        public override void InitialSpDescription()
        {

        }

        public override void AddSpDescription()
        {
            if (PART_SpDescription != null)
            {
#if DEBUG
                logger.Debug($"SuccessStory [Ignored] - PART_SpDescription allready insert");
#endif
                return;
            }

            try
            {
                ScDescriptionIntegration SpDescription = new ScDescriptionIntegration();
                SpDescription.Name = SpDescriptionName;

                ui.AddElementInGameSelectedDescription(SpDescription, PluginDatabase.PluginSettings.IntegrationTopGameDetails);
                PART_SpDescription = IntegrationUI.SearchElementByName(SpDescriptionName);

                if (PluginDatabase.PluginSettings.EnableIntegrationInDescriptionWithToggle && PART_SpDescription != null)
                {
                    if (PART_BtActionBar != null && PART_BtActionBar is ToggleButton)
                    {
                        ((ToggleButton)PART_BtActionBar).IsChecked = false;
                    }
                    else
                    {
                        logger.Warn($"SuccessStory - PART_BtActionBar is null or not ToggleButton");
                    }

                    PART_SpDescription.Visibility = Visibility.Collapsed;
                }
            }
            catch (Exception ex)
            {
                Common.LogError(ex, "SuccessStory", "Error on AddSpDescription()");
            }
        }

        public override void RefreshSpDescription()
        {

        }
        #endregion


        #region CustomElements
        public override void InitialCustomElements()
        {

        }

        public override void AddCustomElements()
        {
            if (ListCustomElements.Count > 0)
            {
#if DEBUG
                logger.Debug($"SuccessStory [Ignored] - CustomElements allready insert - {ListCustomElements.Count}");
#endif
                return;
            }

            FrameworkElement PART_ScButtonWithJustIcon = null;
            FrameworkElement PART_ScButtonWithTitle = null;
            FrameworkElement PART_ScButtonWithTitleAndDetails = null;

            FrameworkElement PART_Achievements_ProgressBar = null;
            FrameworkElement PART_Achievements_Graphics = null;
            FrameworkElement PART_Achievements_List = null;
            FrameworkElement PART_Achievements_ListComptact = null;
            FrameworkElement PART_Achievements_ListComptactVertical = null;
            FrameworkElement PART_Achievements_ListCompactUnlocked = null;
            FrameworkElement PART_Achievements_ListCompactLocked = null;
            FrameworkElement PART_Achievements_ListCompactVerticalUnlocked = null;
            FrameworkElement PART_Achievements_ListCompactVerticalLocked = null;

            FrameworkElement PART_ScUserStats = null;
            try
            {
                PART_ScButtonWithJustIcon = IntegrationUI.SearchElementByName("PART_ScButtonWithJustIcon", false, true);
                PART_ScButtonWithTitle = IntegrationUI.SearchElementByName("PART_ScButtonWithTitle", false, true);
                PART_ScButtonWithTitleAndDetails = IntegrationUI.SearchElementByName("PART_ScButtonWithTitleAndDetails", false, true);

                PART_Achievements_ProgressBar = IntegrationUI.SearchElementByName("PART_Achievements_ProgressBar", false, true);
                PART_Achievements_Graphics = IntegrationUI.SearchElementByName("PART_Achievements_Graphics", false, true);
                PART_Achievements_List = IntegrationUI.SearchElementByName("PART_Achievements_List", false, true);
                PART_Achievements_ListComptact = IntegrationUI.SearchElementByName("PART_Achievements_ListComptact", false, true);
                PART_Achievements_ListComptactVertical = IntegrationUI.SearchElementByName("PART_Achievements_ListComptactVertical", false, true);
                PART_Achievements_ListCompactUnlocked = IntegrationUI.SearchElementByName("PART_Achievements_ListCompactUnlocked", false, true);
                PART_Achievements_ListCompactLocked = IntegrationUI.SearchElementByName("PART_Achievements_ListCompactLocked", false, true);
                PART_Achievements_ListCompactVerticalUnlocked = IntegrationUI.SearchElementByName("PART_Achievements_ListCompactVerticalUnlocked", false, true);
                PART_Achievements_ListCompactVerticalLocked = IntegrationUI.SearchElementByName("PART_Achievements_ListCompactVerticalLocked", false, true);

                PART_ScUserStats = IntegrationUI.SearchElementByName("PART_ScUserStats", false, true);
            }
            catch (Exception ex)
            {
                Common.LogError(ex, "SuccessStory", $"Error on find custom element");
            }

            if (PART_ScButtonWithJustIcon != null)
            {
                PART_ScButtonWithJustIcon = new SuccessStoryButton(true);
                ((Button)PART_ScButtonWithJustIcon).Click += OnBtActionBarClick;
                try
                {
                    ui.AddElementInCustomTheme(PART_ScButtonWithJustIcon, "PART_ScButtonWithJustIcon");
                    ListCustomElements.Add(new CustomElement { ParentElementName = "PART_ScButtonWithJustIcon", Element = PART_ScButtonWithJustIcon });
                }
                catch (Exception ex)
                {
                    Common.LogError(ex, "SuccessStory", "Error on AddCustomElements()");
                }
            }
            else
            {
#if DEBUG
                logger.Debug($"SuccessStory [Ignored] - PART_ScButtonWithJustIcon not find");
#endif
            }

            if (PART_ScButtonWithTitle != null)
            {
                PART_ScButtonWithTitle = new SuccessStoryButton(false);
                ((Button)PART_ScButtonWithTitle).Click += OnBtActionBarClick;
                try
                {
                    ui.AddElementInCustomTheme(PART_ScButtonWithTitle, "PART_ScButtonWithTitle");
                    ListCustomElements.Add(new CustomElement { ParentElementName = "PART_ScButtonWithTitle", Element = PART_ScButtonWithTitle });
                }
                catch (Exception ex)
                {
                    Common.LogError(ex, "SuccessStory", "Error on AddCustomElements()");
                }
            }
            else
            {
#if DEBUG
                logger.Debug($"SuccessStory [Ignored] - PART_ScButtonWithTitle not find");
#endif
            }

            if (PART_ScButtonWithTitleAndDetails != null)
            {
                PART_ScButtonWithTitleAndDetails = new SuccessStoryButtonDetails();
                ((Button)PART_ScButtonWithTitleAndDetails).Click += OnBtActionBarClick;
                try
                {
                    ui.AddElementInCustomTheme(PART_ScButtonWithTitleAndDetails, "PART_ScButtonWithTitleAndDetails");
                    ListCustomElements.Add(new CustomElement { ParentElementName = "PART_ScButtonWithTitleAndDetails", Element = PART_ScButtonWithTitleAndDetails });
                }
                catch (Exception ex)
                {
                    Common.LogError(ex, "SuccessStory", "Error on AddCustomElements()");
                }
            }
            else
            {
#if DEBUG
                logger.Debug($"SuccessStory [Ignored] - PART_ScButtonWithTitleAndDetails not find");
#endif
            }


            if (PART_Achievements_ProgressBar != null && PluginDatabase.PluginSettings.IntegrationShowProgressBar)
            {
                PART_Achievements_ProgressBar = new SuccessStoryAchievementsProgressBar();
                PART_Achievements_ProgressBar.Name = "Achievements_ProgressBar";
                try
                {
                    ui.AddElementInCustomTheme(PART_Achievements_ProgressBar, "PART_Achievements_ProgressBar");
                    ListCustomElements.Add(new CustomElement { ParentElementName = "PART_Achievements_ProgressBar", Element = PART_Achievements_ProgressBar });
                }
                catch (Exception ex)
                {
                    Common.LogError(ex, "SuccessStory", "Error on AddCustomElements()");
                }
            }
            else
            {
#if DEBUG
                logger.Debug($"SuccessStory [Ignored] - PART_Achievements_ProgressBar not find");
#endif
            }

            if (PART_Achievements_Graphics != null && PluginDatabase.PluginSettings.IntegrationShowGraphic)
            {
                PART_Achievements_Graphics = new SuccessStoryAchievementsGraphics();
                PART_Achievements_Graphics.Name = "Achievements_Graphics";
                try
                {
                    ui.AddElementInCustomTheme(PART_Achievements_Graphics, "PART_Achievements_Graphics");
                    ListCustomElements.Add(new CustomElement { ParentElementName = "PART_Achievements_Graphics", Element = PART_Achievements_Graphics });
                }
                catch (Exception ex)
                {
                    Common.LogError(ex, "SuccessStory", "Error on AddCustomElements()");
                }
            }
            else
            {
#if DEBUG
                logger.Debug($"SuccessStory [Ignored] - PART_Achievements_Graphics not find");
#endif
            }

            if (PART_Achievements_ListComptact != null && PluginDatabase.PluginSettings.IntegrationShowAchievementsCompact)
            {
                PART_Achievements_ListComptact = new ScAchievementsListCompact();
                PART_Achievements_ListComptact.Name = "Achievements_ListCompact";
                try
                {
                    ui.AddElementInCustomTheme(PART_Achievements_ListComptact, "PART_Achievements_ListComptact");
                    ListCustomElements.Add(new CustomElement { ParentElementName = "PART_Achievements_ListComptact", Element = PART_Achievements_ListComptact });
                }
                catch (Exception ex)
                {
                    Common.LogError(ex, "SuccessStory", "Error on AddCustomElements()");
                }
            }
            else
            {
#if DEBUG
                logger.Debug($"SuccessStory [Ignored] - PART_Achievements_ListComptact not find");
#endif
            }

            if (PART_Achievements_ListComptactVertical != null)// && PluginDatabase.PluginSettings.IntegrationShowAchievementsCompact)
            {
                PART_Achievements_ListComptactVertical = new ScAchievementsListCompactVertical();
                PART_Achievements_ListComptactVertical.Name = "Achievements_ListCompactVertical";
                try
                {
                    ui.AddElementInCustomTheme(PART_Achievements_ListComptactVertical, "PART_Achievements_ListComptactVertical");
                    ListCustomElements.Add(new CustomElement { ParentElementName = "PART_Achievements_ListComptactVertical", Element = PART_Achievements_ListComptactVertical });
                }
                catch (Exception ex)
                {
                    Common.LogError(ex, "SuccessStory", "Error on AddCustomElements()");
                }
            }
            else
            {
#if DEBUG
                logger.Debug($"SuccessStory [Ignored] - PART_Achievements_ListComptactVertical not find");
#endif
            }

            if (PART_Achievements_List != null && PluginDatabase.PluginSettings.IntegrationShowAchievements)
            {
                PART_Achievements_List = new SuccessStoryAchievementsList();
                PART_Achievements_List.Name = "Achievements_List";
                try
                {
                    ui.AddElementInCustomTheme(PART_Achievements_List, "PART_Achievements_List");
                    ListCustomElements.Add(new CustomElement { ParentElementName = "PART_Achievements_List", Element = PART_Achievements_List });
                }
                catch (Exception ex)
                {
                    Common.LogError(ex, "SuccessStory", "Error on AddCustomElements()");
                }
            }
            else
            {
#if DEBUG
                logger.Debug($"SuccessStory [Ignored] - PART_Achievements_List not find");
#endif
            }

            if (PART_Achievements_ListCompactUnlocked != null && PluginDatabase.PluginSettings.IntegrationShowAchievements)
            {
                PART_Achievements_ListCompactUnlocked = new SuccessStoryAchievementsCompact(true);
                PART_Achievements_ListCompactUnlocked.Name = "Achievements_ListCompactUnlocked";
                try
                {
                    ui.AddElementInCustomTheme(PART_Achievements_ListCompactUnlocked, "PART_Achievements_ListCompactUnlocked");
                    ListCustomElements.Add(new CustomElement { ParentElementName = "PART_Achievements_ListCompactUnlocked", Element = PART_Achievements_ListCompactUnlocked });
                }
                catch (Exception ex)
                {
                    Common.LogError(ex, "SuccessStory", "Error on AddCustomElements()");
                }
            }
            else
            {
#if DEBUG
                logger.Debug($"SuccessStory [Ignored] - PART_Achievements_ListCompactUnlocked not find");
#endif
            }

            if (PART_Achievements_ListCompactLocked != null && PluginDatabase.PluginSettings.IntegrationShowAchievements)
            {
                PART_Achievements_ListCompactLocked = new SuccessStoryAchievementsCompact();
                PART_Achievements_ListCompactLocked.Name = "Achievements_ListCompactLocked";
                try
                {
                    ui.AddElementInCustomTheme(PART_Achievements_ListCompactLocked, "PART_Achievements_ListCompactLocked");
                    ListCustomElements.Add(new CustomElement { ParentElementName = "PART_Achievements_ListCompactLocked", Element = PART_Achievements_ListCompactLocked });
                }
                catch (Exception ex)
                {
                    Common.LogError(ex, "SuccessStory", "Error on AddCustomElements()");
                }
            }
            else
            {
#if DEBUG
                logger.Debug($"SuccessStory [Ignored] - PART_Achievements_ListCompactLocked not find");
#endif
            }

            if (PART_Achievements_ListCompactVerticalUnlocked != null)// && PluginDatabase.PluginSettings.IntegrationShowAchievements)
            {
                PART_Achievements_ListCompactVerticalUnlocked = new SuccessStoryAchievementsCompactVertical(true);
                PART_Achievements_ListCompactVerticalUnlocked.Name = "Achievements_ListCompactVerticalUnlocked";
                try
                {
                    ui.AddElementInCustomTheme(PART_Achievements_ListCompactVerticalUnlocked, "PART_Achievements_ListCompactVerticalUnlocked");
                    ListCustomElements.Add(new CustomElement { ParentElementName = "PART_Achievements_ListCompactVerticalUnlocked", Element = PART_Achievements_ListCompactVerticalUnlocked });
                }
                catch (Exception ex)
                {
                    Common.LogError(ex, "SuccessStory", "Error on AddCustomElements()");
                }
            }
            else
            {
#if DEBUG
                logger.Debug($"SuccessStory [Ignored] - PART_Achievements_ListCompactVerticalUnlocked not find");
#endif
            }

            if (PART_Achievements_ListCompactVerticalLocked != null)// && PluginDatabase.PluginSettings.IntegrationShowAchievements)
            {
                PART_Achievements_ListCompactVerticalLocked = new SuccessStoryAchievementsCompactVertical();
                PART_Achievements_ListCompactVerticalLocked.Name = "Achievements_ListCompactVerticalLocked";
                try
                {
                    ui.AddElementInCustomTheme(PART_Achievements_ListCompactVerticalLocked, "PART_Achievements_ListCompactVerticalLocked");
                    ListCustomElements.Add(new CustomElement { ParentElementName = "PART_Achievements_ListCompactVerticalLocked", Element = PART_Achievements_ListCompactVerticalLocked });
                }
                catch (Exception ex)
                {
                    Common.LogError(ex, "SuccessStory", "Error on AddCustomElements()");
                }
            }
            else
            {
#if DEBUG
                logger.Debug($"SuccessStory [Ignored] - PART_Achievements_ListCompactLocked not find");
#endif
            }

            if (PART_ScUserStats != null && PluginDatabase.PluginSettings.IntegrationShowUserStats)
            {
                PART_ScUserStats = new SuccessStoryUserStats();
                PART_ScUserStats.Name = "UserStats_List";
                try
                {
                    ui.AddElementInCustomTheme(PART_ScUserStats, "PART_ScUserStats");
                    ListCustomElements.Add(new CustomElement { ParentElementName = "PART_ScUserStats", Element = PART_ScUserStats });
                }
                catch (Exception ex)
                {
                    Common.LogError(ex, "SuccessStory", "Error on AddCustomElements()");
                }
            }
            else
            {
#if DEBUG
                logger.Debug($"SuccessStory [Ignored] - PART_UserStats not find");
#endif
            }
        }

        public override void RefreshCustomElements()
        {

        }
        #endregion




        public override DispatcherOperation AddElementsFS()
        {
            if (_PlayniteApi.ApplicationInfo.Mode == ApplicationMode.Fullscreen)
            {
                if (IsFirstLoad)
                {
#if DEBUG
                    logger.Debug($"SuccessStory [Ignored] - IsFirstLoad");
#endif
                    Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Loaded, new ThreadStart(delegate
                    {
                        System.Threading.SpinWait.SpinUntil(() => IntegrationUI.SearchElementByName("PART_ButtonContext") != null, 5000);
                    })).Wait();
                    IsFirstLoad = false;
                }

                return Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Loaded, new ThreadStart(delegate
                {
                    if (PluginDatabase.PluginSettings.EnableIntegrationFS)
                    {
#if DEBUG
                        logger.Debug($"SuccessStory [Ignored] - AddBtInfoBarFS()");
#endif
                        AddSpInfoBarFS();
                    }
                }));
            }

            return null;
        }


        #region SpInfoBarFS
        public override void InitialSpInfoBarFS()
        {

        }

        public override void AddSpInfoBarFS()
        {
            if (PART_SpInfoBarFS != null)
            {
#if DEBUG
                logger.Debug($"SuccessStory [Ignored] - PART_BtInfoBar allready insert");
#endif

                ((SuccessStoryProgressionFS)PART_SpInfoBarFS).SetData(PluginDatabase.Get(SuccessStoryDatabase.GameSelected));
                return;
            }

            FrameworkElement SpInfoBar;
            SpInfoBar = new SuccessStoryProgressionFS();

            SpInfoBar.Name = SpInfoBarFSName;
            SpInfoBar.Margin = new Thickness(50, 0, 0, 0);

            try
            {
                ui.AddStackPanelInGameSelectedInfoBarFS(SpInfoBar);
                PART_SpInfoBarFS = IntegrationUI.SearchElementByName(SpInfoBarFSName);

                if (PART_SpInfoBarFS != null)
                {
                    ((SuccessStoryProgressionFS)PART_SpInfoBarFS).SetData(PluginDatabase.Get(SuccessStoryDatabase.GameSelected));
                }
            }
            catch (Exception ex)
            {
                Common.LogError(ex, "SuccessStory");
            }
        }

        public override void RefreshSpInfoBarFS()
        {

        }
        #endregion


        #region BtActionBarFS
        public override void InitialBtActionBarFS()
        {

        }

        public override void AddBtActionBarFS()
        {

        }

        public override void RefreshBtActionBarFS()
        {

        }
        #endregion
    }
}
