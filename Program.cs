namespace SampleApp
{
    using Autodesk.Data;
    using Autodesk.Data.DataModels;
    using Autodesk.Data.Enums;
    using System.Configuration;
    using System.Data;
    using Client = Autodesk.Data.AECDM.Interface.IClient;

    /// <summary>
    /// AECDM Sample Application - Demonstrates using Autodesk Data SDK for working with AECDM data.
    ///
    /// This application demonstrates the following workflow:
    /// 1. Initialize the Autodesk Data SDK
    /// 2. Query AECDM Elements (filtered, complete groups)
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

                var elementGroupInfo = await SelectElementGroupViaNavigationAsync(client);
                if (elementGroupInfo == null)
                {
                    Console.WriteLine("No element group selected. Exiting.");
                    return;
                }

                Console.WriteLine($"\nUsing Element Group: {elementGroupInfo.Name} (ID: {elementGroupInfo.Id})");

                Console.Write("Do you want to include Extended Properties? (Yes/No): ");
                var includeExtendedProperties = Console.ReadLine();

                // IFC Conversion examples
                // Filtered Elements from an Element Group
                await ConvertFilteredAECDMElementsToIFCAsync(client, elementGroupInfo, includeExtendedProperties == "Yes");

                // Complete Element Group
                await ConvertCompleteAECDMElementGroupToIFCAsync(client, elementGroupInfo, includeExtendedProperties == "Yes");

                // Mesh Geometry examples
                // Filtered Elements from an Element Group
                await GetMeshGeometriesForFilteredAECDMElementsAsync(client, elementGroupInfo);

                // Complete Element Group
                await GetMeshGeometriesForCompleteAECDMElementGroupAsync(client, elementGroupInfo);

                // Advanced example with options
                await GetMeshGeometriesExampleWithOptions(client, elementGroupInfo);


                Console.WriteLine("\nSample workflows completed successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\nApplication failed with error: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                Environment.Exit(1);
            }
        }

        /// <summary>
        /// Uses the Navigation API to let the user browse Hubs → Projects → ElementGroups
        /// and interactively select an ElementGroup to use in the workflow.
        /// </summary>
        private static async Task<ElementGroupInfo> SelectElementGroupViaNavigationAsync(Client aecdmClient)
        {
            Console.WriteLine("\n========================================");
            Console.WriteLine("AECDM Navigation: Browse Element Groups");
            Console.WriteLine("========================================\n");

            // Step 1: Get all Hubs
            Console.WriteLine("Fetching Hubs...");
            var hubs = await aecdmClient.GetHubsAsync();
            if (hubs.Count == 0)
            {
                Console.WriteLine("No hubs found. Make sure AECDM is enabled on your account.");
                return null;
            }

            Console.WriteLine($"Found {hubs.Count} hub(s):");
            for (int i = 0; i < hubs.Count; i++)
            {
                Console.WriteLine($"  {i + 1}. {hubs[i].Name} (ID: {hubs[i].Id})");
            }

            var selectedHub = hubs[0];
            if (hubs.Count > 1)
            {
                Console.Write($"Select a hub (1-{hubs.Count}) [default: 1]: ");
                var hubInput = Console.ReadLine()?.Trim();
                if (int.TryParse(hubInput, out int hubIndex) && hubIndex >= 1 && hubIndex <= hubs.Count)
                {
                    selectedHub = hubs[hubIndex - 1];
                }
            }
            Console.WriteLine($"→ Using hub: {selectedHub.Name}\n");

            // Step 2: Get Projects in the selected Hub
            Console.WriteLine("Fetching Projects...");
            var projects = await aecdmClient.GetProjectsAsync(selectedHub);
            if (projects.Count == 0)
            {
                Console.WriteLine("No projects found in this hub.");
                return null;
            }

            Console.WriteLine($"Found {projects.Count} project(s):");
            for (int i = 0; i < projects.Count; i++)
            {
                Console.WriteLine($"  {i + 1}. {projects[i].Name} (ID: {projects[i].Id})");
            }

            var selectedProject = projects[0];
            if (projects.Count > 1)
            {
                Console.Write($"Select a project (1-{projects.Count}) [default: 1]: ");
                var projInput = Console.ReadLine()?.Trim();
                if (int.TryParse(projInput, out int projIndex) && projIndex >= 1 && projIndex <= projects.Count)
                {
                    selectedProject = projects[projIndex - 1];
                }
            }
            Console.WriteLine($"→ Using project: {selectedProject.Name}\n");

            // Step 3: Get ElementGroups in the selected Project
            Console.WriteLine("Fetching Element Groups (Revit models)...");
            var elementGroups = await aecdmClient.GetElementGroupsAsync(selectedProject);
            if (elementGroups.Count == 0)
            {
                Console.WriteLine("No element groups found. Make sure Revit 2024+ models were uploaded after AECDM activation.");
                return null;
            }

            Console.WriteLine($"Found {elementGroups.Count} element group(s):");
            for (int i = 0; i < elementGroups.Count; i++)
            {
                Console.WriteLine($"  {i + 1}. {elementGroups[i].Name} (ID: {elementGroups[i].Id})");
            }

            var selectedElementGroup = elementGroups[0];
            if (elementGroups.Count > 1)
            {
                Console.Write($"Select an element group (1-{elementGroups.Count}) [default: 1]: ");
                var egInput = Console.ReadLine()?.Trim();
                if (int.TryParse(egInput, out int egIndex) && egIndex >= 1 && egIndex <= elementGroups.Count)
                {
                    selectedElementGroup = elementGroups[egIndex - 1];
                }
            }
            Console.WriteLine($"→ Selected: {selectedElementGroup.Name} (ID: {selectedElementGroup.Id})");

            return selectedElementGroup;
        }

        #region IFC Conversion Examples
        /// <summary>
        /// IFC Conversion Example: Shows how to provide a region when creating an ElementGroup.
        /// The region can be US (default), EMEA, or AUS.
        /// </summary>
        private static async Task ConvertFilteredAECDMElementsToIFCAsync(Client client, ElementGroupInfo elementGroupInfo, bool includeExtendedProperties)
        {
            var elementGroup = new ElementGroup(client, Region.US); // Used to group and process AECDM elements

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
            await elementGroup.GetElementsAsync(elementGroupInfo, filter, includeExtendedProperties);

            // Export ElementGroup to IFC format
            Console.WriteLine("Converting elements to IFC format...");
            var ifcFilePath = await elementGroup.ConvertToIfc("IFCFileName");
            Console.WriteLine($"IFC file created at: {ifcFilePath}");

        }

        private static async Task ConvertCompleteAECDMElementGroupToIFCAsync(Client client, ElementGroupInfo elementGroupInfo, bool includeExtendedProperties)
        {
            var elementGroup = new ElementGroup(client, Region.US); // Used to group and process AECDM elements

            Console.WriteLine("Fetching all AECDM elements from a sample element group...");

            // Populate ElementGroup with all elements
            await elementGroup.GetElementsAsync(elementGroupInfo, includeExtendedProperties: includeExtendedProperties);

            // Export ElementGroup to IFC format
            Console.WriteLine("Converting elements to IFC format...");
            var ifcFilePath = await elementGroup.ConvertToIfc("IFCFileName");
            Console.WriteLine($"IFC file created at: {ifcFilePath}");
        }

        #endregion

        #region Mesh Geometry Examples
        private static async Task GetMeshGeometriesForFilteredAECDMElementsAsync(Client client, ElementGroupInfo elementGroupInfo)
        {
            var elementGroup = new ElementGroup(client, Region.US); // Used to group and process AECDM elements

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
            await elementGroup.GetElementsAsync(elementGroupInfo, filter);

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

        private static async Task GetMeshGeometriesForCompleteAECDMElementGroupAsync(Client client, ElementGroupInfo elementGroupInfo)
        {
            var elementGroup = new ElementGroup(client, Region.US); // Used to group and process AECDM elements

            Console.WriteLine("Fetching all AECDM elements from a sample element group...");

            // Populate ElementGroup with all elements
            await elementGroup.GetElementsAsync(elementGroupInfo);

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

        private static async Task GetMeshGeometriesExampleWithOptions(Client client, ElementGroupInfo elementGroupInfo)
        {
            var elementGroup = new ElementGroup(client, Region.US); // Used to group and process AECDM elements

            Console.WriteLine("Fetching all AECDM elements from a sample element group...");

            // Populate ElementGroup with all elements and capture the returned elements
            var elements = await elementGroup.GetElementsAsync(elementGroupInfo);

            // Retrieve and process mesh geometry information for specific elements in the group
            // Specify Mesh Conversion options for Brep to Mesh conversion
            var wallElements = elements.Where(e => e.Category == "Walls");
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
            var clientFactory = new DataSdkClientFactory();
            Client aecdmClient = clientFactory.CreateAecdmClient(sdkOptions);
            return aecdmClient;
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

            var clientFactory = new DataSdkClientFactory();
            Client aecdmClient = clientFactory.CreateAecdmClient(sdkOptions);
            return aecdmClient;
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
