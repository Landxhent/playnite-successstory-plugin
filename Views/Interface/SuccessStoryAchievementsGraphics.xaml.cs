﻿using LiveCharts;
using LiveCharts.Configurations;
using LiveCharts.Wpf;
using Newtonsoft.Json;
using Playnite.SDK;
using PluginCommon;
using PluginCommon.LiveChartsCommon;
using SuccessStory.Models;
using SuccessStory.Services;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace SuccessStory.Views.Interface
{
    /// <summary>
    /// Logique d'interaction pour SuccessStoryAchievementsGraphics.xaml
    /// </summary>
    public partial class SuccessStoryAchievementsGraphics : UserControl
    {
        private static readonly ILogger logger = LogManager.GetLogger();

        private SuccessStoryDatabase PluginDatabase = SuccessStory.PluginDatabase;


        public SuccessStoryAchievementsGraphics()
        {
            InitializeComponent();


            //let create a mapper so LiveCharts know how to plot our CustomerViewModel class
            var customerVmMapper = Mappers.Xy<CustomerForSingle>()
                .X((value, index) => index)
                .Y(value => value.Values);

            //lets save the mapper globally
            Charting.For<CustomerForSingle>(customerVmMapper);
            

            PluginDatabase.PropertyChanged += OnPropertyChanged;
        }


        protected void OnPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            try
            {
#if DEBUG
                logger.Debug($"SuccessStoryAchievementsGraphics.OnPropertyChanged({e.PropertyName}): {JsonConvert.SerializeObject(PluginDatabase.GameSelectedData)}");
#endif
                if (e.PropertyName == "GameSelectedData" || e.PropertyName == "PluginSettings")
                {
                    this.Dispatcher.BeginInvoke(DispatcherPriority.Loaded, new ThreadStart(delegate
                    {
                        SetScData(PluginDatabase.GameSelectedData.Id);
                    }));
                }
            }
            catch (Exception ex)
            {
                Common.LogError(ex, "SuccessStory");
            }
        }


        public void SetScData(Guid? Id = null, int limit = 0)
        {
            AchievementsGraphicsDataCount GraphicsData = null;

            if (limit == 0)
            {
                limit = (PluginDatabase.PluginSettings.IntegrationGraphicOptionsCountAbscissa - 1);
            }

            if (!PluginDatabase.PluginSettings.GraphicAllUnlockedByDay)
            {
                GraphicsData = PluginDatabase.GetCountByMonth(Id, limit);
            }
            else
            {
                GraphicsData = PluginDatabase.GetCountByDay(Id, limit);
            }
            string[] StatsGraphicsAchievementsLabels = GraphicsData.Labels;
            SeriesCollection StatsGraphicAchievementsSeries = new SeriesCollection();
            StatsGraphicAchievementsSeries.Add(new LineSeries
            {
                Title = string.Empty,
                Values = GraphicsData.Series
            });


            StatsGraphicAchievements.Series = StatsGraphicAchievementsSeries;
            StatsGraphicAchievementsX.Labels = StatsGraphicsAchievementsLabels;

            if (!PluginDatabase.PluginSettings.IgnoreSettings)
            {
                StatsGraphicAchievementsX.ShowLabels = PluginDatabase.PluginSettings.EnableIntegrationAxisGraphic;
                StatsGraphicAchievementsY.ShowLabels = PluginDatabase.PluginSettings.EnableIntegrationOrdinatesGraphic;
            }
        }


        public void DisableAnimations(bool IsDisable)
        {
            StatsGraphicAchievements.DisableAnimations = IsDisable;
        }


        private void StatsGraphicAchievements_Loaded(object sender, RoutedEventArgs e)
        {
            IntegrationUI.SetControlSize((FrameworkElement)sender);

            ((FrameworkElement)((FrameworkElement)sender).Parent).Height = ((FrameworkElement)sender).Height;
            ((FrameworkElement)sender).Height = ((FrameworkElement)sender).Height + 18;
        }
    }
}
