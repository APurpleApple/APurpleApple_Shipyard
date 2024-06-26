﻿using HarmonyLib;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Shockah.Shared;

public static class HarmonyExt
{
    public static void PatchVirtual(
        this Harmony self,
        MethodBase? original,
        ILogger logger,
        LogLevel problemLogLevel = LogLevel.Error,
        LogLevel successLogLevel = LogLevel.Trace,
        HarmonyMethod? prefix = null,
        HarmonyMethod? postfix = null,
        HarmonyMethod? transpiler = null,
        HarmonyMethod? finalizer = null
    )
    {
        if (original is null)
        {
            logger.Log(problemLogLevel, "Could not patch method - the mod may not work correctly.\nReason: Unknown method to patch.");
            return;
        }

        Type? declaringType = original.DeclaringType;
        if (declaringType == null)
            throw new ArgumentException($"{nameof(original)}.{nameof(original.DeclaringType)} is null.");
        foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            IEnumerable<Type> subtypes = Enumerable.Empty<Type>();
            try
            {
                subtypes = assembly.GetTypes().Where(t => t.IsAssignableTo(declaringType));
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Trace, "There was a problem while getting types defined in assembly {Assembly}, ignoring it. Reason:\n{Exception}", assembly.GetName().Name, ex);
            }

            foreach (Type subtype in subtypes)
            {
                var originalParameters = original.GetParameters();
                var subtypeOriginal = AccessTools.Method(
                    subtype,
                    original.Name,
                    originalParameters.Select(p => p.ParameterType).ToArray()
                );
                if (subtypeOriginal is null)
                    continue;
                if (!subtypeOriginal.IsDeclaredMember())
                    continue;
                if (!subtypeOriginal.HasMethodBody())
                    continue;

                static bool ContainsNonSpecialArguments(HarmonyMethod patch)
                    => patch.method.GetParameters().Any(p => !(p.Name ?? "").StartsWith("__"));

                if (
                    (prefix is not null && ContainsNonSpecialArguments(prefix)) ||
                    (postfix is not null && ContainsNonSpecialArguments(postfix)) ||
                    (finalizer is not null && ContainsNonSpecialArguments(finalizer))
                )
                {
                    var subtypeOriginalParameters = subtypeOriginal.GetParameters();
                    for (int i = 0; i < original.GetParameters().Length; i++)
                        if (originalParameters[i].Name != subtypeOriginalParameters[i].Name)
                            throw new InvalidOperationException($"Method {declaringType.Name}.{original.Name} cannot be automatically patched for subtype {subtype.Name}, because argument #{i} has a mismatched name: `{originalParameters[i].Name}` vs `{subtypeOriginalParameters[i].Name}`.");
                }

                self.Patch(subtypeOriginal, prefix, subtypeOriginal.HasMethodBody() ? postfix : null, transpiler, finalizer);
                logger.Log(successLogLevel, "Patched method {Method}.", subtypeOriginal.FullDescription());
            }
        }
    }

    public static bool TryPatch(
        this Harmony self,
        Func<MethodBase?> original,
        ILogger logger,
        LogLevel problemLogLevel = LogLevel.Error,
        LogLevel successLogLevel = LogLevel.Trace,
        HarmonyMethod? prefix = null,
        HarmonyMethod? postfix = null,
        HarmonyMethod? transpiler = null,
        HarmonyMethod? finalizer = null
    )
    {
        var originalMethod = original();
        if (originalMethod is null)
        {
            logger.Log(problemLogLevel, "Could not patch method - the mod may not work correctly.\nReason: Unknown method to patch.");
            return false;
        }

        try
        {

            self.Patch(originalMethod, prefix, postfix, transpiler, finalizer);
            logger.Log(successLogLevel, "Patched method {Method}.", originalMethod.FullDescription());
            return true;
        }
        catch (Exception ex)
        {
            logger.Log(problemLogLevel, "Could not patch method {Method} - the mod may not work correctly.\nReason: {Exception}", originalMethod, ex);
            return false;
        }
    }

    public static (int patched, int total) TryPatchVirtual(
        this Harmony self,
        Func<MethodBase?> original,
        ILogger logger,
        LogLevel problemLogLevel = LogLevel.Error,
        LogLevel successLogLevel = LogLevel.Trace,
        HarmonyMethod? prefix = null,
        HarmonyMethod? postfix = null,
        HarmonyMethod? transpiler = null,
        HarmonyMethod? finalizer = null
    )
    {
        var originalMethod = original();
        if (originalMethod is null)
        {
            logger.Log(problemLogLevel, "Could not patch method - the mod may not work correctly.\nReason: Unknown method to patch.");
            return (0, 1);
        }

        try
        {
            int patched = 0;
            int total = 0;
            Type declaringType = originalMethod.DeclaringType ?? throw new ArgumentException($"{nameof(original)}.{nameof(originalMethod.DeclaringType)} is null.");
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                total++;
                IEnumerable<Type> subtypes = Enumerable.Empty<Type>();
                try
                {
                    subtypes = assembly.GetTypes().Where(t => t.IsAssignableTo(declaringType));
                }
                catch (Exception ex)
                {
                    logger.Log(LogLevel.Trace, "There was a problem while getting types defined in assembly {Assembly}, ignoring it. Reason:\n{Exception}", assembly.GetName().Name, ex);
                }

                foreach (Type subtype in subtypes)
                {
                    //???
                    if (subtype == typeof(AJumpScript)) continue;

                    try
                    {
                        var originalParameters = originalMethod.GetParameters();
                        var subtypeOriginal = AccessTools.Method(
                            subtype,
                            originalMethod.Name,
                            originalParameters.Select(p => p.ParameterType).ToArray()
                        );
                        if (subtypeOriginal is null)
                            continue;
                        if (!subtypeOriginal.IsDeclaredMember())
                            continue;
                        if (!subtypeOriginal.HasMethodBody())
                            continue;

                        static bool ContainsNonSpecialArguments(HarmonyMethod patch)
                            => patch.method.GetParameters().Any(p => !(p.Name ?? "").StartsWith("__"));

                        if (
                            (prefix is not null && ContainsNonSpecialArguments(prefix)) ||
                            (postfix is not null && ContainsNonSpecialArguments(postfix)) ||
                            (finalizer is not null && ContainsNonSpecialArguments(finalizer))
                        )
                        {
                            var subtypeOriginalParameters = subtypeOriginal.GetParameters();
                            for (int i = 0; i < originalMethod.GetParameters().Length; i++)
                                if (originalParameters[i].Name != subtypeOriginalParameters[i].Name)
                                    throw new InvalidOperationException($"Method {declaringType.Name}.{originalMethod.Name} cannot be automatically patched for subtype {subtype.Name}, because argument #{i} has a mismatched name: `{originalParameters[i].Name}` vs `{subtypeOriginalParameters[i].Name}`.");
                        }

                        self.Patch(subtypeOriginal, prefix, postfix, transpiler, finalizer);
                        logger.Log(successLogLevel, "Patched method {Method}.", subtypeOriginal.FullDescription());
                        patched++;
                    }
                    catch (Exception ex)
                    {
                        logger.Log(problemLogLevel, "Could not patch method - the mod may not work correctly.\nReason: {Exception}", ex);
                    }
                }
            }
            return (patched, total);
        }
        catch (Exception ex)
        {
            logger.Log(problemLogLevel, "Could not patch method {Method} - the mod may not work correctly.\nReason: {Exception}", originalMethod, ex);
            return (0, 1);
        }
    }
}