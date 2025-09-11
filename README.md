# aps-aecdm-data-sdk-code-sample (**beta**)

[![OAuth2](https://img.shields.io/badge/OAuth2-v2-green.svg)](http://developer.autodesk.com/)
[![AEC-DM-Geometry](https://img.shields.io/badge/AEC%20DM%20Geometry-beta-blue.svg)](http://developer.autodesk.com/)
![Data SDK Version](https://img.shields.io/badge/Data%20SDK-beta-blue.svg)


![.NET](https://img.shields.io/badge/NET-4.8%20%7C%208.0-green.svg)
![Platforms](https://img.shields.io/badge/Web-Windows%20%7C%20MacOS%20%7C%20Linux-lightgray.svg)


![Advanced](https://img.shields.io/badge/Level-Advanced-red.svg)
[![MIT](https://img.shields.io/badge/License-MIT-blue.svg)](http://opensource.org/licenses/MIT)

# Disclaimer
Please note that AEC DM Geometry API and DataSDK is currently in beta testing, API calls are subject to changes based on the feedback received. We recommend avoiding using this API to build production software during this phase. If you have an urgent requirement, please contact our team before utilizing the API.

You are required to participate in the [AEC Data Model Public Beta Program](https://feedback.autodesk.com/), follow the instructions, download the DataSDK, and provide your feedback there.

# Description
A comprehensive sample application demonstrating how to integrate Autodesk's Data SDK with AECDM (Architecture, Engineering, Construction, and Design Manufacturing) data. This application shows how to query AECDM data, process geometry, and export to IFC format.

## 🎯 What This Application Does

This sample demonstrates the complete workflow for working with AECDM data:

1. **Authentication** - Secure connection to Autodesk APIs using OAuth 2.0
2. **Data Querying** - Retrieve elements from AECDM using high-level API methods. You can fetch all elements from an element group, or fetch a single element by its ID.
3. **SDK Integration** - Add elements to the Data SDK's ElementGroup abstraction automatically
4. **Export** - Convert all elements to IFC format (geometry processing is handled by the SDK)
5. **Geometry Display** - Retrieve and display mesh geometry information for each element
6. **Advanced Option** - You can also fetch geometry data directly for individual elements and use it as needed, but the recommended workflow is to use the ConvertToIfc and mesh methods for simplicity and reliability.

## 📋 Prerequisites

### Software Requirements
- **Visual Studio 2022** or **Visual Studio Code** with C# extension
- **.NET Framework 4.8 or .NET 8.0 SDK**
- **Internet connection** for API access

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
     - **Callback URL**: `http://localhost:8080/api/auth/callback/` (for testing)
   - Select APIs: Check "Data Management API" and "Model Derivative API"
   - Save the application

3. **Get Your Credentials**
   - **Client ID**: Copy from your app's details page
   - **Client Secret**: Copy from your app's details page (keep this secure!)

## 🚀 Quick Start Guide

### Step 1: Clone and Setup

```bash
# Clone the repository
git clone <your-repository-url>
cd data-sdk-aecdm-sample

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
        <add key="AuthClientSecret" value="YOUR_CLIENT_SECRET_HERE" />
        <!-- Required: OAuth callback URL -->
        <add key="AuthCallBack" value="http://localhost:8080/api/auth/callback/" />
        <!-- Optional: Application settings -->
        <add key="ApplicationName" value="AECDMSampleApp" />
        <add key="ApplicationDataPath" value="" />
        <add key="LogLevel" value="Info" />
    </appSettings>
</configuration>
```

### Step 3: Update Element Group ID or Element IDs
The sample uses placeholders for element group ID or element IDs for demonstration. You can also fetch a single element by its ID. Replace these with your actual GraphQL `elementGroup` or element IDs in `Program.cs`:

```csharp
// Replace this with your actual element group ID
await ConvertCompleteAECDMElementGroupToIFCAsync(client, "Your_GraphQLElementGroupId");

// Or fetch a single element
await ConvertSingleAECDMElementToIFCAsync(client, "Your_GraphQLElementId");

// Or fetch multiple elements by Id
await ConvertMultipleAECDMElementsToIFCAsync(client, new List<string> { "Your_GraphQLElementId1", "Your_GraphQLElementId2" });
```

### Step 4: Build and Run
# Build the application
dotnet build

# Run the application
dotnet run
```
## 🌍 Element Group Region Selection

When creating an `ElementGroup`, you can specify the region to optimize data access and processing. The available regions are:
- `ElementGroup.Region.US` (default)
- `ElementGroup.Region.EMEA`
- `ElementGroup.Region.AUS`

**Example:**// Create an ElementGroup for the US region (default)
var elementGroup = ElementGroup.Create(client, ElementGroup.Region.US);

// Or for EMEA
var elementGroup = ElementGroup.Create(client, ElementGroup.Region.EMEA);

// Or for AUS
var elementGroup = ElementGroup.Create(client, ElementGroup.Region.AUS);
Specifying the region is optional. If not provided, US is used by default. Choose the region closest to your data or users for best performance.

## 📁 Project Structure

```
data-sdk-aecdm-sample/
├── Program.cs              # Main application entry point with workflow orchestration
├── App.config              # Configuration file for credentials and settings
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
| `AuthCallBack`        | OAuth callback URL                                                          | `http://localhost:8080/api/auth/callback/` |
| `ApplicationName`     | Custom application name                                                     | `MyAECDMApp`                    |
| `ApplicationDataPath` | Custom data directory                                                       | `C:\MyApp\Data`               |
| `LogLevel`            | Logging verbosity                                                           | `Info`, `Debug`, `Error`        |

## 🔍 Understanding the Code

### Main Workflow (`Program.cs`)
The application uses the following high-level API methods for AECDM data access:

```csharp
// Initialize SDK and element group
var client = SetupAutodeskDataSDK();
var elementGroup = ElementGroup.Create(client);

// Fetch all elements in a group (recommended)
await elementGroup.GetElementsAsync(SAMPLE_ELEMENT_GROUP_ID);

// Or fetch and add a single element by its ID
var element = await elementGroup.GetElementAsync(SAMPLE_ELEMENT_ID_1);

// Export all elements to IFC format
var ifcFilePath = await elementGroup.ConvertToIfc();
Console.WriteLine($"IFC file created at: {ifcFilePath}");
```

### IFC and Mesh Workflows

#### IFC Conversion

##### Single Element
```csharp
await ConvertSingleAECDMElementToIFCAsync(client, "Your_GraphQLElementId");
```

##### Multiple Elements
```csharp
await ConvertMultipleAECDMElementsToIFCAsync(client, new List<string> { "Your_GraphQLElementId1", "Your_GraphQLElementId2" });
```

##### Filtered Elements from an Element Group
```csharp
await ConvertFilteredAECDMElementsToIFCAsync(client, "Your_GraphQLElementGroupId");
```

##### Complete Element Group
```csharp
await ConvertCompleteAECDMElementGroupToIFCAsync(client, "Your_GraphQLElementGroupId");
```

#### Mesh Geometry Retrieval

##### Single Element
```csharp
await GetMeshGeometryForSingleAECDMElementAsync(client, "Your_GraphQLElementId");
```

##### Multiple Elements
```csharp
await GetMeshGeometriesForMultipleAECDMElementsAsync(client, new List<string> { "Your_GraphQLElementId1", "Your_GraphQLElementId2" });
```

##### Filtered Elements from an Element Group
```csharp
await GetMeshGeometriesForFilteredAECDMElementsAsync(client, "Your_GraphQLElementGroupId");
```

##### Complete Element Group
```csharp
await GetMeshGeometriesForCompleteAECDMElementGroupAsync(client, "Your_GraphQLElementGroupId");
```

##### Advanced example with options
```csharp
await GetMeshGeometriesExampleWithOptions(client, "Your_GraphQLElementGroupId");
```

## 🐛 Troubleshooting

### Common Issues

#### Authentication Errors
**Problem**: `Required authentication configuration 'AuthClientID' is missing`
**Solution**: Ensure all authentication values are filled in `App.config`

**Problem**: `Token request failed with status 401`
**Solution**: 
- Verify your Client ID and Client Secret are correct
- Ensure your app has the required API permissions

#### Network/API Errors
**Problem**: `Failed to connect to authentication service`
**Solution**: 
- Check your internet connection
- Verify API endpoints are accessible
- Check if your firewall is blocking the requests

#### Element Data Issues
**Problem**: `No element data found for Element1`
**Solution**: 
- Verify the element IDs exist in your AECDM dataset
- Check that you have permission to access these elements
- Ensure the elements contain the expected data structure

### Getting Help

1. **Check the Console Output**: The application provides detailed logging
2. **Verify Configuration**: Double-check all values in `App.config`
3. **Test Authentication**: Ensure your credentials work in the Autodesk Developer Portal
4. **Check Element IDs**: Verify your element IDs are valid and accessible

---

## 🎉 Next Steps

Once you have this sample running:

1. **Explore the Data**: Examine the AECDM data structure and properties
2. **Customize Processing**: Modify element creation logic for your specific needs
3. **Add Features**: Implement additional geometry processing or export formats
4. **Scale Up**: Process larger datasets or integrate with your existing workflows

Happy coding! 🚀


## License

This sample is licensed under the terms of the [MIT License](http://opensource.org/licenses/MIT). Please see the [LICENSE](LICENSE) file for full details.

## Written by
**Wilson Picardo** and **Aditya Singh** from AEC DM team.

