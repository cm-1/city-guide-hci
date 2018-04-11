﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MapView
{
    public class Utils
    {
        public static double Map(double value, double min, double max, double newMin, double newMax)
        {
            if (value==min)
            {
                return newMin;
            }
            return (((newMax - newMin) / (max - min)) * (value - min)) + newMin;
        }
    }

    public class RangeSliderChangedEventArgs : EventArgs
    {
        public double Low { get; set; }
        public double High { get; set; }
        public double Range
        {
            get
            {
                return Math.Abs(this.High - this.Low);
            }
        }

        public RangeSliderChangedEventArgs(double low, double high)
        {
            this.Low = low;
            this.High = high;
        }
    }
    /// <summary>
    /// Interaction logic for prioSlider.xaml
    /// Author: David Ledo 2018, Modified by Singularity
    /// </summary>
    public partial class prioSlider : UserControl
    {
        private double minimum = 0;
        private double maximum = 300.00;

        private double currentMin;
        private double currentMax;

        private Shape selection = null;

        private double rangeClickOffset;

        public event EventHandler<RangeSliderChangedEventArgs> Changed;

        private void RaiseChanged(RangeSliderChangedEventArgs e)
        {
            if (this.Changed != null)
            {
                Changed(this, e);
            }
        }

        public prioSlider()
        {
            InitializeComponent();

            if (DesignerProperties.GetIsInDesignMode(new DependencyObject())) return;

            this.Loaded += OnLoaded;
            this.currentMin = 0;
            this.currentMax = 300.00;

            this.lowerBoundTextBox.Visibility = Visibility.Hidden;
            this.upperBoundTextBox.Visibility = Visibility.Hidden;

        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            Window.GetWindow(this).MouseMove += OnWindowMouseMove;
            Window.GetWindow(this).MouseLeftButtonUp += OnBoundClickReleased;

            this.priceMinEllipse.MouseLeftButtonDown += OnBoundClicked;
            this.priceMaxEllipse.MouseLeftButtonDown += OnBoundClicked;
            this.rangeRect.MouseLeftButtonDown += OnBoundClicked;

            this.priceMinText.MouseLeftButtonDown += OnBoundTextClicked;
            this.priceMaxText.MouseLeftButtonDown += OnBoundTextClicked;

            this.lowerBoundTextBox.KeyUp += OnTextBoxEnterReleased;
            this.upperBoundTextBox.KeyUp += OnTextBoxEnterReleased;

            UpdateMaxMinFromValues();
        }

        private void UpdateMaxMinFromValues()
        {
            double minPosition = Utils.Map(this.currentMin, this.minimum, this.maximum, 0, this.priceSliderRect.Width);
            Canvas.SetLeft(this.priceMinEllipse, minPosition);

            double maxPosition = Utils.Map(this.currentMax, this.minimum, this.maximum, 0, this.priceSliderRect.Width);
            Canvas.SetLeft(this.priceMaxEllipse, maxPosition);

            this.priceMinText.Text = String.Format("{0:0.##}", this.currentMin);
            this.priceMaxText.Text = String.Format("{0:0.##}", this.currentMax);

            MoveRangeRectangle();

            RaiseChanged(new RangeSliderChangedEventArgs(this.currentMin, this.currentMax));
        }

        private void MoveRangeRectangle()
        {
            double startX = Canvas.GetLeft(this.priceMinEllipse);
            double endX = Canvas.GetLeft(this.priceMaxEllipse);
            Canvas.SetLeft(this.rangeRect, startX);
            if (endX - startX > 0)
            {
                this.rangeRect.Width = endX - startX;
            }
        }

        private void OnTextBoxEnterReleased(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                TextBox castSender = sender as TextBox;
                string textContent = castSender.Text;

                double newValue;

                if (Double.TryParse(castSender.Text, out newValue))
                {
                    if (castSender == this.lowerBoundTextBox)
                    {
                        this.currentMin = newValue;
                    }
                    else
                    {
                        this.currentMax = newValue;
                    }
                    UpdateMaxMinFromValues();
                }
                castSender.Visibility = Visibility.Hidden;
            }
        }

        private void OnBoundTextClicked(object sender, MouseButtonEventArgs e)
        {
            TextBlock castSender = sender as TextBlock;
            if(castSender == this.priceMinText)
            {
                this.lowerBoundTextBox.Visibility = Visibility.Visible;
                this.lowerBoundTextBox.Text = this.priceMinText.Text;
                this.lowerBoundTextBox.Focus();
                this.lowerBoundTextBox.SelectAll();
            }
            else if (castSender == this.priceMaxText)
            {
                this.upperBoundTextBox.Visibility = Visibility.Visible;
                this.upperBoundTextBox.Text = this.priceMaxText.Text;
                this.upperBoundTextBox.Focus();
                this.upperBoundTextBox.SelectAll();
            }
        }

        private void OnBoundClicked(object sender, MouseButtonEventArgs e)
        {
            this.selection = sender as Shape;
            if (this.selection == this.rangeRect)
            {
                this.rangeClickOffset = e.GetPosition(this.priceContainer).X - Canvas.GetLeft(this.rangeRect);
            }
        }

        private void OnBoundClickReleased(object sender, MouseButtonEventArgs e)
        {
            this.selection = null;
        }

        private void UpdateMaxMin()
        {
            double minPosition = Canvas.GetLeft(this.priceMinEllipse);
            this.currentMin = Utils.Map(minPosition, 0, this.priceSliderRect.Width, this.minimum, this.maximum);

            double maxPosition = Canvas.GetLeft(this.priceMaxEllipse);
            this.currentMax = Utils.Map(maxPosition, 0, this.priceSliderRect.Width, this.minimum, this.maximum);

            this.priceMinText.Text = String.Format("{0:0.##}", this.currentMin);
            this.priceMaxText.Text = String.Format("{0:0.##}", this.currentMax);

            RaiseChanged(new RangeSliderChangedEventArgs(this.currentMin, this.currentMax));
        }

        private void OnWindowMouseMove(object sender, MouseEventArgs e)
        {
            if (selection != null)
            {
                Canvas.SetLeft(selection, e.GetPosition(this.priceContainer).X);
                if (this.selection != rangeRect)
                {
                    MoveRangeRectangle();
                }
                else
                {
                    Canvas.SetLeft(this.priceMinEllipse, e.GetPosition(this.priceContainer).X - this.rangeClickOffset);
                    Canvas.SetLeft(this.priceMaxEllipse, e.GetPosition(this.priceContainer).X + this.rangeRect.Width - this.rangeClickOffset);
                    Canvas.SetLeft(this.rangeRect, e.GetPosition(this.priceContainer).X - this.rangeClickOffset);
                }
                UpdateMaxMin();
            }
        }
    }
}
