# aps-aecdm-data-sdk-code-sample (**beta**)

[![OAuth2](https://img.shields.io/badge/OAuth2-v2-green.svg)](http://developer.autodesk.com/)
[![AEC-DM-Geometry](https://img.shields.io/badge/AEC%20DM%20Geometry-beta-blue.svg)](http://developer.autodesk.com/)
![Data SDK Version](https://img.shields.io/badge/Data%20SDK-0.2.2--beta-blue.svg)


![.NET](https://img.shields.io/badge/NET-8.0-green.svg)
![Platforms](https://img.shields.io/badge/Web-Windows%20%7C%20MacOS%20%7C%20Linux-lightgray.svg)


![Advanced](https://img.shields.io/badge/Level-Advanced-red.svg)
[![MIT](https://img.shields.io/badge/License-MIT-blue.svg)](http://opensource.org/licenses/MIT)

# Disclaimer
Please note that AEC DM Geometry API and DataSDK is currently in beta testing, API calls are subject to changes based on the feedback received. We recommend avoiding using this API to build production software during this phase. If you have an urgent requirement, please contact our team before utilizing the API.

You are required to participate in the [AEC Data Model Public Beta Program](https://feedback.autodesk.com/), follow the instructions, download the DataSDK, and provide your feedback there.

# Description
A comprehensive sample application demonstrating how to integrate Autodesk's Data SDK (`Autodesk.Data 0.2.2-beta`) with AECDM (Architecture, Engineering, Construction, and Design Manufacturing) data. This application shows how to navigate hubs and projects, query AECDM elements, process geometry, and export to IFC format.

## 🎯 What This Application Does

This sample demonstrates the complete workflow for working with AECDM data:

1. **Authentication** - Secure connection to Autodesk APIs using OAuth 2.0
2. **Navigation** - Interactively browse Hubs → Projects → Element Groups using the Navigation API
3. **Data Querying** - Retrieve elements from an Element Group using filters or fetch all elements at once
4. **SDK Integration** - Add elements to the Data SDK's `ElementGroup` abstraction for processing
5. **Export** - Convert all elements to IFC format (geometry processing is handled by the SDK)
6. **Geometry Display** - Retrieve and display mesh geometry information for each element, with optional BRep-to-mesh conversion options

## 📋 Prerequisites

### Software Requirements
- **Visual Studio 2022** or **Visual Studio Code** with C# extension
- **.NET Framework 4.8** or **.NET 8.0 SDK**
- **Internet connection** for API access

### NuGet Package
`Autodesk.Data 0.2.2-beta` is a **private beta package** — You can download it from the [AEC Data Model Public Beta Program portal](https://feedback.autodesk.com/project/version/item.html?cap=9635c95e635b453393da82849304c1fc&arttypeid={56534ead-d6be-454e-b67c-2013caa4b8e0}&artid={438E8347-46CC-4912-9497-7D288E47B171}) and add it as a local NuGet source before running `dotnet restore`.

### Autodesk Developer Account Setup
1. **Create Autodesk Developer Account**
   - Go to [Autodesk Developer Portal](https://aps.autodesk.com/)
   - Sign up for a free developer account

2. **Create an Application**
   - Navigate to "My Apps" in the developer portal
   - Click "Create App"
   - Fill in application details:
     - **App Name**: Your application name
     - **App Description**: Brief description
     - **Callback URL**: `http://localhost:65314/api/auth/callback/` (for testing)
   - Select APIs: Check "AEC Data Model API"
   - Save the application
   - Provide access to your APS App client ID in your Forma Hub by following the steps mentioned in the [Provisioning access in other products](https://get-started.aps.autodesk.com/#provision-access-in-other-products) page.

3. **Get Your Credentials**
   - **Client ID**: Copy from your app's details page
   - **Client Secret**: Copy from your app's details page (keep this secure!)

## 🚀 Quick Start Guide

> **Region:** When the application starts it will ask you to pick a region — **US**, **EMEA**, or **AUS**. Make sure you know which region your Autodesk Hub is hosted in before running. See [Region Selection](#-region-selection) for details.

### Step 1: Clone and Setup

```bash
# Clone the repository
git clone <your-repository-url>
cd aps-aecdm-data-sdk-code-sample

# Restore NuGet packages
dotnet restore SampleApp.csproj

# Create configuration file
cp App.config.template App.config
```

### Step 2: Configure Authentication
1. Copy `App.config.template` to `App.config` in the project root
2. Fill in your Autodesk credentials in the new `App.config` file:

```xml
<?xml version="1.0" encoding="utf-8" ?>
<configuration>
    <appSettings>
        <!-- Required: Your Autodesk app credentials -->
        <add key="AuthClientID" value="YOUR_CLIENT_ID_HERE" />
        <add key="AuthClientSecret" value="YOUR_CLIENT_SECRET_HERE" /> <!-- Ignore this key for PKCE app -->
        <!-- Required: OAuth callback URL -->
        <add key="AuthCallBack" value="http://localhost:8080/api/auth/callback" />
        <!-- Optional: Application settings -->
        <add key="ApplicationName" value="AECDMSampleApp" />
        <add key="ApplicationDataPath" value="" />
        <add key="LogLevel" value="Info" />
    </appSettings>
</configuration>
```

### Step 3: Build and Run

```bash
# Build the application
dotnet build

# Run the application
dotnet run
```

### Step 4: Navigate to Your Element Group

When the application starts, it first asks you to select a region, then uses the **Navigation API** to let you interactively browse your data:

```
Select a region:
  1. US (default)
  2. EMEA
  3. AUS
Enter choice (1-3) [default: 1]: 1

========================================
AECDM Navigation: Browse Element Groups
========================================

Fetching Hubs...
Found 2 hub(s):
  1. My Company Hub (ID: a.YnVzaW5lc3M6...)
  2. Personal Hub (ID: a.cGVyc29uYWw6...)
Select a hub (1-2) [default: 1]: 1
→ Using hub: My Company Hub

Fetching Projects...
Found 3 project(s):
  1. Office Building (ID: b.project1...)
  2. Residential Tower (ID: b.project2...)
  ...
Select a project (1-3) [default: 1]: 1

Fetching Element Groups (Revit models)...
Found 1 element group(s):
  1. Building Model v3 (ID: urn:adsk.dtm:...)
→ Selected: Building Model v3

Do you want to include Extended Properties? (Yes/No): No
```

No hardcoded IDs are needed — the app discovers your available data at runtime. The selected region is applied consistently to all Navigation API calls and all `ElementGroup` operations throughout the session.

## 🌍 Region Selection

At startup the application prompts you to choose a region:

```
Select a region:
  1. US (default)
  2. EMEA
  3. AUS
Enter choice (1-3) [default: 1]:
```

The chosen region is used consistently for:
- All three Navigation API calls (`GetHubsAsync`, `GetProjectsAsync`, `GetElementGroupsAsync`)
- Every `ElementGroup` instantiation (`new ElementGroup(client, region)`)

Supported values from `Autodesk.Data.Enums.Region`:

| Choice | Region | Notes |
|--------|--------|-------|
| 1 | `Region.US` | Default — routes to US |
| 2 | `Region.EMEA` | Europe, Middle East, Africa |
| 3 | `Region.AUS` | Australia |

Entering nothing or an invalid value defaults to US. Specify the region that matches where your Hub and data are hosted.

## 📁 Project Structure

```
aps-aecdm-data-sdk-code-sample/
├── Program.cs              # Main application entry point with workflow orchestration
├── App.config              # Configuration file for credentials and settings (gitignored)
├── App.config.template     # Template configuration file for easy setup
├── SampleApp.csproj        # Project file with dependencies
├── README.md               # This documentation
└── GeometryFiles/          # Created at runtime for downloaded geometry files
```

## 🔧 Configuration Options

### App.config Settings
| Setting               | Description                                                                 | Example                         |
|-----------------------|-----------------------------------------------------------------------------|---------------------------------|
| `AuthClientID`        | Your Autodesk app's Client ID                                               | `abc123def456...`               |
| `AuthClientSecret`    | Your Autodesk app's Client Secret *(Not required for PKCE Auth)*            | `xyz789uvw012...`               |
| `AuthCallBack`        | OAuth callback URL                                                          | `http://localhost:8080/api/auth/callback` |
| `ApplicationName`     | Custom application name                                                     | `MyAECDMApp`                    |
| `ApplicationDataPath` | Custom data directory                                                       | `C:\MyApp\Data`               |
| `LogLevel`            | Logging verbosity                                                           | `Info`, `Debug`, `Error`        |

## 🔍 Understanding the Code

### Main Workflow (`Program.cs`)

The application follows this high-level flow:

```csharp
// 1. Create the SDK client
// Autodesk.Data.AECDM.Client is the main entry point for all SDK operations.
// It is created via DataSdkClientFactory using credentials from App.config.
var clientFactory = new DataSdkClientFactory();
Autodesk.Data.AECDM.Client aecdmClient = clientFactory.CreateAecdmClient(sdkOptions);

// 2. Prompt the user to select a region (US / EMEA / AUS)
// The chosen region is passed into every Navigation API call and ElementGroup constructor.
var region = PromptForRegion();

// 3. Navigate to an element group interactively (Hubs → Projects → Element Groups)
// Returns an ElementGroupInfo object that identifies the selected model.
var elementGroupInfo = await SelectElementGroupViaNavigationAsync(aecdmClient, region);

// 4. Optionally include extended properties in element data
// false (default): only standard Revit properties
// true: includes any AECDM extended properties if available.
//       — expect larger payloads and slower responses, especially for large element groups
Console.Write("Do you want to include Extended Properties? (Yes/No): ");
var includeExtendedProperties = Console.ReadLine();

// 5. Run IFC conversion and mesh geometry workflows using the selected ElementGroupInfo
await ConvertFilteredAECDMElementsToIFCAsync(aecdmClient, elementGroupInfo, includeExtendedProperties == "Yes", region);
await ConvertCompleteAECDMElementGroupToIFCAsync(aecdmClient, elementGroupInfo, includeExtendedProperties == "Yes", region);
await GetMeshGeometriesForFilteredAECDMElementsAsync(aecdmClient, elementGroupInfo, region);
await GetMeshGeometriesForCompleteAECDMElementGroupAsync(aecdmClient, elementGroupInfo, region);
await GetMeshGeometriesExampleWithOptions(aecdmClient, elementGroupInfo, region);
```

### Navigation API

The `SelectElementGroupViaNavigationAsync` method uses the Navigation API to browse the account hierarchy and returns an `ElementGroupInfo` that is passed into all downstream workflows:

```csharp
// aecdmClient is Autodesk.Data.AECDM.Client
// region is the Region enum value selected by the user at startup

// Step 1: Get all hubs accessible to the account
var hubs = await aecdmClient.GetHubsAsync(region);                        // returns List<HubInfo>

// Step 2: Get all projects within the chosen hub
var projects = await aecdmClient.GetProjectsAsync(selectedHub, region);   // returns List<ProjectInfo>

// Step 3: Get all element groups (Revit models) within the chosen project
var elementGroups = await aecdmClient.GetElementGroupsAsync(selectedProject, region); // returns List<ElementGroupInfo>

// The selected ElementGroupInfo is returned and used by all IFC/mesh methods
return selectedElementGroup; // ElementGroupInfo
```

### IFC and Mesh Workflows

#### IFC Conversion

The generated `.ifc` file is saved to:
```
%AppData%\{ConnectorName}\{user}\Geometry\{ExchangeID}\Geometries\{ifcFileId}.ifc
```
The full path is printed to the console on completion. `ifcFileId` is the string you pass to `ConvertToIfc(...)` — if omitted, a GUID is used.

##### Filtered Elements from an Element Group
```csharp
await ConvertFilteredAECDMElementsToIFCAsync(client, elementGroupInfo, includeExtendedProperties: true, region);
```

##### Complete Element Group
```csharp
await ConvertCompleteAECDMElementGroupToIFCAsync(client, elementGroupInfo, includeExtendedProperties: false, region);
```

#### Mesh Geometry Retrieval

##### Filtered Elements from an Element Group
```csharp
await GetMeshGeometriesForFilteredAECDMElementsAsync(client, elementGroupInfo, region);
```

##### Complete Element Group
```csharp
await GetMeshGeometriesForCompleteAECDMElementGroupAsync(client, elementGroupInfo, region);
```

##### Advanced example with BRep-to-Mesh options
```csharp
await GetMeshGeometriesExampleWithOptions(client, elementGroupInfo, region);
```

**`ElementPropertyFilter` reference** — `PropertyName` and `Operator` are free-form strings passed directly to the AECDM GraphQL API.

Common property names:

| Property | Example values |
|---|---|
| `category` | `"Walls"`, `"Doors"`, `"Windows"`, `"Roofs"` |
| `Element Context` | `"Instance"`, `"Type"` |
| `name` | Any element name string |
| `Area` | Numeric string, e.g. `"100"` |
| `Revit Element ID` | Revit element ID string |

Supported operators: `==`, `!=`, `>`, `<`, `>=`, `<=`

Composition: `AllOf(...)` = AND, `AnyOf(...)` = OR. These can be nested freely.

---

Inside this method, elements are retrieved first, then filtered, and geometry is fetched with custom tessellation options:

```csharp
var elements = await elementGroup.GetElementsAsync(elementGroupInfo);
var wallElements = elements.Where(e => e.Category == "Walls");
var elementGeometryMap = await elementGroup.GetElementGeometriesAsMeshAsync(wallElements, new Autodesk.Data.Geometry.BRepToMeshOptions()
{
    SurfaceTolerance = 1.0,
    NormalTolerance = 15,
    MaxEdgeLength = 2.0,
    GridAspectRatio = 0.1,
});
```

**`BRepToMeshOptions` reference** — all values use model units (meters/feet) except `NormalTolerance` (degrees). Pass `null` to use SDK defaults.

| Parameter | Unit | What it controls | Tighter = |
|---|---|---|---|
| `SurfaceTolerance` | Model units | Max gap between mesh and original surface | More triangles on curves |
| `NormalTolerance` | Degrees | Max angle between adjacent triangle normals | Smoother curved surfaces |
| `MaxEdgeLength` | Model units | Max triangle edge length | More uniform density |
| `GridAspectRatio` | Ratio | Max triangle aspect ratio (prevents slivers) | Better mesh quality |

Quick guidance: for **visualization**, tighten `SurfaceTolerance` (`0.001`–`0.01`) and `NormalTolerance` (`5`–`10`). For **lightweight preview**, loosen them (`0.5`–`1.0`, `30`–`45`). Use `MaxEdgeLength` when you need predictable triangle sizes for analysis pipelines.

---

## 🔄 Migrating from `Autodesk.Data 0.1.7-beta` to `0.2.2-beta`

This section documents all breaking changes and new features introduced in `0.2.2-beta`.

---

### 1. Client Creation — Factory Pattern

The `Client` constructor is replaced by `DataSdkClientFactory`. The returned type is `Autodesk.Data.AECDM.Client`, which is the main entry point for all SDK operations.

**Before (0.1.7-beta):**
```csharp
using Autodesk.Data;

var sdkOptions = new SDKOptionsDefaultSetup { ... };
return new Client(sdkOptions);
```

**After (0.2.2-beta):**
```csharp
using Autodesk.Data;

var sdkOptions = new SDKOptionsDefaultSetup { ... };
var clientFactory = new DataSdkClientFactory();
Autodesk.Data.AECDM.Client aecdmClient = clientFactory.CreateAecdmClient(sdkOptions);
return aecdmClient;
```

This applies to both the standard (`ClientSecret`) and PKCE authentication setups.

---

### 2. ElementGroup Construction — Constructor replaces Static Factory

`ElementGroup.Create(...)` is replaced by the `new ElementGroup(...)` constructor. The `Region` enum has also moved from a nested type on `ElementGroup` to the `Autodesk.Data.Enums` namespace.

**Before (0.1.7-beta):**
```csharp
// Region was nested inside ElementGroup
var elementGroup = ElementGroup.Create(client, ElementGroup.Region.US);

// Default (no region)
var elementGroup = ElementGroup.Create(client);
```

**After (0.2.2-beta):**
```csharp
using Autodesk.Data.Enums;

// Region is now in Autodesk.Data.Enums
var elementGroup = new ElementGroup(client, Region.US);
```

---

### 3. Navigation API — Replaces Hardcoded IDs

In `0.1.7-beta`, all methods accepted raw `string` IDs that had to be copied from the portal and hardcoded. In `0.2.2-beta`, the **Navigation API** discovers your data at runtime. Navigation API methods accept a type-safe `Region?` enum directly — no string conversion needed.

**Before (0.1.7-beta):**
```csharp
// IDs had to be hardcoded as string constants
await ConvertCompleteAECDMElementGroupToIFCAsync(client, "Your_GraphQLElementGroupId");
await GetMeshGeometriesForFilteredAECDMElementsAsync(client, "Your_GraphQLElementGroupId");
```

**After (0.2.2-beta):**
```csharp
// Prompt user for region (returns Region enum value — US, EMEA, or AUS)
var region = PromptForRegion();
var elementGroupInfo = await SelectElementGroupViaNavigationAsync(client, region);

// Pass the resolved ElementGroupInfo and region to all methods
await ConvertCompleteAECDMElementGroupToIFCAsync(client, elementGroupInfo, includeExtendedProperties, region);
await GetMeshGeometriesForFilteredAECDMElementsAsync(client, elementGroupInfo, region);
```

The Navigation API methods accept `Region?` directly — no string conversion needed:
```csharp
var hubs     = await aecdmClient.GetHubsAsync(region);                        // Region? — type-safe
var projects = await aecdmClient.GetProjectsAsync(selectedHub, region);        // Region? — type-safe
var groups   = await aecdmClient.GetElementGroupsAsync(selectedProject, region); // Region? — type-safe
```

---

### 4. Method Signatures — `ElementGroupInfo` replaces `string`

All methods that previously accepted a raw `string` element group ID now accept `ElementGroupInfo`.

**Before (0.1.7-beta):**
```csharp
private static async Task ConvertFilteredAECDMElementsToIFCAsync(Client client, string GraphQLElementGroupId)
{
    var elementGroup = ElementGroup.Create(client);
    await elementGroup.GetElementsAsync(GraphQLElementGroupId, filter);
    ...
}
```

**After (0.2.2-beta):**
```csharp
private static async Task ConvertFilteredAECDMElementsToIFCAsync(Autodesk.Data.AECDM.Client client, ElementGroupInfo elementGroupInfo, bool includeExtendedProperties, Region region)
{
    var elementGroup = new ElementGroup(client, region);
    await elementGroup.GetElementsAsync(elementGroupInfo, filter, includeExtendedProperties);
    ...
}
```

---

### 5. Extended Properties Support — New Parameter

`GetElementsAsync` now accepts an `includeExtendedProperties` flag for IFC conversion workflows.

**Before (0.1.7-beta):**
```csharp
await elementGroup.GetElementsAsync(GraphQLElementGroupId);
await elementGroup.GetElementsAsync(GraphQLElementGroupId, filter);
```

**After (0.2.2-beta):**
```csharp
// With filter and extended properties
await elementGroup.GetElementsAsync(elementGroupInfo, filter, includeExtendedProperties: true);

// Without filter, with extended properties as named parameter
await elementGroup.GetElementsAsync(elementGroupInfo, includeExtendedProperties: false);
```

---

### 6. `GetElementsAsync` Return Value

`GetElementsAsync` now returns the collection of elements directly, instead of requiring access via `elementGroup.Elements`.

**Before (0.1.7-beta):**
```csharp
await elementGroup.GetElementsAsync(GraphQLElementGroupId);
var wallElements = elementGroup.Elements.Where(e => e.Category == "Walls");
```

**After (0.2.2-beta):**
```csharp
var elements = await elementGroup.GetElementsAsync(elementGroupInfo);
var wallElements = elements.Where(e => e.Category == "Walls");
```

---

### 7. Removed Methods

The following methods that operated on individual elements by raw string ID have been removed in `0.2.2-beta`:

| Removed Method | Reason |
|---|---|
| `ConvertSingleAECDMElementToIFCAsync(client, string elementId)` | Use filtered `GetElementsAsync` with an element group |
| `ConvertMultipleAECDMElementsToIFCAsync(client, List<string> elementIds)` | Use filtered `GetElementsAsync` with an element group |
| `GetMeshGeometryForSingleAECDMElementAsync(client, string elementId)` | Use `GetElementGeometriesAsMeshAsync` with an element group |
| `GetMeshGeometriesForMultipleAECDMElementsAsync(client, List<string> elementIds)` | Use `GetElementGeometriesAsMeshAsync` with an element group |

---

### 8. New `using` Directives Required

**Before (0.1.7-beta):**
```csharp
using Autodesk.Data;
using Autodesk.Data.DataModels;
using System.Configuration;
```

**After (0.2.2-beta):**
```csharp
using Autodesk.Data;
using Autodesk.Data.DataModels;
using Autodesk.Data.Enums;  // For Region enum (Region.US, Region.EMEA, Region.AUS)
using System.Configuration;
using System.Data;
// Autodesk.Data.AECDM.Client is used as the fully qualified type — no alias needed
```

---

## 🐛 Troubleshooting

### Common Issues

#### Authentication Errors
**Problem**: `Required authentication configuration 'AuthClientID' is missing`
**Solution**: Ensure all authentication values are filled in `App.config`

**Problem**: `Token request failed with status 401`
**Solution**:
- Verify your Client ID and Client Secret are correct
- Ensure your app has the required API permissions

#### Navigation / No Data Found
**Problem**: `No hubs found. Make sure AECDM is enabled on your account.`
**Solution**: Ensure your Autodesk account has AECDM enabled and your credentials have access to at least one hub. Make sure your APS App is already integrated with Forma Hub, either by Custom Integration or Install from App Gallery.

**Problem**: `No element groups found. Make sure Revit 2024+ models were uploaded after AECDM activation.`
**Solution**: Element Groups (Revit models) only appear if they were uploaded to the project *after* AECDM was activated on the account. Re-upload models if needed.

#### Network/API Errors
**Problem**: `Failed to connect to authentication service`
**Solution**:
- Check your internet connection
- Verify API endpoints are accessible
- Check if your firewall is blocking the requests

### Getting Help

1. **Check the Console Output**: The application provides detailed logging
2. **Verify Configuration**: Double-check all values in `App.config`
3. **Test Authentication**: Ensure your credentials work in the Autodesk Developer Portal

---

## 🎉 Next Steps

Once you have this sample running:

1. **Explore the Data**: Examine the AECDM data structure and properties
2. **Customize Filters**: Modify the `ElementPropertyFilter` expressions to target your specific element categories
3. **Tune Mesh Options**: Adjust `BRepToMeshOptions` (surface tolerance, normal tolerance, edge length) to balance quality and performance
4. **Add Features**: Implement additional geometry processing or export formats
5. **Scale Up**: Process larger datasets or integrate with your existing workflows

Happy coding! 🚀


## License

This sample is licensed under the terms of the [MIT License](http://opensource.org/licenses/MIT). Please see the [LICENSE](LICENSE) file for full details.

