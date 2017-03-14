﻿using System;
using Codartis.NsDepCop.Core.Interface.Config;
using MoreLinq;

namespace Codartis.NsDepCop.Core.Implementation.Config
{
    /// <summary>
    /// Abstract base class for config provider implementations.
    /// </summary>
    /// <remarks>
    /// Uses locking to ensure that no property can be read while refreshing the config.
    /// </remarks>
    internal abstract class ConfigProviderBase : IConfigProvider, IConfigInitializer<ConfigProviderBase>
    {
        /// <summary>
        /// This lock ensures that no property can be read while refreshing the config.
        /// </summary>
        private readonly object _refreshLockObject = new object();

        private bool _isInitialized;
        private ConfigLoadResult _configLoadResult;

        protected Action<string> DiagnosticMessageHandler { get; }
        protected Parsers? OverridingParser { get; private set; }
        protected Parsers? DefaultParser { get; private set; }
        protected Importance? DefaultInfoImportance { get; private set; }

        protected ConfigProviderBase(Action<string> diagnosticMessageHandler)
        {
            DiagnosticMessageHandler = diagnosticMessageHandler;
        }

        public ConfigProviderBase OverrideParser(Parsers? overridingParser)
        {
            OverridingParser = overridingParser;
            return this;
        }

        public ConfigProviderBase SetDefaultParser(Parsers? defaultParser)
        {
            DefaultParser = defaultParser;
            return this;
        }

        public ConfigProviderBase SetDefaultInfoImportance(Importance? defaultInfoImportance)
        {
            DefaultInfoImportance = defaultInfoImportance;
            return this;
        }

        public IAnalyzerConfig Config
        {
            get
            {
                lock (_refreshLockObject)
                {
                    EnsureInitialized();
                    return _configLoadResult.Config;
                }
            }
        }

        public AnalyzerConfigState ConfigState
        {
            get
            {
                lock (_refreshLockObject)
                {
                    EnsureInitialized();
                    return _configLoadResult.ConfigState;
                }
            }
        }

        public Exception ConfigException
        {
            get
            {
                lock (_refreshLockObject)
                {
                    EnsureInitialized();
                    return _configLoadResult.ConfigException;
                }
            }
        }

        public void RefreshConfig()
        {
            lock (_refreshLockObject)
            {
                EnsureInitialized();

                var oldConfig = Config;

                _configLoadResult = RefreshConfigCore();

                if (oldConfig != Config)
                    DumpConfigToDiagnosticOutput();
            }
        }

        protected abstract ConfigLoadResult LoadConfigCore();
        protected abstract ConfigLoadResult RefreshConfigCore();

        private void EnsureInitialized()
        {
            if (_isInitialized)
                return;

            _isInitialized = true;

            DiagnosticMessageHandler?.Invoke($"Loading config {this}");
            _configLoadResult = LoadConfigCore();

            DumpConfigToDiagnosticOutput();
        }

        private void DumpConfigToDiagnosticOutput()
        {
            DiagnosticMessageHandler?.Invoke($"ConfigState={_configLoadResult.ConfigState} ({this})");

            if (_configLoadResult.Config != null)
            {
                if (OverridingParser.HasValue)
                    DiagnosticMessageHandler?.Invoke($"Parser overridden with {OverridingParser.Value}.");

                _configLoadResult.Config?.DumpToStrings().ForEach(i => DiagnosticMessageHandler?.Invoke($"  {i}"));
            }
        }
    }
}