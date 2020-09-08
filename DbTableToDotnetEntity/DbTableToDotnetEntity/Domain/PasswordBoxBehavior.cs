﻿using System.Windows;
using System.Windows.Controls;
using System.Windows.Interactivity;

namespace DbTableToDotnetEntity.Domain
{
    public class PasswordBoxBehavior : Behavior<PasswordBox>
    {
        protected override void OnAttached()
        {
            base.OnAttached();

            AssociatedObject.PasswordChanged += OnPasswordChanged;
        }

        private static void OnPasswordChanged(object sender, RoutedEventArgs e)
        {
            PasswordBox passwordBox = sender as PasswordBox;

            string password = PasswordBoxHelper.GetPassword(passwordBox);

            if (passwordBox != null && passwordBox.Password != password)
            {
                PasswordBoxHelper.SetPassword(passwordBox, passwordBox.Password);
            }
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();

            AssociatedObject.PasswordChanged -= OnPasswordChanged;
        }
    }
}