namespace SampleApp
{
    using Autodesk.Data;
    using Autodesk.Data.DataModels;
    using System.Configuration;

    /// <summary>
    /// AECDM Sample Application - Demonstrates using Autodesk Data SDK for working with AECDM data.
    ///
    /// This application demonstrates the following workflow:
    /// 1. Initialize the Autodesk Data SDK
    /// 2. Query AECDM Elements (single, multiple, filtered, complete groups)
    /// 3. Export processed data to IFC format (Download and processing of geometry data is handled by the SDK)
    /// 4. Get Element geometries as granular mesh data
    ///
    /// Prerequisites:
    /// - Autodesk developer credentials (Client ID, Client Secret)
    /// - Configured App.config file
    /// - Network access to Autodesk APIs
    /// </summary>
    internal class Program
    {
        /// <summary>
        /// Main entry point of the application. Demonstrates sample workflows for AECDM data processing using Autodesk Data SDK.
        /// </summary>
        static async Task Main(string[] args)
        {
            try
            {
                // Initialize the Autodesk Data SDK and required services
                var client = SetupAutodeskDataSDK();

                // Alternatively, use PKCE Authentication setup if preferred
                // var client = SetupAutodeskDataSDKWithPKCEAuth();

                // IFC Conversion examples
                // Single Element
                await ConvertSingleAECDMElementToIFCAsync(client, "Your_GraphQLElementId");

                // Multiple Elements
                await ConvertMultipleAECDMElementsToIFCAsync(client, new List<string> { "Your_GraphQLElementId1", "Your_GraphQLElementId2" });

                // Filtered Elements from an Element Group
                await ConvertFilteredAECDMElementsToIFCAsync(client, "Your_GraphQLElementGroupId");

                // Complete Element Group
                await ConvertCompleteAECDMElementGroupToIFCAsync(client, "Your_GraphQLElementGroupId");


                // Mesh Geometry examples
                // Single Element
                await GetMeshGeometryForSingleAECDMElementAsync(client, "Your_GraphQLElementId");

                // Multiple Elements
                await GetMeshGeometriesForMultipleAECDMElementsAsync(client, new List<string> { "Your_GraphQLElementId1", "Your_GraphQLElementId2" });

                // Filtered Elements from an Element Group
                await GetMeshGeometriesForFilteredAECDMElementsAsync(client, "Your_GraphQLElementGroupId");

                // Complete Element Group
                await GetMeshGeometriesForCompleteAECDMElementGroupAsync(client, "Your_GraphQLElementGroupId");

                // Advanced example with options
                await GetMeshGeometriesExampleWithOptions(client, "Your_GraphQLElementGroupId");


                Console.WriteLine("\nSample workflows completed successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\nApplication failed with error: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                Environment.Exit(1);
            }
        }

        #region IFC Conversion Examples
        /// <summary>
        /// IFC Conversion Example: Shows how to provide a region when creating an ElementGroup.
        /// The region can be US (default), EMEA, or AUS.
        /// </summary>
        private static async Task ConvertSingleAECDMElementToIFCAsync(Client client, string GraphQLElementId)
        {
            // You can specify the region for the element group: US (default), EMEA, or AUS
            // Example: ElementGroup.Create(client, ElementGroup.Region.EMEA) or ElementGroup.Region.AUS
            var elementGroup = ElementGroup.Create(client, ElementGroup.Region.US); // Used to group and process AECDM elements

            Console.WriteLine($"Fetching single AECDM element: {GraphQLElementId}");

            // Populate ElementGroup with single element by Id
            await elementGroup.GetElementAsync(GraphQLElementId);

            // Export ElementGroup to IFC format
            Console.WriteLine("Converting elements to IFC format...");
            var ifcFilePath = await elementGroup.ConvertToIfc("IFCFileName");
            Console.WriteLine($"IFC file created at: {ifcFilePath}");
        }

        private static async Task ConvertMultipleAECDMElementsToIFCAsync(Client client, List<string> GraphQLElementIds)
        {
            var elementGroup = ElementGroup.Create(client); // Used to group and process AECDM elements

            Console.WriteLine($"Fetching multiple AECDM elements by Ids");

            // Populate ElementGroup with multiple elements by collection of Ids
            await elementGroup.GetElementsAsync(GraphQLElementIds);

            // Export ElementGroup to IFC format
            Console.WriteLine("Converting elements to IFC format...");
            var ifcFilePath = await elementGroup.ConvertToIfc("IFCFileName");
            Console.WriteLine($"IFC file created at: {ifcFilePath}");
        }

        private static async Task ConvertFilteredAECDMElementsToIFCAsync(Client client, string GraphQLElementGroupId)
        {
            var elementGroup = ElementGroup.Create(client); // Used to group and process AECDM elements

            // Build the filter equivalent to:
            // (property.name.category == Doors and 'property.name.Element Context'==Instance) or ... for Walls, Windows, Roofs
            // This filter selects elements where:
            // (category == "Doors" AND Element Context == "Instance")
            //  OR (category == "Walls" AND Element Context == "Instance")
            //  OR (category == "Windows" AND Element Context == "Instance")
            //  OR (category == "Roofs" AND Element Context == "Instance")
            var filter = ElementPropertyFilter.AnyOf(
                ElementPropertyFilter.AllOf(
                    ElementPropertyFilter.Property("category", "==", "Doors"),
                    ElementPropertyFilter.Property("Element Context", "==", "Instance")
                ),
                ElementPropertyFilter.AllOf(
                    ElementPropertyFilter.Property("category", "==", "Walls"),
                    ElementPropertyFilter.Property("Element Context", "==", "Instance")
                ),
                ElementPropertyFilter.AllOf(
                    ElementPropertyFilter.Property("category", "==", "Windows"),
                    ElementPropertyFilter.Property("Element Context", "==", "Instance")
                ),
                ElementPropertyFilter.AllOf(
                    ElementPropertyFilter.Property("category", "==", "Roofs"),
                    ElementPropertyFilter.Property("Element Context", "==", "Instance")
                )
            );

            Console.WriteLine("Fetching filtered AECDM elements from a sample element group...");

            // Populate ElementGroup with filtered elements
            await elementGroup.GetElementsAsync(GraphQLElementGroupId, filter);

            // Export ElementGroup to IFC format
            Console.WriteLine("Converting elements to IFC format...");
            var ifcFilePath = await elementGroup.ConvertToIfc("IFCFileName");
            Console.WriteLine($"IFC file created at: {ifcFilePath}");

        }

        private static async Task ConvertCompleteAECDMElementGroupToIFCAsync(Client client, string GraphQLElementGroupId)
        {
            var elementGroup = ElementGroup.Create(client); // Used to group and process AECDM elements

            Console.WriteLine("Fetching all AECDM elements from a sample element group...");

            // Populate ElementGroup with all elements
            await elementGroup.GetElementsAsync(GraphQLElementGroupId);

            // Export ElementGroup to IFC format
            Console.WriteLine("Converting elements to IFC format...");
            var ifcFilePath = await elementGroup.ConvertToIfc("IFCFileName");
            Console.WriteLine($"IFC file created at: {ifcFilePath}");
        }

        #endregion

        #region Mesh Geometry Examples
        private static async Task GetMeshGeometryForSingleAECDMElementAsync(Client client, string GraphQLElementId)
        {
            var elementGroup = ElementGroup.Create(client); // Used to group and process AECDM elements

            Console.WriteLine($"Fetching single AECDM element: {GraphQLElementId}");

            // Populate ElementGroup with single element by Id
            await elementGroup.GetElementAsync(GraphQLElementId);

            // Retrieve and process mesh geometry information for each element in the group
            var elementGeometryMap = await elementGroup.GetElementGeometriesAsMeshAsync().ConfigureAwait(false);
            foreach (var kv in elementGeometryMap)
            {
                var element = kv.Key;
                var meshList = kv.Value;
                if (meshList.Count() > 0)
                {
                    Console.WriteLine($"Element ID: {element.Id} has {meshList.Count()} mesh geometries.");
                    foreach (var meshObj in meshList)
                    {
                        if (meshObj is MeshGeometry meshGeometry)
                        {
                            Console.WriteLine($"  Mesh Vertices Count: {meshGeometry.Mesh?.Vertices.Count}");
                        }
                    }
                }
            }
        }

        private static async Task GetMeshGeometriesForMultipleAECDMElementsAsync(Client client, List<string> GraphQLElementIds)
        {
            var elementGroup = ElementGroup.Create(client); // Used to group and process AECDM elements

            Console.WriteLine($"Fetching multiple AECDM elements by Ids");

            // Populate ElementGroup with multiple elements by collection of Ids
            await elementGroup.GetElementsAsync(GraphQLElementIds);

            // Retrieve and process mesh geometry information for each element in the group
            var elementGeometryMap = await elementGroup.GetElementGeometriesAsMeshAsync().ConfigureAwait(false);
            foreach (var kv in elementGeometryMap)
            {
                var element = kv.Key;
                var meshList = kv.Value;
                if (meshList.Count() > 0)
                {
                    Console.WriteLine($"Element ID: {element.Id} has {meshList.Count()} mesh geometries.");
                    foreach (var meshObj in meshList)
                    {
                        if (meshObj is MeshGeometry meshGeometry)
                        {
                            Console.WriteLine($"  Mesh Vertices Count: {meshGeometry.Mesh?.Vertices.Count}");
                        }
                    }
                }
            }
        }

        private static async Task GetMeshGeometriesForFilteredAECDMElementsAsync(Client client, string GraphQLElementGroupId)
        {
            var elementGroup = ElementGroup.Create(client); // Used to group and process AECDM elements

            // Build the filter equivalent to:
            // (property.name.category == Doors and 'property.name.Element Context'==Instance) or ... for Walls, Windows, Roofs
            // This filter selects elements where:
            // (category == "Doors" AND Element Context == "Instance")
            //  OR (category == "Walls" AND Element Context == "Instance")
            //  OR (category == "Windows" AND Element Context == "Instance")
            //  OR (category == "Roofs" AND Element Context == "Instance")
            var filter = ElementPropertyFilter.AnyOf(
                ElementPropertyFilter.AllOf(
                    ElementPropertyFilter.Property("category", "==", "Doors"),
                    ElementPropertyFilter.Property("Element Context", "==", "Instance")
                ),
                ElementPropertyFilter.AllOf(
                    ElementPropertyFilter.Property("category", "==", "Walls"),
                    ElementPropertyFilter.Property("Element Context", "==", "Instance")
                ),
                ElementPropertyFilter.AllOf(
                    ElementPropertyFilter.Property("category", "==", "Windows"),
                    ElementPropertyFilter.Property("Element Context", "==", "Instance")
                ),
                ElementPropertyFilter.AllOf(
                    ElementPropertyFilter.Property("category", "==", "Roofs"),
                    ElementPropertyFilter.Property("Element Context", "==", "Instance")
                )
            );

            Console.WriteLine("Fetching filtered AECDM elements from a sample element group...");

            // Populate ElementGroup with filtered elements
            await elementGroup.GetElementsAsync(GraphQLElementGroupId, filter);

            // Retrieve and process mesh geometry information for each element in the group
            var elementGeometryMap = await elementGroup.GetElementGeometriesAsMeshAsync().ConfigureAwait(false);
            foreach (var kv in elementGeometryMap)
            {
                var element = kv.Key;
                var meshList = kv.Value;
                if (meshList.Count() > 0)
                {
                    Console.WriteLine($"Element ID: {element.Id} has {meshList.Count()} mesh geometries.");
                    foreach (var meshObj in meshList)
                    {
                        if (meshObj is MeshGeometry meshGeometry)
                        {
                            Console.WriteLine($"  Mesh Vertices Count: {meshGeometry.Mesh?.Vertices.Count}");
                        }
                    }
                }
            }

        }

        private static async Task GetMeshGeometriesForCompleteAECDMElementGroupAsync(Client client, string GraphQLElementGroupId)
        {
            var elementGroup = ElementGroup.Create(client); // Used to group and process AECDM elements

            Console.WriteLine("Fetching all AECDM elements from a sample element group...");

            // Populate ElementGroup with all elements
            await elementGroup.GetElementsAsync(GraphQLElementGroupId);

            // Retrieve and process mesh geometry information for each element in the group
            var elementGeometryMap = await elementGroup.GetElementGeometriesAsMeshAsync().ConfigureAwait(false);
            foreach (var kv in elementGeometryMap)
            {
                var element = kv.Key;
                var meshList = kv.Value;
                if (meshList.Count() > 0)
                {
                    Console.WriteLine($"Element ID: {element.Id} has {meshList.Count()} mesh geometries.");
                    foreach (var meshObj in meshList)
                    {
                        if (meshObj is MeshGeometry meshGeometry)
                        {
                            Console.WriteLine($"  Mesh Vertices Count: {meshGeometry.Mesh?.Vertices.Count}");
                        }
                    }
                }
            }
        }

        private static async Task GetMeshGeometriesExampleWithOptions(Client client, string GraphQLElementGroupId)
        {
            var elementGroup = ElementGroup.Create(client); // Used to group and process AECDM elements

            Console.WriteLine("Fetching all AECDM elements from a sample element group...");

            // Populate ElementGroup with all elements
            await elementGroup.GetElementsAsync(GraphQLElementGroupId);

            // Retrieve and process mesh geometry information for specific elements in the group
            // Specify Mesh Conversion options for Brep to Mesh conversion
            var wallElements = elementGroup.Elements.Where(e => e.Category == "Walls");
            var elementGeometryMap = await elementGroup.GetElementGeometriesAsMeshAsync(wallElements, new Autodesk.Data.Geometry.BRepToMeshOptions() 
                                                                                        {
                                                                                            SurfaceTolerance = 1.0,
                                                                                            NormalTolerance = 15,
                                                                                            MaxEdgeLength = 2.0,
                                                                                            GridAspectRatio = 0.1,
                                                                                        }).ConfigureAwait(false);
            foreach (var kv in elementGeometryMap)
            {
                var element = kv.Key;
                var meshList = kv.Value;
                if (meshList.Count() > 0)
                {
                    Console.WriteLine($"Element ID: {element.Id} has {meshList.Count()} mesh geometries.");
                    foreach (var meshObj in meshList)
                    {
                        if (meshObj is MeshGeometry meshGeometry)
                        {
                            Console.WriteLine($"  Mesh Vertices Count: {meshGeometry.Mesh?.Vertices.Count}");
                        }
                    }
                }
            }
        }
        #endregion


        /// <summary>
        /// Configures and initializes the Autodesk Data SDK using values from App.config.
        /// </summary>
        /// <returns>Returns a configured SDK Client instance.</returns>
        private static Client SetupAutodeskDataSDK()
        {
            // Read configuration from App.config
            var authClientID = GetRequiredConfigValue("AuthClientID");
            var authClientSecret = GetRequiredConfigValue("AuthClientSecret");
            var authCallBack = GetRequiredConfigValue("AuthCallBack");
            var applicationName = ConfigurationManager.AppSettings["ApplicationName"] ?? "AECDMSampleApp";
            var logLevel = ConfigurationManager.AppSettings["LogLevel"] ?? "Info";

            // Determine application data path
            var appBasePath = ConfigurationManager.AppSettings["ApplicationDataPath"];
            if (string.IsNullOrEmpty(appBasePath))
            {
                appBasePath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            }

            Console.WriteLine($"Application: {applicationName}");
            Console.WriteLine($"Data Path: {appBasePath}");
            Console.WriteLine($"Log Level: {logLevel}");

            // Configure SDK options
            var sdkOptions = new SDKOptionsDefaultSetup
            {
                ConnectorName = applicationName,
                ClientId = authClientID,
                ClientSecret = authClientSecret,
                CallBack = authCallBack,
                HostApplicationName = applicationName,
                HostApplicationVersion = "1.0.0",
                ConnectorVersion = "1.0.0",
            };

            return new Client(sdkOptions);
        }

        /// <summary>
        /// Demonstrates initializing the Autodesk Data SDK using credentials for PKCE Authentiication.
        /// </summary>
        /// <returns>Returns a configured SDK Client instance.</returns>
        private static Client SetupAutodeskDataSDKWithPKCEAuth()
        {
            // Read configuration from App.config
            var authClientID = GetRequiredConfigValue("AuthClientID"); //PKCE Client ID
            var authCallBack = GetRequiredConfigValue("AuthCallBack"); //PKCE Callback URL.No Client Secret needed for PKCE
            var applicationName = ConfigurationManager.AppSettings["ApplicationName"] ?? "AECDMSampleApp";
            var logLevel = ConfigurationManager.AppSettings["LogLevel"] ?? "Info";

            //// Determine application data path
            var appBasePath = ConfigurationManager.AppSettings["ApplicationDataPath"];
            if (string.IsNullOrEmpty(appBasePath))
            {
                appBasePath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            }

            Console.WriteLine($"Application: {applicationName}");
            Console.WriteLine($"Data Path: {appBasePath}");
            Console.WriteLine($"Log Level: {logLevel}");

            // Configure SDK options
            var sdkOptions = new SDKOptionsDefaultSetup
            {
                ConnectorName = applicationName,
                ClientId = authClientID,
                CallBack = authCallBack,
                HostApplicationName = applicationName,
                HostApplicationVersion = "1.0.0",
                ConnectorVersion = "1.0.0",
            };

            return new Client(sdkOptions);
        }

        /// <summary>
        /// Retrieves a required configuration value from App.config. Throws an exception if the value is missing or empty.
        /// </summary>
        /// <param name="key">The configuration key to retrieve.</param>
        /// <returns>The configuration value associated with the specified key.</returns>
        private static string GetRequiredConfigValue(string key)
        {
            var value = ConfigurationManager.AppSettings[key];
            if (string.IsNullOrEmpty(value))
            {
                throw new InvalidOperationException(
                    $"Required configuration value '{key}' is missing or empty in App.config. " +
                    "Please check the README for setup instructions.");
            }
            return value;
        }
    }
}
