﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Duracellko.PlanningPoker.Domain {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "17.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class Resources {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Resources() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("Duracellko.PlanningPoker.Domain.Resources", typeof(Resources).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Deserialization of Scrum Team failed..
        /// </summary>
        internal static string Error_DeserializationFailed {
            get {
                return ResourceManager.GetString("Error_DeserializationFailed", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Cannot start estimation. Estimation is already in progress..
        /// </summary>
        internal static string Error_EstimationIsInProgress {
            get {
                return ResourceManager.GetString("Error_EstimationIsInProgress", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Estimation of value {0} is not available in the team..
        /// </summary>
        internal static string Error_EstimationIsNotAvailableInTeam {
            get {
                return ResourceManager.GetString("Error_EstimationIsNotAvailableInTeam", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Estimation result is read-only..
        /// </summary>
        internal static string Error_EstimationResultIsReadOnly {
            get {
                return ResourceManager.GetString("Error_EstimationResultIsReadOnly", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Invalid session ID..
        /// </summary>
        internal static string Error_InvalidSessionId {
            get {
                return ResourceManager.GetString("Error_InvalidSessionId", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Timer duration must be more greater than 0 seconds..
        /// </summary>
        internal static string Error_InvalidTimerDuraction {
            get {
                return ResourceManager.GetString("Error_InvalidTimerDuraction", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Member or observer named &apos;{0}&apos; already exists in the team..
        /// </summary>
        internal static string Error_MemberAlreadyExists {
            get {
                return ResourceManager.GetString("Error_MemberAlreadyExists", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The given member was not present in the estimation result..
        /// </summary>
        internal static string Error_MemberNotInResult {
            get {
                return ResourceManager.GetString("Error_MemberNotInResult", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Scrum Master is already set for the team..
        /// </summary>
        internal static string Error_ScrumMasterAlreadyExists {
            get {
                return ResourceManager.GetString("Error_ScrumMasterAlreadyExists", resourceCulture);
            }
        }
    }
}
