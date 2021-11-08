﻿// Decompiled with JetBrains decompiler
// Type: Microsoft.Band.LoggerProviderStub
// Assembly: Microsoft.Band, Version=1.3.20517.1, Culture=neutral, PublicKeyToken=null
// MVID: AFCBFE03-E2CF-481D-86F4-92C60C36D26A
// Assembly location: C:\Users\Pdawg\Downloads\Microsoft Band Sync Setup\Microsoft_Band.dll

using System;
using System.Collections.Generic;
using System.Net;
#if DEBUG
using Logger = System.Diagnostics.Debug;
#else
using Logger = System.Console;
#endif

namespace Microsoft.Band
{
    internal class LoggerProviderStub : ILoggerProvider
    {
        public static readonly LoggerProviderStub _default = new();

        public static LoggerProviderStub Default => _default;

        public void Log(ProviderLogLevel level, string message, params object[] args)
        {
            Logger.WriteLine($"[{level}]  {GetCallerName()}" + Environment.NewLine +
                FormatAndIndent(message, args));
        }

        public void LogException(ProviderLogLevel level, Exception e)
        {
            Logger.WriteLine($"[{level}]  Exception thrown from {GetCallerName()}:" + Environment.NewLine +
                FormatAndIndent(e.ToString()));
        }

        public void LogWebException(ProviderLogLevel level, WebException e)
        {
            Logger.WriteLine($"[{level}]  Web exception thrown from {GetCallerName()}:" + Environment.NewLine +
                $"{FormatAndIndent(e.ToString())}");
        }

        public void LogException(ProviderLogLevel level, Exception e, string message, params object[] args)
        {
            Logger.WriteLine($"[{level}]  {GetCallerName()}" + Environment.NewLine +
                $"{FormatAndIndent(message, args)}" + Environment.NewLine +
                $"Exception:" + Environment.NewLine +
                $"{FormatAndIndent(e.ToString())}");
        }

        public void PerfStart(string eventName)
        {
        }

        public void PerfEnd(string eventName)
        {
        }

        public void TelemetryEvent(string eventName, IDictionary<string, string> properties, IDictionary<string, double> metrics)
        {
        }

        private static string GetCallerName()
        {
            return (new System.Diagnostics.StackTrace()).GetFrame(2).GetMethod().Name;
        }

        private static string FormatAndIndent(string message, params object[] args)
        {
            string output = "\t" + string.Format(message, args);
            return output.Replace(Environment.NewLine, Environment.NewLine + "\t");
        }
    }
}
