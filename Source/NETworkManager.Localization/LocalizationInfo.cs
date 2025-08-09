﻿using System;

namespace NETworkManager.Localization;

/// <summary>
///     Class to hold all information's about a localization.
/// </summary>
public class LocalizationInfo
{
    /// <summary>
    ///     Create an empty instance of <see cref="LocalizationInfo" />.
    /// </summary>
    public LocalizationInfo()
    {

    }

    /// <summary>
    ///     Create an instance of <see cref="LocalizationInfo" /> with all parameters.
    /// </summary>
    /// <param name="name"><see cref="Name" />.</param>
    /// <param name="nativeName"><see cref="NativeName" />.</param>
    /// <param name="flagUri"><see cref="FlagUri" />.</param>
    /// <param name="code"><see cref="Code" />.</param>
    /// <param name="isOfficial"><see cref="IsOfficial" />.</param>
    public LocalizationInfo(string name, string nativeName, Uri flagUri, string code, bool isOfficial = false)
    {
        Name = name;
        NativeName = nativeName;
        FlagUri = flagUri;
        Code = code;
        IsOfficial = isOfficial;
    }

    /// <summary>
    ///     Name of the Language.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    ///     Native name of the language.
    /// </summary>
    public string NativeName { get; set; }

    /// <summary>
    ///     Uri of the country flag.
    /// </summary>
    public Uri FlagUri { get; set; }

    /// <summary>
    ///     Culture code of the language.
    /// </summary>
    public string Code { get; set; }

    /// <summary>
    ///     Indicates whether the language has been translated by the maintainer or the community
    /// </summary>
    public bool IsOfficial { get; set; }
}