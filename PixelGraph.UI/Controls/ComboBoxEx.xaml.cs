﻿using System.Windows;

namespace PixelGraph.UI.Controls
{
    public partial class ComboBoxEx
    {
        public object Placeholder {
            get => GetValue(PlaceholderProperty);
            set => SetValue(PlaceholderProperty, value);
        }


        public ComboBoxEx()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty PlaceholderProperty = DependencyProperty.Register("Placeholder", typeof(object), typeof(ComboBoxEx));
    }
}