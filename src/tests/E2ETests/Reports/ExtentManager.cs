using AventStack.ExtentReports;
using AventStack.ExtentReports.Reporter;
using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace E2ETests.Reporting
{
    /// <summary>
    /// ExtentManager: initializes an ExtentReports instance and attaches an HTML reporter.
    /// This implementation uses reflection to locate a compatible HTML reporter type inside
    /// the installed ExtentReports package so it is resilient to minor API differences
    /// between package versions.
    /// </summary>
    public static class ExtentManager
    {
        private static ExtentReports? _instance;
        private static readonly object _sync = new();

        public static ExtentReports Instance
        {
            get
            {
                if (_instance == null)
                    throw new InvalidOperationException("ExtentManager is not initialized. Call Init() first.");
                return _instance;
            }
        }

        /// <summary>
        /// Initialize ExtentReports and attach a discovered HTML reporter.
        /// Safe to call once per test run.
        /// </summary>
        /// <param name="reportsFolder">Optional explicit reports folder; defaults to {projectRoot}/Reports</param>
        public static void Init(string? reportsFolder = null)
        {
            lock (_sync)
            {
                if (_instance != null) return;

                // Determine project root and reports folder
                var projectRoot = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", ".."));
                var reportsDir = reportsFolder ?? Path.Combine(projectRoot, "Reports");
                Directory.CreateDirectory(reportsDir);

                var reportFile = Path.Combine(reportsDir, $"ExtentReport_{DateTime.Now:yyyyMMdd_HHmmss}.html");

                // Attempt to create a compatible HTML reporter via reflection (resilient approach)
                object? htmlReporter = CreateHtmlReporterInstance(reportFile);

                if (htmlReporter == null)
                {
                    throw new InvalidOperationException(
                        "Could not find a compatible HTML reporter type in the AventStack.ExtentReports assembly. " +
                        "Ensure the installed ExtentReports package contains an HTML reporter class (e.g., ExtentHtmlReporter or ExtentV3HtmlReporter)." 
                    );
                }

                // Create ExtentReports instance and attach reporter using reflection-safe invocation
                _instance = new ExtentReports();
                AttachReporterSafely(_instance, htmlReporter);

                // Add environment/system info
                _instance.AddSystemInfo("Machine", Environment.MachineName);
                _instance.AddSystemInfo("OS", Environment.OSVersion.ToString());
                _instance.AddSystemInfo("User", Environment.UserName);
                _instance.AddSystemInfo("Generated", DateTime.UtcNow.ToString("u"));
            }
        }

        /// <summary>
        /// Create ExtentTest for a given test name.
        /// </summary>
        public static ExtentTest CreateTest(string testName)
        {
            return Instance.CreateTest(testName);
        }

        /// <summary>
        /// Flush the report to disk.
        /// </summary>
        public static void Flush()
        {
            _instance?.Flush();
        }

        // -------------------------
        // Internal helpers
        // -------------------------

        private static object? CreateHtmlReporterInstance(string reportFile)
        {
            // Common candidate type names used across ExtentReports variants
            var candidateTypeNames = new[]
            {
                "AventStack.ExtentReports.Reporter.ExtentHtmlReporter",
                "AventStack.ExtentReports.Reporter.ExtentV3HtmlReporter",
                "AventStack.ExtentReports.Reporter.ExtentKlovReporter"
            };

            // Try Type.GetType with assembly-qualified name first
            foreach (var typeName in candidateTypeNames)
            {
                var t = Type.GetType(typeName + ", AventStack.ExtentReports", throwOnError: false);
                if (t != null)
                {
                    var ctor = t.GetConstructor(new[] { typeof(string) });
                    if (ctor != null)
                    {
                        return ctor.Invoke(new object[] { reportFile });
                    }
                }
            }

            // If that didn't work, search loaded assemblies for a reporter type with a string ctor
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var asm in assemblies)
            {
                try
                {
                    var types = asm.GetTypes()
                        .Where(x => x.Namespace != null && x.Namespace.Contains("Reporter", StringComparison.OrdinalIgnoreCase));

                    foreach (var type in types)
                    {
                        var ctor = type.GetConstructor(new[] { typeof(string) });
                        if (ctor != null)
                        {
                            try
                            {
                                return ctor.Invoke(new object[] { reportFile });
                            }
                            catch
                            {
                                // if construction failed for some reason, continue searching
                            }
                        }
                    }
                }
                catch (ReflectionTypeLoadException)
                {
                    // Ignore assemblies we cannot reflect over
                }
            }

            // Nothing found
            return null;
        }

        private static void AttachReporterSafely(ExtentReports extent, object reporter)
        {
            // Try a strongly-typed AttachReporter overload if present
            var reporterType = reporter.GetType();
            var attachMethod = typeof(ExtentReports).GetMethod("AttachReporter", new[] { reporterType });
            if (attachMethod != null)
            {
                attachMethod.Invoke(extent, new[] { reporter });
                return;
            }

            // Try object overload
            attachMethod = typeof(ExtentReports).GetMethod("AttachReporter", new[] { typeof(object) });
            if (attachMethod != null)
            {
                attachMethod.Invoke(extent, new object[] { reporter });
                return;
            }

            // Fallback: try dynamic invocation (works in most builds/runtimes)
            try
            {
                // Using dynamic here gives a late-bound call that most ExtentReports builds accept
                dynamic dExtent = extent;
                dExtent.AttachReporter((dynamic)reporter);
                return;
            }
            catch
            {
                // last resort: attempt to find any AttachReporter method and call it
                var allAttach = typeof(ExtentReports).GetMethods().FirstOrDefault(m => m.Name == "AttachReporter");
                if (allAttach != null)
                {
                    // If it accepts params, pass as single-element array
                    var ps = allAttach.GetParameters();
                    if (ps.Length == 1 && ps[0].ParameterType.IsArray)
                    {
                        var elementType = ps[0].ParameterType.GetElementType() ?? typeof(object);
                        var arr = Array.CreateInstance(elementType, 1);
                        arr.SetValue(reporter, 0);
                        allAttach.Invoke(extent, new object[] { arr });
                        return;
                    }

                    // Otherwise, try calling with the reporter as object
                    allAttach.Invoke(extent, new object[] { reporter });
                    return;
                }
            }

            // If we reached here, attaching failed
            throw new InvalidOperationException("Unable to attach reporter to ExtentReports instance via any available mechanism.");
        }
        // Add these public helpers to the ExtentManager class (near the other helpers)

/// <summary>
/// Public wrapper so other code can create an HTML reporter using the same reflection logic.
/// Returns the reporter object (or null if none found).
/// </summary>
public static object? CreateHtmlReporter(string reportFile)
{
    // re-use the existing private helper (CreateHtmlReporterInstance)
    // If your private helper is named CreateHtmlReporterInstance, call it here.
    // If not, copy the reflection logic here.
    return CreateHtmlReporterInstance(reportFile);
}

/// <summary>
/// Public wrapper to attach a reporter to an ExtentReports instance using the robust logic.
/// </summary>
public static void AttachReporterTo(ExtentReports extent, object reporter)
{
    AttachReporterSafely(extent, reporter);
}

    }
    
}
