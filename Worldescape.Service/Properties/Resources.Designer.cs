﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Worldescape.Service.Properties {
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
    public class Resources {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Resources() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        public static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("Worldescape.Service.Properties.Resources", typeof(Resources).Assembly);
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
        public static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to [
        ///	{
        ///		&quot;StatusBoundImageUrls&quot;: [
        ///			{
        ///				&quot;Status&quot;: 0,
        ///				&quot;ImageUrl&quot;: &quot;ms-appx:///Images/Avatar_Profiles/Jenna_The_Adventurer/character_femaleAdventurer_idle.png&quot;
        ///			},
        ///			{
        ///				&quot;Status&quot;: 1,
        ///				&quot;ImageUrl&quot;: &quot;ms-appx:///Images/Avatar_Profiles/Jenna_The_Adventurer/character_femaleAdventurer_attackKick.png&quot;
        ///			},
        ///			{
        ///				&quot;Status&quot;: 2,
        ///				&quot;ImageUrl&quot;: &quot;ms-appx:///Images/Avatar_Profiles/Jenna_The_Adventurer/character_femaleAdventurer_drag.png&quot;
        ///			},
        ///			{
        ///				&quot;Status&quot;: 3,
        ///				&quot;ImageUrl&quot;: &quot;ms-app [rest of string was truncated]&quot;;.
        /// </summary>
        public static string CharacterAssets {
            get {
                return ResourceManager.GetString("CharacterAssets", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to [
        ///  {
        ///    &quot;Category&quot;: &quot;Blocks&quot;,
        ///    &quot;SubCategory&quot;: &quot;Blocks\\Abstract&quot;,
        ///    &quot;Name&quot;: &quot;Abstract Tile 0 1 &quot;,
        ///    &quot;ImageUrl&quot;: &quot;World_Objects\\Blocks\\Abstract\\abstractTile_01.png&quot;
        ///  },
        ///  {
        ///    &quot;Category&quot;: &quot;Blocks&quot;,
        ///    &quot;SubCategory&quot;: &quot;Blocks\\Abstract&quot;,
        ///    &quot;Name&quot;: &quot;Abstract Tile 0 2 &quot;,
        ///    &quot;ImageUrl&quot;: &quot;World_Objects\\Blocks\\Abstract\\abstractTile_02.png&quot;
        ///  },
        ///  {
        ///    &quot;Category&quot;: &quot;Blocks&quot;,
        ///    &quot;SubCategory&quot;: &quot;Blocks\\Abstract&quot;,
        ///    &quot;Name&quot;: &quot;Abstract Tile 0 3 &quot;,
        ///    &quot;ImageUrl&quot;: &quot;World_Objects\\ [rest of string was truncated]&quot;;.
        /// </summary>
        public static string ConstructAssets {
            get {
                return ResourceManager.GetString("ConstructAssets", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to http://localhost:5034/worldescapehub.
        /// </summary>
        public static string DevHubService {
            get {
                return ResourceManager.GetString("DevHubService", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to http://localhost:5034.
        /// </summary>
        public static string DevWebService {
            get {
                return ResourceManager.GetString("DevWebService", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to http://192.168.31.186:9899/worldescapehub.
        /// </summary>
        public static string ProdHubService {
            get {
                return ResourceManager.GetString("ProdHubService", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to http://192.168.31.186:9899.
        /// </summary>
        public static string ProdWebService {
            get {
                return ResourceManager.GetString("ProdWebService", resourceCulture);
            }
        }
    }
}
