﻿using Microsoft.Extensions.DependencyInjection;
using PixelGraph.UI.Internal.Utilities;
using PixelGraph.UI.ViewModels;
using System;
using System.Windows;

namespace PixelGraph.UI.Windows
{
    public partial class NewMaterialWindow
    {
        private readonly NewMaterialViewModel viewModel;


        public NewMaterialWindow(IServiceProvider provider)
        {
            var themeHelper = provider.GetRequiredService<IThemeHelper>();

            InitializeComponent();
            themeHelper.ApplyCurrent(this);

            viewModel = new NewMaterialViewModel {
                Model = Model,
            };

            viewModel.UpdateBlockList();
            viewModel.UpdateLocation();
        }

        private void OnGameObjectTypeChanged(object sender, EventArgs e)
        {
            viewModel.UpdateBlockList();
        }

        private void OnGameObjectLocationChanged(object sender, EventArgs e)
        {
            viewModel.UpdateLocation();
        }

        private void OnCancelButtonClick(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void OnCreateButtonClick(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(Model.GameNamespace)) {
                MessageBox.Show(this, "Namespace cannot be empty!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                NamespaceComboBox.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(Model.GameObjectName)) {
                MessageBox.Show(this, "Name cannot be empty!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                NameComboBox.Focus();
                return;
            }

            DialogResult = true;
        }
    }
}
