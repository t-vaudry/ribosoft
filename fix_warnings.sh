#!/bin/bash

# Fix CS8618 warnings by initializing string properties to string.Empty

# Account ViewModels
sed -i 's/public string Email { get; set; }/public string Email { get; set; } = string.Empty;/g' Ribosoft/Models/AccountViewModels/*.cs
sed -i 's/public string Password { get; set; }/public string Password { get; set; } = string.Empty;/g' Ribosoft/Models/AccountViewModels/*.cs
sed -i 's/public string ConfirmPassword { get; set; }/public string ConfirmPassword { get; set; } = string.Empty;/g' Ribosoft/Models/AccountViewModels/*.cs
sed -i 's/public string Code { get; set; }/public string Code { get; set; } = string.Empty;/g' Ribosoft/Models/AccountViewModels/*.cs
sed -i 's/public string RecoveryCode { get; set; }/public string RecoveryCode { get; set; } = string.Empty;/g' Ribosoft/Models/AccountViewModels/*.cs
sed -i 's/public string TwoFactorCode { get; set; }/public string TwoFactorCode { get; set; } = string.Empty;/g' Ribosoft/Models/AccountViewModels/*.cs

# Manage ViewModels
sed -i 's/public string Username { get; set; }/public string Username { get; set; } = string.Empty;/g' Ribosoft/Models/ManageViewModels/*.cs
sed -i 's/public string PhoneNumber { get; set; }/public string PhoneNumber { get; set; } = string.Empty;/g' Ribosoft/Models/ManageViewModels/*.cs
sed -i 's/public string StatusMessage { get; set; }/public string StatusMessage { get; set; } = string.Empty;/g' Ribosoft/Models/ManageViewModels/*.cs
sed -i 's/public string NewPassword { get; set; }/public string NewPassword { get; set; } = string.Empty;/g' Ribosoft/Models/ManageViewModels/*.cs
sed -i 's/public string OldPassword { get; set; }/public string OldPassword { get; set; } = string.Empty;/g' Ribosoft/Models/ManageViewModels/*.cs
sed -i 's/public string LoginProvider { get; set; }/public string LoginProvider { get; set; } = string.Empty;/g' Ribosoft/Models/ManageViewModels/*.cs
sed -i 's/public string ProviderKey { get; set; }/public string ProviderKey { get; set; } = string.Empty;/g' Ribosoft/Models/ManageViewModels/*.cs
sed -i 's/public string SharedKey { get; set; }/public string SharedKey { get; set; } = string.Empty;/g' Ribosoft/Models/ManageViewModels/*.cs
sed -i 's/public string AuthenticatorUri { get; set; }/public string AuthenticatorUri { get; set; } = string.Empty;/g' Ribosoft/Models/ManageViewModels/*.cs

# Request ViewModels
sed -i 's/public string Name { get; set; }/public string Name { get; set; } = string.Empty;/g' Ribosoft/Models/RequestViewModels/*.cs

# Other Models
sed -i 's/public string Name { get; set; }/public string Name { get; set; } = string.Empty;/g' Ribosoft/Models/RibozymeViewModel/*.cs
sed -i 's/public string Name { get; set; }/public string Name { get; set; } = string.Empty;/g' Ribosoft/Models/*.cs

echo "Fixed common CS8618 warnings"
