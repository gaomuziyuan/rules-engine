# RulesEngine

## Overview
The RulesEngine project is designed to determine client eligibility for the Winter Supplement and calculate the appropriate supplement amount based on predefined rules. The application uses event-driven architecture with MQTT messaging for communication. This implementation fulfills the requirements of the ISL 21R Full Stack Developer competition.

## Architecture
The project follows a modular and enterprise-grade architecture with clear separation of concerns:

1. Controllers
   MqttController.cs: Manages API endpoints for triggering MQTT communication and testing the rules engine.
2. Models
   InputData.cs: Defines the input data schema, such as family composition and the number of children.
   OutputData.cs: Defines the output schema, including eligibility status, base amount, child amount, and total supplement.
3. Services
   IMqttService.cs: Interface for the MQTT communication service.
   MqttService.cs: Handles MQTT communication, including subscribing to input topics and publishing results.
   IRulesEngineService.cs: Interface for the rules engine service.
   RulesEngineService.cs: Implements the rules engine logic for calculating the Winter Supplement.
4. Tests
   MqttServiceTests.cs: Unit tests for the MQTT service.
   RulesEngineServiceTests.cs: Unit tests for the rules engine service. 

## Data Flow
Input Data (Published to MQTT Topic: BRE/calculateWinterSupplementInput/<MQTT topic ID>):

    {
    "id": "string",
    "numberOfChildren": "integer",
    "familyComposition": "string",
    "familyUnitInPayForDecember": "boolean"
    }

    
Processing:

RulesEngineService determines eligibility and calculates supplement amounts based on family composition and the number of children.
Output Data (Published to MQTT Topic: BRE/calculateWinterSupplementOutput/<MQTT topic ID>):

    {
    "id": "string",
    "isEligible": "boolean",
    "baseAmount": "float",
    "childrenAmount": "float",
    "supplementAmount": "float"
    }

## Setup Instructions
### Prerequisites
.NET 7.0 SDK:Ensure the latest version of .NET SDK is installed.
   
MQTT Broker: Use a free MQTT broker, such as test.mosquitto.org, on port 1883.

### Clone the Repository
    git clone <repository_url>
    cd RulesEngine
### Configure the Environment

Ensure the appsettings.json or appsettings.Development.json files are configured if additional settings are required.

### Build and Run the Application
    dotnet build
    dotnet run